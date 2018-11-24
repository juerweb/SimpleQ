using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models
{
    public partial class SimpleQDBEntities
    {
        [DbFunction("SimpleQDBModel.Store", "fn_CalcPricePerClick")]
        public virtual decimal? fn_CalcPricePerClick(int amount)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;

            var parameters = new List<ObjectParameter>
            {
                new ObjectParameter("amount", amount)
            };

            return objectContext.CreateQuery<decimal>("SimpleQDBModel.Store.fn_CalcPricePerClick(@amount)", parameters.ToArray())
                 .Execute(MergeOption.NoTracking)
                 .FirstOrDefault();
        }
    }
}