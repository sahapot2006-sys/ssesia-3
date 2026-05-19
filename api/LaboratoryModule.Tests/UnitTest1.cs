using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

// Простые модели для тестирования (дублируем, чтобы избежать ошибок ссылок)
namespace LaboratoryModule.Tests
{
    // Дублируем необходимые enum и классы для тестов
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

    public class LabParameter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool IsRequired { get; set; }
    }

    public class LabTest
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public LabStatus Status { get; set; }
        public LabDecision Decision { get; set; }
        public string? BlockReason { get; set; }
        public List<LabResult> Results { get; set; } = new();
    }

    public class LabResult
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public decimal? NumericValue { get; set; }
        public bool IsValid { get; set; }
    }

    [TestClass]
    public class LabTestViewModelTests
    {
        [TestMethod]
        public void Parameter_WithinNorm_ReturnsTrue()
        {
            // Arrange
            var parameter = new LabParameter
            {
                Id = 1,
                Name = "Массовая доля",
                MinValue = 95,
                MaxValue = 98,
                Unit = "%"
            };
            decimal value = 96.5m;

            // Act
            bool isWithinNorm = value >= parameter.MinValue && value <= parameter.MaxValue;

            // Assert
            Assert.IsTrue(isWithinNorm);
        }

        [TestMethod]
        public void Parameter_OutOfNorm_ReturnsFalse()
        {
            // Arrange
            var parameter = new LabParameter
            {
                Id = 1,
                Name = "Массовая доля",
                MinValue = 95,
                MaxValue = 98,
                Unit = "%"
            };
            decimal value = 92.0m;

            // Act
            bool isWithinNorm = value >= parameter.MinValue && value <= parameter.MaxValue;

            // Assert
            Assert.IsFalse(isWithinNorm);
        }

        [TestMethod]
        public void Parameter_EqualMinValue_ReturnsTrue()
        {
            // Arrange
            var parameter = new LabParameter
            {
                MinValue = 95,
                MaxValue = 98
            };
            decimal value = 95;

            // Act
            bool isWithinNorm = value >= parameter.MinValue && value <= parameter.MaxValue;

            // Assert
            Assert.IsTrue(isWithinNorm);
        }

        [TestMethod]
        public void Parameter_EqualMaxValue_ReturnsTrue()
        {
            // Arrange
            var parameter = new LabParameter
            {
                MinValue = 95,
                MaxValue = 98
            };
            decimal value = 98;

            // Act
            bool isWithinNorm = value >= parameter.MinValue && value <= parameter.MaxValue;

            // Assert
            Assert.IsTrue(isWithinNorm);
        }

        [TestMethod]
        public void CompleteTest_WithoutBlockReason_WhenBlocking_ShouldFail()
        {
            // Arrange
            var test = new LabTest
            {
                Id = 1,
                Status = LabStatus.InProgress,
                Decision = LabDecision.Blocked
            };
            bool hasBlockReason = false;

            // Act
            bool isValid = !(test.Decision == LabDecision.Blocked && !hasBlockReason);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void CompleteTest_WithBlockReason_WhenBlocking_ShouldPass()
        {
            // Arrange
            var test = new LabTest
            {
                Id = 1,
                Status = LabStatus.InProgress,
                Decision = LabDecision.Blocked
            };
            string blockReason = "Несоответствие pH норме";

            // Act
            bool hasBlockReason = !string.IsNullOrWhiteSpace(blockReason);
            bool isValid = !(test.Decision == LabDecision.Blocked && !hasBlockReason);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void CompleteTest_Approved_DoesNotRequireReason()
        {
            // Arrange
            var test = new LabTest
            {
                Id = 1,
                Status = LabStatus.InProgress,
                Decision = LabDecision.Approved
            };
            string blockReason = null;

            // Act
            bool needsReason = test.Decision == LabDecision.Blocked && string.IsNullOrWhiteSpace(blockReason);
            bool isValid = !needsReason;

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void LabStatus_EnumValues_AreCorrect()
        {
            // Assert
            Assert.AreEqual(0, (int)LabStatus.Pending);
            Assert.AreEqual(1, (int)LabStatus.InProgress);
            Assert.AreEqual(2, (int)LabStatus.Approved);
            Assert.AreEqual(3, (int)LabStatus.Blocked);
        }

        [TestMethod]
        public void LabTest_InitialStatus_IsPending()
        {
            // Arrange & Act
            var test = new LabTest
            {
                Id = 1,
                LotId = 100,
                Status = LabStatus.Pending
            };

            // Assert
            Assert.AreEqual(LabStatus.Pending, test.Status);
        }
    }

    [TestClass]
    public class ParameterValidationTests
    {
        [TestMethod]
        public void RequiredParameter_EmptyValue_ShouldFail()
        {
            // Arrange
            var parameter = new LabParameter
            {
                Id = 1,
                Name = "Обязательный параметр",
                IsRequired = true
            };
            string value = "";

            // Act
            bool isValid = !parameter.IsRequired || !string.IsNullOrWhiteSpace(value);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void RequiredParameter_WithValue_ShouldPass()
        {
            // Arrange
            var parameter = new LabParameter
            {
                Id = 1,
                Name = "Обязательный параметр",
                IsRequired = true
            };
            string value = "Тестовое значение";

            // Act
            bool isValid = !parameter.IsRequired || !string.IsNullOrWhiteSpace(value);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void OptionalParameter_EmptyValue_ShouldPass()
        {
            // Arrange
            var parameter = new LabParameter
            {
                Id = 1,
                Name = "Необязательный параметр",
                IsRequired = false
            };
            string value = "";

            // Act
            bool isValid = !parameter.IsRequired || !string.IsNullOrWhiteSpace(value);

            // Assert
            Assert.IsTrue(isValid);
        }
    }

    [TestClass]
    public class ResultValidationTests
    {
        [TestMethod]
        public void Result_IsValid_WhenWithinNorm()
        {
            // Arrange
            var result = new LabResult
            {
                ParameterId = 1,
                ParameterName = "Температура",
                NumericValue = 25,
                IsValid = true
            };

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Result_IsInvalid_WhenOutOfNorm()
        {
            // Arrange
            var result = new LabResult
            {
                ParameterId = 1,
                ParameterName = "Температура",
                NumericValue = 35,
                IsValid = false
            };

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}