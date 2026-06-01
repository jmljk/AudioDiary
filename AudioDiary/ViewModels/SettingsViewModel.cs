using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AudioDiary.Commands;
using AudioDiary.Models;
using NAudio.Wave; // Підключення бібліотеки для мікрофонів

namespace AudioDiary.ViewModels
{
    public class SettingsViewModel
    {
        public ObservableCollection<string> AvailableMicrophones { get; set; }
        public string SelectedMicrophone { get; set; }
        public double Sensitivity { get; set; }

        private UserAccount _currentUser;
        public ICommand SaveCommand { get; }

        public SettingsViewModel(UserAccount currentUser)
        {
            _currentUser = currentUser;
            AvailableMicrophones = new ObservableCollection<string>();

            LoadMicrophones();

            // Завантаження поточних налаштувань
            SelectedMicrophone = _currentUser.Settings.SelectedMicrophone;
            Sensitivity = _currentUser.Settings.MicrophoneSensitivity;

            SaveCommand = new RelayCommand(SaveSettings);
        }

        private void LoadMicrophones()
        {
            // Отримуємо всі мікрофони системи, включаючи вебкамери
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                AvailableMicrophones.Add(deviceInfo.ProductName);
            }

            if (AvailableMicrophones.Count == 0)
                AvailableMicrophones.Add("Мікрофони не знайдено");
        }

        private void SaveSettings(object windowParameter)
        {
            _currentUser.Settings.SelectedMicrophone = SelectedMicrophone;
            _currentUser.Settings.MicrophoneSensitivity = (float)Sensitivity;

          

            MessageBox.Show("Налаштування збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            if (windowParameter is Window window) window.Close();
        }
    }
}