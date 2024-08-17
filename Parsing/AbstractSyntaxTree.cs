using System.Text;

namespace Symbolic_Algebra_Solver.Parsing
{
    public enum NodeType
    {
        Numeric,
        NonNumeric, // symbols, keywords, functions
        Operator, 
    }

    public abstract class AbstractSyntaxTree 
    {
        public readonly NodeType Type;

        public AbstractSyntaxTree(NodeType type)
        {
            Type = type;
        }

        public abstract void ToLatex(StringBuilder builder);
        public abstract void ToString(StringBuilder builder);
    }

    public abstract class UnaryOperatorNode : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree Child;

        public UnaryOperatorNode(AbstractSyntaxTree child) : base(NodeType.Operator)
        {
            Child = child;
        }
    }

    public abstract class BinaryOperatorNode : AbstractSyntaxTree 
    {
        public readonly AbstractSyntaxTree Left;
        public readonly AbstractSyntaxTree Right;

        public BinaryOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(NodeType.Operator)
        {
            Left = left;
            Right = right;
        }
    }

    #region Operators

    public class NegationOperatorNode : UnaryOperatorNode
    {
        public NegationOperatorNode(AbstractSyntaxTree child) : base(child) { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append('-');
            Child.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('-');
            Child.ToString(builder);
        }
    }

    public class PlusOperatorNode : BinaryOperatorNode
    {
        public PlusOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            Left.ToLatex(builder);
            builder.Append('+');
            Right.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('+');
            Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class MinusOperatorNode : BinaryOperatorNode
    {
        public MinusOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            Left.ToLatex(builder);
            builder.Append('-');
            Right.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder);
            builder.Append('-');
            Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class MultiplyOperatorNode : BinaryOperatorNode
    {
        public MultiplyOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            var leftType = Left.GetType();
            var rightType = Right.GetType();
            string leftStr, rightStr;

            // enclose left/right operands in parenthesis if they are plus or minus operator nodes

            if (leftType == typeof(PlusOperatorNode) || leftType == typeof(MinusOperatorNode))
            {
                builder.Append("\\left(");
                Left.ToLatex(builder);
                builder.Append("\\right)");
            }
            else
            {
                Left.ToLatex(builder);
            }

            leftStr = builder.ToString();
            builder.Clear();

            if (rightType == typeof(PlusOperatorNode) || rightType == typeof(MinusOperatorNode))
            {
                builder.Append("\\left(");
                Right.ToLatex(builder);
                builder.Append("\\right)");
            }
            else
            {
                Right.ToLatex(builder);
            }

            rightStr = builder.ToString();
            builder.Clear();

            // handle if the muliplication should be explicitly printed or not for a less verbose latex expression
            builder.Append(leftStr);
            if (leftType == typeof(DivideOperatorNode) || rightType == typeof(DivideOperatorNode))
            {
                builder.Append(" \\cdot ");
            }
            else if (Left.Type == NodeType.Numeric)
            {
                if (Right.Type == NodeType.Numeric)
                {
                    builder.Append(" \\cdot ");
                }
                else if (rightType == typeof(MultiplyOperatorNode))
                {
                    MultiplyOperatorNode temp = (MultiplyOperatorNode) Right;
                    if (temp.Left.Type == NodeType.Numeric)
                    {
                        builder.Append(" \\cdot ");
                    }
                }
                else if (rightType == typeof(PowerOperatorNode))
                {
                    PowerOperatorNode temp = (PowerOperatorNode) Right;
                    if (temp.Left.Type == NodeType.Numeric)
                    {
                        builder.Append(" \\cdot ");
                    }
                }
            }
            else if (Right.Type == NodeType.Numeric)
            {
                builder.Append(" \\cdot ");
            }
            builder.Append(' ');
            builder.Append(rightStr);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder);
            builder.Append('*');
            Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class DivideOperatorNode : BinaryOperatorNode
    {
        public DivideOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\frac");

            builder.Append('{');
            Left.ToLatex(builder);

            builder.Append("}{");

            Right.ToLatex(builder);
            builder.Append('}');
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder);
            builder.Append('/');
            Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class PowerOperatorNode : BinaryOperatorNode
    {
        public PowerOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            if (Left.Type == NodeType.Operator)
            {
                builder.Append("\\left(");
                builder.Append('{');
                Left.ToLatex(builder);
                builder.Append('}');
                builder.Append("\\right)");
            }
            else
            {
                builder.Append('{');
                Left.ToLatex(builder);
                builder.Append('}');
            }

            builder.Append('^');

            builder.Append('{');
            Right.ToLatex(builder);
            builder.Append('}');
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder);
            builder.Append('^');
            Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class FunctionCallNode : BinaryOperatorNode
    {
        public FunctionCallNode(KeywordFunction func, AbstractSyntaxTree args) : base(func, args) { }

        public override void ToLatex(StringBuilder builder)
        {
            Left.ToLatex(builder); // print function
            builder.Append("\\left(");
            Right.ToLatex(builder); // print function argument
            builder.Append("\\right)");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder);
            builder.Append('(');
            Right.ToString(builder);
            builder.Append(')');
            builder.Append(')');
        }
    }

    // Node for functions raised to a power, allows power to be easily printed immediately after the function, Ex: sin^(x)(5) or sin(x)^(5)
    public class FunctionCallNodeWithPower : BinaryOperatorNode
    {
        public readonly AbstractSyntaxTree FunctionPower;

        public FunctionCallNodeWithPower(KeywordFunction func, AbstractSyntaxTree args, AbstractSyntaxTree functionPower) : base (func, args)
        {
            FunctionPower = functionPower;
        }

        public override void ToLatex(StringBuilder builder)
        {
            Left.ToLatex(builder); // print function

            builder.Append('^');

            builder.Append('{');
            FunctionPower.ToLatex(builder); // print power
            builder.Append('}');

            builder.Append("\\left(");
            Right.ToLatex(builder); // print function argument
            builder.Append("\\right)");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            Left.ToString(builder); // print function
            builder.Append('(');
            Right.ToString(builder); // print argument
            builder.Append(')');
            builder.Append(')');

            builder.Append("^");
            builder.Append('(');
;           FunctionPower.ToString(builder); // print power
            builder.Append(')');
        }
    }

    #endregion

    public class KeywordFunction :AbstractSyntaxTree
    {
        private readonly string _latex;
        private readonly string _raw;

        public KeywordFunction(string latex, string raw) : base(NodeType.NonNumeric) 
        {
            _latex = latex;
            _raw = raw;
        }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append(_latex);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append(_raw);
        }
    }

    public class KeywordSymbol : AbstractSyntaxTree
    {
        private readonly string _latex;
        private readonly string _raw;

        public KeywordSymbol(string latex, string raw) : base(NodeType.NonNumeric) 
        {
            _latex = latex;
            _raw = raw;
        }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append(_latex);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append(_raw);
        }
    }


    public class SymbolNode : AbstractSyntaxTree
    {
        public readonly string Symbol;

        public SymbolNode(string symbol) : base(NodeType.NonNumeric)
        {
            Symbol = symbol;
        }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append(Symbol);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append(Symbol);
        }
    }

    public class SpecialSymbolNode : AbstractSyntaxTree
    {
        private readonly string _latex;
        private readonly string _raw;

        public SpecialSymbolNode(string latex, string raw) : base(NodeType.NonNumeric)
        {
            _latex = latex;
            _raw = raw;
        }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append(_latex);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append(_raw);
        }
    }

    public class NumericNode : AbstractSyntaxTree
    {
        public readonly string Numeric;

        public NumericNode(string numeric) : base(NodeType.Numeric)
        {
            Numeric = numeric;
        }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append(Numeric);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append(Numeric);
        }
    }

    public class EmptyOperandNode : AbstractSyntaxTree
    {
        public EmptyOperandNode() : base(NodeType.Numeric) { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\square");
        }

        public override void ToString(StringBuilder builder)
        {
            throw new EmptyOperandException("Please fill out empty operands!");
        }
    }

    public class EmptyOperandException : Exception
    {
        public EmptyOperandException(string message) : base(message) { }
    }
}
