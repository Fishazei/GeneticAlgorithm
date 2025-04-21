namespace GeneticAlgorithm.Functions
{
    public struct FuncParams
    {
        public double[] A;
        public double[] B;
        public double[] Eps;
        public int Dimensions => A?.Length ?? 0;
    }

    public interface IOptimizationFunction
    {
        double Evaluate(params double[] x);
        double Fitness(params double[] x);
    }

    // Непосредственно функции
    public class SinFunction : IOptimizationFunction
    {
        public double Evaluate(params double[] x) =>
            -1.3 * Math.Sin(1.6 * Math.Pow(x[0], 2) - 0.3) * Math.Exp(-0.3 * x[0] + 0.5);
        public double Fitness(params double[] x) => -Evaluate(x) + 10;
    }
    public class ZYXFunction : IOptimizationFunction
    {
        public double Evaluate(params double[] x) =>
            Math.Pow(x[0]* x[0] + x[1] - 11, 2) + Math.Pow(x[0] + x[1] * x[1] - 7, 2);
        public double Fitness(params double[] x) => -Evaluate(x) + 300;
    }
}
