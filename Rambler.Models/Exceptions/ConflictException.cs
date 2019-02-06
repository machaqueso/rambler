using System;
using System.Collections.Generic;
using System.Text;

namespace Rambler.Models.Exceptions
{
    public class ConflictException : InvalidOperationException
    {
        public ConflictException()
        {
        }

        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
