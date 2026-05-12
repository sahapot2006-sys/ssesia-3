using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class ReportsPage : Page
    {
        private ApiService apiService = new ApiService();

        public ReportsPage()
        {
            InitializeComponent();
        }

        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DateTime from = dpFrom.SelectedDate ?? DateTime.Now.AddMonths(-1);
            DateTime to = dpTo.SelectedDate ?? DateTime.Now;

            btnGenerate.IsEnabled = false;

            try
            {
                var batches = await apiService.GetBatches();
                var filtered = batches.Where(b =>
                    b.start_time.HasValue &&
                    b.start_time.Value.Date >= from.Date &&
                    b.start_time.Value.Date <= to.Date).ToList();

                if (filtered.Any())
                {
                    dgReport.ItemsSource = filtered;
                    dgReport.Visibility = Visibility.Visible;
                    txtInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgReport.ItemsSource = null;
                    dgReport.Visibility = Visibility.Collapsed;
                    txtInfo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnGenerate.IsEnabled = true;
            }
        }
    }
}