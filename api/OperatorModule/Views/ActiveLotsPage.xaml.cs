using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OperatorModule.Models;

namespace OperatorModule.Views
{
    public partial class ActiveLotsPage : Page
    {
        private List<ProductionLot> _allLots = new();

        public ActiveLotsPage()
        {
            InitializeComponent();
            this.Loaded += ActiveLotsPage_Loaded;
        }

        private async void ActiveLotsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLotsAsync();
        }

        private async System.Threading.Tasks.Task LoadLotsAsync()
        {
            try
            {
                MainWindow.SetStatus("Загрузка активных партий...");

                if (MainWindow.Api == null)
                {
                    MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _allLots = await MainWindow.Api.GetActiveLotsAsync();

                LotsGrid.ItemsSource = _allLots;

                MainWindow.SetStatus($"Активных партий: {_allLots.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                MainWindow.SetStatus("Ошибка загрузки");
            }
        }

        private void LotsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = LotsGrid.SelectedItem as ProductionLot;
            ContinueButton.IsEnabled = selected != null;
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var lot = LotsGrid.SelectedItem as ProductionLot;
            if (lot == null) return;

            try
            {
                NavigationService?.Navigate(new ProgramPage(lot.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLotsAsync();
        }
    }
}