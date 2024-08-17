using System.Diagnostics.CodeAnalysis;

namespace Symbolic_Algebra_Solver.Parsing
{
    public class Parser
    {
        // The parser is implemented using recursive descent, below is a general description of what syntax the parser expects.
        // "|" means either or, items inside brackets mean zero or more. So for example a expression here is defined as a Term0 folowed by zero or more Term0s' that have
        // either a plus or minus operator infront.

        /*
            Expression   := Term0 { ("+" | "-") Term0 }
            Term0        := Term1 { ( "*" | "/" ) Term1 }
            Term1        := Factor | ( Factor "^" Factor )
            Factor       := { "-" Factor } | ( Symbol | Keyword | KeywordFunction | Numeric | "(" Expression ")" )

            Symbols are any single character unicode letters, keywords can be words that represent symbols such as pi, theta, alpha, etc, keyword functions represent sin, cos, etc,
            and numerics are any number literals.
         */

        private Token? _nextToken;
        private Tokenizer _scanner = new();

        /// <summary>
        /// Parses the input string into a abstract syntax tree.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="result">Abstract synatx tree output, null on parsing fail</param>
        /// <param name="status">String that contains error message if parsing fails</param>
        /// <returns>True on parsing success, else false</returns>
        public bool TryParse(string input, [NotNullWhen(true)] out AbstractSyntaxTree? result, [NotNullWhen(false)] out string? status)
        {
            if (input == string.Empty)
            {
                result = null;
                status = "Cannot parse empty string!";
                return false;
            }

            if (!_scanner.TryTokenize(input, out status))
            {
                result = null;
                return false;
            }

            _nextToken = _scanner.ScanToken();

            try 
            {
                result = ParseExpression();
            }
            catch (ParsingFailedException e)
            {
                status = e.Message;
                result = null;
                return false;
            }

            if (_nextToken.Value != ";")
            {
                status = ( _nextToken.Value == ")" ) ? "Unexpected ')'." : "Parsing error.";
                result = null;
                return false;
            }

            status = null;
            return true;
        }

        private AbstractSyntaxTree ParseExpression()
        {
            var left = ParseTerm0();

            while (true)
            {
                if (_nextToken!.Value == "+")
                {
                    _nextToken = _scanner!.ScanToken();
                    left = new PlusOperatorNode(left, ParseTerm0());
                }
                else if (_nextToken!.Value == "-")
                {
                    _nextToken = _scanner!.ScanToken();
                    left = new MinusOperatorNode(left, ParseTerm0());
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private AbstractSyntaxTree ParseTerm0()
        {
            var left = ParseTerm1();

            while (true)
            {
                if (_nextToken!.Value == "*")
                {
                    _nextToken = _scanner.ScanToken();
                    left = new MultiplyOperatorNode(left, ParseTerm1());
                }
                else if (_nextToken.Value == "/")
                {
                    _nextToken = _scanner.ScanToken();
                    left = new DivideOperatorNode(left, ParseTerm1());
                }
                else
                {
                    // handle possible implicit multiplication Ex: 5x == 5*x
                    if (_nextToken.Value != ";")
                    {
                        if (_nextToken.Value != ")" && !Grammer.IsOperator(_nextToken.Value))
                        {
                            left = new MultiplyOperatorNode(left, ParseTerm1());
                            continue;
                        }
                    }
                    break;
                }
            }

            return left;
        }

        private AbstractSyntaxTree ParseTerm1()
        {
            var left = ParseFactor();

            if (_nextToken!.Value == "^")
            {
                _nextToken = _scanner.ScanToken();
                left = new PowerOperatorNode(left, ParseTerm1());
            }

            return left;
        }

        private AbstractSyntaxTree ParseFactor()
        {
            if (_nextToken!.Value == "-")
            {
                _nextToken = _scanner.ScanToken();
                return new NegationOperatorNode(ParseFactor());
            }

            switch (_nextToken.Type)
            {
                case TokenType.Numeric:
                    string num = _nextToken.Value;
                    _nextToken = _scanner.ScanToken();
                    return new NumericNode(num);

                case TokenType.Symbol:
                    string symbol = _nextToken.Value;
                    _nextToken = _scanner.ScanToken();
                    return new SymbolNode(symbol);

                case TokenType.SpecialSymbol:
                    var specialSymbol = Grammer.SpecialSymbols[Char.Parse(_nextToken.Value)];
                    _nextToken = _scanner.ScanToken();
                    return new SpecialSymbolNode(specialSymbol.Latex, specialSymbol.Raw.ToString());

                case TokenType.Operator:
                    return new EmptyOperandNode();

                case TokenType.Function:
                    return ParseFunction();

                case TokenType.Keyword:  
                    var keywordSymbol = Grammer.Keywords[_nextToken.Value];
                    AbstractSyntaxTree keywordNode = new KeywordSymbol(keywordSymbol.Latex, keywordSymbol.Raw);
                    _nextToken = _scanner.ScanToken();
                    return keywordNode;

                default:
                    // beginning of a expression
                    if (_nextToken.Value == "(") 
                    {
                        _nextToken = _scanner!.ScanToken();
                        var expr = ParseExpression();
                        if (_nextToken.Value == ")") _nextToken = _scanner.ScanToken();

                        return expr;
                    }
                    // end of tokenlist marker
                    else if (_nextToken.Value == ";")
                    {
                        return new EmptyOperandNode();
                    }
                    // error
                    else if (_nextToken.Value == ")")
                    {
                        throw new ParsingFailedException("Unexpected ')'");
                    }
                    // error
                    else
                    {
                        throw new ParsingFailedException("Parsing failed.");
                    }      
            }
        }

        private AbstractSyntaxTree ParseFunction()
        {
            var keywordFunc = Grammer.Keywords[_nextToken!.Value];
            KeywordFunction func = new(keywordFunc.Latex, keywordFunc.Raw);
            AbstractSyntaxTree args;
            _nextToken = _scanner.ScanToken();

            if (_nextToken.Value == "(")
            {
                _nextToken = _scanner.ScanToken();
                args = ParseExpression();
                // consume closing parenthesis if one exists
                if (_nextToken.Value == ")") 
                {
                    _nextToken = _scanner.ScanToken();
                    if (_nextToken.Value == "^") // if function is raised to a power, parse the power and build a function node with a power
                    {
                        _nextToken = _scanner.ScanToken();
                        if (_nextToken.Value == ";")
                        {
                            return new FunctionCallNodeWithPower(func, args, new EmptyOperandNode());
                        }
                        else if (_nextToken.Value != "(")
                        {
                            throw new ParsingFailedException("Please use parenthesis for function powers! Ex: sin(x)^(5+1) instead of sin(x)^5+1");
                        }

                        _nextToken = _scanner.ScanToken(); // consume opening parenthesis
                        AbstractSyntaxTree pow = ParseExpression();
                        if (_nextToken.Value == ")") _nextToken = _scanner.ScanToken(); // consume closing parenthesis if one exists

                        return new FunctionCallNodeWithPower(func, args, pow);
                    }

                }

                return new FunctionCallNode(func, args);
            }
            else if (_nextToken.Value == "^")
            {
                // handle power expression first
                // function power must be followed up by a opening parenthesis
                _nextToken = _scanner.ScanToken();
                if (_nextToken.Value == ";")
                {
                    return new FunctionCallNodeWithPower(func, new EmptyOperandNode(), new EmptyOperandNode());
                }
                else if ( _nextToken.Value != "(")  // power operator after a function must be followed up by a opening parenthesis
                {
                    throw new ParsingFailedException("Please use parenthesis for function powers! Ex: sin^(5)x instead of sin^5x");
                }

                // parse power expression
                _nextToken = _scanner.ScanToken();
                AbstractSyntaxTree power = ParseExpression();
                if (_nextToken.Value == ")") _nextToken = _scanner.ScanToken(); // consume closing parenthesis if one exists


                // handle function argument second
                // function argument after power must start with a opening parenthesis
                if (_nextToken.Value == ";")
                {
                    return new FunctionCallNodeWithPower(func, new EmptyOperandNode(), power);
                }
                else if (_nextToken.Value != "(")
                {
                    throw new ParsingFailedException("Function argument missing parenthesis!");
                }
                
                // parse function argument
                _nextToken = _scanner.ScanToken();
                args = ParseExpression();
                if (_nextToken.Value == ")") _nextToken = _scanner.ScanToken(); // consume closing parenthesis if one exists


                return new FunctionCallNodeWithPower(func, args, power);
            }
            else if (_nextToken.Value == ";")
            {
                return new FunctionCallNode( func, new EmptyOperandNode() );
            }
            else
            {
                throw new ParsingFailedException("Function argument missing parenthesis!");
            }
        }
    }

    public class AssertionFailedException : Exception
    {
        public AssertionFailedException(string message) : base(message) { }
    }

    public class ParsingFailedException : Exception
    {
        public ParsingFailedException (string message) : base(message) { }
    }
}
