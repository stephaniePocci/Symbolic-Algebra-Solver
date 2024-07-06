using Python.Runtime;

namespace Sympy
{
    public partial class SympyCS
    {
        private static readonly PyObject sympy;
        private static readonly PyObject parseExpr;

        static SympyCS()
        {
            sympy = Py.Import("sympy");
            parseExpr = Py.Import("parseExpr");
        }

        private static PyObject parse_expr(string input)
        {
            var pyargs = new PyString(input);
            return parseExpr.InvokeMethod("parse_expression", new PyObject[] { pyargs });
        }

        public static string latex(PyObject input)
        {
            PyObject outExpr = sympy.InvokeMethod("latex", new PyObject[] { input }); ;
            return outExpr.ToString()!;
        }

        public static string simplify(string input)
        {
            var pyargs = parse_expr(input);
            PyObject outExpr = sympy.InvokeMethod("simplify", pyargs);
            return latex(outExpr);
        }

        public static string factor(string input)
        {
            var pyargs = parse_expr(input);
            PyObject outExpr = sympy.InvokeMethod("factor", new PyObject[] { pyargs });
            return latex(outExpr);
        } 
    }
}
