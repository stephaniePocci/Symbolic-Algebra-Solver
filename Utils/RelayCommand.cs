using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Symbolic_Algebra_Solver.Utils
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private Action<Object> ExecuteHandler { get; set; }
        private Predicate<Object> CanExecuteHandler { get; set; }

        public RelayCommand(Action<Object> executeMethod, Predicate<Object> canExecuteMethod)
        {
            ExecuteHandler = executeMethod;
            CanExecuteHandler = canExecuteMethod;
        }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteHandler(parameter);
        }

        public void Execute(object? parameter)
        {
            ExecuteHandler(parameter);
        }
    }
}
