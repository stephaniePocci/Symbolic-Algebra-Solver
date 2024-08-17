using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Symbolic_Algebra_Solver.Parsing
{
    public static class Grammer
    {
        public static readonly ReadOnlyDictionary<string, Keyword> Keywords = new(new Dictionary<string, Keyword>
        {
            { "sin", new Keyword(isFunction: true, latex: "\\sin", raw: "sin") },
            { "cos", new Keyword(isFunction: true, latex: "\\cos", raw: "cos") },
            { "tan", new Keyword(isFunction: true, latex: "\\tan", raw: "tan") },
            { "arcsin", new Keyword(isFunction: true, latex: "\\arcsin", raw: "asin") },
            { "arccos", new Keyword(isFunction: true, latex: "\\arccos", raw: "acos") },
            { "arctan", new Keyword(isFunction: true, latex: "\\arctan", raw: "atan") },

            { "alpha",  new Keyword(isFunction: false, latex: "\\alpha", raw: "α") },
            { "beta",   new Keyword(isFunction: false, latex: "\\beta",  raw: "ß") },
            { "gamma",  new Keyword(isFunction: false, latex: "\\gamma", raw: "γ") },

            { "theta", new Keyword(isFunction: false, latex: "\\theta", raw: "θ") },

            { "pi",  new Keyword(isFunction: false, latex: "\\pi", raw: "π") },
        });

        // for now we assume operators are only single characters
        private static readonly HashSet<string> _operators = new()
        {
            "-",
            "+",
            "*",
            "^",
            "/",
        };

        public static readonly ReadOnlyDictionary<char, SpecialSymbol> SpecialSymbols = new(new Dictionary<char, SpecialSymbol>
        {
            { '\u03B1', new SpecialSymbol(latex: "\\alpha", raw: 'α') },
            { '\u03B2', new SpecialSymbol(latex: "\\beta",  raw: 'ß') },
            { '\u03B3', new SpecialSymbol(latex: "\\gamma", raw: 'γ') },
            
            { '\u03B8', new SpecialSymbol(latex: "\\theta", raw: 'θ') },

            { '\u03C0', new SpecialSymbol(latex: "\\pi",    raw: 'π') },
        });

        public static bool IsSpecialSymbol(char matchChar, [MaybeNullWhen(false)] out SpecialSymbol symbolValue)
        {
            if (SpecialSymbols.TryGetValue(matchChar, out var value))
            {
                symbolValue = value;

                return true;
            }
            else
            {
                symbolValue = null;
                return false;
            }
        }

        public static bool IsKeyword(string matchKeyword)
        {
            return Keywords.ContainsKey(matchKeyword);
        }

        public static bool IsKeywordFunction(string matchKeyword)
        {
            if (Keywords.TryGetValue(matchKeyword, out Keyword? value))
            {
                return value.IsFunction;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if input string is a operator.
        /// </summary>
        /// <param name="match">String to check</param>
        /// <returns>True ifstring is a operator, else false.</returns>
        public static bool IsOperator(string match)
        {
            return _operators.Contains(match);
        }

        /// <summary>
        /// Check if input char is a operator.
        /// </summary>
        /// <param name="match">Char to match</param>
        /// <returns>True if charis a operator, else false.</returns>
        public static bool IsOperator(char match)
        {
            return _operators.Contains(match.ToString());
        }
    }

    public class Keyword
    {
        public readonly bool IsFunction;
        public readonly string Latex;
        public readonly string Raw;

        public Keyword(bool isFunction, string latex, string raw)
        {
            IsFunction = isFunction;
            Latex = latex;
            Raw = raw;
        }
    }

    public class SpecialSymbol
    {
        public readonly string Latex;
        public readonly char Raw;

        public SpecialSymbol(string latex, char raw)
        {
            Latex = latex;
            Raw = raw;
        }
    }
}
