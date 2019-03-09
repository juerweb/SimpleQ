using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SimpleQ.Webinterface.Models;
using NLog;
using System.Data.Entity.Infrastructure;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Web.Hosting;
using System.Data.Entity;

namespace SimpleQ.Webinterface.Schedulers
{
    [DisallowConcurrentExecution]
    public class BackupJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                try
                {
                    logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                    using (var ef = new SimpleQDBEntities())
                    {
                        File.Create(HostingEnvironment.MapPath($"~/Backups/{DateTime.Today.ToString("yyyy-MM-dd")}.sql")).Close();
                        logger.Debug($"Created backup file {DateTime.Today.ToString("yyyy-MM-dd")}.sql");
                        var arr = ef.Database.Connection.ConnectionString.Split(';');
                        var server = new Server(new ServerConnection(new SqlConnection(string.Join(";", arr.Take(arr.Length - 2)))));
                        var db = server.Databases[ef.Database.Connection.Database];
                        var options = new ScriptingOptions
                        {
                            FileName = HostingEnvironment.MapPath($"~/Backups/{DateTime.Today.ToString("yyyy-MM-dd")}.sql"),
                            EnforceScriptingOptions = true,
                            WithDependencies = false,
                            IncludeHeaders = false,
                            ScriptSchema = false,
                            ScriptDrops = false,
                            ScriptData = true,
                            DriAll = true,
                            DriIncludeSystemNames = false,
                            AppendToFile = true,
                            ScriptBatchTerminator = false,
                            NoCommandTerminator = true
                        };

                        var tables = new string[] { "PaymentMethod", "Customer", "Bill", "Department", "Person", "Employs", "BaseQuestionType", "AnswerType", "Activates",
                            "PredefinedAnswerOption", "SurveyCategory", "Survey", "Asking", "AnswerOption", "Vote", "Chooses", "FaqEntry", "DataConstraint"};

                        foreach (var t in tables)
                        {
                            var c = db.Tables[t].EnumScript(options).Count();
                            logger.Debug($"Finished backup for table {t} successfully ({c} records)");
                        }
                    }

                    foreach (var f in Directory.GetFiles(HostingEnvironment.MapPath("~/Backups/")))
                    {
                        if (DateTime.TryParse(Path.GetFileNameWithoutExtension(f), out DateTime date))
                        {
                            if (DateTime.Now - date >= TimeSpan.FromDays(7))
                            {
                                File.Delete(f);
                                logger.Debug($"Deleted old backup file {Path.GetFileName(f)}");
                            }
                        }
                        else
                        {
                            logger.Warn($"Failed to delete old backup file due to invalid name: {Path.GetFileNameWithoutExtension(f)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Check for exceeded survey data failed unexpectedly.");
                }
                finally
                {
                    logger.Debug($"Next fire time {context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }
            });
        }
    }
}