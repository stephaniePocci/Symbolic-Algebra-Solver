using System.Text;
using static Symbolic_Algebra_Solver.Parser.TokenState;

namespace Symbolic_Algebra_Solver.Parser
{
    enum TokenState
    {
        TsIsDigit,
        TsIsOperator,
        TsIsSymbol,
        TsInvalidCharacter,
    }

    public static class Tokenizer
    {
        /// <summary>
        /// Dictionary of keywords contructed from <see cref="Grammer.keywords"/>. 
        /// Each key is the first letter of the corresponding
        /// array of keywords. For example key "a" maps to a array of tuples containing
        /// the string name of the keyword followed by the <see cref="Keyword"/> object that contains extra info about the keyword.
        /// </summary>
        /// <remarks>
        /// This dictionary is marked as readonly but that only applies to the object reference.
        /// Keys can still be added which is something that should be avoided outside of the static constructor.
        /// </remarks>
        private static readonly Dictionary<string, Keyword[]> keywords = [];

        static Tokenizer()
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            // initialize keywords dictionary with keywords from the Grammer class
            
            foreach (char c in alphabet)
            {
                string[] keys = Grammer.keywords.Keys.Where(key => key.StartsWith(c.ToString(), StringComparison.OrdinalIgnoreCase)).ToArray();
                if (keys.Length > 0)
                {
                    Array.Sort(keys, (x, y) => y.Length.CompareTo(x.Length)); // sort keys in decending length order
                    Keyword[] values = new Keyword[keys.Length];

                    for (int i = 0; i < keys.Length; ++i)
                    {
                        values[i] = Grammer.keywords[keys[i]];
                    }
                    keywords.Add(c.ToString(), values);
                }
            }
        }

        public static List<string> Tokenize(string input)
        {
            List<string> tokenList = new();
            StringBuilder tokenBuilder = new();

            TokenState state;
            int index = 0;

            do
            {
                state = SetTokenState(input, index); // update state based on character at current index of input string

                switch (state)
                {
                    case TsIsDigit:
                        TokenizeNumber(ref index, input, tokenList, tokenBuilder);
                        break;
                    case TsIsSymbol:
                        TokenizeSymbol(ref index, input, tokenList, tokenBuilder);
                        break;
                    case TsIsOperator:
                        TokenizeOperator(ref index, input, tokenList, tokenBuilder);
                        break;
                    case TsInvalidCharacter:
                        index = input.Length; // exit loop
                        break;
                }
            } while (index < input.Length);

            return tokenList;
        }

        /// <summary>Set tokenizer state based the character of the string at the specified index</summary>
        /// <param name="input">Input string</param>
        /// <param name="index">Index of character within string to determine tokenizer state.</param>
        /// <returns>Initial tokenizer state</returns>
        private static TokenState SetTokenState(string input, int index) 
        {
            TokenState state;

            if ( Char.IsDigit(input[index]) || input[index] == Char.Parse(Grammer.Decimal) )
            {
                state = TsIsDigit;
            }
            else if ( Char.IsLetter(input[index]) )
            {
                state = TsIsSymbol;
            }
            else if ( Grammer.operators.ContainsKey(input[index].ToString()) )
            {
                state = TsIsOperator;
            }
            else
            {
                state = TsInvalidCharacter;
            }
            
            return state;
        }

        /// <summary>
        /// Consume trailing whitespace by advancing the index until a non-whitespace character is encountered.
        /// </summary>
        /// <param name="index">Reference to start index</param>
        /// <param name="input">Input string</param>
        private static void ConsumeTrailingWhiteSpace(ref int index, string input)
        {
            while (index < input.Length && Char.IsWhiteSpace(input[index])) { index++; }
        }

        /// <summary>
        /// Tokenizes a number from a input char array string and appends the token the to tokenList. 
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
        private static void TokenizeNumber(ref int index, string input, List<String> tokenList, StringBuilder builder)
        {   
            builder.Clear();

            bool isFloat = false;
            if (input[index] == Char.Parse(Grammer.Decimal))
            {
                builder.Append(input[index++]);
                isFloat = true;
            }

            while ( index < input.Length && Char.IsDigit(input[index]) ) 
            {
                builder.Append(input[index++]);

                if ( !isFloat && (index + 1 < input.Length) && input[index] == Char.Parse(Grammer.Decimal) )
                {
                    if (Char.IsDigit(input[index + 1]))
                    {
                        builder.Append(input[index]);
                    }
                    isFloat = true;
                    index++;
                }
            }
            tokenList.Add(builder.ToString());

            ImplicitMultiplcation(ref index, input, tokenList, builder);
        }

        /// <summary>
        /// Tokenizes a symbol from <paramref name="input"/> and appends the token to the <paramref name="tokenList"/>.
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        /// <param name="builder">StringBuilder object used to build the token. The StringBuilder is cleared first before it is used.</param>
        private static void TokenizeSymbol(ref int index, string input, List<String> tokenList, StringBuilder builder)
        {
            builder.Clear();

            Keyword? foundKeyword = null;
            if ( keywords.TryGetValue(input[index].ToString().ToLower(), out var values) ) // check if symbol is possibly a starting character of a keyword
            {
                foreach( var keyword in values )
                {
                    int endIndex = index + (keyword.Name.Length - 1);
                    
                    if (endIndex < input.Length)
                    {
                        int resultIndex = input.IndexOf(keyword.Name, index, keyword.Name.Length, StringComparison.OrdinalIgnoreCase);

                        if (resultIndex != -1)
                        {
                            foundKeyword = keyword;
                            while (index <= endIndex)
                            {
                                builder.Append(input[index]);
                                index++;
                            }
                            tokenList.Add( builder.ToString() );
                            break; // exit loop once matching keyword is found
                        }
                    }
                }
            }

            // no keyword was found so just append the single character symbol as a token
            if (foundKeyword == null)
            {
                builder.Append(input[index++]);
                tokenList.Add(builder.ToString());
                ImplicitMultiplcation(ref index, input, tokenList, builder);
            }
            else
            {
                // if keyword has arguments, implicitly add a opening parenthesis if one is not already present
                if (foundKeyword.ArgCount > 0) 
                {
                    ConsumeTrailingWhiteSpace(ref index, input);
                    if (index < input.Length && input[index] != Char.Parse(Grammer.LParenthesis))
                    {
                        builder.Clear();
                        builder.Append(Grammer.LParenthesis);
                        tokenList.Add(builder.ToString());
                    }
                }
                else // keyword has no argument and is simply just a multi-character symbol
                {
                    ImplicitMultiplcation(ref index, input, tokenList, builder);
                }
            }
        }

        /// <summary>
        /// Tokenize valid operators such as +,-,*,/, etc..
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        /// <param name="builder">StringBuilder object used to build the token. The StringBuilder is cleared first before it is used.</param>
        private static void TokenizeOperator(ref int index, string input, List<String> tokenList, StringBuilder builder)
        {
            builder.Clear();
            bool specialCasePresent = false;

            // check for two possible edge cases first.
            // subtraction operator could represent the negative sign.
            if (input[index] == Char.Parse(Grammer.Minus))
            {
                if ( tokenList.Count == 0 || Grammer.operators.ContainsKey(tokenList.Last()) )
                {
                    builder.Append(Grammer.Negate);
                    tokenList.Add(builder.ToString());
                    index++;
                    specialCasePresent = true;
                }
            }
            // "*" sign can be followed up immediately by another "*", in which case it needs to be parsed as the power operater "^".
            // In otherwords, "**" equals to "^"
            else if (input[index] == Char.Parse(Grammer.Multiply))
            {
                if (index + 1 < input.Length && input[index + 1] == Char.Parse(Grammer.Multiply))
                {
                    builder.Append(Grammer.Power);
                    tokenList.Add(builder.ToString());
                    index += 2;
                    specialCasePresent = true;
                }
            }
            
            // else the operator is added to the tokenList as normal
            if (!specialCasePresent)
            {
                builder.Append(input[index++]);
                tokenList.Add(builder.ToString());

                // if operator is a closing parenthesis, check for implicit multiplication.
                // Ex: "(5)x" equals "(5)*x"
                if (builder.ToString() == Grammer.RParenthesis) 
                {
                    ImplicitMultiplcation(ref index, input, tokenList, builder);
                }
            }
            ConsumeTrailingWhiteSpace(ref index, input);
        }

        /// <summary>
        /// Handle implicit multiplication by checking if the next character after consuming any existing
        /// whitespaces is a letter, digit, decimal point, or a opening parenthesis.
        /// Example: The string "3 5" is parsed as "3*5", the string "3y" is parsed as "3*y"
        /// </summary>
        /// <param name="index">Reference to starting index to begin tokenizing</param>
        /// <param name="input">Input string</param>
        /// <param name="tokenList">List of tokens that the processed token will be appended to.</param>
        /// <param name="builder">StringBuilder object used to build the token. The StringBuilder is cleared first before it is used.</param>
        private static void ImplicitMultiplcation(ref int index, string input, List<String> tokenList, StringBuilder builder)
        {
            ConsumeTrailingWhiteSpace(ref index, input);
            if ( index < input.Length && ( Char.IsLetterOrDigit(input[index]) || (input[index] == Char.Parse(Grammer.LParenthesis)) || (input[index] == Char.Parse(Grammer.Decimal)) ) ) 
            {
                builder.Clear();
                builder.Append(Grammer.Multiply);
                tokenList.Add(builder.ToString());
            }
        }
    }
}
