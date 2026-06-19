using System;
using System.IO;
using System.Text.Json; 
using NAudio.Wave;
using Vosk;

namespace AudioDiary.Services
{
    public class SpeechRecognitionService
    {
        
        public event EventHandler<string> TextRecognized;

        private Model _model;
        private VoskRecognizer _recognizer;
        private WaveInEvent _waveIn;

        public SpeechRecognitionService()
        {
            Vosk.Vosk.SetLogLevel(-1);
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VoskModel");

            
            if (!Directory.Exists(modelPath))
            {
                System.Windows.MessageBox.Show($"КРИТИЧНА ПОМИЛКА: Не знайдено папку з моделлю Vosk за шляхом:\n{modelPath}\n\nБудь ласка, переконайся, що папка VoskModel скопійована в папку з програмою.", "Помилка моделі");
                return; 
            }

            _model = new Model(modelPath);
            _recognizer = new VoskRecognizer(_model, 16000.0f);
        }

        public void StartRecording()
        {
            _waveIn = new WaveInEvent();
            _waveIn.DeviceNumber = AppConfig.SelectedMicrophoneIndex;
            _waveIn.WaveFormat = new WaveFormat(16000, 1);
            _waveIn.DataAvailable += WaveIn_DataAvailable;
            _waveIn.StartRecording();
        }

        public void StopRecording()
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            float sensitivity = AppConfig.MicrophoneSensitivity;

            if (sensitivity != 1.0f)
            {
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                    float amplified = sample * sensitivity;

                    if (amplified > short.MaxValue) amplified = short.MaxValue;
                    if (amplified < short.MinValue) amplified = short.MinValue;

                    sample = (short)amplified;
                    e.Buffer[i] = (byte)(sample & 0x00FF);
                    e.Buffer[i + 1] = (byte)(sample >> 8);
                }
            }

         
            if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
               
                string jsonResult = _recognizer.Result();

               
                using (var doc = JsonDocument.Parse(jsonResult))
                {
                    string recognizedText = doc.RootElement.GetProperty("text").GetString();

                 
                    if (!string.IsNullOrWhiteSpace(recognizedText))
                    {
                        
                        TextRecognized?.Invoke(this, recognizedText);
                    }
                }
            }
        }
    }
}