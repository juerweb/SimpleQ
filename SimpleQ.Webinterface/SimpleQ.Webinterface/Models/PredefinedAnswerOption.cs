//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimpleQ.Webinterface.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PredefinedAnswerOption
    {
        public int PreAnsId { get; set; }
        public string PreAnsText { get; set; }
        public int TypeId { get; set; }
    
        [System.Web.Script.Serialization.ScriptIgnore(ApplyToOverrides = true)]
        [Newtonsoft.Json.JsonIgnore]
        public virtual AnswerType AnswerType { get; set; }
    }
}
