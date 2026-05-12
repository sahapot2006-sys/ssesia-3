using System;
using System.Windows.Controls;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class DashboardPage : Page
    {
        private ApiService apiService = new ApiService();

        public DashboardPage()
        {
            InitializeComponent();
        }

        private async void DashboardPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var products = await apiService.GetProducts();
                txtProductCount.Text = products.Count.ToString();

                var recipes = await apiService.GetRecipes();
                txtRecipeCount.Text = recipes.Count.ToString();

                var orders = await apiService.GetOrders();
                txtOrderCount.Text = orders.Count.ToString();

                var deviations = await apiService.GetDeviations();
                txtDeviationCount.Text = deviations.Count.ToString();

                var events = await apiService.GetEvents(20);
                dgEvents.ItemsSource = events;
            }
            catch (Exception)
            {
                dgEvents.ItemsSource = null;
            }
        }
    }
}