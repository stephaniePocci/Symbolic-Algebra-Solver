using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Symbolic_Algebra_Solver.Parsing
{
    public partial class Tokenizer
    {
        private static readonly ReadOnlyDictionary<string, string[]> _keywordDictionary;

        [GeneratedRegex(@"\s+")]
        private static partial Regex _MatchWhiteSpace();

        /// <summary>
        /// Static constructor to initialize a dictionary of keywords.
        /// A ReadOnlyDictionary where each key is the first letter of the corresponding
        /// array of keywords. For example key "a" maps to string array contain keywords
        /// that all begin with letter a. The array is ordered from the longest to shortest string.
        /// </summary>
        static Tokenizer()
        {
            Dictionary<string, string[]> keywordDictionary = new Dictionary<string, string[]>();
            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            foreach (char c in alphabet)
            {
                string[] keys = Grammer.Keywords.Keys.Where(key => key.StartsWith(c.ToString(), StringComparison.OrdinalIgnoreCase)).ToArray();
                if (keys.Length > 0)
                {
                    Array.Sort(keys, (x, y) => y.Length.CompareTo(x.Length)); // sort keys in decending length order
                    string[] values = new string[keys.Length];

                    for (int i = 0; i < keys.Length; ++i)
                    {
                        values[i] = keys[i];
                    }
                    keywordDictionary.Add(c.ToString(), values);
                }
            }

            _keywordDictionary = new ReadOnlyDictionary<string, string[]>(keywordDictionary);
        }

        private readonly List<Token> _tokenList = new();
        private readonly StringBuilder _builder = new();
        private int _position = 0;

        /// <summary>
        /// Parse a input expression into a list of tokens.
        /// </summary>
        /// <param name="input">Input expression string</param>
        /// <param name="error">String containing error message if method returns false</param>
        /// <returns>
        /// True on success, Else false on fail.
        /// </returns>
        public bool TryTokenize(string input, [NotNullWhen(false)]out string? error)
        {
            error = null;
            _builder.Clear();
            _tokenList.Clear();
            _position = 0;

            input = ReplaceWhiteSpace(input, "");
            int index = 0;

            while (index < input.Length) 
            {
                char c = input[index];

                if (Char.IsDigit(c) || c == '.')
                {
                    error = TokenizeNumeric(ref index, input);
                }
                else if (Char.IsAsciiLetter(c))
                {
                    TokenizeSymbol(ref index, input);
                }
                else if (Grammer.IsOperator(c))
                {
                    TokenizeOperator(ref index, input);
                }
                else if (c == '(' || c == ')')
                {
                    _tokenList.Add(new Token(input[index++].ToString(), TokenType.None));
                }
                else
                {
                    if (Grammer.IsSpecialSymbol(c, out var value))
                    {
                        _tokenList.Add(new Token(value.Raw, TokenType.SpecialSymbol));
                        ++index;
                    }
                    else
                    {
                        // unrecognized token/character
                        error = $"Invalid character: '{c}'";
                    }
                }

                if (error != null)
                {
                    _tokenList.Clear();
                    _position = 0;
                    return false;
                }
            }

            _tokenList.Add(new Token(";", TokenType.None)); // semi colon to mark end of token list

            return true;
        }

        /// <summary>
        /// Moves the pointer from current token to the next and returns that result.
        /// </summary>
        /// <exception cref="AssertionFailedException"></exception>
        /// <returns>
        /// The token that is being pointed after calling ScanToken.
        /// Throws exception if token is null due to TryTokenizer failing or not being run on a string input.
        /// If ScanToken attempts to scan pass the end of the tokenlist then it will simply just return the last token.
        /// 
        /// </returns>
        public Token ScanToken()
        {
            if (_tokenList == null)
            {
                throw new AssertionFailedException("Scaning null token list failure!");
            }

            if (_position < _tokenList.Count)
            {
                return _tokenList[_position++];
            }
            else
            {
                return _tokenList[_tokenList.Count - 1];
            }
        }

        public Token GetPrevToken()
        {
            if (_tokenList == null)
            {
                throw new AssertionFailedException("Null token list failure!");
            }

            if (_position - 1 >= 0)
            {
                return _tokenList[_position - 1];
            }
            else
            {
                return _tokenList[0];
            }
        }

        private static string ReplaceWhiteSpace(string input, string replacement) 
        {
            return _MatchWhiteSpace().Replace(input, replacement);
        }

        /// <summary>
        /// Tokenizes a numeric from a input string and appends the token the to tokenList. 
        /// </summary>
        /// <remarks>
        /// For example given the string "35+10" and a starting index of 0; 
        /// this function will continuously process each character until a non-digit character is encountered.
        /// In this case the character '+' is the first non-digit character so tokenizing ends here and the ouput
        /// "35" is appended to the token list. Non-digit characters also include white-space.
        /// </remarks>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        private string? TokenizeNumeric(ref int index, string input)
        {   
            _builder.Clear();
            bool isFloat = false;

            // a decimal followed by digits is a valid numeric, Ex: .3 == 0.3
            if (input[index] == '.')
            {
                if (index + 1 < input.Length && Char.IsDigit(input[index + 1])) 
                {
                    _builder.Append('0');            // append add leading zero
                    _builder.Append(input[index++]); // append decimal point
                    _builder.Append(input[index++]); // append the digit following the decimal point
                    isFloat = true;
                }
                else
                {
                    // Error we have a lone decimal point which is a invalid token
                    return "Invalid character: '.'";
                }
            }
            // check possible invalid numeric
            else if (input[index] == '0')
            {
                _builder.Append(input[index++]);

                // a numeric starting with zero is invalid unless followed up by a decimal, or is a lone zero, Ex: 05 is invalid but 0.5 is valid
                if ( index < input.Length && input[index] == '.' )
                {
                    _builder.Append(input[index++]);
                    isFloat = true;
                }
                else
                {
                    if (index < input.Length && Char.IsDigit(input[index]))
                    {
                        // invalid integer
                         return "Invalid numeric, integers cannot start with zero.";
                    }
                }
            }

            while ( index < input.Length && Char.IsDigit(input[index]) ) 
            {
                _builder.Append(input[index++]);

                // decimal point is only added once per numeric
                if ( !isFloat && index < input.Length && input[index] == '.' )
                {
                    _builder.Append(input[index++]);
                    isFloat = true;
                }
            }

            // if float has a trailing decimal point remove it, Ex: 5. is simply 5
            if (isFloat && input[index - 1] == '.') 
            {
                _builder.Remove(_builder.Length - 1, 1);    
            }

            _tokenList!.Add( new Token(_builder.ToString(), TokenType.Numeric) );

            return null;
        }

        /// <summary>
        /// Tokenizes a symbol or keyword from <paramref name="input"/> and appends the token to the token list.
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        private void TokenizeSymbol(ref int index, string input)
        {
            _builder.Clear();

            string? foundKeyword = null;
            if ( _keywordDictionary.TryGetValue(input[index].ToString().ToLower(), out var values) ) // check if symbol is possibly a starting character of a keyword
            {
                foreach( var keyword in values )
                {
                    int endIndex = index + (keyword.Length - 1);
                    
                    if (endIndex < input.Length)
                    {
                        int resultIndex = input.IndexOf(keyword, index, keyword.Length, StringComparison.OrdinalIgnoreCase);

                        if (resultIndex != -1)
                        {
                            foundKeyword = keyword;

                            index = endIndex + 1;

                            if (Grammer.IsKeywordFunction(foundKeyword))
                            {
                                _tokenList!.Add(new Token(foundKeyword, TokenType.Function));
                            }
                            else
                            {
                                _tokenList!.Add( new Token(foundKeyword, TokenType.Keyword));
                            }

                            break; // exit loop once matching keyword is found
                        }
                    }
                }
            }

            // no keyword was found so just append the single character symbol as a token
            if (foundKeyword == null)
            {
                _builder.Append(input[index++]);
                _tokenList!.Add( new Token(_builder.ToString(), TokenType.Symbol) );
            }  
        }

        /// <summary>
        /// Tokenize valid operators such as +,-,*,/, etc..
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        private void TokenizeOperator(ref int index, string input)
        {
            char token = input[index++];

            // double multiply means power operator
            if (token == '*' && index < input.Length && input[index] == '*')
            {
                _tokenList.Add(new Token("^", TokenType.Operator));
                ++index;
            }
            else
            {
                _tokenList!.Add( new Token(token.ToString(), TokenType.Operator) );
            }
        }
    }

    public enum TokenType
    {
        Numeric,
        Symbol,
        SpecialSymbol,
        Operator,
        Function,
        Keyword,
        None,
    }

    public class Token 
    {
        public readonly string Value;
        public readonly TokenType Type;

        public Token(string value, TokenType type)
        {
            Type = type;
            Value = value;
        }

        public Token(char value, TokenType type)
        {
            Type = type;
            Value = value.ToString();
        }
    }
}
