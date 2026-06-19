using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AudioDiary.Models;

namespace AudioDiary.Services
{
    public class FileService
    {
        private readonly string _filePath = "diary_data.json";

        public void SaveEntries(List<DiaryEntry> entries)
        {
            string json = JsonSerializer.Serialize(entries);
            File.WriteAllText(_filePath, json);
        }

        public List<DiaryEntry> LoadEntries(string username)
        {
           
            string filePath = $"{username}_entries.json";

            if (!File.Exists(filePath))
            {
                return new List<DiaryEntry>(); 
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
        }

        
        public void SaveEntries(IEnumerable<DiaryEntry> entries, string username)
        {
            string filePath = $"{username}_entries.json";

            
            var options = new JsonSerializerOptions { WriteIndented = true };

            string json = JsonSerializer.Serialize(entries, options);
            File.WriteAllText(filePath, json);
        }
    }
}