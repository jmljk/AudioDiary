using System;
using System.Text.Json;
using System.Windows;
using NAudio.Wave;
using Vosk;

namespace AudioDiary.Services
{
    public class SpeechRecognitionService : ISpeechService
    {
        public event EventHandler<string> TextRecognized;

        private WaveInEvent _waveIn;
        private Model _model;
        private VoskRecognizer _recognizer;
        private bool _isRecording = false;

        public SpeechRecognitionService()
        {
            try
            {
                // Вимикаємо зайвий системний текст у консолі
                Vosk.Vosk.SetLogLevel(-1);

                // Шлях до вашої розпакованої папки з моделлю (перевірте, чи він правильний)
                _model = new Model(@"C:\VoskModel");

                // Налаштовуємо розпізнавач на 16 кГц
                _recognizer = new VoskRecognizer(_model, 16000.0f);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити модель Vosk. Перевірте шлях до папки.\n{ex.Message}",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartRecording()
        {
            if (_isRecording || _model == null) return;

            try
            {
                _waveIn = new WaveInEvent();

                _waveIn.DeviceNumber = 0;
                _waveIn.WaveFormat = new WaveFormat(16000, 1);

               
                _waveIn.DataAvailable += WaveInOnDataAvailable;

                _waveIn.StartRecording();
                _isRecording = true;

               
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TextRecognized?.Invoke(this, "\n [Слухаю мікрофон...] ");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка мікрофона:\n{ex.Message}");
            }
        }

        // Цей метод ловить звук з мікрофона і передає нейромережі
        private void WaveInOnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var jsonResult = _recognizer.Result();
                var text = ExtractTextFromJson(jsonResult);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TextRecognized?.Invoke(this, text + " ");
                    });
                }
            }
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _isRecording = false;

            // Отримуємо останнє слово перед зупинкою
            var jsonResult = _recognizer.FinalResult();
            var text = ExtractTextFromJson(jsonResult);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    TextRecognized?.Invoke(this, text + " ");
                }
                TextRecognized?.Invoke(this, "[Запис зупинено]\n");
            });
        }

        // Допоміжний метод для діставання тексту з JSON
        private string ExtractTextFromJson(string json)
        {
            try
            {
                using (var doc = JsonDocument.Parse(json))
                {
                    return doc.RootElement.GetProperty("text").GetString();
                }
            }
            catch { return string.Empty; }
        }
    }
}