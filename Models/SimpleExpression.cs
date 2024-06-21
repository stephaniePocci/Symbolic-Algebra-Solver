using MathNet.Symbolics;
using Symbolic_Algebra_Solver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Symbolic_Algebra_Solver.Models
{
    public class SimpleExpression:Observable
    {
        public string InputExpression { get; set; } = String.Empty;

        private string _SimplifiedExpression = String.Empty;
        public string SimplifiedExpression
        {
            get { return _SimplifiedExpression; }
            set
            {
                _SimplifiedExpression = value;
                OnPropertyChanged();
            }
        }

        public void Simplify()
        {
            try
            {
                SimplifiedExpression = SymbolicExpression.Parse(InputExpression).ToLaTeX();
            }
            catch (Exception e)
            {
                SimplifiedExpression = SimplifiedExpression;
            }
        }
    }
}
