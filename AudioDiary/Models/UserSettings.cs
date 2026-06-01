namespace AudioDiary.Models
{
    public class UserSettings
    {
        // Чутливість розпізнавання (від 0.1 до 1.0). 
        // Чим вище, тим чіткіше треба говорити, але менше "хибних" слів.
        public float MicrophoneSensitivity { get; set; } = 0.5f;
        public string SelectedMicrophone { get; set; } = "За умовчанням";

        public bool IsDarkModeEnabled { get; set; } = false;

    }
}