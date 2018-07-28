using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepID { get; set; }
        public string DepName { get; set; }
        public int CustID { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<AskedPerson> AskedPeople { get; set; }
        public virtual ICollection<Contains> Contains { get; set; }
    }
}