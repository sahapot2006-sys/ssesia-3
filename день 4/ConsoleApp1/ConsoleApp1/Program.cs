using BmiLibrary;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            double weight = ReadDouble("Введите массу тела в кг: ");
            double height = ReadDouble("Введите рост в метрах: ");

            double bmi = BmiCalculator.GetBmi(weight, height);

            Console.WriteLine($"Ваш ИМТ: {bmi}");
        }

        /// <summary>
        /// Читает значение <see cref="double"/> из консоли
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        private static double ReadDouble(string prompt)
        {
            Console.WriteLine(prompt);

            return double.Parse(Console.ReadLine());
        }
    }
}