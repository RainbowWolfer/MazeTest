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
		public SingleGrid(Node node, string str) {
			this.node = node;
			this.InitializeComponent();
			MyText.Visibility = Visibility.Visible;
			MyText.Text = str;
		}
		public void Initialize() {
			if(node == null) {
				throw new Exception("???");
			}

			bool b = Instance.row * Instance.col > 400;
			MyText.Visibility = b ? Visibility.Collapsed : Visibility.Visible;

			MyText.Text = node.pos.ToString();
			if(Instance.unvisited.Contains(node)) {
				MyText.Text += "\nUNVISITED";
			}
			SetDirections(node.d);
			if(node.previous != null) {
				MyText.Text += "\nPre " + node.previous.pos;
			}
			if(node.onWay) {
				centerR.Fill = new SolidColorBrush(Colors.Red);
			} else {
				centerR.Fill = new SolidColorBrush(Colors.Transparent);
			}
		}
		public void AdjustBorder(int w) {
			topR.Height = w;
			bottomR.Height = w;
			leftR.Width = w;
			rightR.Width = w;
		}
		public void SetFill(Color color) {
			centerR.Fill = new SolidColorBrush(color);
		}
		public void SetDirections(Direction dir) {
			topR.Visibility = !dir.up ? Visibility.Visible : Visibility.Collapsed;
			bottomR.Visibility = !dir.down ? Visibility.Visible : Visibility.Collapsed;
			leftR.Visibility = !dir.left ? Visibility.Visible : Visibility.Collapsed;
			rightR.Visibility = !dir.right ? Visibility.Visible : Visibility.Collapsed;
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			//Instance.CreatePath(this.node);
			if(Instance.finishGenerating) {
				//Instance.PrintPath(node);
			}
		}

	}
}
