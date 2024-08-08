

using System.Text;

namespace Symbolic_Algebra_Solver.Parsing
{
    #region Abstract

    public abstract class AbstractSyntaxTree 
    {
        public abstract void ToLatex(StringBuilder builder);
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

        
    }

    public class FunctionCallOperatorNode : BinaryOperatorNode
    {
        public FunctionCallOperatorNode(AbstractSyntaxTree func, AbstractSyntaxTree funcArg) : base(func, funcArg) { }

        public override void ToLatex(StringBuilder builder)
        {
            this.Left.ToLatex(builder);
            builder.Append("\\left(");
            this.Right.ToLatex(builder);
            builder.Append("\\right)");
        }
    }

    #endregion

    #region KeywordFunctions

    public abstract class KeywordFunctionNode : AbstractSyntaxTree
    {
    }

    public class KeywordSinNode : KeywordFunctionNode
    {
        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\sin");
        }
    }

    public class KeywordCosNode : KeywordFunctionNode
    {
        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\cos");
        }
    }

    public class KeywordTanNode : KeywordFunctionNode
    {
        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\tan");
        }
    }

    #endregion

    public class KeywordPiNode : AbstractSyntaxTree
    {
        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\pi");
        }
    }


    public class SymbolNode : AbstractSyntaxTree
    {
        public readonly string Symbol;

        public SymbolNode(char symbol)
        {
            Symbol = symbol.ToString();
        }

        public override void ToLatex(StringBuilder builder)
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
    }

    public class EmptyOperandNode : AbstractSyntaxTree
    {
        public EmptyOperandNode() { }

        public override void ToLatex(StringBuilder builder)
        {
            builder.Append("\\square");
        }
    }
}
