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

        public List<DiaryEntry> LoadEntries()
        {
            if (!File.Exists(_filePath)) return new List<DiaryEntry>();
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json) ?? new List<DiaryEntry>();
        }
    }
}