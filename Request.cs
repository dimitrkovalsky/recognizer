using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace recognizer
{
    public class Request
    {
        public Request() { }

        public Request(int requestType, object data)
        {
            this.requestType = requestType;
            this.data = data;
        }

        public int requestType { set; get; }
        public object data { set; get; }
    }
}
