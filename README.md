# Hotkeys
A small .NET Library for Global Hotkey binding. Updated to .NET 6.0

# Usage

See the `Hotkeys.Example` project for a more complicated example demonstrating the `HotkeyService` and how to handle some edge cases that come up when implementing a KeyDown listener to let a user choose their own hotkeys.

Simple Example
```cs
using Hotkeys;
using System.Windows;
using System.Windows.Input;

Hotkey key = new(
    Key.S,
    this,
    (ModifierKeys.Control | ModifierKeys.Alt),
    "Heavy Duty Save Hotkey",
    hotkey => MessageBox.Show($"{hotkey.Name} was pressed!"));

key.Dispose();
```
