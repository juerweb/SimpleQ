//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimpleQ.Webinterface.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Person
    {
        public int PersId { get; set; }
        public int DepId { get; set; }
        public string CustCode { get; set; }
        public string DeviceId { get; set; }
    
        public virtual Department Department { get; set; }
    }
}
