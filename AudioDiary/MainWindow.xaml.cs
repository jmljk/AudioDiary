using System.Windows;
using AudioDiary.Models;

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