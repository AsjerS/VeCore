using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace VeCore;

public sealed record AppHotkey(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    Keys Key = Keys.PrintScreen,
    bool Ctrl = true,
    bool Alt = true,
    bool Shift = false,
    bool Win = false
)
{
    public override string ToString()
    {
        var parts = new List<string>(5);
        if (Ctrl) parts.Add("Ctrl");
        if (Shift) parts.Add("Shift");
        if (Alt) parts.Add("Alt");
        if (Win) parts.Add("Win");
        parts.Add(Key.ToString());
        return string.Join("+", parts);
    }
}

public sealed partial class HotkeyListener : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 1;

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Action _callback;

    public HotkeyListener(AppHotkey hotkey, Action callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        CreateHandle(new CreateParams());

        uint fsModifiers = MOD_NOREPEAT;
        if (hotkey.Alt) fsModifiers |= MOD_ALT;
        if (hotkey.Ctrl) fsModifiers |= MOD_CONTROL;
        if (hotkey.Shift) fsModifiers |= MOD_SHIFT;
        if (hotkey.Win) fsModifiers |= MOD_WIN;

        if (!RegisterHotKey(Handle, HOTKEY_ID, fsModifiers, (uint)hotkey.Key))
        {
            DestroyHandle();
            throw new InvalidOperationException(
                $"Could not register hotkey '{hotkey}'. It may already be in use by another application."
            );
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            _callback.Invoke();
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            UnregisterHotKey(Handle, HOTKEY_ID);
            DestroyHandle();
        }
    }
}
