using System.Windows;
using Technolog.Models;
using Technolog.Pages;

namespace Technolog
{
    public partial class MainWindow : Window
    {
        private User currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            this.Title = $"АГРО - Технолог: {user.full_name}";
            MainFrame.Navigate(new DashboardPage());
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductsPage());
        }

        private void btnRecipes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RecipesPage());
        }

        private void btnProcessCards_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProcessCardsPage());
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void btnBatches_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BatchesPage());
        }

        private void btnExtruder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExtruderPage());
        }

        private void btnDeviations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DeviationsPage());
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportsPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из системы?", "Подтверждение",
                          MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }
    }
}