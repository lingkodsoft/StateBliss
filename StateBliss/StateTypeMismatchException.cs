using System;

namespace StateBliss
{
    public class StateTypeMismatchException : Exception
    {
        public StateTypeMismatchException(string message): base(message)
        {
        }
    }
}