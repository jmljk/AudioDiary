using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AudioDiary.Models;
using AudioDiary.Services;
using AudioDiary.Commands;
using AudioDiary.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioDiary.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private SpeechRecognitionService _speechService;
        private FileService _fileService;
        private UserAccount _currentUser;
        public event PropertyChangedEventHandler PropertyChanged;

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
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

            Entries = new ObservableCollection<DiaryEntry>(_fileService.LoadEntries(_currentUser.Username));

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


        public string GenerateTags(string recognizedText)
        {
            if (string.IsNullOrWhiteSpace(recognizedText)) return "";

            // словник українських стоп-слів 
            var stopWords = new HashSet<string> {
        "його", "було", "буде", "коли", "дуже", "також", "який", "яка", "яке",
        "тільки", "навіть", "просто", "після", "перед", "через", "тому", "якщо",
        "щоб", "мене", "мені", "тебе", "тобі", "собі", "бути", "вони", "вона"
    };

            // очищаємо від пунктуації та розбиваємо на слова
            var punctuation = recognizedText.Where(char.IsPunctuation).Distinct().ToArray();
            var allWords = recognizedText.ToLower()
                .Split(new[] { ' ', '\n', '\r', '\t' }.Concat(punctuation).ToArray(), StringSplitOptions.RemoveEmptyEntries);

            // слова від 4 символів, яких немає у стоп-листі
            var validWords = allWords.Where(w => w.Length >= 4 && !stopWords.Contains(w)).ToList();

            // розумних слів назбиралося менше 3, знижуємо поріг до 3 символів
            if (validWords.Count < 3)
            {
                validWords = allWords.Where(w => w.Length >= 3 && !stopWords.Contains(w)).ToList();
            }

            // слова, які повторюються найчастіше
            var topTags = validWords
                .GroupBy(w => w)                            // групуємо однакові слова
                .Select(g => new { Word = g.Key, Count = g.Count() }) // рахуємо, скільки разів кожне зустрічається
                .OrderByDescending(x => x.Count)            // спочатку найчастіші
                .ThenByDescending(x => x.Word.Length)       // частота однакова = пріоритет довшим словам
                .Take(3)                                    // топ-3
                .Select(x => "#" + x.Word)                  // хештег
                .ToList();

            return string.Join(" ", topTags);
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

            _fileService.SaveEntries(Entries, _currentUser.Username);

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
                _fileService.SaveEntries(Entries, _currentUser.Username);
                SelectedEntry = null;
                CurrentText = string.Empty;
                CurrentTags = string.Empty;
            }
        }

        private void OpenSettings()
        {
           
            var settingsWindow = new SettingsWindow(_currentUser);

           
            Window mainWin = System.Windows.Application.Current.MainWindow;

            
            if (mainWin != null && mainWin != settingsWindow)
            {
                settingsWindow.Owner = mainWin;
            }

            
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settingsWindow.ShowDialog();
        }
    }
}