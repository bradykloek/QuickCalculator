using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    public enum TokenCategory : byte
    {
        Uncategorized,
        Number,
        Operator,
        Variable,
        Function,
        Parameter,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        Assignment,
        Comma,
        Negation,
        Inquiry,
        Command,
    }
}
