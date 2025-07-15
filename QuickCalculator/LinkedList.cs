using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class LinkedList
    {
        private Node? head;
        private Node? tail;
        public LinkedList() {
            head = null;
            tail = null;
        }

        public Node GetHead()
        {
            return head;
        }

        public void Insert(Node node)
        {
            if(head == null)
            {
                head = node;
                tail= node;
            }
            else
            {
                tail.SetNext(node);
                tail = node;
            }
        }

        public string ToString()
        {
            string output = "";
            Node currNode = head;
            while (currNode != null)
            {
                output += " " + currNode.GetToken() + " ";
                currNode = currNode.GetNext();
            }
            return output;
        }
    }
}
