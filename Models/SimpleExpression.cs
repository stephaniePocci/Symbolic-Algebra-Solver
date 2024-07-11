using Python.Runtime;
using Symbolic_Algebra_Solver.Utils;
using Sympy;
using System.Windows;

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


        /// <summary>
        /// Check if expression has matching opening and closing parenthesis.
        /// </summary>
        static private bool CheckParenthesis(string input)
        {
            int opening = 0, rawOpening = 0;
            int closing = 0, rawClosing = 0;
            foreach (char c in input)
            {
                if (c == '(')
                {
                    opening++;
                    rawOpening++;
                }
                else if (c == ')')
                {
                    if (closing < opening)
                    {
                        closing++;
                    }
                    rawClosing++;
                }
            }

            return (opening == closing) && (rawOpening == rawClosing);
        }

        public void Simplify()
        {
            if (!CheckParenthesis(InputExpression))
            {
                MessageBox.Show("Missmatching parenthesis!");
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.simplify(InputExpression);
            }
        }

        public void Factor()
        {
            if (!CheckParenthesis(InputExpression))
            {
                MessageBox.Show("Missmatching parenthesis!");
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.factor(InputExpression);
            }
        }
    }
}