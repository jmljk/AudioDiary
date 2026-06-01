namespace AudioDiary.Services
{
    public interface ISpeechService
    {
        void StartRecording();
        void StopRecording();
        event EventHandler<string> TextRecognized;
    }
}