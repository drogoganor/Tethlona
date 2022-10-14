using Cosmopolis.Data;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Cosmopolis.Providers
{
    public interface ISettingsProvider
    {
        Settings Settings { get; }
    }

    public class SettingsProvider : ISettingsProvider
    {
        private readonly Settings settings;

        public Settings Settings => settings;

        public SettingsProvider()
        {
            var settingsPath = Path.Combine(Environment.CurrentDirectory, @"Content/settings.json");

            if (File.Exists(settingsPath))
            {
                using var fs = File.OpenRead(settingsPath);
                using var sr = new StreamReader(fs, Encoding.UTF8);
                string content = sr.ReadToEnd();

                settings = JsonSerializer.Deserialize<Settings>(content);
            }
            else
            {
                // TODO: Supply default
            }
        }

        public Settings GetSettings()
        {
            return settings;
        }
    }
}
