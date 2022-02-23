# Hotkeys
A small .NET Library for Global Hotkey binding. Updated to .NET 6.0

# Usage

Example _(C#, in a WPF Application)_:
```cs
using mrousavy;
// ...
var key = new HotKey(
    (ModifierKeys.Control | ModifierKeys.Alt), 
    Key.S, 
    this, 
    (hotkey) => {
        MessageBox.Show("Ctrl + Alt + S was pressed!");
    }
);
// ...
key.Dispose();
```

> Note #1: Since `HotKey` implements the [`IDisposable` interface](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netcore-3.1), you must call `Dispose()` to unregister the Hotkey, e.g. when your Window closes. Keep in mind that when you're using `using(...) { ... }`, the HotKey gets unregistered and disposed once you exit the using-statement's scope.

> Note #2: Use the binary **or** operator (`|`) to combine key combinations for the constructor.

> Note #3: Use a Window Reference as the third Argument. This may be a [WPF `Window`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.window?view=netcore-3.1), a [`WindowInteropHelper`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.windowinterophelper?view=netcore-3.1), or a Window Handle ([`IntPtr`](https://stackoverflow.com/questions/1953582/how-to-i-get-the-window-handle-by-giving-the-process-name-that-is-running)). So you can use your own WPF/WinForms Window, as well as another process's Window using it's Handle. Keep in mind that this is sketchy behaviour and not the intention of this library.
