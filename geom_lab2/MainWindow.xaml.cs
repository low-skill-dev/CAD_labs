using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphicLibrary.MathModels;
using InteractiveLibrary;

namespace geom_lab2;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	enum States
	{
		NoPointSelected,
		PointSelected
	}
	private States currentState;
	private int selectedPointId;

	private ImageEditor imageEditor;

	public MainWindow()
	{
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

		InitializeComponent();

		imageEditor = new((int)this.GameImage.Width, (int)this.GameImage.Height);
		currentState = States.NoPointSelected;

		imageEditor.RenderCurrentState();
		this.GameImage.Source = imageEditor.CurrentFrameImage;
	}

	private void GameImage_MouseDown(object sender, MouseButtonEventArgs e)
	{
		var pos = (PointF)e.GetPosition((Image)sender);

		if(currentState == States.NoPointSelected) {
			var capture = imageEditor.FindSelectedByClickId(pos, true);
			if(capture is not null) {
				selectedPointId = capture.Value;
				currentState = States.PointSelected;
			} else {
				this.imageEditor.AddControlledDot(pos);
				SetDotsCollection(imageEditor.ControlledDots.Select(x=> x.Point));
			}
		} else
		if(currentState == States.PointSelected) {
			imageEditor.ControlledDots[selectedPointId] = new(pos, false);
			currentState = States.NoPointSelected;
		}

		imageEditor.RenderCurrentState();
		this.GameImage.Source = imageEditor.CurrentFrameImage;
	}

	private void GameImage_MouseMove(object sender, MouseEventArgs e)
	{
		if(currentState == States.NoPointSelected) return;

		var pos = (PointF)e.GetPosition((Image)sender);
		if(currentState == States.PointSelected) {
			imageEditor.ControlledDots[selectedPointId] = new(pos, true);
		}

		imageEditor.RenderCurrentState();
		this.GameImage.Source = imageEditor.CurrentFrameImage;
	}

	private void SetDotsCollection(IEnumerable<PointF> points)
	{
		DisplayedDotsSP.Children.Clear();
		foreach(var point in points) {
			var wrapper = new StackPanel() { Orientation = Orientation.Horizontal };
			var tbX = new TextBox() { Width = 145, Text = ((int)point.X).ToString(), IsReadOnly=true };
			var tbY = new TextBox() { Width = 145, Text = ((int)point.Y).ToString(), IsReadOnly = true };
			wrapper.Children.Add(tbX);
			wrapper.Children.Add(tbY);

			DisplayedDotsSP.Children.Add(wrapper);
		}

		GC.Collect();
	}
}
