using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Symbolic_Algebra_Solver.Parsing
{
    public static partial class Tokenizer
    {
        private static readonly ReadOnlyDictionary<string, string[]> _keywordDictionary;

        [GeneratedRegex(@"\s+")]
        private static partial Regex _MatchWhiteSpace();

        private enum TokenState
        {
            TsIsDigit,
            TsIsOperator,
            TsIsSymbol,
            TsInvalidCharacter,
        }

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

        /// <summary>
        /// Parse a input expression into a list of tokens.
        /// </summary>
        /// <param name="input">Input expression string</param>
        /// <param name="tokenList">Output list of tokens</param>
        /// <param name="error">String containing error message if method returns false</param>
        /// <returns>
        /// True on success, <paramref name="tokenList"/> will contain list of tokens and <paramref name="error"/> will be null.
        /// False on fail, <paramref name="tokenList"/> will be null and <paramref name="error"/> will contain a error message.
        /// </returns>
        public static bool TryTokenize(string input, [NotNullWhen(true)] out List<string>? tokenList, [NotNullWhen(false)] out string? error)
        {
            error = null;
            StringBuilder builder = new StringBuilder();
            tokenList = new List<string>();

            input = ReplaceWhiteSpace(input, "");
            int index = 0;

            while (index < input.Length) 
            {
                char c = input[index];

                if (Char.IsDigit(c) || c == '.')
                {
                    error = TokenizeNumeric(ref index, input, tokenList, builder);
                }
                else if (Char.IsLetter(c))
                {
                    TokenizeSymbol(ref index, input, tokenList, builder);
                }
                else if (Grammer.IsOperator(input[index]))
                {
                    TokenizeOperator(ref index, input, tokenList);
                }
                else
                {
                    // unrecognized token/character
                    error = $"Invalid character: '{c}'";
                }

                if (error != null)
                {
                    return false;
                }
            }

            tokenList.Add(";"); // semi colon to indicate end of token list

            return true;
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
        /// "35" is appended to <paramref name="tokenList"/>. Non-digit characters also include white-space.
        /// </remarks>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        /// <param name="builder">StringBuilder object used to build the token. The StringBuilder is cleared first before it is used.</param>
        private static string? TokenizeNumeric(ref int index, string input, List<string> tokenList, StringBuilder builder)
        {   
            builder.Clear();
            bool isFloat = false;

            // a decimal followed by digits is a valid numeric, Ex: .3 == 0.3
            if (input[index] == '.')
            {
                if (index + 1 < input.Length && Char.IsDigit(input[index + 1])) 
                {
                    builder.Append('0');            // append add leading zero
                    builder.Append(input[index++]); // append decimal point
                    builder.Append(input[index++]); // append the digit following the decimal point
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
                builder.Append(input[index++]);

                // a numeric starting with zero is invalid unless followed up by a decimal, Ex: 05 is invalid but 0.5 is valid
                if ( index < input.Length && input[index] == '.' )
                {
                    builder.Append(input[index++]);
                    isFloat = true;
                }
                else
                {
                    if (index < input.Length)
                    {
                        // invalid integer
                         return "Invalid numeric, integers cannot start with zero.";
                    }
                }
            }

            while ( index < input.Length && Char.IsDigit(input[index]) ) 
            {
                builder.Append(input[index++]);

                // decimal point is only added once per numeric
                if ( !isFloat && index < input.Length && input[index] == '.' )
                {
                    builder.Append(input[index++]);
                    isFloat = true;
                }
            }

            // if float has a trailing decimal point remove it, Ex: 5. is simply 5
            if (isFloat && input[index - 1] == '.') 
            {
                builder.Remove(index - 1, 1);    
            }

            tokenList.Add(builder.ToString());
            //ImplicitMultiplication(index, input, tokenList);

            return null;
        }

        /// <summary>
        /// Tokenizes a symbol or keyword from <paramref name="input"/> and appends the token to the <paramref name="tokenList"/>.
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        /// <param name="builder">StringBuilder object used to build the token. The StringBuilder is cleared first before it is used.</param>
        private static void TokenizeSymbol(ref int index, string input, List<string> tokenList, StringBuilder builder)
        {
            builder.Clear();

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

                            while (index <= endIndex)
                            {
                                builder.Append(input[index]);
                                index++;
                            }
                            
                            tokenList.Add(builder.ToString().ToLower());

                            /*if (!Grammer.Keywords[foundKeyword].IsFunction)
                            {
                                ImplicitMultiplication(index, input, tokenList);
                            }*/

                            break; // exit loop once matching keyword is found
                        }
                    }
                }
            }

            // no keyword was found so just append the single character symbol as a token
            if (foundKeyword == null)
            {
                builder.Append(input[index++]);
                tokenList.Add( builder.ToString() );
               // ImplicitMultiplication(index, input, tokenList);
            }  
        }

        /// <summary>
        /// Tokenize valid operators such as +,-,*,/, etc..
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        private static void TokenizeOperator(ref int index, string input, List<string> tokenList)
        {
            char token = input[index++];
            tokenList.Add( token.ToString() );

            // if the added character is a closing parenthesis, check for implicit multiplication. Ex: (x)5 == x * 5
            /*if (token == ')')
            {
                ImplicitMultiplication(index, input, tokenList);
            }*/
        }

        private static void ImplicitMultiplication(int index, string input, List<string> tokenList)
        {
            if (index < input.Length)
            {
                char c = input[index];

                if (Char.IsLetterOrDigit(c) || c == '(')
                {
                    tokenList.Add("*");
                }
            }
        }
    }
}
