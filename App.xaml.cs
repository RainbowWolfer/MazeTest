using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MazeTest {
	sealed partial class App: Application {
		public App() {
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e) {
			if(!(Window.Current.Content is Frame rootFrame)) {
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;
				if(e.PreviousExecutionState == ApplicationExecutionState.Terminated) {

				}
				Window.Current.Content = rootFrame;
			}
			if(e.PrelaunchActivated == false) {
				if(rootFrame.Content == null) {
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				Window.Current.Activate();
			}
			Window.Current.Content.KeyDown += (s, c) => {
				if(c.Key == VirtualKey.F) {
					MainPage.Instance.SwitchGrid();
				}
			};
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			deferral.Complete();
		}
		public static Color GetRandomColor() {
			return new Color() {
				A = 1,
				R = (byte)(new Random().Next(0, 255)),
				G = (byte)(new Random().Next(0, 255)),
				B = (byte)(new Random().Next(0, 255))
			};
		}
		public static bool GetRandomBoolean() {
			return new Random().Next(0, 100) >= 50;
		}
	}
}
