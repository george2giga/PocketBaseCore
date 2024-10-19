using System;
using System.Net;

namespace PocketBaseCore
{
    public class PocketSharpException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public PocketSharpException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}