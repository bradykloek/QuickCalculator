using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        private string input;                   // The raw string input
        private bool throwException;            // Whether we should throw exceptions or just store them
        private List<Token> tokens;                 // Stores the tokens that get created
        private List<EvaluationException> exceptions;       // If we don't throw exceptions, they will be stored here
        private Evaluator evaluator;

        private static char[] operators = { '+', '-', '*', '/', '^', '%', '!', '=' };
        private static char[] validSymbols = { '.', ',', '(', ')', '[', ']' };

        bool hasAssignment = false;
        bool hasDecimal;
        bool implicitMultiplication;
        int parenLevel = 0;
        Token addToken;

        char current = '\0';
        int currentIndex = -1;
        char category = '\0';
        string token = "";
        int inputStart = 0;

        public Tokenizer(string input, Evaluator evaluator)
        {
            this.input = input;
            this.evaluator = evaluator;
            tokens = new List<Token>(input.Length / 2);
            // Starts tokens with a capacity of 50% of input.Length to avoid the initial resizings which would almost always occur

            if (input.Length > 0) Tokenize();
        }


        public List<Token> GetTokens()
        {
            return tokens;
        }


        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Count(); i++)
            {
                if (i == currentIndex)
                {
                    sb.Append(" <" + tokens[i].ToString() + "> ");
                }
                else
                {
                    sb.Append(tokens[i] + " ");
                }
            }
            return sb.ToString();
        }

        private bool NextChar()
        {
            if (currentIndex >= input.Length - 1) return false;

            currentIndex++;
            current = input[currentIndex];
            return true;
        }

        private void AddToken(Token newToken)
        {
            if(addToken != null)
            {
                tokens.Add(addToken);
                addToken = null;
            }
            else
            {
                tokens.Add(newToken);
            }
        }

        /// <summary>
        /// Takes in the input string to populate the tokens array.
        /// If !throwExceptions, it will also fill the exceptions list.
        /// This is called from the constructor.
        /// </summary>
        private void Tokenize()
        {
            while(NextChar())
            {
                if (current.Equals(' ')) { // Whitespace is ignored entirely-- even for symbol names and numbers
                    continue;
                }

                switch (category)
                {
                    case '\0': // If a category hasn't yet been determined, we need to start with 
                        implicitMultiplication = false;
                        hasDecimal = false;
                        if (StartToken()) break;
                        else continue;
                    case 'v':
                        if (TokenizeVariable()) break;
                        else continue;
                    case 'n':
                        if (TokenizeNumber()) break;
                        else continue;
                }
                
                AddToken(new Token(token, category, inputStart, currentIndex + 1));

                if (implicitMultiplication)
                {
                    // If implicit multiplication was detected, add a * token (of length 0)
                    AddToken(new Token("*", 'o', currentIndex, currentIndex));
                }
                category = '\0'; // Change the category to indicate that we are starting a new token
                token = "";
            }

            if (category == 'v' || category == 'n' || category == 'f') // If the last token is a symbol, number, or function, it hasn't been added yet
            {
                AddToken(new Token(token, category, inputStart, input.Length));
            }

            // Check if all parentheses have been matched
            if(parenLevel != 0)
            {
                checkParentheses();
            }

            /* If an assignment operator = has been used, we will save the variable that is being assigned and send it to the evaluator.
             * Then we will remove this variable and the assignment operator from the tokens so the rest of the expression can be parsed normally.
             * We only do this if the Tokenizer hasn't encountered any errors.
             */
            if (hasAssignment && evaluator.GetExceptionCount() == 0)
            {
                prepareAssignment();
            }
        }


        private bool StartToken()
        {
            token += current;
            inputStart = currentIndex;

            /* If the token starts with a letter, then it is a Variable or Function. It will be marked as a variable
             * and stay as such unless a [ is found indicating it is a function */
            if (Char.IsLetter(current))
            {
                category = 'v'; // v indicates Variable
                return false;
            }

            // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
            else if (Char.IsDigit(current) || current == '.')
            {
                category = 'n'; // n indicates Number
                hasDecimal = current == '.';
                return false;
            }

            else if (operators.Contains(current))
            {
                category = 'o';
                return TokenizeOperator();
            }
            else if (current == '(' || current == ')')
            {
                category = current;

                TokenizeParenthesis();
                return true;
            }
            else
            {
                // Any other character is invalid for the start of a token
                evaluator.AddException("Invalid character '" + current + "' for start of token.", currentIndex, currentIndex + 1);
            }
            return true;
        }

        private bool TokenizeNumber()
        {
            // This character continues the token only if it is a digit or decimal
            if (Char.IsDigit(current))
            {
                token += current;
                return false;
            }
            else if (current == ',')
            {
                return false; // Commas can be in numbers and are simply ignored
            }
            else if (current == '.')
            {
                if (hasDecimal)
                {
                    evaluator.AddException("Number contains multiple decimal points.", currentIndex, currentIndex + 1);
                }
                else
                {
                    hasDecimal = true;
                    token += current;
                    return false;
                }
            }
            else
            {
                /*  If none of the above, then the current token ends and we will start a new one.
                *  We must move the counter (i) back so the loop will check the current character again */
                currentIndex--;

                if (current == '(')
                {
                    /*  If we encounter a parenthesis immediately following the number token, it is interpreted
                    *   as implicit multiplication */
                    implicitMultiplication = true;
                }
            }
            return true;
        }
        private bool TokenizeVariable()
        {
            // This character continues the token if it is a letter or digit
            if (Char.IsLetter(current) || Char.IsDigit(current))
            {
                token += current;
                return false;
            }

            if (current == '[')
            {
                category = 'f'; // If we encounter a [, this indicates this variable is actually a function
            }

            /*  The current token ends and we will start a new one.
                *  We must move the counter (i) back so the loop will check the current character again */
            currentIndex--;
            return true;
        }

        private bool TokenizeOperator()
        {
            /* Before checking the other operators, we first must handle a special case for '-' 
             * to check if it should be the unary negation operator instead of subtraction.     */
            if (current == '-'  && (tokens.Count() == 0 || tokens[tokens.Count() - 1].GetCategory() == 'o'))
            {   // If this is the first token or the previous token was an operator, it is unary 
                category = 'n';     // It should be interpreted as a number isntead of an operator
                return false;   
            }

            if (current == '=')
            {
                if (hasAssignment)
                {
                    evaluator.AddException("Expression contains multiple assignment operators '='.", currentIndex, currentIndex + 1);
                }
                hasAssignment = true;
            }

            // The only operator that is more than one character is // for integer division. Special case.
            if (current == '/')
            {
                if (currentIndex + 1 < input.Length && input[currentIndex + 1] == '/')
                {
                    token = "//";
                    NextChar(); // Move past the second '/' since we have already tokenized it
                }
            }

            return true;
        }

        private void TokenizeParenthesis()
        {
            if (current == '(') addToken = new ParenToken(token, category, inputStart, currentIndex, parenLevel++);
            else
            {
                if (parenLevel == 0)
                {
                    evaluator.AddException("Unmatched closing parenthesis.", currentIndex, currentIndex + 1);
                }
                else
                {
                    addToken = new ParenToken(token, category, inputStart, currentIndex, --parenLevel);
                }
            }
        }

        /// <summary>
        /// Finds which open parentheses tokens are unmatched by a closing parentheses at the same level.
        /// This method is only called after it is known that there is at least one unmatched parenthesis
        /// </summary>
        private void checkParentheses()
        {
            for (int i = 0; i < tokens.Count(); i++)
            {
                bool matched = false;
                if (tokens[i].GetCategory() == '(')
                {
                    ParenToken openParen = (ParenToken)tokens[i];
                    for (int j = i + 1; j < tokens.Count(); j++)
                    {
                        if (tokens[j].GetCategory() == ')')
                        {
                            ParenToken closeParen = (ParenToken)tokens[j];
                            if (openParen.GetLevel() == closeParen.GetLevel())
                            {
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        evaluator.AddException("Unmatched open parenthesis.", openParen.GetStart(), openParen.GetEnd());
                    }

                }
            }
        }

        private void prepareAssignment()
        {
            int assignmentIdx = 0;
            int variableIdx;
            for (; assignmentIdx < tokens.Count; assignmentIdx++)
            {   // Find the assignment operator. It is known that there is only one because we checked that there aren't any exceptions.
                if (tokens[assignmentIdx].GetToken().Equals("=")){ break; }
            }
            /*  Assignment can only take place at the beginning or end of the tokens list.
             *  At least one operand must be a variable
             */

            if (assignmentIdx == tokens.Count() - 1)
            {   // Assignment operator is the last token
                evaluator.AddException("Assignment needs two operands.", tokens[assignmentIdx].GetStart(), tokens[assignmentIdx].GetEnd());
                return;
            }

            if  (assignmentIdx != 1     &&   assignmentIdx != tokens.Count() - 2)    // Assignment token is not second or second to last           
            {
                evaluator.AddException("Assignment can only be done to an isolated variable", tokens[assignmentIdx].GetStart(), tokens[assignmentIdx].GetEnd());
                return;
            }

            Token prevToken = tokens[assignmentIdx - 1];
            Token nextToken = tokens[assignmentIdx + 1];

            if (prevToken.GetCategory() == 'v' && nextToken.GetCategory() == 'v') // If both are variables
            {
                if (VariableTable.vars.ContainsKey(nextToken.GetToken()))
                {   // So long as next is defined, we assign prev <= next (leftward asignment by default)
                    variableIdx = assignmentIdx - 1;
                }
                else if (VariableTable.vars.ContainsKey(prevToken.GetToken()))
                {   // If next isn't defined but prev is, then we do rightward assignment - prev => next
                    variableIdx = assignmentIdx + 1;
                }
                else
                {   // Otherwise, neither are defined and we cannot successfully assign either
                    evaluator.AddException("Cannot assign between two undefined variables.", prevToken.GetStart(), nextToken.GetEnd());
                    return;
                }

            }
            else if (prevToken.GetCategory() == 'v')
            {   // prev is the token that will receive the assignment
                variableIdx = assignmentIdx - 1;
            }
            else if (nextToken.GetCategory() == 'v')
            {   // next is the token that will receive the assignment
                variableIdx = assignmentIdx + 1;
            }
            else
            {   // Neither operand is a variable
                evaluator.AddException("At least one operand must be a variable for assignment", prevToken.GetStart(), nextToken.GetEnd());
                return;
            }

            // Send the variable to the evaluator
            evaluator.SetAssignVariable(tokens[variableIdx].GetToken());

            // Removing from the List will shift indices. We need to remove the lowest index twice to remove the variable and the assignment operator
            tokens.RemoveAt(Math.Min(assignmentIdx, variableIdx));
            tokens.RemoveAt(Math.Min(assignmentIdx, variableIdx));
        }
    }
}
