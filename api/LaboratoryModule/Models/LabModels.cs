using System;
using System.Collections.Generic;

namespace LaboratoryModule.Models
{
    public enum LotType
    {
        RawMaterial,
        FinalProduct
    }

    public enum LabStatus
    {
        Pending,
        InProgress,
        Approved,
        Blocked
    }

    public enum LabDecision
    {
        None,
        Approved,
        Blocked
    }

    public class ProductionLot
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public LotType Type { get; set; }
        public LabStatus LabStatus { get; set; }
        public string? BlockReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LabParameter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }

    public class LabResult
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal? NumericValue { get; set; }
        public string? StringValue { get; set; }
        public bool IsValid { get; set; }
        public string? NormRange { get; set; }
    }

    public class LabTest
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public LabStatus Status { get; set; }
        public LabDecision Decision { get; set; }
        public string? BlockReason { get; set; }
        public string? PerformedBy { get; set; }
        public List<LabResult> Results { get; set; } = new();
    }

    public class AuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}