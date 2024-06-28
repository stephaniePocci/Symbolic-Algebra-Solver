using MathNet.Symbolics;
using Python.Runtime;
using Symbolic_Algebra_Solver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Microsoft.FSharp.Core.ByRefKinds;

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
                SimplifiedExpression = e.Message;
            }
        }

        public void SympySimplify()
        {

            using (Py.GIL())
            {
                dynamic mod = Py.Import("sympy");
                try
                {
                    //var res = mod.latex(mod.factor(InputExpression));
                    //SimplifiedExpression = res;
                    PyString str = new PyString(InputExpression);
                    var res = mod.InvokeMethod("factor", new PyObject[] { str });
                    var res2 = mod.InvokeMethod("latex", new PyObject[] { res });
                    if (res2 != null) { SimplifiedExpression = res2.ToString(); }
                }
                catch(Exception e)
                {
                    SimplifiedExpression = e.Message;
                }

            }

            
        }
    }
}
