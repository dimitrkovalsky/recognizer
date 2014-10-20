using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace recognizer
{
    public class Request
    {
        public Request() { }

        public Request(int requestType, string data)
        {
            this.requestType = requestType;
            this.data = data;
        }

        public int requestType { set; get; }
        public string data { set; get; }
    }
}
