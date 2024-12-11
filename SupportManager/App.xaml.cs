// App.xaml.cs
using System.Windows;
using SupportManager.Services;

namespace SupportManager
{
    public partial class App : Application
    {
        private ISupportRecordService _service;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _service = new SupportRecordService();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _service.SaveChanges();
        }
    }
}
