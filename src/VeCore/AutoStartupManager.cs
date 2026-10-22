using Microsoft.Win32;

namespace VeCore;

public static class AutoStartupManager<TAppInfo>
    where TAppInfo : IAppInfo
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(TAppInfo.Name) is not null;
    }

    public static void SetEnabled(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        
        if (enable)
            key?.SetValue(TAppInfo.Name, $"\"{Environment.ProcessPath}\"");
        else
            key?.DeleteValue(TAppInfo.Name, throwOnMissingValue: false);
    }
}
