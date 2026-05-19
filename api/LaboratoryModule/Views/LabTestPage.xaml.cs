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
    public partial class LabTestPage : Page
    {
        private int _testId;
        private LabTest? _test;
        private ProductionLot? _lot;
        private List<LabParameter> _parameters = new();
        private Dictionary<int, TextBox> _valueInputs = new();

        public LabTestPage(int testId)
        {
            InitializeComponent();
            _testId = testId;
            this.Loaded += LabTestPage_Loaded;
        }

        private async void LabTestPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTestDataAsync();
        }

        private async System.Threading.Tasks.Task LoadTestDataAsync()
        {
            try
            {
                MainWindow.SetStatus("Загрузка данных...");

                if (MainWindow.Api == null)
                {
                    MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _test = await MainWindow.Api.GetActiveTestAsync(_testId);
                if (_test == null)
                {
                    MessageBox.Show("Испытание не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }

                _lot = await MainWindow.Api.GetLotAsync(_test.LotId);

                if (LotInfoText != null)
                    LotInfoText.Text = $"Партия: {_lot?.LotNumber}";
                if (ProductInfoText != null)
                    ProductInfoText.Text = $"Продукт: {_lot?.ProductName} | Тип: {(_lot?.Type == LotType.RawMaterial ? "Сырье" : "Готовая продукция")}";

                if (_lot != null)
                {
                    _parameters = await MainWindow.Api.GetTestTemplateAsync(_lot.ProductId, _lot.Type);
                }

                CreateParameterInputs();

                MainWindow.SetStatus("Готов к вводу данных");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateParameterInputs()
        {
            if (ParametersPanel == null) return;

            ParametersPanel.Children.Clear();
            _valueInputs.Clear();

            foreach (var param in _parameters)
            {
                var border = new Border
                {
                    Margin = new Thickness(0, 5, 0, 5),
                    Padding = new Thickness(10),
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

                var nameLabel = new TextBlock
                {
                    Text = $"{param.Name}:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameLabel, 0);
                grid.Children.Add(nameLabel);

                var inputBox = new TextBox
                {
                    Width = 200,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (_test != null)
                {
                    var existing = _test.Results.FirstOrDefault(r => r.ParameterId == param.Id);
                    if (existing != null)
                    {
                        inputBox.Text = existing.NumericValue?.ToString() ?? existing.StringValue;
                    }
                }

                Grid.SetColumn(inputBox, 1);
                grid.Children.Add(inputBox);
                _valueInputs[param.Id] = inputBox;

                string normText = "";
                if (param.MinValue.HasValue && param.MaxValue.HasValue)
                    normText = $"Норма: {param.MinValue} - {param.MaxValue} {param.Unit}";
                else if (param.MinValue.HasValue)
                    normText = $"Норма: ≥ {param.MinValue} {param.Unit}";
                else if (param.MaxValue.HasValue)
                    normText = $"Норма: ≤ {param.MaxValue} {param.Unit}";

                var normLabel = new TextBlock
                {
                    Text = normText,
                    FontSize = 11,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(normLabel, 2);
                grid.Children.Add(normLabel);

                border.Child = grid;
                ParametersPanel.Children.Add(border);
            }
        }

        private async void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool hasErrors = false;
            List<string> errorMessages = new();

            if (MainWindow.Api == null)
            {
                MessageBox.Show("API клиент не инициализирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var param in _parameters)
            {
                if (_valueInputs.TryGetValue(param.Id, out var input))
                {
                    if (string.IsNullOrWhiteSpace(input.Text) && param.IsRequired)
                    {
                        hasErrors = true;
                        errorMessages.Add($"Параметр '{param.Name}' обязателен");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(input.Text))
                    {
                        try
                        {
                            object value;
                            if (param.MinValue.HasValue || param.MaxValue.HasValue)
                            {
                                value = decimal.Parse(input.Text);
                            }
                            else
                            {
                                value = input.Text;
                            }

                            await MainWindow.Api.SaveResultAsync(_testId, param.Id, value);
                        }
                        catch (Exception ex)
                        {
                            hasErrors = true;
                            errorMessages.Add($"Ошибка в '{param.Name}': {ex.Message}");
                        }
                    }
                }
            }

            if (hasErrors)
            {
                MessageBox.Show(string.Join("\n", errorMessages), "Ошибки", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new DecisionDialogWindow();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var completedTest = await MainWindow.Api.CompleteTestAsync(_testId, dialog.SelectedDecision, dialog.BlockReason);

                    string message = dialog.SelectedDecision == LabDecision.Approved
                        ? "Партия допущена!"
                        : $"Партия заблокирована. Причина: {dialog.BlockReason}";

                    MessageBox.Show(message, "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService?.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}