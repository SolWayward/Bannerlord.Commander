using System;
using Bannerlord.Commander.Settings;

namespace Bannerlord.Commander.Settings
{
    public class SettingsManager
    {
        private static readonly Lazy<SettingsManager> _instance = new(() => new());
        private HeroSettings _heroSettings = new();

        public static SettingsManager Instance => _instance.Value;
        public static HeroSettings HeroSettings => Instance._heroSettings;

        private SettingsManager()
        {
            _heroSettings.ShowHiddenInfo = true;
        }
    }
}