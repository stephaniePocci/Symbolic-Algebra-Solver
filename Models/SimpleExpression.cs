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
                Console.WriteLine("Attempting to parse expression...");
                var parsedExpression = Infix.ParseOrThrow(InputExpression);
                SimplifiedExpression = LaTeX.Format(parsedExpression);
                Console.WriteLine("Simplified expression: " + SimplifiedExpression);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                SimplifiedExpression = e.Message;
            }
        }

        public void SympySimplify()
        {
            try
            {
                using (Py.GIL())
                {
                    Console.WriteLine("Attempting to import sympy...");
                    dynamic sympy = Py.Import("sympy");
                    Console.WriteLine("Sympy import successful...");

                    try
                    {
                        PyObject inputExpression = new PyString(InputExpression);
                        dynamic parsedExpression = sympy.sympify(inputExpression);
                        dynamic factoredExpression = sympy.factor(parsedExpression);
                        dynamic latexExpression = sympy.latex(factoredExpression);

                        SimplifiedExpression = latexExpression.ToString();
                        Console.WriteLine("Sympy simplified expression: " + SimplifiedExpression);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Sympy error: " + e.Message);
                        SimplifiedExpression = e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error: " + ex.Message);
                SimplifiedExpression = $"An error occurred: {ex.Message}";
            }
        }

    }
}