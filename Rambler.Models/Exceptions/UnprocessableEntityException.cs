using System;

namespace Rambler.Models.Exceptions
{
    public class UnprocessableEntityException : InvalidOperationException
    {
        public UnprocessableEntityException()
        {
        }

        public UnprocessableEntityException(string message) : base(message)
        {
        }

        public UnprocessableEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}