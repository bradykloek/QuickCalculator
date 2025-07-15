using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Node
    {
        private Node? next;
        private string token; // String representing an isolated token
        private char type; /* Indicates the type of the token
                            *      s = Symbol (variable or function)
                            *      n = Number
                            *      o = Operator
                            */
        public Node(string token, char type)
        {
            this.token = token;
            this.type = type;
            this.next = null;
        }

        public Node GetNext()
        {
            return next;
        }

        public void SetNext(Node next)
        {
            this.next = next;
        }

        public string GetToken()
        {
            return token;
        }

        public char GetType()
        {
            return type;
        }
    }
}
