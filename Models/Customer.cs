using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustID { get; set; }
        [Index(IsUnique = true)]
        public string CustName { get; set; }
        [Index(IsUnique = true)]
        public string CustEmail { get; set; }
        public string CustPwdHash
        {
            get => custPwdHash;
            set => custPwdHash = value.GetSHA512();
        }

        public string Street { get; set; }
        public string Plz { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public double CostBalance { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Survey> Surveys { get; set; }


        private string custPwdHash;
    }
}