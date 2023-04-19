using InteractiveLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PointF = GraphicLibrary.MathModels.PointF;

namespace geom_lab2;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private enum States
	{
		NoPointSelected,
		PointSelected
	}
	private States currentState;
	private int selectedPointId;

	private readonly ImageEditor imageEditor;
	private readonly float sizeCoeff = 1f;

	public MainWindow()
	{
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

		InitializeComponent();

		imageEditor = new((int)(GameImage.Width * sizeCoeff), (int)(GameImage.Height * sizeCoeff));
		currentState = States.NoPointSelected;

		imageEditor.RenderCurrentState();
		GameImage.Source = imageEditor.CurrentFrameImage;
	}

	private void GameImage_MouseDown(object sender, MouseButtonEventArgs e)
	{
		var pos = sizeCoeff * (PointF)e.GetPosition((Image)sender);


		if(currentState == States.NoPointSelected) {
			var capture = imageEditor.FindSelectedByClickId(pos, true);

			if(capture is not null) {
				if(e.LeftButton == MouseButtonState.Pressed) {
					selectedPointId = capture.Value;
					currentState = States.PointSelected;
				} else
				if(e.RightButton == MouseButtonState.Pressed) {
					imageEditor.ControlledDots.RemoveAt(capture.Value);
				}
			} else {
				imageEditor.AddControlledDot(pos);
				selectedPointId = -1;
			}
		} else
		if(currentState == States.PointSelected) {
			imageEditor.ControlledDots[selectedPointId] = new(pos, false);
			currentState = States.NoPointSelected;
			selectedPointId = -1;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
		}

		SetDotsCollection(imageEditor.ControlledDots.Select(x => x.Point), selectedPointId);
		imageEditor.RenderCurrentState();
		GameImage.Source = imageEditor.CurrentFrameImage;
	}

	private readonly DateTime LastMoved = DateTime.UtcNow;
	private void GameImage_MouseMove(object sender, MouseEventArgs e)
	{
		if(currentState == States.NoPointSelected) {
			return;
		}

		var pos = (PointF)e.GetPosition((Image)sender);
		if(currentState == States.PointSelected) {
			imageEditor.ControlledDots[selectedPointId] = new(pos, true);

			var dtms = (DateTime.UtcNow - LastMoved).TotalMilliseconds;

			if(dtms > 300) {
				imageEditor.RenderCurrentState();
			}

			GameImage.Source = imageEditor.CurrentFrameImage;
		}
	}

	private void SetDotsCollection(IEnumerable<PointF> points, int selectedId = -1)
	{
		var yellowBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 50));
		var whiteBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));


		DisplayedDotsSP.Children.Clear();
		var count = 0;
		foreach(var point in points) {
			var wrapper = new StackPanel() { Orientation = Orientation.Horizontal };
			var tbX = new TextBox() { Width = 145, FontSize = 24, IsReadOnly = true, Background = count == selectedId ? yellowBrush : whiteBrush, Text = ((int)point.X).ToString() };
			var tbY = new TextBox() { Width = 145, FontSize = 24, IsReadOnly = true, Background = count == selectedId ? yellowBrush : whiteBrush, Text = ((int)point.Y).ToString() };

			_ = wrapper.Children.Add(tbX);
			_ = wrapper.Children.Add(tbY);

			_ = DisplayedDotsSP.Children.Add(wrapper);
			count++;
		}
	}

	private void ClearPointsBT_Click(object sender, RoutedEventArgs e)
	{
		currentState = States.NoPointSelected;
		selectedPointId = -1;
		imageEditor.ControlledDots.Clear();
		imageEditor.Reset();
		DisplayedDotsSP.Children.Clear();
		imageEditor.RenderCurrentState();
		GameImage.Source = imageEditor.CurrentFrameImage;
	}
}
