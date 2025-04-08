using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Все возможные функции

namespace GeneticAlgorithm.Functions
{
    public interface IFunction
    {
        string? Name { get; set; }
        double Evaluate(params double[] x);
        double Fitness(double value);
    }

    // Абстрактные классы для дальнейшей перезаписи
    public abstract class Function1D : IFunction
    {
        public string? Name { get { return Name ?? "Func"; } set { Name = value; } }
        public abstract double Evaluate(params double[] x);

        public virtual double Fitness(double value)
        {
            // Для задач минимизации
            return 1 / (1 + value);
        }
    }

    public abstract class Function2D : IFunction
    {
        public string? Name { get { return Name ?? "Func"; } set { Name = value; } }
        public abstract double Evaluate(params double[] x);

        public virtual double Fitness(double value)
        {
            // Для задач минимизации
            return 1 / (1 + value);
        }
    }

    // Непосредственно функции
    public static class SinFunction
    {
        public static double Evaluate(params double[] x)
        {
            return -1.3 * Math.Sin(1.6 * Math.Pow(x[0], 2) - 0.3) * Math.Exp(-0.3 * x[0] + 0.5);
        }
    }
}
