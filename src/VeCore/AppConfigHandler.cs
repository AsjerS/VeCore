using System.Reflection;
using Tomlyn;

namespace VeCore;

public abstract class AppConfigHandler<TAppConfig, TAppInfo>
    where TAppConfig : AppConfigHandler<TAppConfig, TAppInfo>, new()
    where TAppInfo : IAppInfo
{
    public static TAppConfig? Load(string filePath)
    {
        try
        {
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                string tomlText = File.ReadAllText(filePath);
                var config = TomlSerializer.Deserialize<TAppConfig>(tomlText) ?? new TAppConfig();

                if (!HasAllKeys(tomlText))
                {
                    config.Save(filePath);
                }

                return config;
            }

            return null;
        }
        catch (Exception ex)
        {
            if (AppDialogs<TAppInfo>.Confirm(
                $"Failed to read configuration:\n\n{ex.Message}\n\nReset to defaults?",
                MessageBoxIcon.Warning))
            {
                return new TAppConfig().Save(filePath);
            }

            return null;
        }
    }

    public TAppConfig Save(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        string serialized = TomlSerializer.Serialize((TAppConfig)this)
            .Replace("\n[", "\n\n[").Trim()
            + Environment.NewLine;

        File.WriteAllText(filePath, serialized);
        return (TAppConfig)this;
    }

    private static bool HasAllKeys(string tomlText) =>
        typeof(TAppConfig)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(section =>
                section.PropertyType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(prop => prop.Name)
                    .Prepend($"[{section.Name}]")
            )
            .All(token => tomlText.Contains(token, StringComparison.OrdinalIgnoreCase));
}
