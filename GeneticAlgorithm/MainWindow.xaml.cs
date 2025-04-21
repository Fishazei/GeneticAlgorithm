using GeneticAlgorithm.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeneticAlgorithm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Создание страниц
            var TaskPage1 = new TaskPage1_V() { DataContext = new Page1VM() };
            var TaskPage2 = new TaskPage2_V() { DataContext = new Page2VM() };

            // Обработчик изменения вкладки
            MainTabControl.SelectionChanged += (sender, e) =>
            {
                if (MainTabControl.SelectedItem == TabPage1) MainFrame.Content = TaskPage1;
                else if (MainTabControl.SelectedItem == TabPage2) MainFrame.Content = TaskPage2;
            };

            // Первая страница по умолчанию
            MainFrame.Content = TaskPage1;
        }
    }
}