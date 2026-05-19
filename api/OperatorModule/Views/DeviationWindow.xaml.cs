using System.Windows;
using OperatorModule.Models;

namespace OperatorModule.Views
{
    public partial class DeviationWindow : Window
    {
        private int _lotId;
        private int? _stepId;

        public DeviationWindow(int lotId, int? stepId = null)
        {
            InitializeComponent();
            _lotId = lotId;
            _stepId = stepId;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ParameterBox.Text) || string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MainWindow.Api == null) return;

            var deviation = new ProcessDeviation
            {
                LotId = _lotId,
                StepId = _stepId,
                ParameterName = ParameterBox.Text,
                Description = DescriptionBox.Text,
                Severity = "Warning",
                CreatedAt = DateTime.Now,
                CreatedBy = "Аппаратчик"
            };

            try
            {
                await MainWindow.Api.ReportDeviationAsync(deviation);
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}