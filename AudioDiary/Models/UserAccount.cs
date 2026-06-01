namespace AudioDiary.Models
{
    public class UserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; } 
        public UserSettings Settings { get; set; } = new UserSettings();
    }
}