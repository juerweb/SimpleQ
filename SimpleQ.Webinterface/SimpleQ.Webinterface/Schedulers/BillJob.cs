using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using NLog;
using System.Threading;
using SimpleQ.Webinterface.Models;
using System.IO;
using System.Web.Hosting;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Properties;

namespace SimpleQ.Webinterface.Schedulers
{
    public class BillJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                using (var db = new SimpleQDBEntities())
                {
                    var result = await Task.Run(() => db.sp_CreateBills().ToList());
                    logger.Debug($"{result.Count()} bills created");
                    var bills = db.Bills.ToList().Where(b => !b.Paid && result.Contains(b.BillId)).ToList();
                    logger.Debug($"{bills.Count} bills loaded for sending");

                    foreach (Bill clinton in bills)
                    {
                        var lastBillDate = await db.Bills.Where(b => b.CustCode == clinton.CustCode)
                            .OrderByDescending(b => b.BillDate)
                            .Select(b => b.BillDate)
                            .Skip(1)
                            .FirstOrDefaultAsync();
                        logger.Debug($"Last bill date of customer {clinton.CustCode}: {lastBillDate.ToString("yyyy-MM-dd")}");

                        var surveys = await db.Surveys
                            .Where(s => s.CustCode == clinton.CustCode
                                    && s.StartDate <= clinton.BillDate
                                    && s.EndDate > lastBillDate)
                            .OrderBy(s => s.StartDate)
                            .ToArrayAsync();
                        logger.Debug($"Surveys to bill for customer {clinton.CustCode}: {surveys.Length}");

                        var stream = new MemoryStream();
                        if (Pdf.CreateBill(ref stream, clinton.Customer, clinton, surveys, HostingEnvironment.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
                        {
                            logger.Debug($"Pdf document for bill {clinton.BillId} created successfully");

                            var body = BackendResources.BillEmailMessage;

                            if (await Email.Send("payment@simpleq.at", clinton.Customer.CustEmail, BackendResources.BillEmailSubject, body, false,
                                new System.Net.Mail.Attachment(stream, $"SimpleQ_Bill_{clinton.BillDate.ToString("yyyy-MM-dd")}.pdf", "application/pdf")))
                            {
                                clinton.Sent = true;
                                await db.SaveChangesAsync();
                                logger.Debug($"Bill {clinton.BillId} sent successfully");
                            }
                            else
                            {
                                logger.Error($"Failed to send bill {clinton.BillId}");
                            }
                        }
                        else
                        {
                            logger.Error($"Failed to create pdf document for bill {clinton.BillId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Creating/sending bill failed");
            }
            finally
            {
                logger.Debug($"Next fire time {context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            }
        }
    }
}