using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using OperatorModule.Models;

namespace OperatorModule.Views
{
    public partial class ProgramPage : Page
    {
        private int _lotId;
        private ProductionLot? _lot;
        private List<TechCardStep> _allSteps = new();
        private List<StepDisplay> _stepsDisplay = new();
        private TechCardStep? _currentStep;
        private StepExecution? _currentExecution;
        private DispatcherTimer? _telemetryTimer;

        public ProgramPage(int lotId)
        {
            InitializeComponent();
            _lotId = lotId;
            this.Loaded += ProgramPage_Loaded;
            this.Unloaded += ProgramPage_Unloaded;
        }

        private async void ProgramPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProgramDataAsync();
            StartTelemetryUpdates();
        }

        private void ProgramPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _telemetryTimer?.Stop();
        }

        private async System.Threading.Tasks.Task LoadProgramDataAsync()
        {
            try
            {
                MainWindow.SetStatus("Загрузка программы партии...");

                if (MainWindow.Api == null) return;

                _lot = await MainWindow.Api.GetLotAsync(_lotId);
                if (_lot == null)
                {
                    MessageBox.Show("Партия не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }

                _allSteps = await MainWindow.Api.GetLotStepsAsync(_lotId);

                // Формируем список для отображения
                _stepsDisplay.Clear();
                for (int i = 0; i < _allSteps.Count; i++)
                {
                    var step = _allSteps[i];
                    _stepsDisplay.Add(new StepDisplay
                    {
                        Id = step.Id,
                        StepNumber = step.StepNumber,
                        Name = step.Name,
                        IsCompleted = step.StepNumber < _lot.CurrentStepIndex,
                        IsCurrent = step.StepNumber == _lot.CurrentStepIndex,
                        Status = step.StepNumber < _lot.CurrentStepIndex ? "Выполнен" :
                                 step.StepNumber == _lot.CurrentStepIndex ? "Текущий" : "Ожидает"
                    });
                }

                StepsList.ItemsSource = _stepsDisplay;

                // Загружаем информацию о партии
                LotNumberText.Text = $"Партия: {_lot.LotNumber}";
                ProductNameText.Text = $"Продукт: {_lot.ProductName}";
                StatusText.Text = $"Статус: {_lot.Status}";

                // Загружаем текущий шаг
                _currentExecution = await MainWindow.Api.GetCurrentStepExecutionAsync(_lotId);

                if (_currentExecution != null && _currentExecution.IsCompleted == false)
                {
                    _currentStep = _allSteps.FirstOrDefault(s => s.Id == _currentExecution.StepId);
                    DisplayCurrentStep();
                }

                MainWindow.SetStatus($"Партия {_lot.LotNumber} загружена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCurrentStep()
        {
            if (_currentStep == null) return;

            CurrentStepNameText.Text = _currentStep.Name;
            CurrentStepDescText.Text = _currentStep.Description;

            // Отображаем параметры для ввода
            ParametersPanel.Children.Clear();

            foreach (var param in _currentStep.Parameters)
            {
                var stack = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };

                var label = new TextBlock
                {
                    Text = $"{param.Name} ({param.Unit})",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 3)
                };
                stack.Children.Add(label);

                var hint = "";
                if (param.MinValue.HasValue && param.MaxValue.HasValue)
                    hint = $"Норма: {param.MinValue} - {param.MaxValue}";
                else if (param.MinValue.HasValue)
                    hint = $"Норма: ≥ {param.MinValue}";
                else if (param.MaxValue.HasValue)
                    hint = $"Норма: ≤ {param.MaxValue}";

                var hintLabel = new TextBlock
                {
                    Text = hint,
                    FontSize = 10,
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                stack.Children.Add(hintLabel);

                var input = new TextBox
                {
                    Tag = param.Id,
                    Margin = new Thickness(0, 3, 0, 0),
                    Height = 30
                };

                // Загружаем существующее значение
                var existing = _currentExecution?.ActualValues.FirstOrDefault(v => v.ParameterId == param.Id);
                if (existing != null)
                {
                    input.Text = existing.Value;
                }

                input.TextChanged += (s, ev) =>
                {
                    SaveParameterValue(param.Id, input.Text);
                };

                stack.Children.Add(input);
                ParametersPanel.Children.Add(stack);
            }

            StartStepButton.IsEnabled = _currentExecution == null;
            CompleteStepButton.IsEnabled = _currentExecution != null && !_currentExecution.IsCompleted;
            ReportDeviationButton.IsEnabled = _currentExecution != null && !_currentExecution.IsCompleted;
        }

        private async void SaveParameterValue(int parameterId, string value)
        {
            if (_currentExecution == null) return;
            if (MainWindow.Api == null) return;

            try
            {
                await MainWindow.Api.SaveStepParameterAsync(_currentExecution.Id, parameterId, value);
            }
            catch (Exception ex)
            {
                // Не показываем ошибку при каждом сохранении, просто логируем
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private async void StartStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == null || _lot == null) return;
            if (MainWindow.Api == null) return;

            try
            {
                _currentExecution = await MainWindow.Api.StartStepAsync(_lotId, _currentStep.Id);

                // Обновляем отображение
                DisplayCurrentStep();

                MainWindow.SetStatus($"Начат шаг: {_currentStep.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CompleteStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentExecution == null) return;
            if (MainWindow.Api == null) return;

            var result = MessageBox.Show("Вы завершили этот шаг?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await MainWindow.Api.CompleteStepAsync(_currentExecution.Id);

                // Перезагружаем данные
                await LoadProgramDataAsync();

                MainWindow.SetStatus("Шаг завершен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PauseLotButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lot == null) return;
            if (MainWindow.Api == null) return;

            try
            {
                await MainWindow.Api.PauseLotAsync(_lotId);
                await LoadProgramDataAsync();
                MainWindow.SetStatus("Партия поставлена на паузу");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ReportDeviationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentExecution == null || _currentStep == null) return;

            var deviationWindow = new DeviationWindow(_lotId, _currentStep.Id);
            if (deviationWindow.ShowDialog() == true)
            {
                MainWindow.SetStatus("Отклонение зарегистрировано");
            }
        }

        private void StepsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно показывать информацию о шаге, но не разрешать переключение
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void StartTelemetryUpdates()
        {
            _telemetryTimer = new DispatcherTimer();
            _telemetryTimer.Interval = TimeSpan.FromSeconds(3);
            _telemetryTimer.Tick += async (s, e) => await UpdateTelemetryAsync();
            _telemetryTimer.Start();
        }

        private async System.Threading.Tasks.Task UpdateTelemetryAsync()
        {
            if (MainWindow.Api == null) return;

            try
            {
                var telemetry = await MainWindow.Api.GetTelemetryAsync(1); // ID экструдера
                if (telemetry != null)
                {
                    TempZ1.Text = $"T1: {telemetry.TemperatureZ1:F1}°C";
                    TempZ2.Text = $"T2: {telemetry.TemperatureZ2:F1}°C";
                    TempZ3.Text = $"T3: {telemetry.TemperatureZ3:F1}°C";
                    TempZ4.Text = $"T4: {telemetry.TemperatureZ4:F1}°C";
                    ScrewSpeed.Text = $"Шнек: {telemetry.ScrewSpeed:F0} об/мин";
                    Pressure.Text = $"Давление: {telemetry.Pressure:F1} бар";
                    CurrentLoad.Text = $"Ток: {telemetry.CurrentLoad:F1} A";
                    TelemetryTime.Text = telemetry.Timestamp.ToString("HH:mm:ss");
                }
            }
            catch
            {
                // Если телеметрия не доступна - просто игнорируем
            }
        }

        // Вспомогательный класс для отображения шагов
        private class StepDisplay
        {
            public int Id { get; set; }
            public int StepNumber { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}