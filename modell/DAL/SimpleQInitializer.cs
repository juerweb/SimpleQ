using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.DAL
{
    public class SimpleQInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<SimpleQContext>
    {
        protected override void Seed(SimpleQContext context)
        {
            var list = new List<Customer>
            {
                new Customer { CustName = "Name", CustEmail = "Email", CustPwdHash = "asdf1234", Street = "Street", Plz = "1010",
                               City = "City", Country = "AT", PaymentMethod = PaymentMethod.BANK, CostBalance = 0.0 },
                new Customer { CustName = "Name2", CustEmail = "Email2", CustPwdHash = "asdf1234", Street = "Street2", Plz = "1010",
                               City = "City2", Country = "AT", PaymentMethod = PaymentMethod.PAYPAL, CostBalance = 0.0 },
                new Customer { CustName = "Name3", CustEmail = "Email3", CustPwdHash = "asdf1234", Street = "Street3", Plz = "1010",
                               City = "City3", Country = "AT", PaymentMethod = PaymentMethod.BANK, CostBalance = 0.0 },
            };
            list.ForEach(c => context.Customers.Add(c));
            context.SaveChanges();
        }
    }
}