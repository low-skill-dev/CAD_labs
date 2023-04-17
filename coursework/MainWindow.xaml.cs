﻿using coursework.DataModels;
using coursework.ModelsInterfaces;
using coursework.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenTK.Audio.OpenAL.Extensions.Creative.EnumerateAll;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using PointF = GraphicLibrary.MathModels.PointF;

namespace coursework;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private StatefulEditor editor;
	private int width, height;
	private readonly List<Button> buttons = new();
	private readonly List<Button> groupButtons = new();

	private const int LightGreenArgb = -7278960;
	private const int LightCoralArgb = -1015680;
	private readonly SolidColorBrush LightGreenBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
		byte.MaxValue, byte.MaxValue * 3 / 4, byte.MaxValue, byte.MaxValue * 3 / 4));
	private readonly SolidColorBrush LightRedBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
		byte.MaxValue, byte.MaxValue, byte.MaxValue * 3 / 4, byte.MaxValue * 3 / 4));
	private readonly SolidColorBrush LightOrangeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
		255, 255, 213, 128));
	private readonly SolidColorBrush WhiteBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
		255, 255, 255, 255));
#if DEBUG
	[DllImport("kernel32")] public static extern bool AllocConsole();
#endif
	public MainWindow()
	{
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
#if DEBUG
		AllocConsole();
#endif

		InitializeComponent();

		this.width = (int)this.DisplayedImageI.Width;
		this.height = (int)this.DisplayedImageI.Height;

		this.editor = new(width, height) { AddAxes = false, FillerSelectionFastMode = true };

		buttons.AddRange(new Button[] {
			this.AccurateSelectionToolBT,
			this.PartialSelectionToolBT,
			this.ArcToolBT,
			this.CircleToolBT,
			this.DeleteSelectedBT,
			this.FillClosedLoopsBT,
			this.MoveSelectedBT,
			this.PolyLineToolBT,
			this.SelectBackgroundColorBT,
			this.SetColorSelectedBT,
			this.SelectRelativenessPointBT,
			this.RotateSelectedBT,
			this.FillerToolBT
		});

		groupButtons.AddRange(new Button[] {
			this.DeleteSelectedBT,
			this.FillClosedLoopsBT,
			this.MoveSelectedBT,
			this.SelectBackgroundColorBT,
			this.SetColorSelectedBT,
			this.SelectRelativenessPointBT,
			this.RotateSelectedBT,
		});

		this.DisplayedImageI.Source = editor.RenderCurrentState();
		SetUI();
	}

	private void DisableAllButtons(params Button[] except)
	{
		for(int i = 0; i < buttons.Count; i++) {
			// buttons[i].Background = except.Contains(buttons[i]) ? LightGreenBrush : LightOrangeBrush;
			buttons[i].IsEnabled = except.Contains(buttons[i]) ? true : false;
		}
	}
	private void DisableAllButtons(Button setAsActive)
	{
		for(int i = 0; i < buttons.Count; i++) {
			// buttons[i].Background = buttons[i] == setAsActive ? LightGreenBrush : LightOrangeBrush;
			buttons[i].IsEnabled = false;
		}
	}
	private void EnableAllButtons()
	{
		for(int i = 0; i < buttons.Count; i++) {
			// buttons[i].Background = WhiteBrush;
			buttons[i].IsEnabled = true;
		}
	}
	private Color? GetColorByDialog()
	{
		var colorDialog = new System.Windows.Forms.ColorDialog();
		if(colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
			return Color.FromArgb(
				colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
		}

		return null;
	}
	private void SelectBackgroundColorBT_Click(object sender, RoutedEventArgs e)
	{
		var color = GetColorByDialog();
		if(!color.HasValue) return;


		this.editor.BackgroundColorArgb = System.Drawing.Color.FromArgb(
				color.Value.A, color.Value.R, color.Value.G, color.Value.B).ToArgb();
		SelectBackgroundColorBT.Background = new SolidColorBrush(Color.FromArgb(
				color.Value.A, color.Value.R, color.Value.G, color.Value.B));

		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
	private void SelectRelativenessPointBT_Click(object sender, RoutedEventArgs e)
	{
		this.DisableAllButtons(this.SelectRelativenessPointBT);
		this.editor.State.Push(StatefulEditor.States.RelativePointSelection);
	}
	private void FillClosedLoopsBT_Click(object sender, RoutedEventArgs e)
	{
		this.DisableAllButtons(this.SelectRelativenessPointBT);
		this.editor.State.Push(StatefulEditor.States.Filling);
	}
	private void SetColorSelectedBT_Click(object sender, RoutedEventArgs e)
	{
		var color = GetColorByDialog();
		if(!color.HasValue) return;
		var colorV = color.Value;

		this.editor.CurrentlySelectedObjects.ForEach(o => o.ColorArgb =
			System.Drawing.Color.FromArgb(colorV.A, colorV.R, colorV.G, colorV.B).ToArgb());

		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
	private void MoveSelectedBT_Click(object sender, RoutedEventArgs e)
	{
		string input = Microsoft.VisualBasic.Interaction.InputBox(
			"Значение перемещения",
			"Введите значение перемещения по осям (X,Y) в формате двух чисел через пробел.",
			string.Empty);

		if(string.IsNullOrWhiteSpace(input)) return;
		float[] vals = null!;
		try {
			vals = input.Split("(){}[] ;,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Select(x => float.Parse(x.Replace(',', '.'), CultureInfo.InvariantCulture)).ToArray();
			if(vals.Length != 2) return;

		} catch {
			return;
		}
		var d = new PointF(vals[0], vals[1]);
		this.editor.CurrentlySelectedObjects.ForEach(o => o.Move(d));

		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
	private void RotateSelectedBT_Click(object sender, RoutedEventArgs e)
	{
		string input = Microsoft.VisualBasic.Interaction.InputBox(
			"Значение поворота",
			"Введите значение поворота против часовой стрелки в градусах.",
			string.Empty);

		if(string.IsNullOrWhiteSpace(input)) return;
		float[] vals = null!;
		try {
			vals = input.Split("(){}[] ;,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Select(x => float.Parse(x.Replace(',', '.'), CultureInfo.InvariantCulture)).ToArray();
			if(vals.Length != 1) return;

		} catch {
			return;
		}

		float
			stepD = vals[0],
			stepR = stepD * float.Pi / 180;

		this.editor.CurrentlySelectedObjects.ForEach(o => o.Rotate(stepR, this.editor.RelativenessPoint));

		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
	private void DeleteSelectedBT_Click(object sender, RoutedEventArgs e)
	{
		this.editor.DeleteObjects(this.editor.CurrentlySelectedObjects);
		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
	private void PartialSelectionToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.PartialSelection_NotStarted);
	}
	private void AccurateSelectionToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.FullSelection_NotStarted);
	}
	private void PolyLineToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.PolylineDrawing_NotStarted);
	}
	private void CircleToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.CircleDrawing_NotStarted);
	}
	private void ArcToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.ArcDrawing_NotStarted);
	}
	private void FillerToolBT_Click(object sender, RoutedEventArgs e)
	{
		DisableAllButtons();
		this.editor.State.Push(StatefulEditor.States.Filling);
	}

	private void SetUI()
	{
		this.CurrentStatusTB.Text = $"Состояние: {editor.CurrentStateString}";
		this.RelativenessPointTB.Text = $"{(int)editor.RelativenessPoint.X}; {(int)editor.RelativenessPoint.Y}";
	}

	private void DisplayAxesCB_Checked(object sender, RoutedEventArgs e)
	{
		editor.AddAxes = DisplayAxesCB?.IsChecked ?? false;
		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}

	private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Escape && editor.TryExit()) {
			EnableAllButtons();
		}

		this.DisplayedImageI.Source = editor.RenderCurrentState();

		SetUI();
	}

	private void SelectDrawingColorBT_Click(object sender, RoutedEventArgs e)
	{
		var color = GetColorByDialog();
		if(!color.HasValue) return;


		this.editor.DrawingColorArgb = System.Drawing.Color.FromArgb(
				color.Value.A, color.Value.R, color.Value.G, color.Value.B).ToArgb();
		SelectDrawingColorBT.Background = new SolidColorBrush(Color.FromArgb(
				color.Value.A, color.Value.R, color.Value.G, color.Value.B));
	}

	private void DisplayedImageI_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		var pos = (PointF)(e.GetPosition(this.DisplayedImageI));

		this.editor.MouseAt = pos;

		switch(editor.CurrentState) {
			case StatefulEditor.States.ArcDrawing_SecondPointAdded:
			case StatefulEditor.States.CircleDrawing_CenterSelected:
			case StatefulEditor.States.PolylineDrawing_LastIsCircle:
			case StatefulEditor.States.PolylineDrawing_LastIsLine:
				SetUI();
				this.DisplayedImageI.Source = editor.RenderCurrentState();
				break;
		}
	}

	private void DisplayedImageI_MouseDown(object sender, MouseButtonEventArgs e)
	{
		var pos = (GraphicLibrary.MathModels.PointF)e.GetPosition(this.DisplayedImageI);
		if(editor.CurrentState == 0) return;


		editor.MouseDown(pos, e);
		if(editor.CurrentState == StatefulEditor.States.ClickCapture) {
			EnableAllButtons();
		}
		if(editor.CurrentState == StatefulEditor.States.CapturedObjectsEdition) {
			DisableAllButtons(groupButtons.ToArray());
			this.SelectedObjectsSP.Children.Clear();
			foreach(var obj in editor.CurrentlySelectedObjects) {
				var tb = new TextBlock() { FontSize = 16 };
				tb.Text = obj.ToRussianString;
				this.SelectedObjectsSP.Children.Add(tb);
			}
		} else {
			this.SelectedObjectsSP.Children.Clear();
		}

		SetUI();
		this.DisplayedImageI.Source = editor.RenderCurrentState();
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
	}



	private void SaveAsFile_Click(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.SaveFileDialog dialog = new();
		dialog.Filter = "VectorDraw files (*.vcdrproj)|*.vcdrproj";
		if(!(dialog.ShowDialog(this) ?? false)) {
			return;
		}

		var opts = new System.Text.Json.JsonSerializerOptions(JsonSerializerDefaults.General);
		opts.IncludeFields = true;
		opts.IgnoreReadOnlyFields = true;
		opts.IgnoreReadOnlyFields = true;


		var saving = this.editor.ToSaved();
		var json =
			System.Text.Json.JsonSerializer.Serialize(saving, opts);

		System.IO.File.WriteAllText(dialog.FileName, json);
	}

	private void LoadFromFile_Click(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.OpenFileDialog dialog = new();
		dialog.Filter = "VectorDraw files (*.vcdrproj)|*.vcdrproj";
		if(!(dialog.ShowDialog(this) ?? false)) {
			return;
		}

		var opts = new System.Text.Json.JsonSerializerOptions(JsonSerializerDefaults.General);
		opts.IncludeFields = true;
		opts.IgnoreReadOnlyFields = true;
		opts.IgnoreReadOnlyFields = true;

		Saved obj = null!;
		try {
			if(obj is null)
				obj = System.Text.Json.JsonSerializer.Deserialize<Saved>(System.IO.File.ReadAllText(dialog.FileName), opts);
			if(obj is null)
				obj = JsonConvert.DeserializeObject<Saved>(System.IO.File.ReadAllText(dialog.FileName));
			if(obj is null)
				obj = Utf8Json.JsonSerializer.Deserialize<Saved>(System.IO.File.ReadAllText(dialog.FileName));
			if(obj is null)
				throw new AggregateException(nameof(Saved));
		} catch(Exception ex) {
			return;
		}

		this.editor = new(obj);
		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}

	// https://learn.microsoft.com/ru-ru/dotnet/api/system.drawing.imaging.encoderparameter?view=dotnet-plat-ext-7.0
	private ImageCodecInfo GetEncoder(ImageFormat format)
	{
		ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

		foreach(ImageCodecInfo codec in codecs) {
			if(codec.FormatID == format.Guid) {
				return codec;
			}
		}

		return null;
	}
	private void ExportToFile_Click(object sender, RoutedEventArgs e)
	{
		Microsoft.Win32.SaveFileDialog dialog = new();
		dialog.Filter = "Images (*.png)|*.bmp";
		if(!(dialog.ShowDialog(this) ?? false)) {
			return;
		}


		ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

		System.Drawing.Imaging.Encoder myEncoder =
			System.Drawing.Imaging.Encoder.Quality;

		EncoderParameters myEncoderParameters = new EncoderParameters(1);
		EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
		myEncoderParameters.Param[0] = myEncoderParameter;


		this.editor.Bitmap.Save(dialog.FileName, jpgEncoder, myEncoderParameters);
	}
}
