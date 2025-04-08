using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string _title;

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
            Title = title;
            PlotModel = new PlotModel { Title = title };

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 2,
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 2,
            });
        }

        public void AddPointsOnXAxis(IEnumerable<double> xValues, OxyColor color, string title = "")
        {
            var points = xValues.Select(x => new DataPoint(x, 0)); // Y=0 для точек на оси OX

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

            PlotModel.Series.Add(scatterSeries);
            PlotModel.InvalidatePlot(true);
        }

        public void UpdatePlot(IEnumerable<DataPoint> points, OxyColor color, string seriesTitle = "")
        {
            PlotModel.Series.Clear();

            var series = new LineSeries
            {
                Title = seriesTitle,
                Color = color,
                StrokeThickness = 2,
                ItemsSource = points
            };

            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }
    }
}
