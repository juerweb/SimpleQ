using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class GroupAdministrationController : Controller
    {
        [HttpGet]
        public ActionResult List()
        {
            using (var db = new SimpleQDBEntities())
            {
                var model = new GroupAdministrationModel
                {
                    Departments = db.Departments.Where(d => d.CustCode == CustCode).ToList()
                };

                return PartialView(viewName: "_GroupAdministration", model: model);
            }
        }

        [HttpGet]
        public ActionResult Create(string depName)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Departments.Add(new Department {
                    DepId = db.Departments.Where(d => d.CustCode == CustCode).Max(d => d.DepId) + 1,
                    DepName = depName,
                    CustCode = CustCode
                });
                db.SaveChanges();
            }
            return List();
        }

        [HttpGet]
        public ActionResult Modify(int depId, string depName)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault().DepName = depName;
                db.SaveChanges();
            }
            return List();
        }

        [HttpGet]
        public ActionResult Delete(int depId)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Departments.RemoveRange(db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode));
                db.SaveChanges();
            }
            return List();
        }


        private string CustCode
        {
            get
            {
                return "m4rku5";//Session["custCode"] as string;
            }
        }
    }
}