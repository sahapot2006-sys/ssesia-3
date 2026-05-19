using System.Windows;
using LaboratoryModule.Services;
using LaboratoryModule.Views;

namespace LaboratoryModule
{
    public partial class MainWindow : Window
    {
        public static ApiClient? Api { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация API
            Api = new ApiClient("http://localhost:63519");

            // Проверяем, что StatusText существует
            if (StatusText != null)
            {
                StatusText.Text = "Готов к работе";
            }

            // Переход на страницу списка партий
            if (MainFrame != null)
            {
                MainFrame.Navigate(new LotsListPage());
            }
        }

        public static void SetStatus(string message)
        {
            // Безопасная установка статуса
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow window)
                {
                    if (window.StatusText != null)
                    {
                        window.StatusText.Text = message;
                    }
                }
            });
        }
    }
}