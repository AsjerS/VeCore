namespace VeCore;

public static class AppDialogs<TAppInfo>
    where TAppInfo : IAppInfo
{
    public static void Info(string message) =>
        MessageBox.Show(message, TAppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Warning(string message) =>
        MessageBox.Show(message, TAppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void Error(string message) =>
        MessageBox.Show(message, TAppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void Error(Exception ex) =>
        Error(ex.Message);

    public static bool Confirm(string message, MessageBoxIcon icon = MessageBoxIcon.Question) =>
        MessageBox.Show(message, TAppInfo.Name, MessageBoxButtons.YesNo, icon) == DialogResult.Yes;
}
