using System;
using System.Collections.Generic;

namespace OperatorModule.Models
{
    // Статус партии
    public enum LotStatus
    {
        Planned,        // Запланирована
        InProgress,     // В процессе
        OnQualityControl, // На контроле качества
        Completed,      // Завершена
        Blocked,        // Заблокирована
        Paused          // На паузе
    }

    // Тип шага
    public enum StepType
    {
        Instruction,    // Инструкция (текст)
        ParameterInput, // Ввод параметров
        EquipmentControl, // Управление оборудованием
        WaitForLab,     // Ожидание лаборатории
        Complete        // Завершение
    }

    // Производственная партия
    public class ProductionLot
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int TechCardId { get; set; }
        public string TechCardName { get; set; } = string.Empty;
        public LotStatus Status { get; set; }
        public int? CurrentStepId { get; set; }
        public int CurrentStepIndex { get; set; }
        public DateTime StartedAt { get; set; }
        public string? EquipmentName { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
    }

    // Шаг технологической карты
    public class TechCardStep
    {
        public int Id { get; set; }
        public int TechCardId { get; set; }
        public int StepNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public StepType Type { get; set; }
        public int? DurationMinutes { get; set; }
        public List<StepParameter> Parameters { get; set; } = new();
        public List<string> EquipmentCommands { get; set; } = new();
    }

    // Параметр шага (для ввода)
    public class StepParameter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool IsRequired { get; set; }
    }

    // Выполнение шага (фактические данные)
    public class StepExecution
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public int StepId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public List<ParameterActualValue> ActualValues { get; set; } = new();
        public string? DeviationNote { get; set; }
    }

    // Фактическое значение параметра
    public class ParameterActualValue
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public bool IsWithinNorm { get; set; }
    }

    // Телеметрия оборудования (экструдер)
    public class TelemetryData
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public decimal TemperatureZ1 { get; set; }
        public decimal TemperatureZ2 { get; set; }
        public decimal TemperatureZ3 { get; set; }
        public decimal TemperatureZ4 { get; set; }
        public decimal ScrewSpeed { get; set; }
        public decimal Pressure { get; set; }
        public decimal CurrentLoad { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Отклонение в процессе
    public class ProcessDeviation
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public int? StepId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Warning, Critical
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}