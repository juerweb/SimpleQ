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
    
    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Customer()
        {
            this.Bills = new HashSet<Bill>();
            this.Departments = new HashSet<Department>();
            this.SurveyCategories = new HashSet<SurveyCategory>();
            this.AnswerTypes = new HashSet<AnswerType>();
        }
    
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string CustEmail { get; set; }
        public string CustPwdTmp { get; set; }
        public byte[] CustPwdHash { get; set; }
        public string Street { get; set; }
        public string Plz { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string LanguageCode { get; set; }
        public int DataStoragePeriod { get; set; }
        public int PaymentMethodId { get; set; }
        public int MinGroupSize { get; set; }
        public decimal PricePerClick { get; set; }
        public decimal CostBalance { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Department> Departments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyCategory> SurveyCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AnswerType> AnswerTypes { get; set; }
    }
}
