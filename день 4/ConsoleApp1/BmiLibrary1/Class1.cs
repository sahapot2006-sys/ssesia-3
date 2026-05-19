namespace BmiLibrary1
{
    public class Class1
    {
        /// <summary>
        /// минимальный весь в кг
        /// </summary>
        private const double MinM = 2;
        /// <summary>
        /// минимальный рост в см
        /// </summary>
        private const double MaxM = 700;
        /// <summary>
        /// индекс массы тела 
        /// </summary>
        private const double MinH = 3;
        public static double GetBmi(double m, double h)
        {
            CheckRangeValue(m, MinH, MaxM, "вес ожидается в диапазоне от :");
        }
    }
}
