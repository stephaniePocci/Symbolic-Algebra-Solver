using System;
using System.Collections.ObjectModel;
using static Symbolic_Algebra_Solver.Parser.OperatorAssociativity;

namespace Symbolic_Algebra_Solver.Parser
{
    public static class Grammer
    {
        #region Keywords
        public static string Sin { get; } = "sin";
        public static string Cos { get; } = "cos";
        public static string Tan { get; } = "tan";
        public static string Arcsin { get; } = "arcsin";
        public static string Arccos { get; } = "arccos";
        public static string Arctan { get; } = "arctan";
        public static string Asin { get; } = "asin";
        public static string Acos { get; } = "acos";
        public static string Atan { get; } = "atan";
        public static string Sinh { get; } = "sinh";
        public static string Cosh { get; } = "cosh";
        public static string Tanh { get; } = "tanh";
        #endregion

        #region Operators
        public static string Negate { get; } = "~";
        public static string Minus { get; } = "-";
        public static string Plus { get; } = "+";
        public static string Multiply { get; } = "*";
        public static string Divide { get; } = "/";
        public static string Power { get; } = "^";
        public static string PowerAlias { get; } = "**";
        public static string LParenthesis { get; } = "(";
        public static string RParenthesis { get; } = ")";
        #endregion

        public static string Decimal { get; } = ".";

        /// <summary>
        /// Dictionary of keywords allowed in the input expression of.
        /// The key is the string value of the keyword and the value of the key
        /// is a <see cref="Keyword"/> containing extra metadata about the keyword.
        /// </summary>
        public static readonly ReadOnlyDictionary<string, Keyword> keywords = new(new Dictionary<string, Keyword>
        {
            // trig functions
            {Sin,    new Keyword(name: Sin,    argCount: 1, isOperator: true, inverse: Arcsin)},
            {Cos,    new Keyword(name: Cos,    argCount: 1, isOperator: true, inverse: Arccos)},
            {Tan,    new Keyword(name: Tan,    argCount: 1, isOperator: true, inverse: Arctan)},
            {Arcsin, new Keyword(name: Arcsin, argCount: 1, isOperator: true)},
            {Arccos, new Keyword(name: Arccos, argCount: 1, isOperator: true)},
            {Arctan, new Keyword(name: Arctan, argCount: 1, isOperator: true)},
            {Asin,   new Keyword(name: Asin,   argCount: 1, isOperator: true)},
            {Acos,   new Keyword(name: Acos,   argCount: 1, isOperator: true)},
            {Atan,   new Keyword(name: Atan,   argCount: 1, isOperator: true)},
            {Sinh,   new Keyword(name: Sinh,   argCount: 1, isOperator: true)},
            {Cosh,   new Keyword(name: Cosh,   argCount: 1, isOperator: true)},
            {Tanh,   new Keyword(name: Tanh,   argCount: 1, isOperator: true)},
        });

        public static readonly ReadOnlyDictionary<string, Operator> operators = new(new Dictionary<string, Operator>
        {
            {Negate,     new Operator(argCount: 1, precedence: 5, associativity: Right)},
            {Minus,      new Operator(argCount: 2, precedence: 2, associativity: Left)},
            {Plus,       new Operator(argCount: 2, precedence: 2, associativity: Left)},
            {Multiply,   new Operator(argCount: 2, precedence: 3, associativity: Left)},
            {Divide,     new Operator(argCount: 2, precedence: 3, associativity: Left)},
            {Power,      new Operator(argCount: 2, precedence: 4, associativity: Right)},
            {PowerAlias, new Operator(argCount: 2, precedence: 4, associativity: Right)},

            // parenthesis' are a special case, they have no arguments, precedence, or associativity.
            {LParenthesis, new Operator(argCount: 0)},
            {RParenthesis, new Operator(argCount: 0)},
        });
    }

    public class Keyword
    {
        public string Name { get; }
        public bool IsFunction { get; }
        public string? Inverse { get; }
        public uint ArgCount { get; }

        public Keyword(string name, uint argCount, bool isOperator, string? inverse = null) 
        {
            Name = name;
            IsFunction = isOperator;
            ArgCount = argCount;
            Inverse = inverse;
        }
    }

    public enum OperatorAssociativity
    {
        Left,
        Right,
        None,
    }

    public class Operator 
    {
        public uint ArgCount { get; }
        public uint Precedence { get; }
        public OperatorAssociativity Associativity { get; }

        /// <summary>
        /// Object that represent either a binary or unary operator. Or the special case of 0 arguments such as the left and right parenthesis.
        /// </summary>
        /// <param name="argCount">Number of arguments, can only be 0, 1, or 2.</param>
        /// <param name="precedence">Operator precedence.</param>
        /// <param name="associativity">Associativity of the operator, left or right.</param>
        /// <exception cref = "InvalidArgCountException"/>
        public Operator(uint argCount, uint precedence = 0, OperatorAssociativity associativity = None)
        {
            if (argCount > 2) 
            {
                throw new InvalidArgCountException("Operator can only have 0, 1, or 2 arguments only!");
            }
            else
            {
                ArgCount = argCount;
            }
            Precedence = precedence;
            Associativity = associativity;
        }
    }

    public class InvalidArgCountException : Exception
    {
        public InvalidArgCountException(string message) : base(message) { }
    }
}
