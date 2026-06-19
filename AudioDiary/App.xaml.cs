using System.Configuration;
using System.Data;
using System.Windows;

namespace AudioDiary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Перехоплюємо всі помилки, які ми забули обробити через try-catch
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Критична помилка: {ex.ExceptionObject.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }

}
    