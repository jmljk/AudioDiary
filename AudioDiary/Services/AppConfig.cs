using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDiary.Services
{
    public static class AppConfig
    {
        // За замовчуванням мікрофон 0 (системний) і чутливість 1.0 (стандартна)
        public static int SelectedMicrophoneIndex { get; set; } = 0;
        public static float MicrophoneSensitivity { get; set; } = 1.0f;
    }
}
