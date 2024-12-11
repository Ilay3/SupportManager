// Views/MainWindow.xaml.cs
using System.Windows;
using SupportManager.Services;
using SupportManager.ViewModels;

namespace SupportManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Внедрение зависимости
            var service = new SupportRecordService();
            DataContext = new MainViewModel(service);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Support Manager v1.0\n.", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
