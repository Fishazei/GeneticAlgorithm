using GalaSoft.MvvmLight.Command;
using GeneticAlgorithm.Functions;
using OxyPlot;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeneticAlgorithm.Views
{
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
        private readonly Algorithm _algorithm;
        private CancellationTokenSource _cancellationTokenSource;
        // Команды для управления алгоритмом
        public ICommand RunSingleIterationCommand { get; }
        public ICommand RunToCompletionCommand { get; }
        public ICommand StopAlgorithmCommand { get; }
        public ICommand ResetAlgorithmCommand { get; }
        // Привязка параметров алгоритма
        public Algorithm.Settings AlgorithmSettings => _algorithm.Configuration;

        public Page1VM()
        {
            var functionParams = new FuncParams {
                A = [-5], 
                B = [5], 
                Eps = [0.00001] 
            };
            _algorithm = new Algorithm(functionParams, new SinFunction(), "task1");

            // Инициализация графиков
            ObjectiveFunctionPlot = new GraphViewModel("Целевая функция");
            FitnessFunctionPlot = new GraphViewModel("Функция приспособленности");
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
            // Отрисовка целевой функции
            var points = new List<DataPoint>();
            var fitnessPoints = new List<DataPoint>();
            for (double x = _algorithm.FunctionParameters.A[0];
                        x <= _algorithm.FunctionParameters.B[0];
                        x += _algorithm.FunctionParameters.Eps[0])
            {
                points.Add(new DataPoint(x, _algorithm.Configuration.function1.Evaluate(x)));
                fitnessPoints.Add(new DataPoint(x, _algorithm.Configuration.function1.Fitness(x)));
            }
            ObjectiveFunctionPlot.UpdateLineSeries(points, OxyColors.Blue, "Целевая функция");
            FitnessFunctionPlot.UpdateLineSeries(fitnessPoints, OxyColors.Green, "Приспособленность");
        }

        private void OnGenerationCompleted(Algorithm.State state)
        {
            // Обновление точек популяции
            var populationPoints = state.Population.Pop
                .Select(ind => new DataPoint(ind.Decode()[0], _algorithm.Configuration.function1.Evaluate(ind.Decode())));

            ObjectiveFunctionPlot.UpdateScatterSeries(
                populationPoints,
                OxyColors.Red,
                "Популяция");

            populationPoints = state.Population.Pop
            .Select(ind => new DataPoint(ind.Decode()[0], ind.Fitness));

            FitnessFunctionPlot.UpdateScatterSeries(
                populationPoints,
                OxyColors.Red,
                "Популяция"
                );

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
            ObjectiveFunctionPlot.ClearAllSeries();
            FitnessFunctionPlot.ClearAllSeries();
            AverageFitnessPlot.ClearAllSeries();

            // Повторная инициализация
            InitializeFunctionPlots();
        }
    }
}
