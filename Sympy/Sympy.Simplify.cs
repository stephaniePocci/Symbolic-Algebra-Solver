using Python.Runtime;
using Symbolic_Algebra_Solver.Parsing;

namespace Sympy
{
    public partial class SympyCS
    {
        private static readonly dynamic sympyCore;
        private static readonly dynamic sympyParser;

        private static readonly dynamic[] transformation_functions;

        static SympyCS()
        {
            sympyCore =  Py.Import("sympy");
            sympyParser = Py.Import("sympy.parsing.sympy_parser");
            transformation_functions = 
            [
                sympyParser.lambda_notation, 
                sympyParser.auto_symbol, 
                sympyParser.repeated_decimals, 
                sympyParser.auto_number,
                sympyParser.factorial_notation, 
                sympyParser.implicit_multiplication_application, 
                sympyParser.convert_xor
            ];
        }

        public static PyObject parse_expr(string input)
        {
            PyObject res = sympyParser.parse_expr(input, transformations: transformation_functions, evaluate: false);
            return res;
        }

        public static string latex(PyObject input)
        {
            PyObject outExpr = sympyCore.latex(input, inv_trig_style: "full");
            return outExpr.ToString()!;
        }

        public static string simplify(string input)
        {
            PyObject outExpr = sympyCore.simplify(parse_expr(input));
            return latex(outExpr);
        }

        public static string factor(string input)
        {
            PyObject outExpr = sympyCore.factor(parse_expr(input));
            return latex(outExpr);
        } 

        public static string log(string input)
        {
            PyObject parsedExpr = parse_expr(input); // Parse the input expression
            PyObject outExpr = sympyCore.log(parsedExpr); // Calculate natural logarithm

            // Evaluate the result to a numerical value
            PyObject evaluatedExpr = sympyCore.N(outExpr);
            return evaluatedExpr.ToString();
        }


    }
}
