// Views/MainWindow.xaml.cs
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SupportManager.Services;
using SupportManager.ViewModels;

namespace SupportManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new SupportRecordService());
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedRecord != null)
            {
                string pdfPath = vm.SelectedRecord.PdfPath;
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Для данной записи не указан PDF-файл.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }

    

}
