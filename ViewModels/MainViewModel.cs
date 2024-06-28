using Symbolic_Algebra_Solver.Models;
using Symbolic_Algebra_Solver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Symbolic_Algebra_Solver.ViewModels
{
    public class MainViewModel
    {
        public SimpleExpression Expression { get; set; }
        public ICommand SimplifyExpressionCommand { get; set; }

        public MainViewModel()
        {
            Expression = new SimpleExpression();
            SimplifyExpressionCommand = new RelayCommand(SimplifyExpression, CanSimplifyExpression);
        }

        private bool CanSimplifyExpression(object? parameter)
        {
            return true;
        }
        private void SimplifyExpression(object? parameter)
        {
            Expression.SympySimplify();
        }
    }
}
