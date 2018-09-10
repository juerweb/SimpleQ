using SimpleQ.Webinterface.Models;
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
        public Department AssignedDepartment { get; set; } // Wird ausschließlich nach Registration/Abteilungswechsel gesetzt
        public int PersId { get; set; } // Wird ausschließlich nach Registration gesetzt

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
        REGISTRATION_FAILED_ALREADY_REGISTERED,
        REGISTRATION_FAILED_INVALID_CODE,
        LOGGED_IN,
        LOGIN_FAILED_NOT_REGISTERED,
        DEPARTMENT_CHANGED,
        DEPARTMENT_CHANGING_FAILED_INVALID_DEPARTMENT
    }
}