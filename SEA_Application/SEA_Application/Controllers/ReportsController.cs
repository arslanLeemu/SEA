using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    public class ReportsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Assignment_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Assignment_Report");
        }

        public class reportbyassignment
        {
            public string result { get; set; }
            public int number { get; set; }
            public string color { get; set; }
        }

        [HttpGet]
        public JsonResult ReportByAssignment(int assignment,int percentage)
        {
            List<double?> stu_assign = (from stu_assignment in db.Student_Assignment
                                       where stu_assignment.AssignmentID == assignment
                                       select stu_assignment.MarksGot).ToList();
            double? totalMarks = db.AspNetAssignments.Where(x => x.Id == assignment).Select(x => x.TotalMarks).FirstOrDefault();
            int passcount = 0;
            int failcount = 0;
            foreach(var item in stu_assign)
            {
                double? perc = (item / totalMarks * 100);
                if (perc < percentage)
                {
                    failcount++;
                }
                else
                {
                    passcount++;
                }
            }
            List<reportbyassignment> reportAssignment = new List<reportbyassignment>();
            reportbyassignment passreport = new reportbyassignment();
            passreport.result = "Pass";
            passreport.number = passcount;
            passreport.color = "#4F52BA";
            reportbyassignment failreport = new reportbyassignment();
            failreport.result = "Fail";
            failreport.number = failcount;
            failreport.color = "#ef553a";
            reportAssignment.Add(passreport);
            reportAssignment.Add(failreport);
            return Json(reportAssignment, JsonRequestBehavior.AllowGet);

        }
    }
}