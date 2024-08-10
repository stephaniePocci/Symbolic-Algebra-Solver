

using System.Text;

namespace Symbolic_Algebra_Solver.Parsing
{
    #region Abstract

    public abstract class AbstractSyntaxTree 
    {
        public abstract void ToLatex(StringBuilder builder);
        public abstract void ToString(StringBuilder builder);
    }

    public abstract class UnaryOperatorNode : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree Child;

        public UnaryOperatorNode(AbstractSyntaxTree child)
        {
            Child = child;
        }
    }

    public abstract class BinaryOperatorNode : AbstractSyntaxTree 
    {
        public readonly AbstractSyntaxTree Left;
        public readonly AbstractSyntaxTree Right;

        public BinaryOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right)
        {
            Left = left;
            Right = right;
        }
    }

    #endregion

    #region Operators

    public class NegationOperatorNode : UnaryOperatorNode
    {
        public NegationOperatorNode(AbstractSyntaxTree child) : base(child) { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append('-');
            this.Child.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('-');
            this.Child.ToString(builder);
        }
    }

    public class PlusOperatorNode : BinaryOperatorNode
    {
        public PlusOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder);
            builder.Append('+');
            this.Right.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('+');
            this.Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class MinusOperatorNode : BinaryOperatorNode
    {
        public MinusOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder);
            builder.Append('-');
            this.Right.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('-');
            this.Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class MultiplyOperatorNode : BinaryOperatorNode
    {
        public MultiplyOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder);
            builder.Append(" \\cdot ");
            this.Right.ToLatex(builder);
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('*');
            this.Right.ToString(builder);
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
            this.Left.ToLatex(builder);

            builder.Append("}{");

            this.Right.ToLatex(builder);
            builder.Append('}');
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('/');
            this.Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class PowerOperatorNode : BinaryOperatorNode
    {
        public PowerOperatorNode(AbstractSyntaxTree left, AbstractSyntaxTree right) : base(left, right) { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append('{');
            this.Left.ToLatex(builder);
            builder.Append('}');

            builder.Append('^');

            builder.Append('{');
            this.Right.ToLatex(builder);
            builder.Append('}');
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('^');
            this.Right.ToString(builder);
            builder.Append(')');
        }
    }

    public class FunctionCallNode : BinaryOperatorNode
    {
        public FunctionCallNode(KeywordFunction func, AbstractSyntaxTree args) : base(func, args) { }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder); // print function
            builder.Append("\\left(");
            this.Right.ToLatex(builder); // print function argument
            builder.Append("\\right)");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder);
            builder.Append('(');
            this.Right.ToString(builder);
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
            this.FunctionPower = functionPower;
        }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder); // print function

            builder.Append('^');

            builder.Append('{');
            this.FunctionPower.ToLatex(builder); // print power
            builder.Append('}');

            builder.Append("\\left(");
            this.Right.ToLatex(builder); // print function argument
            builder.Append("\\right)");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('(');
            this.Left.ToString(builder); // print function
            builder.Append('(');
            this.Right.ToString(builder); // print argument
            builder.Append(')');
            builder.Append(')');

            builder.Append("^");
            builder.Append('(');
;           this.FunctionPower.ToString(builder); // print power
            builder.Append(')');
        }
    }

    #endregion

    #region KeywordFunctions

    public abstract class KeywordFunction :AbstractSyntaxTree
    {

    }

    public class KeywordSinNode : KeywordFunction
    {
        public KeywordSinNode() { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\sin"); 
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append("sin");
        }
    }

    public class KeywordCosNode : KeywordFunction
    {
        public KeywordCosNode() { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\cos");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append("cos");
        }
    }

    public class KeywordTanNode : KeywordFunction
    {
        public KeywordTanNode() { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\tan");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append("tan");
        }
    }

    #endregion

    #region KeywordSymbols

    public class KeywordPiNode : AbstractSyntaxTree
    {
        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\pi");
        }

        public override void ToString(StringBuilder builder)
        {
            builder.Append('π');
        }
    }

    #endregion

    public class SymbolNode : AbstractSyntaxTree
    {
        public readonly string Symbol;

        public SymbolNode(string symbol)
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

    public class NumericNode : AbstractSyntaxTree
    {
        public readonly string Numeric;

        public NumericNode(string numeric)
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
        public EmptyOperandNode() { }

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
