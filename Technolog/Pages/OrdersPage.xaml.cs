using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class OrdersPage : Page
    {
        private ApiService apiService = new ApiService();

        public OrdersPage()
        {
            InitializeComponent();
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private async System.Threading.Tasks.Task LoadOrders()
        {
            try
            {
                var orders = await apiService.GetOrders();
                dgOrders.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtOrderNumber.Text = "";
            txtRecipeId.Text = "";
            txtQuantity.Text = "1000";
            dpStartDate.SelectedDate = DateTime.Now.AddDays(7);
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtOrderNumber.Text) || string.IsNullOrEmpty(txtRecipeId.Text))
            {
                txtAddError.Text = "Заполните обязательные поля";
                return;
            }

            try
            {
                var model = new OrderModel
                {
                    order_number = txtOrderNumber.Text,
                    recipe_id = int.Parse(txtRecipeId.Text),
                    planned_quantity = decimal.Parse(txtQuantity.Text),
                    planned_start_date = dpStartDate.SelectedDate ?? DateTime.Now
                };

                await apiService.CreateOrder(model);
                gridAdd.Visibility = Visibility.Collapsed;
                await LoadOrders();
            }
            catch (Exception ex)
            {
                txtAddError.Text = ex.Message;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridAdd.Visibility = Visibility.Collapsed;
        }
    }
}