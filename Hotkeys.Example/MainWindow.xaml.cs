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
using Hotkeys;

namespace Hotkeys.Example
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly IHotkeyService hotkeyService;

		public MainWindow(IHotkeyService hotkeyService)
		{
			this.hotkeyService = hotkeyService;
			InitializeComponent();

			AddHotkeyButton.Click += AddHotkeyButton_Click;
			HotkeysListBox.ItemsSource = hotkeyService.Hotkeys;
			HotkeysListBox.DisplayMemberPath = "Name";
			HotkeysListBox.MouseDoubleClick += HotkeysListBox_MouseDoubleClick;
		}

		private void HotkeysListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (HotkeysListBox.SelectedItem is Hotkey hotkey)
			{
				try
				{
					hotkeyService.Remove(hotkey.Key, hotkey.Modifiers, false);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
				finally
				{
					HotkeysListBox.Items.Refresh();
				}
			}
		}

		private void AddHotkeyButton_Click(object sender, RoutedEventArgs e)
		{
			AddHotkeyButton.IsEnabled = false;
			AddHotkeyButton.Content = "Listening. Press ESC to cancel";

			this.KeyDown += MainWindow_KeyDown;
			this.KeyUp += MainWindow_KeyUp;
		}

		private void MainWindow_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				e.Handled = true;

				EndIntercept();

				Hotkey? newHotkey = null;

				Key key = e.Key;

				if (key == Key.System)
				{
					key = e.SystemKey;
				}

				if (!key.IsModifier())
				{
					newHotkey = hotkeyService.Add(key, this, modifiers: Keyboard.Modifiers, action: hotkey => MessageBox.Show($"{hotkey.Name} was pressed!"));
				}
				else
				{
					ModifierKeys modifiers = Keyboard.Modifiers;

					/*if (key == Key.LeftCtrl || key == Key.RightCtrl)
					{
						modifiers = (modifiers & ~ModifierKeys.Control);
					}

					if (key == Key.LeftAlt || key == Key.RightAlt)
					{
						modifiers = (modifiers & ~ModifierKeys.Alt);
					}

					if (key == Key.LeftShift || key == Key.RightShift)
					{
						modifiers = (modifiers & ~ModifierKeys.Shift);
					}

					if (key == Key.LWin || key == Key.RWin)
					{
						modifiers = (modifiers & ~ModifierKeys.Windows);
					}*/

					newHotkey = hotkeyService.Add(key, this, modifiers: modifiers, action: hotkey => MessageBox.Show($"{hotkey.Name} was pressed!"));
				}

				HeardHotkeyLabel.Content = $"Added: {newHotkey.Name}";
				HotkeysListBox.Items.Refresh();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				e.Handled = true;

				Key key = e.Key;

				if (key == Key.Escape)
				{
					EndIntercept();
					return;
				}

				if (key == Key.System)
				{
					key = e.SystemKey;
				}

				if (!key.IsModifier())
				{
					EndIntercept();
					Hotkey newHotkey = hotkeyService.Add(key, this, modifiers: Keyboard.Modifiers, action: hotkey => MessageBox.Show($"{hotkey.Name} was pressed!"));
					HeardHotkeyLabel.Content = $"Added: {newHotkey.Name}";
					HotkeysListBox.Items.Refresh();
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void EndIntercept()
		{
			AddHotkeyButton.IsEnabled = true;
			AddHotkeyButton.Content = "Add Hotkey";
			this.KeyDown -= MainWindow_KeyDown;
			this.KeyUp -= MainWindow_KeyUp;
		}
	}
}
