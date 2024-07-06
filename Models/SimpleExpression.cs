using Python.Runtime;
using Symbolic_Algebra_Solver.Utils;
using Sympy;

namespace Symbolic_Algebra_Solver.Models
{
    public class SimpleExpression : Observable
    {
        public string InputExpression { get; set; } = string.Empty;

        private string _OutputExpression = string.Empty;
        public string OutputExpression
        {
            get { return _OutputExpression; }
            set
            {
                _OutputExpression = value;
                OnPropertyChanged();
            }
        }

        public void Simplify()
        {
            using (Py.GIL())
            {
                OutputExpression = SympyCS.simplify(InputExpression);
            }
        }

        public void Factor()
        {
            using (Py.GIL())
            {
                OutputExpression = SympyCS.factor(InputExpression);
            }
        }
    }
}