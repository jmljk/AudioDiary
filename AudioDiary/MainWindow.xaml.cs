using System.Windows;
using AudioDiary.Models; // Обов'язково додаємо посилання на моделі

namespace AudioDiary.Views 
{
    public partial class MainWindow : Window
    {

        public MainWindow(UserAccount user)
        {
            InitializeComponent();

           
            this.DataContext = new ViewModels.MainViewModel(user);
        }
    }
}