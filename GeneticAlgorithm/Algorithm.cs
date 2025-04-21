using GeneticAlgorithm.Views;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GeneticAlgorithm.Functions;

namespace GeneticAlgorithm
{
    public class Algorithm
    {
        private Logger _logger;
        // Настройки алгоритма
        public class Settings
        {
            public int PopulationSize { get; set; } = 20;
            public double EliteRatio { get; set; } = 0.2;
            public double CrossoverRate { get; set; } = 0.85;
            public double MutationRate { get; set; } = 0.15;
            public int MaxGenerations { get; set; } = 100;
            public int DelayBetweenGenerations { get; set; } = 500; // ms
            public IOptimizationFunction function1 = new SinFunction();

            public string Log()
            {
                string tmp = "";
                tmp += "|| Algorithm Settings:\n" + $"|| Population size: {PopulationSize}\n"
                    + $"|| Elite ratio: {EliteRatio}\n" + $"|| Crossover rate: {CrossoverRate}\n"
                    + $"|| Mutation rate: {MutationRate}\n"; 
                return tmp;
            }
        }

        // Состояние алгоритма
        public class State
        {
            public int CurrentGeneration { get; set; } = 0;
            public Population Population { get; set; }
            public List<double> AverageFitnessHistory { get; } = new List<double>();
            public bool IsRunning { get; set; } = false;
        }

        // Публичные свойства
        public Settings Configuration { get; } = new Settings();
        public State CurrentState { get; private set; } = new State();
        public FuncParams FunctionParameters { get; private set; }

        // События для внешнего интерфейса
        public event Action<State> OnGenerationCompleted;
        public event Action<State> OnAlgorithmCompleted;

        // Конструктор
        public Algorithm(FuncParams functionParams, IOptimizationFunction function, string filePath)
        {
            FunctionParameters = functionParams;
            Configuration.function1 = function;
            _logger = new Logger(filePath);
            Reset();
        }

        // Сброс алгоритма
        public void Reset()
        {
            CurrentState = new State
            {
                Population = new Population(Configuration.PopulationSize, FunctionParameters, Configuration.function1)
                {
                    CrosRatio = Configuration.CrossoverRate
                },
                CurrentGeneration = 0,
            };
            _logger.Log(Configuration.Log());
        }

        // Запуск одной итерации
        public void RunSingleIteration()
        {
            if (CurrentState.CurrentGeneration >= Configuration.MaxGenerations)
                return;

            var pop = CurrentState.Population; 
            pop.SortPopulate();
            pop.EliteCrossover();
            pop.RouletteSelection();
            //pop.Mutate(Configuration.MutationRate);
            pop.CalcAveFit();

            CurrentState.AverageFitnessHistory.Add(pop.AveFit);
            OnGenerationCompleted?.Invoke(CurrentState);
            CurrentState.CurrentGeneration++;
            if (CurrentState.CurrentGeneration >= Configuration.MaxGenerations)
                OnAlgorithmCompleted?.Invoke(CurrentState);

            _logger.Log(pop.LogPopulate(CurrentState.CurrentGeneration));
            _logger.Log($"| Average fitness in generation: {pop.AveFit}\n");
        }

        // Асинхронный запуск до завершения
        public async Task RunToCompletionAsync(CancellationToken cancellationToken = default)
        {
            CurrentState.IsRunning = true;

            while (CurrentState.CurrentGeneration < Configuration.MaxGenerations &&
                   !cancellationToken.IsCancellationRequested)
            {
                RunSingleIteration();
                await Task.Delay(Configuration.DelayBetweenGenerations, cancellationToken);
            }

            CurrentState.IsRunning = false;
        }

        // Метод для внешней настройки
        public void Configure(Action<Settings> configAction)
        {
            configAction?.Invoke(Configuration);
            Reset(); // Пересоздаём популяцию с новыми параметрами
        }
    }

    // Отдельная популяция
    public class Population
    {
        public List<Individ> Pop = new List<Individ>();
        int _popCount;
        public double CrosRatio = 0.85;    // Вероятность скрещивания
        public double MutateRatio = 0.15;
        public double AveFit { get; private set; }

        private readonly IOptimizationFunction _func;

        public Population(int popCount, FuncParams fp, IOptimizationFunction func)
        {
            _popCount = popCount;
            for (int i = 0; i < popCount; i++)
                Pop.Add(new Individ(fp));
            _func = func;
        }

        // Подсчёт приспособленностей
        public double[] CalcFitness()
        {
            int i = 0;
            double[] result = new double[Pop.Count];
            foreach (var individ in Pop)
            {
                individ.Fitness = _func.Fitness(individ.Decode());
                result[i++] = individ.Fitness;
            }
            return result;
        }
        public void CalcAveFit()
        {
            AveFit = 0;
            foreach (var individ in Pop)
            {
                individ.Fitness = _func.Fitness(individ.Decode());
                AveFit += individ.Fitness;
            }
            AveFit /= Pop.Count;
        }
        // Сортировка популяции по функции приспособленности
        public void SortPopulate()
        {
            CalcFitness();
            Pop.Sort((x, y) => y.Fitness.CompareTo(x.Fitness));
        }
        #region Отбор
        // Отбор Рулеткой
        public void RouletteSelection()
        {
            if (Pop.Count <= _popCount) return;
            SortPopulate();
            var rnd = new Random();
            double[] pr;
            List<Individ> tmp = new List<Individ>();
            while (tmp.Count < _popCount)
            {
                pr = CalcPR();
                double p = rnd.NextDouble();
                double psum = 0;
                int i = -1;
                while (psum < p) psum += pr[++i];
                tmp.Add(Pop[i].Clone());
                Pop.RemoveAt(i);
            }
            Pop.Clear();
            Pop = tmp;
        }
        // Подсчёт вероятностей
        double[] CalcPR()
        {
            double[] pr = new double[Pop.Count];
            double sum = 0;
            foreach (var individ in Pop) sum += individ.Fitness;
            for (int i = 0; i < Pop.Count; i++) pr[i] = Pop[i].Fitness / sum;
            return pr;
        }
        #endregion
        #region Скрещивание и мутация
        public void EliteCrossover(){
            var rnd = new Random();
            // Скрещивание лучших между собой
            for (int i = 0; i < _popCount - 1; i += 2){
                // Учитывание вероятности скрещивания
                if (rnd.NextDouble() >= CrosRatio) continue;
                // Временные дети
                var child1 = new Individ(Pop[i].FP);
                var child2 = new Individ(Pop[i].FP);
                int j = i+1;
                // Скрещивания с выбором случайной точки перелома по каждой хромосоме
                for (int dim = 0; dim < Pop[i].FP.Dimensions; dim++){
                    var cross = rnd.Next(2, Pop[i].BitCount[dim] - 2);
                    child1.Crossbreeding(Pop[i], Pop[j], cross, dim);
                    child2.Crossbreeding(Pop[j], Pop[i], cross, dim);
                    // Мутация
                    if (rnd.NextDouble() < MutateRatio)
                        child1.Mutate(dim, rnd.Next(0, child1.BitCount[dim]));
                    if (rnd.NextDouble() < MutateRatio)
                        child2.Mutate(dim, rnd.Next(0, child2.BitCount[dim]));

                }
                // Подсчёт приспособленности для молодых
                child1.Fitness = _func.Fitness(child1.Decode());
                child2.Fitness = _func.Fitness(child2.Decode());
                // Добавляем в основную популяцию
                Pop.Add(child1); Pop.Add(child2);
            }
        }
        #endregion
        public string LogPopulate(int? a)
        {
            string genTemp = "";
            if (a != null ) genTemp = $"== Generation {a} ==\n";
            foreach (var individ in Pop)
            {
                string l1 = $"| {individ.LogChrom()}|";
                string l2 = $"\t{string.Join(" ", string.Join(" ", individ.Decode().Select(x => $"{x:f2}")))}" +
                            $"\t{individ.Fitness:f2}\t{_func.Evaluate(individ.Decode()):f2}\n";
                genTemp += l1 + l2;
            }
            Debug.WriteLine(genTemp);
            return genTemp;
        }
    }

    // Индивид
    public class Individ
    {
        public int[][] Chromosome;
        public double Fitness;

        public int[] BitCount { get; private set; }
        public FuncParams FP { get; private set; }

        public Individ(FuncParams FP)
        {
            this.FP = FP;
            var rnd = new Random();
            // Пока для одной переменной
            //BitCount = GrayCode.CalcCLenght(FP);
            //Chromosome = new int[BitCount];
            //for (int i = 0; i < BitCount; i++)
            //    Chromosome[i] = rnd.Next(2);

            BitCount = new int[FP.Dimensions];
            Chromosome = new int[FP.Dimensions][];
            for (int i = 0; i < FP.Dimensions; i++){
                BitCount[i] = GrayCode.CalcCLenght(FP, i);
                Chromosome[i] = new int[BitCount[i]];
                for (int j = 0; j < BitCount[i]; j++)
                    Chromosome[i][j] = rnd.Next(2);
            }
        }

        // Перевод кода Грея в число
        public double[] Decode()
        {
            double[] values = new double[FP.Dimensions];
            for (int i = 0; i < FP.Dimensions; i++){
                int grayValue = BinaryArrayToInt(Chromosome[i]);
                int binaryValue = GrayToBinary(grayValue);
                values[i] = FP.A[i] + binaryValue * ((FP.B[i] - FP.A[i]) / (Math.Pow(2, BitCount[i]) - 1));
                values[i] = Math.Round(values[i] / FP.Eps[i]) * FP.Eps[i];
            }
            return values;
        }

        // Скрещивание
        public void Crossbreeding(Individ i1, Individ i2, int CB){
            for (int i = 0; i < FP.Dimensions; i++)
            {
                for (int j = 0; i < CB; j++) Chromosome[i][j] = i1.Chromosome[i][j];
                for (int j = CB; i < BitCount[i]; j++) Chromosome[i][j] = i2.Chromosome[i][j];
            }
        }
        public void Crossbreeding(Individ i1, Individ i2, int CB, int dim)
        {
            if (FP.Dimensions < dim) return;
            for (int j = 0; j < CB; j++) Chromosome[dim][j] = i1.Chromosome[dim][j];
            for (int j = CB; j < BitCount[dim]; j++) Chromosome[dim][j] = i2.Chromosome[dim][j];
            
        }

        //Мутация
        public void Mutate(int dim, int i)
        {
            if (FP.Dimensions < dim) return;
            if (i >= Chromosome[dim].Length || i < 0) return;
            Chromosome[dim][i] = Chromosome[dim][i] == 0 ? 1 : 0;
        }

        // Вспомогательные методы
        private int BinaryArrayToInt(int[] binary)
        {
            int value = 0;
            for (int i = 0; i < binary.Length; i++)
            {
                value += binary[i] << (binary.Length - 1 - i);
            }
            return value;
        }
        private int GrayToBinary(int gray)
        {
            int binary = gray;
            for (int mask = gray >> 1; mask != 0; mask >>= 1)
            {
                binary ^= mask;
            }
            return binary;
        }
        public string LogChrom(){
            string tmp = "";
            foreach(var chrom in Chromosome){
                tmp += string.Join("", chrom);
                tmp += " ";
            }
            return tmp;
        }
        public Individ Clone()
        {
            var clone = new Individ(this.FP);
            clone.Chromosome = (int[][])this.Chromosome.Clone();
            clone.Fitness = this.Fitness;
            return clone;
        }
    }

    public static class GrayCode
    {
        // Бинарный код → код Грея
        public static int[] BinaryToGray(int[] binary)
        {
            int[] gray = new int[binary.Length];
            gray[0] = binary[0];
            for (int i = 1; i < binary.Length; i++)
            {
                gray[i] = binary[i] ^ binary[i - 1]; // XOR
            }
            return gray;
        }

        // Код Грея → бинарный код
        public static int[] GrayToBinary(long grayValue)
        {
            string grayStr = Convert.ToString(grayValue, 2);
            int[] binary = new int[grayStr.Length];
            binary[0] = grayStr[0] - '0';
            for (int i = 1; i < grayStr.Length; i++)
            {
                binary[i] = (grayStr[i] - '0') ^ binary[i - 1]; // XOR
            }
            return binary;
        }

        static public int CalcCLenght(FuncParams FP, int i)
        {
            if (FP.Dimensions < i) return 0;    
            return (int)Math.Ceiling(Math.Log2((FP.B[i] - FP.A[i]) / FP.Eps[i] + 1));
        }
    }

    public class GeneticAlgorithmViewModel : ViewModelBase
    {
        private readonly Algorithm _algorithm;
        private readonly IOptimizationFunction _function;
        private CancellationTokenSource _cancellationTokenSource;

        public GraphViewModel ObjectiveFunctionPlot { get; }
        public GraphViewModel FitnessFunctionPlot { get; }
        public GraphViewModel AverageFitnessPlot { get; }

        // Команды для управления алгоритмом
        public ICommand RunSingleIterationCommand { get; }
        public ICommand RunToCompletionCommand { get; }
        public ICommand StopAlgorithmCommand { get; }
        public ICommand ResetAlgorithmCommand { get; }

        // Привязка параметров алгоритма
        public Algorithm.Settings AlgorithmSettings => _algorithm.Configuration;

        public GeneticAlgorithmViewModel(FuncParams functionParams, IOptimizationFunction function, string logName)
        {
            _algorithm = new Algorithm(functionParams, function, logName);

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
            _function = function;
        }

        private void InitializeFunctionPlots()
        {
            // Отрисовка целевой функции
            var points = new List<DataPoint>();
            for (double x = _algorithm.FunctionParameters.A[0];
                 x <= _algorithm.FunctionParameters.B[0];
                 x += 0.1)
            {
                double y = _function.Evaluate(x);
                points.Add(new DataPoint(x, y));
            }
            ObjectiveFunctionPlot.UpdateLineSeries(points, OxyColors.Blue, "Целевая функция");

            // Отрисовка функции приспособленности (пример)
            var fitnessPoints = points.Select(p => new DataPoint(p.X, 1 / (1 + p.Y)));
            FitnessFunctionPlot.UpdateLineSeries(fitnessPoints, OxyColors.Green, "Приспособленность");
        }

        private void OnGenerationCompleted(Algorithm.State state)
        {
            // Обновление точек популяции
            var populationPoints = state.Population.Pop
                .Select(ind => new DataPoint(ind.Decode()[0], _function.Evaluate(ind.Decode())));

            ObjectiveFunctionPlot.UpdateScatterSeries(
                populationPoints,
                OxyColors.Red,
                "Популяция");

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
