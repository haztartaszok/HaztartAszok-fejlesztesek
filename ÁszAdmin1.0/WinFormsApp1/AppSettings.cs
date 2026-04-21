using System.Text.Json;

namespace WinFormsApp1
{
    internal sealed class AppSettings
    {
        public HotcakesSettings Hotcakes { get; init; } = new();

        public static AppSettings Current { get; private set; } = new();

        public static void LoadFromFile(string baseDirectory)
        {
            string settingsPath = Path.Combine(baseDirectory, "settings.json");

            if (!File.Exists(settingsPath))
            {
                throw new FileNotFoundException("A settings.json fajl nem talalhato.", settingsPath);
            }

            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(
                File.ReadAllText(settingsPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });

            if (settings is null)
            {
                throw new InvalidOperationException("A settings.json fajl nem olvashato be.");
            }

            settings.Validate();
            Current = settings;
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Hotcakes.BaseUrl))
            {
                throw new InvalidOperationException("A Hotcakes.BaseUrl nincs kitoltve a settings.json fajlban.");
            }

            if (string.IsNullOrWhiteSpace(Hotcakes.ApiKey))
            {
                throw new InvalidOperationException("A Hotcakes.ApiKey nincs kitoltve a settings.json fajlban.");
            }
        }
    }

    internal sealed class HotcakesSettings
    {
        public string BaseUrl { get; init; } = string.Empty;

        public string ApiKey { get; init; } = string.Empty;
    }
}
