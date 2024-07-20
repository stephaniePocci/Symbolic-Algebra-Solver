using Prism.Commands;
using Python.Runtime;
using Symbolic_Algebra_Solver.Utils;
using Sympy;
using System.Windows;

namespace Symbolic_Algebra_Solver.Models
{
    public class SimpleExpression : Observable
    {
        public SimpleExpression() 
        {
            SimplifyCommand = new DelegateCommand(SimplifyExpression, CanSimplify);
            FactorCommand   = new DelegateCommand(FactorExpression, CanFactor);
        }

        // parsed latex string of the input expression
        private string _LatexInputExpression = string.Empty;
        public string LatexInputExpression
        {
            get { return _LatexInputExpression; }
            set
            {
                _LatexInputExpression = value;
                OnPropertyChanged();
            }
        }

        private string _InputExpression = string.Empty;
        public string InputExpression
        {
            get { return _InputExpression; }
            set 
            {
                _InputExpression = value;



                OnPropertyChanged();
                SimplifyCommand.RaiseCanExecuteChanged();
            }
        }

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

        #region Expression Methods

        public void Simplify()
        {
            if (!CheckParenthesis(_InputExpression))
            {
                MessageBox.Show("Missmatching parenthesis!");
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.simplify(_InputExpression);
            }
        }

        public void Factor()
        {
            if (!CheckParenthesis(_InputExpression))
            {
                MessageBox.Show("Missmatching parenthesis!");
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.factor(_InputExpression);
            }
        }

        #endregion

        #region ICommand Members

        public DelegateCommand SimplifyCommand { get; set; }

        private bool CanSimplify()
        {
            return CheckParenthesis(_InputExpression) && _InputExpression.Length != 0;
        }
        private void SimplifyExpression()
        {
            Simplify();
        }

        public DelegateCommand FactorCommand { get; set; }

        private bool CanFactor()
        {
            return true;
        }
        private void FactorExpression()
        {
            Factor();
        }

        #endregion
    }
}