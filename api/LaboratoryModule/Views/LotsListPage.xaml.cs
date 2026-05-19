using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using LaboratoryModule.Models;
using LaboratoryModule.Services;

namespace LaboratoryModule.Views
{
    public partial class LotsListPage : Page
    {
        private List<ProductionLot> _allLots = new();

        public LotsListPage()
        {
            InitializeComponent();
            this.Loaded += LotsListPage_Loaded;
        }

        private async void LotsListPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLotsAsync();
        }

        private async System.Threading.Tasks.Task LoadLotsAsync()
        {
            try
            {
                MainWindow.SetStatus("Загрузка партий...");

                // Проверка, что API не null
                if (MainWindow.Api == null)
                {
                    MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LotType? type = null;
                if (TypeFilter?.SelectedItem is ComboBoxItem typeItem)
                {
                    var typeText = typeItem.Content.ToString();
                    if (typeText == "Сырье") type = LotType.RawMaterial;
                    if (typeText == "Готовая продукция") type = LotType.FinalProduct;
                }

                _allLots = await MainWindow.Api.GetLotsForLabAsync(type);
                ApplyStatusFilter();

                MainWindow.SetStatus($"Загружено {_allLots.Count} партий");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                MainWindow.SetStatus("Ошибка загрузки");
            }
        }

        private void ApplyStatusFilter()
        {
            if (StatusFilter?.SelectedItem is ComboBoxItem statusItem)
            {
                var statusText = statusItem.Content.ToString();

                var filtered = _allLots.AsEnumerable();

                if (statusText == "Ожидают")
                    filtered = filtered.Where(l => l.LabStatus == LabStatus.Pending);
                else if (statusText == "В процессе")
                    filtered = filtered.Where(l => l.LabStatus == LabStatus.InProgress);
                else if (statusText == "Допущены")
                    filtered = filtered.Where(l => l.LabStatus == LabStatus.Approved);
                else if (statusText == "Заблокированы")
                    filtered = filtered.Where(l => l.LabStatus == LabStatus.Blocked);

                if (LotsGrid != null)
                {
                    LotsGrid.ItemsSource = filtered.ToList();
                }
            }
        }

        private async void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLotsAsync();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyStatusFilter();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLotsAsync();
        }

        private void LotsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = LotsGrid?.SelectedItem as ProductionLot;
            if (StartTestButton != null)
                StartTestButton.IsEnabled = selected != null && selected?.LabStatus == LabStatus.Pending;
            if (ViewHistoryButton != null)
                ViewHistoryButton.IsEnabled = selected != null;
        }

        private async void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            var lot = LotsGrid?.SelectedItem as ProductionLot;
            if (lot == null) return;

            try
            {
                if (MainWindow.Api == null)
                {
                    MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingTest = await MainWindow.Api.GetActiveTestAsync(lot.Id);

                if (existingTest != null)
                {
                    NavigationService?.Navigate(new LabTestPage(existingTest.Id));
                }
                else
                {
                    var newTest = await MainWindow.Api.CreateTestAsync(lot.Id);
                    NavigationService?.Navigate(new LabTestPage(newTest.Id));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var lot = LotsGrid?.SelectedItem as ProductionLot;
            if (lot == null) return;

            try
            {
                if (MainWindow.Api == null)
                {
                    MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var history = await MainWindow.Api.GetAuditLogAsync(lot.Id);

                var historyText = string.Join("\n", history.Select(h =>
                    $"{h.Timestamp:HH:mm:ss} | {h.UserName} | {h.Action} | {h.Details}"));

                MessageBox.Show(historyText, $"История партии {lot.LotNumber}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}