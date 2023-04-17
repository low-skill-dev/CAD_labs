using coursework.ModelsInterfaces;
using coursework.Services;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace coursework;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private readonly StatefulEditor editor;
	private int width, height;
	private readonly List<Button> buttons = new();

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

	public MainWindow()
	{
		InitializeComponent();

		this.width = (int)this.DisplayedImageI.Width;
		this.height = (int)this.DisplayedImageI.Height;

		this.editor = new(width, height) {AddAxes=false };

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

		this.DisplayedImageI.Source = editor.RenderCurrentState();
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
			this.editor.BackgroundColorArgb = colorDialog.Color.ToArgb();
			return Color.FromArgb(
				colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
		}

		return null;
	}
	private void SelectBackgroundColorBT_Click(object sender, RoutedEventArgs e)
	{
		var color = GetColorByDialog();
		if(!color.HasValue) return;

		SelectBackgroundColorBT.Background = new SolidColorBrush(Color.FromArgb(
				color.Value.A, color.Value.R, color.Value.G, color.Value.B));
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
			stepD = vals[1],
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

	private void DisplayAxesCB_Checked(object sender, RoutedEventArgs e)
	{
		editor.AddAxes = DisplayAxesCB?.IsChecked ?? false;
		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}

	private void DisplayedImageI_MouseDown(object sender, MouseButtonEventArgs e)
	{
		var pos = (GraphicLibrary.MathModels.PointF)e.GetPosition(this.DisplayedImageI);
		if(editor.CurrentState == 0) return;


		editor.MouseDown(pos, e);
		if(editor.CurrentState == StatefulEditor.States.None) {
			EnableAllButtons();
		}

		this.DisplayedImageI.Source = editor.RenderCurrentState();
	}
}
