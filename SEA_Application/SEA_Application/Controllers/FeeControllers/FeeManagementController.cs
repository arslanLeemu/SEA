using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using OfficeOpenXml;

namespace SEA_Application.Controllers.FeeControllers
{
    public class FeeManagementController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        // GET: FeeManagement
        public ActionResult Index()
        {
            return View("FeeManagement_Dashboard");
        }

        public PartialViewResult Fee_Type()
        {
            var feetypes = db.AspNetFeeTypes.ToList() ;
            return PartialView("_Fee_Type", feetypes);
        }

        public PartialViewResult Discount_Type()
        {
            var feetypes = db.AspNetDiscountTypes.ToList();
            return PartialView("_Discount_Type", feetypes);
        }

        public PartialViewResult Duration_Type()
        {
            var feetypes = db.AspNetDurationTypes.ToList();
            return PartialView("_Fee_Duration", feetypes);
        }

        public PartialViewResult Class_Fee()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Class_Fee");
        }

        public PartialViewResult Student_Discount()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Student_Discount");
        }

        public PartialViewResult Challan_Form()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Challan_Form");
        }

        public PartialViewResult Defaulters()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Defaulters");
        }

        public PartialViewResult Student_History()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Student_History");
        }

        [HttpGet]
        public JsonResult FeeTypeByClass(int classID)
        {

            var feeTypes = (from class_fee in db.AspNetClass_FeeType
                            where class_fee.ClassID == classID
                            select new { class_fee.AspNetClass.ClassName, class_fee.AspNetFeeType.TypeName, class_fee.Amount, class_fee.Id }).ToList();
            return Json(feeTypes, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult DiscountByStudent(string studentID)
        {

            var studentdiscount = (from student_discount in db.AspNetStudent_Discount
                            where student_discount.StudentID == studentID
                                   select new { student_discount.AspNetDiscountType.TypeName, student_discount.Percentage, student_discount.Id }).ToList();
            return Json(studentdiscount, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult StudentsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var students = (from student_subject in db.AspNetStudent_Subject
                            join student in db.AspNetUsers on student_subject.StudentID equals student.Id
                            where student_subject.SubjectID == id
                            select new { student.UserName, student.Name, student.Id }).ToList();

            return Json(students, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult ChallanByClass(int id)
        {
            var challan = (from challans in db.AspNetFeeChallans
                           where challans.ClassID == id
                           select new { challans.Id, challans.TotalAmount, challans.DueDate, challans.EndDate, challans.AspNetDurationType.TypeName, challans.StartDate, challans.AspNetClass.ClassName }).ToList();

            return Json(challan, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult StudentsByClass(int id)
        {
            var students = (from student in db.AspNetUsers
                                         join student_subject in db.AspNetStudent_Subject on student.Id equals student_subject.StudentID
                                         join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                                         where subject.ClassID == id
                                         select new { student.Id, student.UserName, student.Name }).Distinct().ToList();
            return Json(students, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DefaultersByClass(int id)
        {
            var student_payments = (from a in db.AspNetStudent_Payment
                                   select a).ToList();
            var students = (from student in db.AspNetUsers
                            join student_subject in db.AspNetStudent_Subject on student.Id equals student_subject.StudentID
                            join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                            join student_payment in db.AspNetStudent_Payment on student.Id equals student_payment.StudentID
                            where subject.ClassID == id && student_payment.PaymentDate==null
                            select new { student.Id, student.UserName, student.Name }).Distinct().ToList();
            return Json(students, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult HistoryByStudent(string studentID)
        {

            var history = (from student_payment in db.AspNetStudent_Payment
                           where student_payment.StudentID == studentID
                           orderby student_payment.FeeChallanID descending
                           select new { student_payment.FeeChallanID, student_payment.AspNetFeeChallan.AspNetDurationType.TypeName, student_payment.AspNetFeeChallan.StartDate, student_payment.AspNetFeeChallan.EndDate, student_payment.AspNetFeeChallan.TotalAmount, student_payment.PaymentAmount, student_payment.PaymentDate }).ToList();
            return Json(history, JsonRequestBehavior.AllowGet);

        }

    }
}