using Symbolic_Algebra_Solver.Models;
using Symbolic_Algebra_Solver.Utils;
using System.Windows.Input;

namespace Symbolic_Algebra_Solver.ViewModels
{
    public class MainViewModel
    {
        public SimpleExpression Expression { get; set; }
        
        public MainViewModel()
        {
            Expression = new SimpleExpression();
            SimplifyCommand = new RelayCommand(SimplifyExpression, CanSimplify);
            FactorCommand = new RelayCommand(FactorExpression, CanFactor);
        }

        public ICommand SimplifyCommand { get; set; }

        private bool CanSimplify(object? parameter)
        {
            return true;
        }
        private void SimplifyExpression(object? parameter)
        {
            Expression.Simplify();
        }

        public ICommand FactorCommand { get; set; }

        private bool CanFactor(object? parameter)
        {
            return true;
        }
        private void FactorExpression(object? parameter)
        {
            Expression.Factor();
        }
    }
}
