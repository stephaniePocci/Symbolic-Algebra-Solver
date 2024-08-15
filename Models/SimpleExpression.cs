using Prism.Commands;
using Python.Runtime;
using Symbolic_Algebra_Solver.Parsing;
using Symbolic_Algebra_Solver.Utils;
using Sympy;
using System.Text;
using System.Windows;

namespace Symbolic_Algebra_Solver.Models
{
    public class SimpleExpression : Observable
    {
        private Parser _parser;
        private StringBuilder _builder;
        private AbstractSyntaxTree? _expressionTree;
        private bool _parseStatus; // true parse succeeded, else parsed input is invalid

        public SimpleExpression()
        {
            _parser = new Parser();
            _builder = new StringBuilder();

            SimplifyCommand = new DelegateCommand(SimplifyExpression, CanSimplify);
            FactorCommand = new DelegateCommand(FactorExpression, CanFactor);
            LogCommand = new DelegateCommand(LogExpression, CanLog);
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
                if (value.Length > 0) 
                {
                    if (_parser.TryParse(value, out var result, out var status))
                    {
                        result.ToLatex(_builder);
                        LatexInputExpression = _builder.ToString();
                        _expressionTree = result;

                        _builder.Clear();
                        _parseStatus = true;
                    }
                    else
                    {
                        MessageBox.Show(status);
                        _parseStatus = false;
                    }
                }
                else
                {
                    LatexInputExpression = string.Empty;
                    _parseStatus = false;
                }

                OnPropertyChanged();
                SimplifyCommand.RaiseCanExecuteChanged();
                FactorCommand.RaiseCanExecuteChanged();
                LogCommand.RaiseCanExecuteChanged();
            }
        }

        private string _OutputExpression = string.Empty;
        public string OutputExpression
        {
            get { return _OutputExpression; }
            set
            {
                foreach(char c in value)
                {
                    if (Grammer.IsSpecialSymbol(c, out var symbolValue))
                    {
                        _builder.Append(symbolValue);
                    }
                    else
                    {
                        _builder.Append(c);
                    }
                }

                _OutputExpression = _builder.ToString();
                _builder.Clear();
                OnPropertyChanged();
            }
        }

        #region ICommand Members

        public DelegateCommand SimplifyCommand { get; set; }

        private bool CanSimplify()
        {
            return _parseStatus;
        }
        private void SimplifyExpression()
        {
            StringBuilder strBuilder = new();
            try
            {
                _expressionTree?.ToString(strBuilder);
                MessageBox.Show(strBuilder.ToString());
            }
            catch (EmptyOperandException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.simplify(strBuilder.ToString());
            }
        }

        public DelegateCommand FactorCommand { get; set; }

        private bool CanFactor()
        {
            return _parseStatus;
        }
        private void FactorExpression()
        {
            StringBuilder strBuilder = new();
            try
            {
                _expressionTree?.ToString(strBuilder);
                MessageBox.Show(strBuilder.ToString());
            }
            catch (EmptyOperandException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.factor(strBuilder.ToString());
            }
        }

        public DelegateCommand LogCommand { get; set; }

        private bool CanLog()
        {
            return _parseStatus;
        }

        private void LogExpression()
        {
            StringBuilder strBuilder = new();
            try
            {
                _expressionTree?.ToString(strBuilder);
                MessageBox.Show(strBuilder.ToString());
            }
            catch (EmptyOperandException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            using (Py.GIL())
            {
                OutputExpression = SympyCS.log(strBuilder.ToString());
            }
        }

        #endregion
    }
}