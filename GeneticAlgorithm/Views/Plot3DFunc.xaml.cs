using GeneticAlgorithm.Functions;
using HelixToolkit.Wpf;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GeneticAlgorithm.Views
{
    /// <summary>
    /// Interaction logic for Plot3DFunc.xaml
    /// </summary>
    public partial class Plot3DFunc : UserControl
    {
        public Plot3DFunc()
        {
            InitializeComponent();
        }
    }

    public class Graph3DViewModel : ViewModelBase
    {
        private Model3DGroup _modelGroup;
        private readonly IOptimizationFunction _function;
        private readonly bool _fitnessFlag;

        public Model3DGroup ModelGroup
        {
            get => _modelGroup;
            set => Set(ref _modelGroup, value);
        }

        public Graph3DViewModel(IOptimizationFunction function, bool fitness = false)
        {
            ModelGroup = new Model3DGroup();
            _function = function;  // Передаем функцию извне
            _fitnessFlag = fitness;
            InitializeAxes();
        }

        private void InitializeAxes()
        {
            var grids = new GridLinesVisual3D
            {
                Width = 10,
                Length = 10,
                MinorDistance = 1,
                MajorDistance = 1,
                Thickness = 0.01,
                Center = new Point3D(0, 0, 0)
            };
            ModelGroup.Children.Add(grids.Model);
            grids = new GridLinesVisual3D
            {
                Width = 16,
                Length = 10,
                MinorDistance = 1,
                MajorDistance = 2,
                Thickness = 0.01,
                Center = new Point3D(0, -5, 8),
                Normal = new Vector3D(0, 1, 0) // Перпендикулярно оси Y (XZ-плоскость)
            };
            ModelGroup.Children.Add(grids.Model);
        }
        public void AddPoints(IEnumerable<double[]> points, Brush color, double size = 0.1)
        {
            var builder = new MeshBuilder();

            foreach (var point in points)
            {
                //point содержит [x, y]
                double x = point[0];
                double y = point[1];
                double z = _fitnessFlag ? _function.Fitness(point) * 0.05 : _function.Evaluate(point) * 0.05;

                builder.AddSphere(new Point3D(x, y, z), size);
            }

            var mesh = builder.ToMesh();
            var material = MaterialHelper.CreateMaterial(color);
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material
            };

            // Добавляем Tag для последующего удаления
            model.SetValue(FrameworkElement.TagProperty, "Points");
            ModelGroup.Children.Add(model);
        }
        public void ClearPoints()
        {
            var oldPoints = ModelGroup.Children
                .OfType<GeometryModel3D>()
                .Where(m => m.GetValue(FrameworkElement.TagProperty)?.ToString() == "Points")
                .ToList();

            foreach (var point in oldPoints)
                ModelGroup.Children.Remove(point);
        }
        public void UpdatePopulation(IEnumerable<double[]> population)
        {
            ClearPoints();
            AddPoints(population, Brushes.Red);
        }
        public void UpdateFunctionSurface(double xMin, double xMax, double yMin, double yMax)
        {
            // Удаляем старую поверхность
            var oldSurface = ModelGroup.Children
                .OfType<GeometryModel3D>()
                .FirstOrDefault(m => m.GetValue(FrameworkElement.TagProperty)?.ToString() == "Surface");
            if (oldSurface != null)
                ModelGroup.Children.Remove(oldSurface);

            // Генерируем точки поверхности
            int resolution = 50;
            var points = new Point3D[resolution, resolution];
            for (int i = 0; i < resolution; i++)
            {
                double x = xMin + (xMax - xMin) * i / (resolution - 1);
                for (int j = 0; j < resolution; j++)
                {
                    double y = yMin + (yMax - yMin) * j / (resolution - 1);
                    double z = _fitnessFlag ? _function.Fitness(x, y) * 0.05 : _function.Evaluate(x, y) * 0.05;  // Вызов ZYXFunction.Evaluate()
                    points[i, j] = new Point3D(x, y, z);
                }
            }

            // Строим mesh поверхности
            var builder = new MeshBuilder();
            for (int i = 0; i < resolution - 1; i++)
            {
                for (int j = 0; j < resolution - 1; j++)
                {
                    builder.AddQuad(
                        points[i, j],
                        points[i + 1, j],
                        points[i + 1, j + 1],
                        points[i, j + 1]);
                }
            }

            var mesh = builder.ToMesh();
            var material = MaterialHelper.CreateMaterial(Brushes.Blue, specularPower: 100);
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = MaterialHelper.CreateMaterial(Brushes.LightBlue)
            };

            model.SetValue(FrameworkElement.TagProperty, "Surface");
            ModelGroup.Children.Add(model);
        }
    }
}
