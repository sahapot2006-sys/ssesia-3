using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class DeviationsPage : Page
    {
        private ApiService apiService = new ApiService();

        public DeviationsPage()
        {
            InitializeComponent();
        }

        private async void DeviationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDeviations();
        }

        private async System.Threading.Tasks.Task LoadDeviations(int? batchId = null)
        {
            try
            {
                if (batchId.HasValue)
                {
                    var deviations = await apiService.GetDeviationsByBatch(batchId.Value);
                    dgDeviations.ItemsSource = deviations;
                    txtInfo.Text = "Показано " + deviations.Count + " отклонений для партии " + batchId;
                }
                else
                {
                    var deviations = await apiService.GetDeviations();
                    dgDeviations.ItemsSource = deviations;
                    txtInfo.Text = "Всего отклонений: " + deviations.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtBatchFilter.Text, out int batchId))
            {
                await LoadDeviations(batchId);
            }
            else
            {
                MessageBox.Show("Введите корректный ID партии");
            }
        }

        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtBatchFilter.Text = "";
            await LoadDeviations();
        }
    }
}