using System.Text;

namespace WinFormsApp1.Tests;

[TestClass]
public sealed class AppSettingsTests
{
    [TestMethod]
    public void LoadFromFile_LoadsSettingsAndSkipsJsonComments()
    {
        // Azt teszteli, hogy a beallitasok kommentes JSON-bol is beolvashatok.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string settingsPath = Path.Combine(tempDirectory, "settings.json");
            File.WriteAllText(
                settingsPath,
                """
                {
                  // test comment
                  "Hotcakes": {
                    "BaseUrl": "https://example.test",
                    "ApiKey": "sample-key"
                  }
                }
                """,
                Encoding.UTF8);

            AppSettings.LoadFromFile(tempDirectory);

            Assert.AreEqual("https://example.test", AppSettings.Current.Hotcakes.BaseUrl);
            Assert.AreEqual("sample-key", AppSettings.Current.Hotcakes.ApiKey);
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void LoadFromFile_ThrowsWhenApiKeyIsMissing()
    {
        // Azt teszteli, hogy ures API kulcs eseten validacios hiba keletkezik.
        string tempDirectory = CreateTempDirectory();

        try
        {
            string settingsPath = Path.Combine(tempDirectory, "settings.json");
            File.WriteAllText(
                settingsPath,
                """
                {
                  "Hotcakes": {
                    "BaseUrl": "https://example.test",
                    "ApiKey": ""
                  }
                }
                """,
                Encoding.UTF8);

            InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
                () => AppSettings.LoadFromFile(tempDirectory));

            StringAssert.Contains(exception.Message, "Hotcakes.ApiKey");
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public void LoadFromFile_ThrowsWhenSettingsFileDoesNotExist()
    {
        // Azt teszteli, hogy hianyzo settings.json fajlra FileNotFoundException jon.
        string tempDirectory = CreateTempDirectory();

        try
        {
            FileNotFoundException exception = Assert.ThrowsExactly<FileNotFoundException>(
                () => AppSettings.LoadFromFile(tempDirectory));

            StringAssert.Contains(exception.FileName ?? string.Empty, "settings.json");
        }
        finally
        {
            DeleteTempDirectory(tempDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"WinFormsApp1.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
