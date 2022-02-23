using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Hotkeys
{
	public sealed class Hotkey : IDisposable
	{
		public const int WM_HOTKEY = 0x0312;

		private readonly IntPtr _handle;
		private readonly int _id;

		private bool _isKeyRegistered;
		private Dispatcher _currentDispatcher;

		private int InteropKey => KeyInterop.VirtualKeyFromKey(Key);

		public event Action<Hotkey> HotkeyPressed;

		public Key Key { get; private set; }
		public ModifierKeys KeyModifier { get; private set; }

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
		public static extern bool WinAPIRegisterHotkey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey")]
		public static extern bool WinAPIUnregisterHotkey(IntPtr hWnd, int id);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		static extern IntPtr WinAPIGetForegroundWindow();

		public Hotkey(ModifierKeys modifierKeys, Key key, Window window)
			: this(modifierKeys, key, new WindowInteropHelper(window), null) { }

		public Hotkey(ModifierKeys modifierKeys, Key key, WindowInteropHelper window)
			: this(modifierKeys, key, window.Handle, null) { }

		public Hotkey(ModifierKeys modifierKeys, Key key, Window window, Action<Hotkey> onKeyAction)
			: this(modifierKeys, key, new WindowInteropHelper(window), onKeyAction) { }

		public Hotkey(ModifierKeys modifierKeys, Key key, WindowInteropHelper window, Action<Hotkey> onKeyAction)
			: this(modifierKeys, key, window.Handle, onKeyAction) { }

		public Hotkey(ModifierKeys modifierKeys, Key key, IntPtr windowHandle, Action<Hotkey> onKeyAction = null)
		{
			Key = key;
			KeyModifier = modifierKeys;
			_id = GetHashCode();
			_handle = windowHandle == IntPtr.Zero ? WinAPIGetForegroundWindow() : windowHandle;
			_currentDispatcher = Dispatcher.CurrentDispatcher;
			RegisterHotkey();
			ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;

			if (onKeyAction != null)
				HotkeyPressed += onKeyAction;
		}

		~Hotkey()
		{
			Dispose();
		}

		public void Dispose()
		{
			try
			{
				ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
			}
			finally
			{
				UnregisterHotkey();
			}
		}

		private void OnHotkeyPressed()
		{
			_currentDispatcher.Invoke(
				delegate
				{
					HotkeyPressed?.Invoke(this);
				});
		}

		private void RegisterHotkey()
		{
			if (Key == Key.None)
			{
				return;
			}

			if (_isKeyRegistered)
			{
				UnregisterHotkey();
			}

			_isKeyRegistered = WinAPIRegisterHotkey(_handle, _id, KeyModifier, InteropKey);

			if (!_isKeyRegistered)
			{
				throw new ApplicationException("An unexpected Error occured! (Hotkey may already be in use)");
			}
		}

		private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
		{
			if (handled)
			{
				return;
			}

			if (msg.message != WM_HOTKEY || (int)(msg.wParam) != _id)
			{
				return;
			}

			OnHotkeyPressed();
			handled = true;
		}

		private void UnregisterHotkey()
		{
			_isKeyRegistered = !WinAPIUnregisterHotkey(_handle, _id);
		}
	}
}
