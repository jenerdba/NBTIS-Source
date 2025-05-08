using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.Exceptions
{
    public class NoNbiBridgesException : InvalidOperationException
    {
        public NoNbiBridgesException()
            : base("No NBI length bridges have been submitted.") { }
    }

    public class StateMismatchException : InvalidOperationException
    {
        public StateMismatchException(string errorMessage)
            : base(errorMessage) { }
    }

    public class JsonFormatException : InvalidOperationException
    {
        public JsonFormatException(string errorMessage)
            : base(errorMessage) { }
    }
}
