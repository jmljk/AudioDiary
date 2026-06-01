using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AudioDiary.Models;
using AudioDiary.Services;
using AudioDiary.Commands;

namespace AudioDiary.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private SpeechRecognitionService _speechService;
        private FileService _fileService;
        private UserAccount _currentUser;

        public ObservableCollection<DiaryEntry> Entries { get; set; }

        private DiaryEntry _selectedEntry;
        public DiaryEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged(nameof(SelectedEntry));

                if (_selectedEntry != null)
                {
                    CurrentText = _selectedEntry.TextContent;
                    CurrentTags = _selectedEntry.Tags; 
                }
            }
        }

        private string _currentText;
        public string CurrentText
        {
            get => _currentText;
            set
            {
                _currentText = value;
                OnPropertyChanged(nameof(CurrentText));
            }
        }

        // Змінна для відображення позначок на екрані
        private string _currentTags;
        public string CurrentTags
        {
            get => _currentTags;
            set
            {
                _currentTags = value;
                OnPropertyChanged(nameof(CurrentTags));
            }
        }

        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand SaveEntryCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand CreateNewCommand { get; }
        public ICommand DeleteEntryCommand { get; }

        public MainViewModel(UserAccount currentUser)
        {
            _currentUser = currentUser;
            _speechService = new SpeechRecognitionService();
            _fileService = new FileService();

            Entries = new ObservableCollection<DiaryEntry>(_fileService.LoadEntries());

            _speechService.TextRecognized += (s, text) =>
            {
                CurrentText += text + " ";
            };

            StartRecordingCommand = new RelayCommand(_ => _speechService.StartRecording());
            StopRecordingCommand = new RelayCommand(_ => _speechService.StopRecording());
            SaveEntryCommand = new RelayCommand(_ => SaveEntry());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            CreateNewCommand = new RelayCommand(_ =>
            {
                SelectedEntry = null;
                CurrentText = string.Empty;
                CurrentTags = string.Empty;
            });

            DeleteEntryCommand = new RelayCommand(_ => DeleteEntry());
        }

        // АВТОМАТИЧНА ГЕНЕРАЦІЯ ПОЗНАЧОК
        private string GenerateTags(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "#запис";

            // Очищаємо текст від службових повідомлень програми, які виводяться на екран
            string cleanText = text.Replace(" [Слухаю мікрофон...]", "")
                                   .Replace(" [Запис зупинено]", "")
                                   .Replace("[Запис пішов... мікрофон активний]", "");

            // Розбиваємо текст на окремі слова
            var words = cleanText.Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Список службових слів (стоп-слів), які часто зустрічаються, але не мають ставати тегами
            var stopWords = new[] { "це", "тут", "там", "вже", "все", "під", "для", "про", "але", "або", "ніж", "мене", "мені", "тебе" };

            // Знижуємо поріг довжини до 4 літер і прибираємо сполучники/прийменники
            var tags = words.Select(w => w.Trim().ToLower())
                            .Where(w => w.Length >= 4 && !stopWords.Contains(w))
                            .Select(w => "#" + w)
                            .Distinct()
                            .Take(3)
                            .ToList();

            // ПЛАН Б: Якщо модель розпізнала лише зовсім короткі слова
            if (tags.Count == 0)
            {
                tags = words.Select(w => w.Trim().ToLower())
                            .Where(w => w.Length >= 3 && !stopWords.Contains(w))
                            .Select(w => "#" + w)
                            .Distinct()
                            .Take(2)
                            .ToList();
            }

            // Якщо текст взагалі не містить придатних слів
            if (tags.Count == 0)
            {
                return "#щоденник #нотатка";
            }

            return string.Join(" ", tags);
        }
        private void SaveEntry()
        {
            if (string.IsNullOrWhiteSpace(CurrentText))
            {
                MessageBox.Show("Не можна зберегти порожній запис.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Генеруємо позначки перед збереженням
            CurrentTags = GenerateTags(CurrentText);

            if (SelectedEntry != null)
            {
                SelectedEntry.TextContent = CurrentText;
                SelectedEntry.Tags = CurrentTags;
                MessageBox.Show("Запис успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Entries.Add(new DiaryEntry { TextContent = CurrentText, Tags = CurrentTags });
                MessageBox.Show("Новий запис збережено з автоматичними позначками!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _fileService.SaveEntries(new System.Collections.Generic.List<DiaryEntry>(Entries));

            SelectedEntry = null;
            CurrentText = string.Empty;
            CurrentTags = string.Empty;
        }

        private void DeleteEntry()
        {
            if (SelectedEntry == null) return;

            var result = MessageBox.Show("Ви впевнені, що хочете видалити цей запис?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Entries.Remove(SelectedEntry);
                _fileService.SaveEntries(new System.Collections.Generic.List<DiaryEntry>(Entries));
                SelectedEntry = null;
                CurrentText = string.Empty;
                CurrentTags = string.Empty;
            }
        }

        private void OpenSettings()
        {
            var settingsWindow = new AudioDiary.Views.SettingsWindow(_currentUser);
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}