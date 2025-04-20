using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GeneticAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for FunctionChart.xaml
    /// </summary>
    public partial class FunctionChart : UserControl
    {
        public FunctionChart()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Базовый класс для всех ViewModel, реализующий INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывает событие PropertyChanged
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и уведомляет об изменении свойства
        /// </summary>
        /// <typeparam name="T">Тип свойства</typeparam>
        /// <param name="field">Ссылка на поле</param>
        /// <param name="value">Новое значение</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>True если значение изменилось</returns>
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Уведомляет об изменении нескольких свойств
        /// </summary>
        /// <param name="propertyNames">Имена свойств</param>
        protected void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }
    }

    // GraphVM
    public class GraphViewModel : ViewModelBase
    {
        private PlotModel _plotModel;
        private string? _title;

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => Set(ref _plotModel, value);
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public GraphViewModel(string title)
        {
            PlotModel = new PlotModel { Title = title };
            InitializeAxes();
        }

        private void InitializeAxes()
        {
            // Ось X
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot
            });

            // Ось Y
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot
            });
        }
        // Метод для добавления/обновления линии графика
        public void UpdateLineSeries(IEnumerable<DataPoint> points, OxyColor color, string title = "")
        {
            // Удаляем предыдущую линию, если есть
            var existingLine = PlotModel.Series
                .OfType<LineSeries>()
                .FirstOrDefault(s => s.Title == title);

            if (existingLine != null)
            {
                PlotModel.Series.Remove(existingLine);
            }

            var lineSeries = new LineSeries
            {
                Title = title,
                Color = color,
                StrokeThickness = 2,
                MarkerType = MarkerType.None
            };

            foreach (var point in points)
            {
                lineSeries.Points.Add(point);
            }

            PlotModel.Series.Add(lineSeries);
            PlotModel.InvalidatePlot(true);
        }

        // Метод для добавления/обновления точек
        public void UpdateScatterSeries(IEnumerable<DataPoint> points, OxyColor color, string title = "")
        {
            // Удаляем предыдущие точки с таким же title
            var existingScatter = PlotModel.Series
                .OfType<ScatterSeries>()
                .FirstOrDefault(s => s.Title == title);

            if (existingScatter != null)
            {
                PlotModel.Series.Remove(existingScatter);
            }

            var scatterSeries = new ScatterSeries
            {
                Title = title,
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = color,
                MarkerStroke = OxyColors.Black,
                MarkerStrokeThickness = 1
            };

            foreach (var point in points)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            Debug.WriteLine($"-= Scatter points count: {scatterSeries.Points.Count()}");

            foreach (var point in scatterSeries.Points)
            {
                Debug.WriteLine($"{point.X} : {point.Y}");
            }
            PlotModel.Series.Add(scatterSeries);
            PlotModel.InvalidatePlot(true);
        }

        // Метод для очистки всех серий
        public void ClearAllSeries()
        {
            PlotModel.Series.Clear();
            PlotModel.InvalidatePlot(true);
        }
    }
}
