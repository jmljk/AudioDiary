using System.Windows;
using NAudio.Wave;
using AudioDiary.Services;

namespace AudioDiary.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadMicrophones();
        }

        public SettingsWindow(object viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            LoadMicrophones();
        }

        private void LoadMicrophones()
        {
            int waveInDevices = WaveIn.DeviceCount;
            for (int i = 0; i < waveInDevices; i++)
            {
                CmbMicrophones.Items.Add(WaveIn.GetCapabilities(i).ProductName);
            }

          
            if (CmbMicrophones.Items.Count > 0)
            {
                CmbMicrophones.SelectedIndex = AppConfig.SelectedMicrophoneIndex;
            }

            SliderSensitivity.Value = AppConfig.MicrophoneSensitivity;
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMicrophones.SelectedIndex >= 0)
            {
                AppConfig.SelectedMicrophoneIndex = CmbMicrophones.SelectedIndex;
            }

            AppConfig.MicrophoneSensitivity = (float)SliderSensitivity.Value;

            MessageBox.Show("Налаштування успішно збережено!");
            this.Close();
        }
    }
}