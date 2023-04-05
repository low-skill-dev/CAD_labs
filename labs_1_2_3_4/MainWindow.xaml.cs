using GraphicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace lab1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		enum States
		{
			WaitingFirstPoint,
			WaitingSecondPoint
		}

		private States _currentState;
		private BitmapDrawer _drawer;
		private Point? prevPoint;

		public MainWindow()
		{
			InitializeComponent();
#if RELEASE
            DebugOut.Visibility = Visibility.Collapsed;
#endif

			this.ShowedImage.Source = Common.BitmapToImageSource(
				new System.Drawing.Bitmap((int)this.ShowedImage.Width, (int)this.ShowedImage.Height));

			_currentState = States.WaitingFirstPoint;
			_drawer = new((int)this.ShowedImage.Width, (int)this.ShowedImage.Height);
			DebugOut.Text = $"Ожидание первой точки.";
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_drawer.Reset();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		}


		private bool isPatternValid()
		{
			var userPattern = this.PatterResolver.Text;
			if(string.IsNullOrWhiteSpace(userPattern) || userPattern.Any(c => c != '+' && c != '-')) {
				this.PatterResolver.Background = new SolidColorBrush(Colors.LightCoral);
				return false;
			}

			this.PatterResolver.Background = new SolidColorBrush(Colors.LightGreen);
			return true;
		}
		private IEnumerator<bool> CreateUserResolver()
		{
			var userPattern = this.PatterResolver.Text;
			while(true) {
				foreach(char c in userPattern) {
					if(c == '+') yield return true;
					if(c == '-') yield return false;
				}
			}
		}

		private void ShowedImage_Click(object sender, MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(ShowedImage);

			if(_currentState == States.WaitingFirstPoint) {
				prevPoint = pos;
				_currentState = States.WaitingSecondPoint;
				DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание второй точки.";
			} else
			if(_currentState == States.WaitingSecondPoint) {
				if(prevPoint is null) {
					throw new AggregateException("Reached second point state without first point appeared.");
				}
				if(prevPoint.Equals(pos)) return;

				var p1 = new System.Drawing.Point((int)prevPoint.Value.X, (int)prevPoint.Value.Y);
				var p2 = new System.Drawing.Point((int)pos.X, (int)pos.Y);
				if(isPatternValid()) {
					if((isSimpleLine.IsChecked ?? false) || (isBresLine.IsChecked ?? false)) {
						_drawer.AddLine(p1, p2, null, CreateUserResolver());
					} else
					if((isSimpleCircle.IsChecked ?? false) || (isBresCircle.IsChecked ?? false)) {
						_drawer.AddCircle(p1, p2, null, CreateUserResolver());
					}
				} else {
					if(isSimpleLine.IsChecked ?? false) {
						_drawer.AddLine(p1, p2, null, GraphicLibrary.Models.Line.GetPatternResolver16());
					}
					else if(isBresLine.IsChecked ?? false) {
						_drawer.AddLine(p1, p2, null, GraphicLibrary.Models.Line.GetBresenhamPatternResolver16());
					}
					else if(isSimpleCircle.IsChecked ?? false) {
						_drawer.AddCircle(p1, p2, null, GraphicLibrary.Models.Circle.GetPatternResolver16());
					} 
					else if(isBresCircle.IsChecked ?? false) {
						_drawer.AddCircle(p1, p2, null, GraphicLibrary.Models.Circle.GetBresenhamPatternResolver16());
					}
				}
				_drawer.RenderFrame();
				ShowedImage.Source = _drawer.CurrentFrameImage;
				_currentState = States.WaitingFirstPoint;
				DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание первой точки.";
			}

			return;
		}
	}
}
