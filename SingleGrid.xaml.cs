using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static MazeTest.MainPage;

namespace MazeTest {
	public sealed partial class SingleGrid: UserControl {
		public Node node { get; set; }

		public SingleGrid(Node node) {
			this.node = node;
			this.InitializeComponent();
			Initialize();
		}
		public void Initialize() {
			if(node == null) {
				throw new Exception("???");
				//SetDirections(Direction.ALLFALSE);
				//return;
			}

			bool b = Instance.row * Instance.col > 400;
			MyText.Visibility = b ? Visibility.Collapsed : Visibility.Visible;

			MyText.Text = node.pos.ToString();
			if(Instance.unvisited.Contains(node)) {
				MyText.Text += "\nUNVISITED";
			}
			SetDirections(node.d);
			//SetDirections(Direction.ALLTRUE);
			if(node.previous != null) {
				//Color c = App.GetRandomColor();
				//SetFill(c);
				//Debug.WriteLine("Color : " + c);
				MyText.Text += "\nPre " + node.previous.pos;
			}
		}
		public void AdjustBorder(int w) {
			topR.Height = w;
			bottomR.Height = w;
			leftR.Width = w;
			rightR.Width = w;
		}
		public void SetFill(Color color) {
			//centerR.Fill = new SolidColorBrush(color);
		}
		public void SetDirections(Direction dir) {
			topR.Visibility = !dir.up ? Visibility.Visible : Visibility.Collapsed;
			bottomR.Visibility = !dir.down ? Visibility.Visible : Visibility.Collapsed;
			leftR.Visibility = !dir.left ? Visibility.Visible : Visibility.Collapsed;
			rightR.Visibility = !dir.right ? Visibility.Visible : Visibility.Collapsed;
			//node.d.up = dir.up;
			//node.d.down = dir.down;
			//node.d.left = dir.left;
			//node.d.right = dir.right;
		}

	}
}
