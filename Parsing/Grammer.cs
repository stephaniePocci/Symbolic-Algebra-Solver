using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using static Symbolic_Algebra_Solver.Parsing.KeywordEnum;

namespace Symbolic_Algebra_Solver.Parsing
{
    #region Enums

    public enum KeywordEnum
    {
        Sin,
        Cos,
        Tan,
        Arcsin,
        Arccos,
        Arctan,

        Alpha,
        Beta,
        Gamma,

        Pi,
    }

    #endregion

    public static class Grammer
    {
        public static readonly ReadOnlyDictionary<string, Keyword> Keywords = new(new Dictionary<string, Keyword>
        {
            { "sin", new Keyword(id: Sin, isFunction: true) },
            { "cos", new Keyword(id: Cos, isFunction: true) },
            { "tan", new Keyword(id: Tan, isFunction: true) },
            { "arcsin", new Keyword(id: Arcsin, isFunction: true) },
            { "arccos", new Keyword(id: Arccos, isFunction: true) },
            { "arctan", new Keyword(id: Arctan, isFunction: true) },

            { "alpha",  new Keyword(id: Alpha, isFunction: false) },
            { "beta",   new Keyword(id: Beta, isFunction: false) },
            { "gamma",  new Keyword(id: Gamma, isFunction: false) },

            { "pi",  new Keyword(id: Pi, isFunction: false) },
        });

        // for now we assume operators are only single characters
        private static readonly HashSet<string> _operators = new HashSet<string>()
        {
            "-",
            "+",
            "*",
            "^",
            "/",
        };

        private static readonly Dictionary<char, string> _specialSymbols = new Dictionary<char, string>()
        {
            { '\u03B1', "\\alpha" }, // α
            { '\u03B2', "\\beta" },  // ß
            { '\u03B3', "\\gamma" }, // γ
            { '\u03C0', "\\pi" }     // π
        };

        public static bool IsSpecialSymbol(char matchChar, [MaybeNullWhen(false)] out string symbolValue)
        {
            if (_specialSymbols.TryGetValue(matchChar, out string? value))
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
        public readonly KeywordEnum Id;
        public readonly bool IsFunction;

        public Keyword(KeywordEnum id, bool isFunction)
        {
            Id = id;
            IsFunction = isFunction;
        }
    }
}
