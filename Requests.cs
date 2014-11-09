using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace recognizer
{
    static class Requests
    {
        public const int CLIENT_CONNECTED = 1;
        public const int LOAD_DICTIONARY = 10;
        public const int ADD_TO_DICTIONARY = 11;
        public const int CHANGE_ACTIVE_GRAMMAR = 13;
        public const int START_RECOGNITION = 15;
        public const int RECOGNITION_STARTED = 16;
        public const int RECOGNITION_RESULT = 20;
        public const int SYNTHESIZE = 30;
    }
}
