using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models
{
    public class Bill
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillID { get; set; }
        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustID { get; set; }
        public double BillAmount { get; set; }
        public DateTime BillDate { get; set; }
        

        public virtual Customer Customer { get; set; }
    }
}