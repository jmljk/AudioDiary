using System;

namespace AudioDiary.Models
{
    public class DiaryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string TextContent { get; set; }

        
        public string Tags { get; set; }
    }
}