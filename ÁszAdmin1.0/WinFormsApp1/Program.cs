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
                MessageBox.Show(
                    $"A settings.json betoltese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Beallitas hiba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new Form1());
        }
    }
}
