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
    
    public partial class AnswerOption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AnswerOption()
        {
            this.Votes = new HashSet<Vote>();
        }
    
        public int AnsId { get; set; }
        public int SvyId { get; set; }
        public string AnsText { get; set; }
    
        public virtual Survey Survey { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vote> Votes { get; set; }
    }
}
