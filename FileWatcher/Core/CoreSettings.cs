using System.Configuration;
using EvilBaschdi.Core.Application;

namespace FileWatcher.Core
{
    /// <summary>
    ///     Retrieves App Settings from AppConfig.
    /// </summary>
    public class CoreSettings : ISettings
    {
        /// <summary>
        ///     MahApps ThemeManager Accent.
        /// </summary>
        public string Accent
        {
            get
            {
                var accent = GetSetting("Accent");
                return string.IsNullOrWhiteSpace(accent)
                    ? "Cyan"
                    : accent;
            }
            set { SetSetting("Accent", value); }
        }

        /// <summary>
        ///     MahApps ThemeManager Theme.
        /// </summary>
        public string Theme
        {
            get
            {
                var theme = GetSetting("Theme");
                return string.IsNullOrWhiteSpace(theme)
                    ? "BaseLight"
                    : theme;
            }
            set { SetSetting("Theme", value); }
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static void SetSetting(string key, string value)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}