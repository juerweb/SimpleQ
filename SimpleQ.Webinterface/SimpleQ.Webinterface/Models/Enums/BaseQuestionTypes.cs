using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.Enums
{
    public enum BaseQuestionTypes : int
    {
        OpenQuestion = 1,
        DichotomousQuestion = 2,
        PolytomousQuestion = 3,
        LikertScaleQuestion = 4,
        FixedAnswerQuestion = 5
    }
}