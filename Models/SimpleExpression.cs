using MathNet.Symbolics;
using Python.Runtime;
using Symbolic_Algebra_Solver.Utils;
using System;
using System.ComponentModel;

namespace Symbolic_Algebra_Solver.Models
{
    public class SimpleExpression : Observable
    {
        public string InputExpression { get; set; } = string.Empty;

        private string _SimplifiedExpression = string.Empty;
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
                var parsedExpression = Infix.ParseOrThrow(InputExpression);
                SimplifiedExpression = LaTeX.Format(parsedExpression);
            }
            catch (Exception e)
            {
                SimplifiedExpression = e.Message;
            }
        }

        public void SympySimplify()
        {
            using (Py.GIL())
            {
                dynamic sympy = Py.Import("sympy");
                try
                {
                    PyObject inputExpression = new PyString(InputExpression);
                    dynamic parsedExpression = sympy.sympify(inputExpression);
                    dynamic factoredExpression = sympy.factor(parsedExpression);
                    dynamic latexExpression = sympy.latex(factoredExpression);

                    SimplifiedExpression = latexExpression.ToString();
                }
                catch (Exception e)
                {
                    SimplifiedExpression = e.Message;
                }
            }
        }
    }
}