using System.Windows;
using AudioDiary.Models;
using AudioDiary.ViewModels;

namespace AudioDiary.Views
{
    public partial class SettingsWindow : Window
    {

        public SettingsWindow(UserAccount currentUser)
        {
            InitializeComponent();

            // Підключаємо логіку налаштувань і передаємо користувача туди
            this.DataContext = new SettingsViewModel(currentUser);
        }
    }
}