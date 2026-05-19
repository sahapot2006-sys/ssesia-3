namespace ValidationLibrary
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Проверяет, что <paramref name="value"/> в диапазоне <paramref name="min"/>
        /// - <paramref name="max"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="message"></param>
        /// <exception cref="ArgumentOutOfRangeException">Если значение не в диапазоне</exception>
        public static void CheckRangeValue(double value,
            double min, double max, string message)
        {
            if (value <= min || value >= max)
            {
                throw new ArgumentOutOfRangeException(message);
            }
        }
    }
}