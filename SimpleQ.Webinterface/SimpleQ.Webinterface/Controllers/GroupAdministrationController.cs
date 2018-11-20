using SimpleQ.Webinterface.Extensions;
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
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found");

                var model = new GroupAdministrationModel
                {
                    Departments = db.Departments.Where(d => d.CustCode == CustCode).ToList()
                };

                return View(viewName: "GroupAdministration", model: model);
            }
        }

        [HttpGet]
        public ActionResult Create(string depName)
        {
            if (string.IsNullOrEmpty(depName))
                return Http.BadRequest("Department name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found.");

                var query = db.Departments.Where(d => d.CustCode == CustCode);

                var dep = new Department
                {
                    DepId = (query.Count() == 0) ? 1 : query.Max(d => d.DepId) + 1,
                    DepName = depName,
                    CustCode = CustCode
                };
                db.Departments.Add(dep);
                db.SaveChanges();

                return Content($"{dep.DepId}", "text/plain");
            }
        }

        [HttpGet]
        public ActionResult Modify(int depId, string depName)
        {
            if (string.IsNullOrEmpty(depName))
                return Http.BadRequest("Department name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                if (dep == null)
                    return Http.NotFound("Department not found.");

                dep.DepName = depName;
                db.SaveChanges();
            }
            return Http.Ok();
        }

        [HttpGet]
        public ActionResult Delete(int depId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                if (dep == null)
                    return Http.NotFound("Department not found.");

                db.Departments.Remove(dep);
                db.SaveChanges();
            }
            return Http.Ok();
        }


        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}