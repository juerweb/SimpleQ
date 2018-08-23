using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Mobile
{
    public class OperationStatus
    {
        public StatusCode StatusCode { get; set; }
        public string Message { get; set; }

        public OperationStatus(StatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }

        public static implicit operator string(OperationStatus op)
        {
            return op.Message;
        }
    }

    public enum StatusCode
    {
        REGISTERED,
        REGISTRATION_FAILED,
        LOGGED_IN,
        LOGIN_FAILED,
    }
}