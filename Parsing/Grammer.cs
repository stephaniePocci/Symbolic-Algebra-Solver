using System.Collections.ObjectModel;
using static Symbolic_Algebra_Solver.Parsing.OperatorAssociativity;
using static Symbolic_Algebra_Solver.Parsing.OperatorEnum;
using static Symbolic_Algebra_Solver.Parsing.OperatorArity;
using static Symbolic_Algebra_Solver.Parsing.KeywordEnum;

namespace Symbolic_Algebra_Solver.Parsing
{
    #region Enums
    public enum OperatorAssociativity
    {
        Left,
        None,
        Right,
    }

    public enum OperatorArity
    {
        Binary,
        Unary,
        None,
    }

    public enum OperatorEnum
    {
        OperatorNone,
        Negation,
        Plus,
        Minus,
        Multiply,
        Divide,
        Power,
        FunctionPower, // special enum for power operators that appear immediately after a function keyword, Ex: sin^(2)(x)
        FunctionCall,
        OpeningParenthesis,
        ClosingParenthesis,
        ImplicitFunctionCall,
    }

    public enum KeywordEnum
    {
        Sin,
        Cos,
        Tan,
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

            { "pi",  new Keyword(id: Pi, isFunction: false) },
        });

        private static readonly HashSet<string> _operators = new HashSet<string>()
        {
            "-",
            "+",
            "*",
            "^",
            "/",
            "(",
            ")",
        };

        public static readonly ReadOnlyDictionary<OperatorEnum, Operator> Operators = new (new Dictionary<OperatorEnum, Operator>
        {
            // Zero precedence has special meaning in that they are effectivly ignored when other operators are pushed onto the stack.
            // In otherwords they will never be popped off when another operator is pushed onto the stack.

            { OperatorNone,       new Operator(arity: OperatorArity.None, precedence: 0, associativity: OperatorAssociativity.None) },
            { OpeningParenthesis, new Operator(arity: OperatorArity.None, precedence: 0, associativity: OperatorAssociativity.None) },
            { ClosingParenthesis, new Operator(arity: OperatorArity.None, precedence: 0, associativity: OperatorAssociativity.None) },

            { Negation,      new Operator(arity: Unary,  precedence: 6, associativity: Right) },
            { Plus,          new Operator(arity: Binary, precedence: 1, associativity: Left) },
            { Minus,         new Operator(arity: Binary, precedence: 1, associativity: Left) },
            { Multiply,      new Operator(arity: Binary, precedence: 2, associativity: Left) },
            { Divide,        new Operator(arity: Binary, precedence: 2, associativity: Left) },
            { Power,         new Operator(arity: Binary, precedence: 3, associativity: Right) },
            { FunctionPower, new Operator(arity: Binary, precedence: 3, associativity: Right) },
            { FunctionCall,  new Operator(arity: Binary, precedence: 0, associativity: Left) },
            { ImplicitFunctionCall,  new Operator(arity: Binary, precedence: 0, associativity: Left) },

        });

        #region Class Methods

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

        public static bool IsOperator(string matchOperator)
        {
            return _operators.Contains(matchOperator);
        }

        public static bool IsOperator(char matchOperator)
        {
            return _operators.Contains(matchOperator.ToString());
        }

        #endregion
    }

    public class Operator 
    {
        public readonly OperatorArity Arity;
        public readonly int Precedence;
        public readonly OperatorAssociativity Associativity;

        /// <summary>
        /// Object that represent either a binary or unary operator.
        /// </summary>
        /// <param name="arity">Arity of the operator which can be unary, binary, or none.</param>
        /// <param name="precedence">Operator precedence.</param>
        /// <param name="associativity">Associativity of the operator, left, right, or none.</param>
        public Operator(OperatorArity arity, int precedence, OperatorAssociativity associativity)
        {
            Arity = arity;
            Precedence = precedence;
            Associativity = associativity;
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
