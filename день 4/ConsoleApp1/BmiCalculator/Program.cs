using ValidationLibrary;

namespace BmiLibrary
{
    /// <summary>
    /// Калькулятор ИМТ
    /// </summary>
    public class BmiCalculator
    {
        #region Константы
        /// <summary>
        /// Минимальный вес в кг
        /// </summary>
        private const double MinWeight = 2;
        /// <summary>
        /// Максимальный вес в кг
        /// </summary>
        private const double MaxWeight = 700;
        /// <summary>
        /// Минимальный рост в м
        /// </summary>
        private const double MinHeight = 0.3;
        /// <summary>
        /// Максимальный рост в м
        /// </summary>
        private const double MaxHeight = 3;
        #endregion

        /// <summary>
        /// Вычисляет индекс массы тела
        /// </summary>
        /// <param name="weight">Масса тела в кг</param>
        /// <param name="height">Рост в метрах</param>
        /// <returns>Индекс массы тела</returns>
        public static double GetBmi(double weight, double height)
        {
            ValidationHelper.CheckRangeValue(weight, MinWeight, MaxWeight,
                $"Вес ожидается в диапазоне от {MinWeight} до {MaxWeight} кг");
            ValidationHelper.CheckRangeValue(height, MinHeight, MaxHeight,
                $"Рост ожидается в диапазоне от {MinHeight} до {MaxHeight} м");

            return weight / (height * height);
        }
    }
}