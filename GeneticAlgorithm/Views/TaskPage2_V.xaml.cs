using GeneticAlgorithm.Functions;
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
using GalaSoft.MvvmLight.Command;

namespace GeneticAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for TaskPage2_V.xaml
    /// </summary>
    public partial class TaskPage2_V : Page
    {
        public TaskPage2_V()
        {
            InitializeComponent();
        }
    }

    public class Page2VM : ViewModelBase
    {
        public Graph3DViewModel Function3DPlot { get; }
        public Graph3DViewModel FitnessFunction3DPlot { get; }
        public GraphViewModel AverageFitnessPlot { get; }
        private readonly Algorithm _algorithm;
        private CancellationTokenSource _cancellationTokenSource;
        // Команды для управления алгоритмом
        public ICommand RunSingleIterationCommand { get; }
        public ICommand RunToCompletionCommand { get; }
        public ICommand StopAlgorithmCommand { get; }
        public ICommand ResetAlgorithmCommand { get; }
        // Привязка параметров алгоритма
        public Algorithm.Settings AlgorithmSettings => _algorithm.Configuration;

        public Page2VM()
        {
            var functionParams = new FuncParams
            {
                A = [-4.0, -4.0],
                B = [4.0, 4.0],
                Eps = [0.001, 0.001]
            };
            _algorithm = new Algorithm(functionParams, new ZYXFunction());

            // Инициализация графиков
            Function3DPlot = new Graph3DViewModel(new ZYXFunction());
            FitnessFunction3DPlot = new Graph3DViewModel(new ZYXFunction());
            AverageFitnessPlot = new GraphViewModel("Средняя приспособленность");

            // Инициализация команд
            RunSingleIterationCommand = new RelayCommand(RunSingleIteration);
            RunToCompletionCommand = new RelayCommand(async () => await RunToCompletionAsync());
            StopAlgorithmCommand = new RelayCommand(StopAlgorithm);
            ResetAlgorithmCommand = new RelayCommand(ResetAlgorithm);

            // Подписка на события алгоритма
            _algorithm.OnGenerationCompleted += OnGenerationCompleted;
            _algorithm.OnAlgorithmCompleted += OnAlgorithmCompleted;
            // Первоначальная инициализация графиков
            InitializeFunctionPlots();
        }

        private void InitializeFunctionPlots()
        {
            // Отрисовка 3D поверхности функции
            Function3DPlot.UpdateFunctionSurface(
                _algorithm.FunctionParameters.A[0],
                _algorithm.FunctionParameters.B[0],
                _algorithm.FunctionParameters.A[1],
                _algorithm.FunctionParameters.B[1]);
        }

        private void OnGenerationCompleted(Algorithm.State state)
        {
            // Обновление точек популяции в 3D
            var populationPoints = state.Population.Pop
                .Select(ind => ind.Decode());

            Function3DPlot.UpdatePopulation(populationPoints);

            //// Подсветка лучшего решения
            //var bestIndividual = state.Population.Pop
            //    .OrderBy(ind => _algorithm.Configuration.function1.Fitness(ind.Decode()))
            //    .FirstOrDefault();

            //if (bestIndividual != null)
            //{
            //    Function3DPlot.HighlightBestSolution(bestIndividual.Decode());
            //}

            // Обновление графика средней приспособленности
            var avgFitnessPoints = state.AverageFitnessHistory
                .Select((value, index) => new DataPoint(index, value));
            AverageFitnessPlot.UpdateLineSeries(
                avgFitnessPoints,
                OxyColors.Purple,
                "Средняя приспособленность");
        }

        private void OnAlgorithmCompleted(Algorithm.State state)
        {
            // Действия по завершению алгоритма
            MessageBox.Show("Алгоритм завершил работу");
        }

        private void RunSingleIteration()
        {
            _algorithm.RunSingleIteration();
        }

        private async Task RunToCompletionAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await _algorithm.RunToCompletionAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Алгоритм был остановлен
            }
        }

        private void StopAlgorithm()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void ResetAlgorithm()
        {
            _algorithm.Reset();

            // Очистка графиков
            //Function3DPlot.ClearPlot();
            AverageFitnessPlot.ClearAllSeries();

            // Повторная инициализация
            InitializeFunctionPlots();
        }
    }
}
