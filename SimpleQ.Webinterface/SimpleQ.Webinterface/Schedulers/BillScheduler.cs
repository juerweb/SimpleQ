using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace SimpleQ.Webinterface.Schedulers
{
    public class BillScheduler : Scheduler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected override string Name => "bill scheduler";

        protected override void Schedule()
        {
            try
            {
                // Sleep bis um 04:20
                logger.Debug($"Create bills scheduler sleeping for {Literal.NextMidnight.Add(TimeSpan.FromHours(4)).Add(TimeSpan.FromMinutes(20)).ToString(@"hh\:mm\:ss\.fff")}");
                Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(4)).Add(TimeSpan.FromMinutes(20)).TotalMilliseconds);
                using (var db = new SimpleQDBEntities())
                {
                    var result = db.sp_CreateBills();
                    logger.Debug($"{result.Count()} bills created");
                    var bills = db.Bills.Where(b => !b.Paid).ToList();
                    logger.Debug($"{bills.Count} bills loaded for sending");

                    foreach (Bill clinton in bills)
                    {
                        var lastBillDate = db.Bills.Where(b => b.CustCode == clinton.CustCode)
                            .OrderByDescending(b => b.BillDate)
                            .Select(b => b.BillDate)
                            .Skip(1)
                            .FirstOrDefault();
                        logger.Debug($"Last bill date of customer {clinton.CustCode}: {lastBillDate.ToString("yyyy-MM-dd")}");

                        var surveys = db.Surveys
                            .Where(s => s.CustCode == clinton.CustCode
                                    && s.StartDate <= clinton.BillDate
                                    && s.EndDate > lastBillDate)
                            .OrderBy(s => s.StartDate)
                            .ToArray();
                        logger.Debug($"Surveys to bill for customer {clinton.CustCode}: {surveys.Length}");

                        var stream = new MemoryStream();
                        if (Pdf.CreateBill(ref stream, clinton.Customer, clinton, surveys, HostingEnvironment.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
                        {
                            logger.Debug($"Pdf document for bill {clinton.BillId} created successfully");
                            var body = $"Dear sir or madam!{Environment.NewLine}{Environment.NewLine}" +
                                $"Enclosed you will find your most recent bill.{Environment.NewLine}" +
                                $"We stay at your entire disposal for further questions.{Environment.NewLine}" +
                                $"{Environment.NewLine}{Environment.NewLine}" +
                                $"Sincerely{Environment.NewLine}" +
                                $"Your SimpleQ team";


                            if (Email.Send("payment@simpleq.at", clinton.Customer.CustEmail, "SimpleQ Bill", body, false,
                                new System.Net.Mail.Attachment(stream, $"SimpleQ_Bill_{clinton.BillDate.ToString("yyyy-MM-dd")}.pdf", "application/pdf")).Result)
                            {
                                clinton.Sent = true;
                                db.SaveChanges();
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
                logger.Error(ex, "Schedule: Creating/sending bill failed");
            }
        }
    }
}