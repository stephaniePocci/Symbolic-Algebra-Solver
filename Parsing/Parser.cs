using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Symbolic_Algebra_Solver.Parsing
{
    public static class Parser
    {
        public static bool TryParse(List<string> tokenList, [NotNullWhen(true)] out AbstractSyntaxTree? output, [NotNullWhen(false)] out string? errorMsg)
        {
            if (tokenList.Count == 0)
            {
                throw new AssertionFailedException("Empty token list is not allowed!");
            }

            Stack<OperatorEnum> operatorStack = new Stack<OperatorEnum>();
            Stack<KeywordFunctionNode> functionStack = new Stack<KeywordFunctionNode>(); // stack to hold function keywords like sin, cos...
            Stack<AbstractSyntaxTree>  operandStack =  new Stack<AbstractSyntaxTree>();

            operatorStack.Push(OperatorEnum.OperatorNone);

            errorMsg = null;
            int unMatchedFunctions   = 0; // track number of function keywords that need a matching function call operator, used to handle inputs such as sin^(2)(x) where pushing a function call operator is delayed
            int index = 0;

            // Alternate between unary and binary loops until all tokens are processed
            while (true) 
            {
                // unary state loop
                while (true)
                {
                    switch (tokenList[index]) 
                    {
                        case "(":
                            operatorStack.Push(OperatorEnum.OpeningParenthesis);
                            ++index;
                            continue; // stay in unary loop
                        case "-":
                            if (operatorStack.Peek() == OperatorEnum.FunctionPower)
                            {
                                errorMsg = "Use parenthsis around function exponents. Ex: sin^(3+5)(x) instead of sin^3+5(x)";
                                output = null;
                                return false;
                            }
                            operatorStack.Push(OperatorEnum.Negation); // highest precendence so just push
                            ++index;
                            continue; // stay in unary loop

                        // matching binary operators while in unary mode means there are missing operands
                        case "+":
                        case "*":
                        case "/":
                        case "^":
                            if (operatorStack.Peek() == OperatorEnum.FunctionPower)
                            {
                                output = null;
                                errorMsg = "Use parenthsis around function exponents. Ex: sin^(3+5)(x) instead of sin^3+5(x)";
                                return false;
                            }
                            operandStack.Push(new EmptyOperandNode());
                            break;
                        case ")":
                            output = null;
                            errorMsg = "Unexpected: ')'";
                            return false;
                        case ";":
                            operandStack.Push(new EmptyOperandNode());
                            goto ExitLoop; // semi colon marks end of token list
                        default:
                            // push keyword node to operand stack or function stack
                            if (Grammer.Keywords.TryGetValue(tokenList[index], out Keyword? value)) 
                            {
                                if (value.IsFunction)
                                {
                                    if (operatorStack.Peek() == OperatorEnum.FunctionPower)
                                    {
                                        output = null;
                                        errorMsg = "Use parenthsis around function exponents. Ex: sin^(3+5)(x) instead of sin^3+5(x)";
                                        return false;
                                    }

                                    functionStack.Push(CreateKeywordFunctionNode(value.Id));
                                    ++index;

                                    // the token following a function must be a power operator or a function call operator
                                    if (index < tokenList.Count)
                                    {
                                        // next token is function call operator so insert
                                        if (tokenList[index] == "(")
                                        {
                                            operatorStack.Push(OperatorEnum.FunctionCall);
                                            ++index;
                                            continue;
                                        }
                                        // if function has a power operator, insert special function power enum, for edge cases like sin^(2)(x)
                                        else if (tokenList[index] == "^")
                                        {
                                            operatorStack.Push(OperatorEnum.FunctionPower);
                                            ++unMatchedFunctions; // increment so we know to later insert a matching function call operator for this keyword function
                                            ++index;
                                            continue;
                                        }
                                        // else the next token after function keyword is not a function call operator, insert function call operator implicitly
                                        else
                                        {
                                            operatorStack.Push(OperatorEnum.ImplicitFunctionCall);
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    operandStack.Push( CreateKeywordSymbolNode(value.Id) );
                                }
                            }
                            // pushing a single character that is a unicode letter as a symbol node to operand stack
                            else if (Char.TryParse(tokenList[index], out char c) && Char.IsLetter(c))
                            {
                                operandStack.Push(new SymbolNode(c));
                                ++index;
                            }
                            // pushing a numeric token
                            else
                            {
                                operandStack.Push(new NumericNode(tokenList[index]));
                                ++index;
                            }
                            
                            break;
                    }

                    break; // exit unary loop
                }
                
                // binary state loop
                while (true) 
                {

                    switch (tokenList[index])
                    {
                        case "-":
                            errorMsg = PushOperatorByPrecedence(OperatorEnum.Minus, operatorStack, operandStack);
                            ++index;
                            break;
                        case "+":
                            errorMsg = PushOperatorByPrecedence(OperatorEnum.Plus, operatorStack, operandStack);
                            ++index;
                            break;
                        case "*":
                            errorMsg = PushOperatorByPrecedence(OperatorEnum.Multiply, operatorStack, operandStack);
                            ++index;
                            break;
                        case "/":
                            errorMsg = PushOperatorByPrecedence(OperatorEnum.Divide, operatorStack, operandStack);
                            ++index;
                            break;
                        case "^":
                            errorMsg = PushOperatorByPrecedence(OperatorEnum.Power, operatorStack, operandStack);
                            ++index;
                            break;
                        case "(":
                            if (unMatchedFunctions > 0 && operatorStack.Peek() == OperatorEnum.FunctionPower)
                            {
                                operatorStack.Push(OperatorEnum.FunctionCall);
                                --unMatchedFunctions;
                                ++index;
                            }
                            // handle implicit muliplication
                            else
                            {
                                errorMsg = PushOperatorByPrecedence(OperatorEnum.Multiply, operatorStack, operandStack);
                            }
                            
                            break;
                        case ")":
                            errorMsg = TryAcceptClosingParenthesis(operatorStack, operandStack, functionStack);
                            if (errorMsg != null)
                            {
                                output = null;
                                return false;
                            }

                            ++index;
                            continue; // stay in binary loop
                        case ";": 
                            goto ExitLoop; // semi colon marks end of token list

                        // encountered operand when expected a operator
                        default:
                            // match functions such as sin^2x, function operator inserted between 2 and x
                            if (unMatchedFunctions > 0 && operatorStack.Peek() == OperatorEnum.FunctionPower)
                            {
                                operatorStack.Push(OperatorEnum.ImplicitFunctionCall);
                                --unMatchedFunctions;
                            }
                            else
                            {
                                // handle implicit muliplication
                                errorMsg = PushOperatorByPrecedence(OperatorEnum.Multiply, operatorStack, operandStack);
                            }
                    
                            break;
                    }

                    if (errorMsg != null)
                    {
                        output = null;
                        return false;
                    }

                    break; // // exit binary loop
                }
            }

            ExitLoop:

            OperatorEnum op;

            // insert remaining unmatched function call operators
            while (unMatchedFunctions > 0)
            {
                op = operatorStack.Peek();

                if (op == OperatorEnum.FunctionPower)
                {
                    operandStack.Push(new EmptyOperandNode());
                    BindFunctionCall(functionStack, operandStack);
                    BindOperator(operatorStack.Pop(), operandStack);

                    --unMatchedFunctions;
                }
                else if (op != OperatorEnum.OpeningParenthesis)
                {
                    if (op == OperatorEnum.FunctionCall || op == OperatorEnum.ImplicitFunctionCall)
                    {
                        operatorStack.Pop();
                        BindFunctionCall(functionStack, operandStack);
                    }
                    else
                    {
                        BindOperator(operatorStack.Pop(), operandStack);  
                    }
                }
                // pop and ignore and left over opening parenthesis
                else
                {
                    operatorStack.Pop();
                }
            }

            // all functions have matching function call operators at this point so just
            // bind any remaining operators
            while ( (op = operatorStack.Pop()) != OperatorEnum.OperatorNone ) 
            {
                if (op == OperatorEnum.FunctionCall || op == OperatorEnum.ImplicitFunctionCall)
                {
                    BindFunctionCall(functionStack, operandStack);
                }
                else if (op != OperatorEnum.OpeningParenthesis)
                {
                    BindOperator(op, operandStack);
                }

                // pop and ignore and left over opening parenthesis
            }

            if (operandStack.Count == 1)
            {
                output = operandStack.Pop();
                return true;
            }
            else
            {
                throw new AssertionFailedException("Operand stack should only have a single abstract syntax tree node when parsing is finished!");
            }
        }

        /// <summary>
        /// Push the specified operator onto the operator stack.
        /// </summary>
        /// <param name="op">Operator enum to be pushed</param>
        /// <param name="operatorStack">Stack of operators</param>
        /// <param name="operandStack">Stack of operands</param>
        /// <returns>Null on success, error message string on fail.</returns>
        private static string? PushOperatorByPrecedence(OperatorEnum op, Stack<OperatorEnum> operatorStack, Stack<AbstractSyntaxTree> operandStack)
        {
            Operator newOp = Grammer.Operators[op]; // operator being pushed

            Operator stackOp; // operator on top of the stack
            OperatorEnum stackOpEnum;

            while ( (stackOpEnum = operatorStack.Peek()) != OperatorEnum.OperatorNone )
            {
                stackOp = Grammer.Operators[stackOpEnum];

                if (newOp.Precedence > stackOp.Precedence)
                {
                    break; // exit while loop
                }
                else if (newOp.Precedence < stackOp.Precedence)
                {
                    BindOperator(operatorStack.Pop(), operandStack);
                }
                else
                {
                    switch (stackOp.Associativity)
                    {
                        case OperatorAssociativity.Left:
                            BindOperator(operatorStack.Pop(), operandStack);
                            continue; // continue to next loop iteration
                        case OperatorAssociativity.None:
                        case OperatorAssociativity.Right:
                            break;
                    }

                    break; // exit while loop
                }
            }

            operatorStack.Push(op);
            return null;
        }

        /// <summary>
        /// Create and push a abstract syntax tree for the input operator with operands from the operand stack onto the operand stack.
        /// This effectively binds the operator with its respective operands from the stack.
        /// </summary>
        /// <remarks>
        /// Function call operators are a special case, use <see cref="BindFunctionCall(Stack{KeywordFunctionNode}, Stack{AbstractSyntaxTree})"/>
        /// to bind functions with their operands.
        /// </remarks>
        /// <param name="op">Operator to create Ast for and to push.</param>
        /// <param name="operandStack">Operand stack use to create the Ast for the operator.</param>
        private static void BindOperator(OperatorEnum op, Stack<AbstractSyntaxTree> operandStack)
        {
            AbstractSyntaxTree left;
            AbstractSyntaxTree right;

            switch (op)
            {
                case OperatorEnum.Negation:
                    operandStack.Push(new NegationOperatorNode(operandStack.Pop()));
                    break;
                case OperatorEnum.Plus:
                    right = operandStack.Pop();
                    left = operandStack.Pop();
                    operandStack.Push(new PlusOperatorNode(left, right));
                    break;
                case OperatorEnum.Minus:
                    right = operandStack.Pop();
                    left = operandStack.Pop();
                    operandStack.Push(new MinusOperatorNode(left, right));
                    break;
                case OperatorEnum.Multiply:
                    right = operandStack.Pop();
                    left = operandStack.Pop();
                    operandStack.Push(new MultiplyOperatorNode(left, right));
                    break;
                case OperatorEnum.Divide:
                    right = operandStack.Pop();
                    left = operandStack.Pop();
                    operandStack.Push(new DivideOperatorNode(left, right));
                    break;
                case OperatorEnum.Power:
                    right = operandStack.Pop();
                    left = operandStack.Pop();
                    operandStack.Push(new PowerOperatorNode(left, right));
                    break;
                case OperatorEnum.FunctionPower: // special enum for power operators that appear immediately after a function keyword, Ex: sin^(2)(x)
                    left = operandStack.Pop();
                    right = operandStack.Pop();
                    operandStack.Push(new PowerOperatorNode(left, right));
                    break;
                default:
                    throw new AssertionFailedException("Failed to bind operator.");
            }
        }

        private static void BindFunctionCall(Stack<KeywordFunctionNode> functionStack, Stack<AbstractSyntaxTree> operandStack)
        {
            AbstractSyntaxTree funcArg;

            if (operandStack.Count != 0) 
            {
                funcArg = operandStack.Pop();
            }
            else
            {
                funcArg = new EmptyOperandNode();
            }

            operandStack.Push(new FunctionCallOperatorNode(functionStack.Pop(), funcArg));
        }

        /// <summary>
        /// Continuously pop operator stack until a function call or opening parenthsis is encountered.
        /// </summary>
        /// <param name="unMatchedParenthesis">Reference to number of unmatched opening parenthesis, decremented when opening parenthesis is found.</param>
        /// <param name="operatorStack">Stack of operators</param>
        /// <param name="operandStack">Stack of operands</param>
        /// /// <param name="functionStack">Stack of function keywords</param>
        /// <returns>
        /// A null string on success and a non-null string containing a error message on failure.
        /// </returns>
        private static string? TryAcceptClosingParenthesis(Stack<OperatorEnum> operatorStack, Stack<AbstractSyntaxTree> operandStack, Stack<KeywordFunctionNode> functionStack)
        {
            string? error = null;
            OperatorEnum op;

            while (true)
            {
                if ( (op = operatorStack.Peek()) != OperatorEnum.OperatorNone )
                {
                    if (op == OperatorEnum.OpeningParenthesis)
                    {
                        operatorStack.Pop();
                        break;
                    }
                    else if (op == OperatorEnum.FunctionCall)
                    {
                        operatorStack.Pop();
                        BindFunctionCall(functionStack, operandStack);

                        if (operatorStack.Peek() == OperatorEnum.FunctionPower)
                        {
                            BindOperator(operatorStack.Pop(), operandStack);
                        }

                        break;
                    }
                    else if (op == OperatorEnum.ImplicitFunctionCall)
                    {
                        operatorStack.Pop();
                        BindFunctionCall(functionStack, operandStack);

                        if (operatorStack.Peek() == OperatorEnum.FunctionPower)
                        {
                            BindOperator(operatorStack.Pop(), operandStack);
                        }
                    }
                    else if (op == OperatorEnum.FunctionPower)
                    {
                        error = "Unexpected: ')'";
                        break;
                    }
                    else
                    {
                        BindOperator(operatorStack.Pop(), operandStack);
                    }
                }
                else
                {
                    // Error, closing parenthesis encountered with no matching opening parenthesis or function call parenthesis
                    error = "Unexpected closing parenthsis: ')'";
                    break;
                }
            }

            return error;
        }

        private static AbstractSyntaxTree CreateKeywordSymbolNode(KeywordEnum type)
        {
            AbstractSyntaxTree? node;

            switch (type)
            {
                case KeywordEnum.Pi:
                    node = new KeywordPiNode();
                    break;
                default:
                    throw new AssertionFailedException("Creating a keyword node that is not a symbol!");
            }

            return node!;
        }

        private static KeywordFunctionNode CreateKeywordFunctionNode(KeywordEnum type)
        {
            KeywordFunctionNode? node;

            switch (type)
            {
                case KeywordEnum.Sin:
                    node = new KeywordSinNode();
                    break;
                case KeywordEnum.Cos:
                    node = new KeywordCosNode();
                    break;
                case KeywordEnum.Tan:
                    node = new KeywordTanNode();
                    break;
                default:
                    throw new AssertionFailedException("Creating a keyword node that is not a function!");
            }

            return node!;
        }

    }

    public class AssertionFailedException : Exception
    {
        public AssertionFailedException(string message) : base(message) { }
    }
}
