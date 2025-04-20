using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Все возможные функции

namespace GeneticAlgorithm.Functions
{

    public struct FuncParams
    {
        public double A;
        public double B;
        public double Eps;
    }

    // Непосредственно функции
    public static class SinFunction
    {
        public static double Evaluate(params double[] x)
        {
            //return x[0] * x[0];
            return -1.3 * Math.Sin(1.6 * Math.Pow(x[0], 2) - 0.3) * Math.Exp(-0.3 * x[0] + 0.5);
        }
        public static double Fitness(params double[] x)
        {
            return -Evaluate(x) + 10;
        }
    }
}
