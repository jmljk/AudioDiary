using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AudioDiary.Models; 

namespace AudioDiary.Services
{
    public class FileService
    {
      
        private readonly string _appDataPath;

        public FileService()
        {
            
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            
            _appDataPath = Path.Combine(roamingPath, "AudioDiary");

           
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }
        }

     

        public List<DiaryEntry> LoadEntries(string username)
        {
            
            string filePath = Path.Combine(_appDataPath, $"{username}_entries.json");

            if (!File.Exists(filePath)) return new List<DiaryEntry>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
        }

        public void SaveEntries(IEnumerable<DiaryEntry> entries, string username)
        {
            string filePath = Path.Combine(_appDataPath, $"{username}_entries.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(entries, options);

            File.WriteAllText(filePath, json); 
        }

      

        public List<UserAccount> GetUsers()
        {
            string filePath = Path.Combine(_appDataPath, "users.json");

            if (!File.Exists(filePath)) return new List<UserAccount>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new List<UserAccount>();
        }

        public void SaveUsers(IEnumerable<UserAccount> users)
        {
            string filePath = Path.Combine(_appDataPath, "users.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(users, options);

            File.WriteAllText(filePath, json);
        }
    }
}