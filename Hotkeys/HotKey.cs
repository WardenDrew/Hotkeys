using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Hotkeys
{
	/// <summary>
	/// Represents a Global Hotkey. Hotkey is active for the lifetime of the object and is removed when the object is disposed.
	/// </summary>
	public sealed class Hotkey : IDisposable
	{
		private const int WM_HOTKEY = 0x0312;

		private readonly IntPtr _handle;
		private readonly int _id;

		private bool _isKeyRegistered;
		private Dispatcher _currentDispatcher;

		private int InteropKey => KeyInterop.VirtualKeyFromKey(Key);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
		private static extern bool WinAPIRegisterHotkey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey")]
		private static extern bool WinAPIUnregisterHotkey(IntPtr hWnd, int id);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		private static extern IntPtr WinAPIGetForegroundWindow();


		/// <summary>
		/// Event that fires when the hotkey is pressed
		/// </summary>
		public event Action<Hotkey> HotkeyPressed;

		/// <summary>
		/// Key that is being watched
		/// </summary>
		public Key Key { get; private set; }

		/// <summary>
		/// Modifiers required for the hotkey to fire
		/// </summary>
		public ModifierKeys KeyModifier { get; private set; }

		/// <summary>
		/// Register a Hotkey
		/// </summary>
		/// <param name="modifiers">Modifier key bit flags</param>
		/// <param name="key">The Key</param>
		/// <param name="window">The window events are dispatched on</param>
		/// <param name="action">Action to take when hotkey occurs</param>
		/// <exception cref="RegisterHotkeyException">Occurs if the hotkey cannot be registered</exception>
		public Hotkey(ModifierKeys modifiers, Key key, Window window, Action<Hotkey> action = null)
			: this(modifiers, key, new WindowInteropHelper(window), action) { }

		/// <summary>
		/// Register a Hotkey
		/// </summary>
		/// <param name="modifiers">Modifier key bit flags</param>
		/// <param name="key">The Key</param>
		/// <param name="window">The window events are dispatched on</param>
		/// <param name="action">Action to take when hotkey occurs</param>
		/// <exception cref="RegisterHotkeyException">Occurs if the hotkey cannot be registered</exception>
		public Hotkey(ModifierKeys modifiers, Key key, WindowInteropHelper window, Action<Hotkey> action = null)
			: this(modifiers, key, window.Handle, action) { }

		/// <summary>
		/// Register a Hotkey
		/// </summary>
		/// <param name="modifiers">Modifier key bit flags</param>
		/// <param name="key">The Key</param>
		/// <param name="window">The window events are dispatched on</param>
		/// <param name="action">Action to take when hotkey occurs</param>
		/// <exception cref="RegisterHotkeyException">Occurs if the hotkey cannot be registered</exception>
		public Hotkey(ModifierKeys modifiers, Key key, IntPtr windowHandle, Action<Hotkey> action = null)
		{
			Key = key;
			KeyModifier = modifiers;
			_id = GetHashCode();
			_handle = windowHandle == IntPtr.Zero ? WinAPIGetForegroundWindow() : windowHandle;
			_currentDispatcher = Dispatcher.CurrentDispatcher;
			RegisterHotkey();
			ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;

			if (action != null)
				HotkeyPressed += action;
		}

		~Hotkey()
		{
			Dispose();
		}

		/// <inheritdoc/>
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
				throw new RegisterHotkeyException("Failed to register the hotkey, it may already be in use!");
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
