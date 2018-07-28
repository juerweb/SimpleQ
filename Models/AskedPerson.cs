using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models
{
    public class AskedPerson
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PersID { get; set; }
        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DepID { get; set; }
        public string PersFirstName { get; set; }
        public string PersLastName { get; set; }
        [Index(IsUnique = true)]
        public string PersEmail { get; set; }
        public string PersPwdHash
        {
            get => persPwdHash;
            set => persPwdHash = value.GetSHA512();
        }

        public virtual Department Department { get; set; }


        private string persPwdHash;
    }
}