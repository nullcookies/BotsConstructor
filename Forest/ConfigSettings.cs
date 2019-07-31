using System;
using System.Configuration;
using System.Linq;

namespace TelegramBot
{
    public static class ConfigSettings
    {
        public static string ReadSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static void WriteSetting(string key, string value)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                currentConfig.AppSettings.Settings.Add(key, value);
            }
            else
            {
                currentConfig.AppSettings.Settings[key].Value = value;
            }

            currentConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void RemoveSetting(string key)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            currentConfig.AppSettings.Settings.Remove(key);

            currentConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
