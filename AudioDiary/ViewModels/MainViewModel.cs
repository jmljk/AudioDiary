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

        
        private string GenerateTags(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "#запис";

            
            string cleanText = text.Replace(" [Слухаю мікрофон...]", "")
                                   .Replace(" [Запис зупинено]", "")
                                   .Replace("[Запис пішов... мікрофон активний]", "");

            
            var words = cleanText.Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            
            var stopWords = new[] { "це", "тут", "там", "вже", "все", "під", "для", "про", "але", "або", "ніж", "мене", "мені", "тебе" };

           
            var tags = words.Select(w => w.Trim().ToLower())
                            .Where(w => w.Length >= 4 && !stopWords.Contains(w))
                            .Select(w => "#" + w)
                            .Distinct()
                            .Take(3)
                            .ToList();

            
            if (tags.Count == 0)
            {
                tags = words.Select(w => w.Trim().ToLower())
                            .Where(w => w.Length >= 3 && !stopWords.Contains(w))
                            .Select(w => "#" + w)
                            .Distinct()
                            .Take(2)
                            .ToList();
            }

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