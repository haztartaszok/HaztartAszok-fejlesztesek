namespace WinFormsApp1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                AppSettings.LoadFromFile(AppContext.BaseDirectory);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    null,
                    "Beállítási hiba",
                    $"A settings.json betöltése nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                return;
            }

            Application.Run(new Form1());
        }
    }
}
