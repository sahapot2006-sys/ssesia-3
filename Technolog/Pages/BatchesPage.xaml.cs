using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class BatchesPage : Page
    {
        private ApiService apiService = new ApiService();

        public BatchesPage()
        {
            InitializeComponent();
        }

        private async void BatchesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBatches();
        }

        private async System.Threading.Tasks.Task LoadBatches()
        {
            try
            {
                var batches = await apiService.GetBatches();
                dgBatches.ItemsSource = batches;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtBatchNumber.Text = "";
            txtOrderId.Text = "";
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBatchNumber.Text) || string.IsNullOrEmpty(txtOrderId.Text))
            {
                txtAddError.Text = "Заполните все поля";
                return;
            }

            try
            {
                var model = new BatchModel
                {
                    batch_number = txtBatchNumber.Text,
                    order_id = int.Parse(txtOrderId.Text),
                    user_id = ApiService.CurrentUser.user_id
                };

                await apiService.CreateBatch(model);
                gridAdd.Visibility = Visibility.Collapsed;
                await LoadBatches();
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

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadBatches();
        }
    }
}