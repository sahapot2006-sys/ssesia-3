using System.Windows;
using OperatorModule.Services;
using OperatorModule.Views;

namespace OperatorModule
{
    public partial class MainWindow : Window
    {
        public static ApiClient? Api { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            Api = new ApiClient("https://localhost:7000");

            if (StatusText != null)
                StatusText.Text = "Готов к работе";

            if (MainFrame != null)
                MainFrame.Navigate(new ActiveLotsPage());
        }

        public static void SetStatus(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow window && window.StatusText != null)
                {
                    window.StatusText.Text = message;
                }
            });
        }
    }
}