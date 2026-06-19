using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using AudioDiary.Commands;
using AudioDiary.Models;
using AudioDiary.Views;
using AudioDiary.Services;

namespace AudioDiary.ViewModels
{
    public class LoginViewModel
    {
        private readonly string _usersFile = "users.json";
        private List<UserAccount> _users;

        public string Username { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoadUsers();
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        private void LoadUsers()
        {
            if (File.Exists(_usersFile))
            {
                string json = File.ReadAllText(_usersFile);
                _users = JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new List<UserAccount>();
            }
            else
            {
                _users = new List<UserAccount>();
            }
        }

        private void SaveUsers()
        {
        
            var fileService = new AudioDiary.Services.FileService();
            fileService.SaveUsers(_users);
        }

        private void Login(object windowParameter)
        {
            
            string hashedInput = PasswordHelper.HashPassword(Password);

            
            var user = _users.FirstOrDefault(u => u.Username == Username && u.Password == hashedInput);
            if (user != null)
            {
                // Відкриваємо головне вікно, передаємо дані користувача
                var mainWindow = new MainWindow(user);
                mainWindow.Show();

                // Закриваємо вікно логіну
                if (windowParameter is Window window) window.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register(object obj)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Заповніть усі поля!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_users.Exists(u => u.Username == Username))
            {
                MessageBox.Show("Користувач з таким логіном вже існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _users.Add(new UserAccount { Username = Username, Password = PasswordHelper.HashPassword(Password) });
            SaveUsers();
            MessageBox.Show("Акаунт створено! Тепер ви можете увійти.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}