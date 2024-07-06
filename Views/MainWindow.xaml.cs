using Symbolic_Algebra_Solver.ViewModels;
using System.Windows;

namespace Symbolic_Algebra_Solver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel MainView = new MainViewModel();
            this.DataContext = MainView;
        }
    }
}