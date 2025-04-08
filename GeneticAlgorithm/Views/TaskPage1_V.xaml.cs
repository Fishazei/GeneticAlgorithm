using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeneticAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for TaskPage1_V.xaml
    /// </summary>
    public partial class TaskPage1_V : Page
    {
        public TaskPage1_V()
        {
            InitializeComponent();
        }
    }

    public class Page1VM : ViewModelBase
    {
        public GraphViewModel ObjectiveFunctionPlot { get; }
        public GraphViewModel FitnessFunctionPlot { get; }
        public GraphViewModel AverageFitnessPlot { get; }

        public Page1VM()
        {
            ObjectiveFunctionPlot = new GraphViewModel("Objective Function");
            FitnessFunctionPlot = new GraphViewModel("Fitness Function");
            AverageFitnessPlot = new GraphViewModel("Average Fitness Over Generations");

            InitializePlots(); // Инициализации графиков
        }

        private void InitializePlots()
        {
            // Пример данных для графика
            var points = new List<DataPoint>();
            var fitpoints = new List<DataPoint>();
            for (double x = -5; x <= 5; x += 0.001)
            {
                points.Add(new DataPoint(x, Functions.SinFunction.Evaluate(x)));
                fitpoints.Add(new DataPoint(x, Functions.SinFunction.Evaluate(x) + 10));
            }

            ObjectiveFunctionPlot.UpdatePlot(points, OxyColors.Blue);
            FitnessFunctionPlot.UpdatePlot(fitpoints, OxyColors.Red);
        }
    }
}
