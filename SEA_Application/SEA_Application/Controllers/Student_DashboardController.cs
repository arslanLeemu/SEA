using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using System.IO;
using System.Web.UI;
using System.Text;
using NReco.PdfGenerator;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.parser;

namespace SEA_Application.Controllers
{
    
    [Authorize(Roles = "Student")]
    public class Student_DashboardController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        private string StudentID;

        public string OUTPUT_FILE { get; private set; }

        public Student_DashboardController()
        {

            StudentID = Convert.ToString(System.Web.HttpContext.Current.Session["StudentID"]);
        }

        // GET: Student_Dashboard
        public ActionResult Student_Dashboard()
        {
            return View("Student_Dashboard");
        }

        public PartialViewResult Student_Assignments()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return PartialView("_Student_Assignment");
        }

        public PartialViewResult Student_Marks()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return PartialView("_Student_Marks");
        }

        public PartialViewResult Student_Attendance()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return PartialView("_Student_Attendance");
        }

        public ActionResult Student_Assignment_Submission(int id)
        {
            //Student_Assignment assignments = db.Student_Assignment.Find(id);
            Student_Assignment assignment = db.Student_Assignment.Include(x => x.AspNetAssignment.AspNetAssignment_Topic).Where(x => x.AssignmentID == id && x.StudentID == StudentID).FirstOrDefault();
            if (assignment.SubmittedFileName == null)
            {
                assignment.SubmittedFileName = " ";
            }
            if (assignment.AspNetAssignment.FileName == null)
            {
                assignment.AspNetAssignment.FileName = " ";
            }
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Student_Assignment_Submission(Student_Assignment stu_assignmnet)
        {
            int id = Convert.ToInt32(Request.Form["id"]);
            Student_Assignment result = (from p in db.Student_Assignment
                                         where p.Id == id
                                         select p).SingleOrDefault();

            HttpPostedFileBase file = Request.Files["document"];
            if (ModelState.IsValid)
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/App_Data/SubmittedAssignments"), fileName);
                    file.SaveAs(path);
                    result.SubmittedFileName = fileName;
                }
            }


            result.Status = "Submitted";
            result.Date = DateTime.Now;

            db.SaveChanges();
            return RedirectToAction("Student_Assignment_Submission", new { id = result.AssignmentID });
        }


        public FileResult downloadSubmittedFile(int id)
        {
            Student_Assignment Assignment = db.Student_Assignment.Find(id);
            try
            {
                var filepath = System.IO.Path.Combine(Server.MapPath("~/App_Data/SubmittedAssignments/"), Assignment.SubmittedFileName);
                return File(filepath, MimeMapping.GetMimeMapping(filepath), Assignment.SubmittedFileName);
            }
            catch
            {
                return null;
            }

        }

        public FileResult downloadFile(int id)
        {
            AspNetAssignment Assignment = db.AspNetAssignments.Find(id);
            try
            {
                var filepath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Assignments/"), Assignment.FileName);
                return File(filepath, MimeMapping.GetMimeMapping(filepath), Assignment.FileName);
            }
            catch
            {
                return null;
            }

        }

        // GET: Assignment/Details/5
        public ActionResult AnnouncementDetail(int? id)
        {

            var announcementID = db.AspNetAnnouncement_Subject.Where(s => s.Id == id).Select(s => s.AnnouncementID).SingleOrDefault();
            AspNetStudent_Announcement result = (from p in db.AspNetStudent_Announcement

                                                 where p.StudentID == StudentID && p.AnnouncementID == announcementID
                                                 select p).SingleOrDefault();
            result.Seen = true;
            db.SaveChanges();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // List<int> student = db.Student_Subject.Where(x => SubjectIDs.Contains(x.SubjectID)).Select(s => s.StudentID).ToList();

            var Announcement = db.AspNetAnnouncement_Subject.Find(id);

            if (Announcement == null)
            {
                return HttpNotFound();
            }
            return View(Announcement);
        }

        public PartialViewResult Student_Announcement()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return PartialView("_Student_Announcement");
        }



        [HttpGet]
        public JsonResult StudentAnnouncement()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var ann2 = (from t1 in db.AspNetAnnouncement_Subject
                        join t3 in db.AspNetStudent_Subject on t1.SubjectID equals t3.SubjectID
                        join t4 in db.AspNetUsers on t3.StudentID equals t4.Id
                        join t2 in db.AspNetStudent_Announcement on t1.AnnouncementID equals t2.AnnouncementID

                        where t2.StudentID == StudentID && t2.Seen == false && t4.Id == StudentID
                        select new { t1.AspNetAnnouncement.Title, t1.Id }).ToList();

            ViewBag.Subjects = ann2;
            return Json(ann2, JsonRequestBehavior.AllowGet);

        }

        public class Marks
        {
            public string Title { get; set; }
            public string DueDate { get; set; }
            public int? TotalMarks { get; set; }
            public Double? MarksGot { get; set; }
            public string Type { get; set; }
        }

        [HttpGet]
        public JsonResult MarksBySubject(int subjectID)
        {
            List<Marks> marks = new List<Marks>();
            var student_Assignment = (from student_assignment in db.Student_Assignment
                                      where student_assignment.AspNetAssignment.SubjectID == subjectID && student_assignment.StudentID == StudentID
                                      select new { student_assignment.AspNetAssignment.Title, student_assignment.AspNetAssignment.DueDate, student_assignment.AspNetAssignment.TotalMarks, student_assignment.MarksGot }).ToList();
            foreach (var item in student_Assignment)
            {
                Marks mark = new Marks();
                mark.DueDate = item.DueDate.ToString();
                mark.Title = item.Title;
                mark.Type = "Assignment";
                mark.TotalMarks = item.TotalMarks;
                mark.MarksGot = item.MarksGot;
                marks.Add(mark);
            }
            var student_Test = (from student_test in db.AspNetStudent_Test
                                where student_test.AspNetTest.SubjectID == subjectID && student_test.StudentID == StudentID
                                select new { student_test.AspNetTest.Title, student_test.AspNetTest.Date, student_test.AspNetTest.TotalMarks, student_test.MarksGot }).ToList();
            foreach (var item in student_Test)
            {
                Marks mark = new Marks();
                mark.DueDate = item.Date.ToString();
                mark.Title = item.Title;
                mark.Type = "Test";
                mark.TotalMarks = item.TotalMarks;
                mark.MarksGot = item.MarksGot;
                marks.Add(mark);
            }

            var student_Exam = (from student_exam in db.AspNetStudent_Exam
                                where student_exam.AspNetExam.SubjectID == subjectID && student_exam.StudentID == StudentID
                                select new { student_exam.AspNetExam.Title, student_exam.AspNetExam.Date, student_exam.AspNetExam.TotalMarks, student_exam.MarksGot }).ToList();
            foreach (var item in student_Exam)
            {
                Marks mark = new Marks();
                mark.DueDate = item.Date.ToString();
                mark.Title = item.Title;
                mark.Type = "Exam";
                mark.TotalMarks = item.TotalMarks;
                mark.MarksGot = item.MarksGot;
                marks.Add(mark);
            }
            return Json(marks, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AttendanceBySubject(int subjectID)
        {
            var attendances = (from attendance in db.AspNetAttendances
                               where attendance.Status == "Absent" && attendance.StudentID == StudentID && attendance.SubjectID == subjectID
                               select new { attendance.Reason, attendance.Status, attendance.Date }).ToList();
            return Json(attendances, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult AssignmentsBySubject(int subjectID)
        {
            var assignments = (from assignment in db.AspNetAssignments
                               where assignment.SubjectID == subjectID
                               select new { assignment.Title, assignment.Id, assignment.Description }).ToList();
            return Json(assignments, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult AnnouncementBySubject(int subjectID)
        {
            var announcements = (from announcement in db.AspNetAnnouncement_Subject
                                 where announcement.SubjectID == subjectID
                                 select new { announcement.AspNetAnnouncement.Title, announcement.AspNetSubject.SubjectName, announcement.Id }).ToList();
            return Json(announcements, JsonRequestBehavior.AllowGet);
        }

        public class feeType
        {
            public string typeName { get; set; }
            public int amount { get; set; }
        }
        public class challanform
        {
            public string SchoolName { get; set; }
            public string BranchName { get; set; }
            public string ChallanCopy { get; set; }
            public DateTime? AcademicSessionStart { get; set; }
            public DateTime? AcademicSessionEnd { get; set; }
            public int? ChallanID { get; set; }
            public string UserID { get; set; }
            public string StudentName { get; set; }
            public string StudentClass { get; set; }
            public List<feeType> FeeType { get; set; }
            public List<String> DiscountNotes { get; set; }
            public DateTime? DueDate { get; set; }
            public List<String> Notes { get; set; }
            public DateTime PrintedDate { get; set; }
            public int TotalAmount { get; set; }


        }
        public byte[] GenerateInvoicePDF(object sender, EventArgs e)
        {
            AspNetClass studentClass = (from student_subject in db.AspNetStudent_Subject
                                        join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                                        where student_subject.StudentID == StudentID
                                        select subject.AspNetClass).FirstOrDefault();
            challanform Challan = new challanform();
            Challan.AcademicSessionStart = db.AspNetSessions.OrderByDescending(x => x.Id).Select(x => x.SessionStartDate).FirstOrDefault();
            Challan.AcademicSessionEnd = db.AspNetSessions.OrderByDescending(x => x.Id).Select(x => x.SessionEndDate).FirstOrDefault();
            Challan.ChallanID = db.AspNetStudent_Payment.Where(x => x.StudentID == StudentID).OrderByDescending(x => x.Id).Select(x => x.FeeChallanID).FirstOrDefault();
            Challan.StudentName = db.AspNetUsers.Where(x => x.Id == StudentID).Select(x => x.Name).FirstOrDefault();
            Challan.StudentClass = studentClass.ClassName;
            Challan.SchoolName = "LGS";
            Challan.BranchName = "Cantt";
            Challan.ChallanCopy = "Student Copy";
/*
            int? discountSum = db.AspNetStudent_Discount.Where(x => x.StudentID == StudentID).Sum(x => x.Percentage);
            if (discountSum == null)
            {
                discountSum = 0;
            }
            int? tuitionFee = db.AspNetClass_FeeType.Where(x => x.AspNetFeeType.TypeName == "Tuition Fee").Select(x => x.Amount).FirstOrDefault();
            int? discount = tuitionFee * discountSum / 100;
            int payabletuitionfee = Convert.ToInt32(tuitionFee - discount);*/
            string FeeDurationTypeName = (from durationtype in db.AspNetDurationTypes
                                          join challan in db.AspNetFeeChallans on durationtype.Id equals challan.DurationTypeID
                                          where challan.Id == Challan.ChallanID
                                          select durationtype.TypeName).FirstOrDefault();
            int? payableAmount = db.AspNetStudent_Payment.Where(x => x.StudentID == StudentID && x.FeeChallanID == Challan.ChallanID).Select(x => x.PaymentAmount).SingleOrDefault();


            var others = (from feetype in db.AspNetFeeTypes
                          join class_feetype in db.AspNetClass_FeeType on feetype.Id equals class_feetype.FeeTypeID
                          where class_feetype.ClassID == studentClass.Id && feetype.TypeName != "Tuition Fee"
                          select new { class_feetype.Amount, feetype.TypeName }).ToList();

            Challan.FeeType = new List<feeType>();
            foreach (var item in others)
            {
                feeType FeeTypes = new feeType();
                FeeTypes.typeName = item.TypeName;

                if (FeeDurationTypeName == "Quarterly")
                {
                    FeeTypes.amount = Convert.ToInt32(item.Amount * 4);
                }
                else if (FeeDurationTypeName == "6 Months")
                {
                    FeeTypes.amount = Convert.ToInt32(item.Amount * 6);
                }
                else if (FeeDurationTypeName == "Yearly")
                {
                    FeeTypes.amount = Convert.ToInt32(item.Amount * 12);
                }
                else
                {
                    FeeTypes.amount = Convert.ToInt32(item.Amount);
                }

                Challan.FeeType.Add(FeeTypes);
            }

            int? tuitionfee=payableAmount- Challan.FeeType.Sum(x => x.amount);

            feeType FeeType = new feeType();
            FeeType.typeName = "Tuition Fee";
            FeeType.amount =Convert.ToInt32(tuitionfee);
            Challan.FeeType.Add(FeeType);

            Challan.DueDate = db.AspNetFeeChallans.Where(x => x.Id == Challan.ChallanID).Select(x => x.DueDate).FirstOrDefault();
            Challan.TotalAmount = Challan.FeeType.Sum(x => x.amount);

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    StringBuilder sb = new StringBuilder();
                  
                
                    sb.Append("<div style='margin-right: -15px;margin - left: -15px;'> ");
                    sb.Append("<div style='width: 33.33333333%; float:left;'>");
                    sb.Append("<center><font size='3'><b>"+Challan.SchoolName+"</b></font></center>");
                    sb.Append("<center><font size='2'><b>" + Challan.BranchName + "</b></font></center>");
                    sb.Append("<br/>");
                    sb.Append("<center><font size='3'><b>" + Challan.ChallanCopy + "</b></font></center>");

                    sb.Append("<section style='border: 1px solid black; margin - top:10px; padding: 7px;'> ");
                    sb.Append("<table>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td><font size='2'><b>Challan ID:</b></font></td>");
                    sb.Append("<td align='center'><font size='2'>" + Challan.ChallanID + "</font></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><font size='2'><b>Academic Session:</b></font></td>");
                    sb.Append("<td align='center'><font size='2'>" + Challan.AcademicSessionStart+" - "+Challan.AcademicSessionEnd + "</font></td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</section>");

                    sb.Append("</div>");
                    sb.Append("</div>");

                   
                    var htmlContent = String.Format(sb.ToString(),
        DateTime.Now);
                    var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                   var bytes=htmlToPdf.GeneratePdf(htmlContent);
                    Response.Clear();
                    MemoryStream ms = new MemoryStream(bytes);
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=labtest.pdf");
                    Response.Buffer = true;
                    ms.WriteTo(Response.OutputStream);
                    Response.End();
                    /*FileStream fs = new FileStream(@"g://somepath.pdf", FileMode.OpenOrCreate);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();*/
                    return bytes;


                    /*
                    sb.Append("< div class='row'>");

                    sb.Append("<div class='col-sm-4' style='width:400px'>");
                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr rowspan='2'>");
                    sb.Append("<td>Academic Session:</td>");
                    sb.Append("<td>" + Challan.AcademicSessionStart + "-" + Challan.AcademicSessionEnd + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Challan ID:</td>");
                    sb.Append("<td>" + Challan.ChallanID + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student ID:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student Name:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Class:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentClass + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well' style='height:400px'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    foreach (var item in Challan.FeeType)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td>" + item.typeName + "</td>");
                        sb.Append("<td align='right'> Rs </td>");
                        sb.Append("<td align='right'>" + item.amount + "</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");


                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>"); 
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Due Date of Payment:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.DueDate + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Amount:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.TotalAmount + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("</div>");

                    sb.Append("<div class='col-sm-4' style='width:400px'>");
                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr rowspan='2'>");
                    sb.Append("<td>Academic Session:</td>");
                    sb.Append("<td>" + Challan.AcademicSessionStart + "-" + Challan.AcademicSessionEnd + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Challan ID:</td>");
                    sb.Append("<td>" + Challan.ChallanID + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student ID:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student Name:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Class:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentClass + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well' style='height:400px'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    foreach (var item in Challan.FeeType)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td>" + item.typeName + "</td>");
                        sb.Append("<td align='right'> Rs </td>");
                        sb.Append("<td align='right'>" + item.amount + "</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");


                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Due Date of Payment:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.DueDate + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Amount:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.TotalAmount + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("</div>");

                    sb.Append("<div class='col-sm-4' style='width:400px'>");
                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr rowspan='2'>");
                    sb.Append("<td>Academic Session:</td>");
                    sb.Append("<td>" + Challan.AcademicSessionStart + "-" + Challan.AcademicSessionEnd + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Challan ID:</td>");
                    sb.Append("<td>" + Challan.ChallanID + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student ID:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Student Name:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentName + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Class:</td>");
                    sb.Append("<td align='right'>" + Challan.StudentClass + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");

                    sb.Append("<div class='panel panel-default well' style='height:400px'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    foreach (var item in Challan.FeeType)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td>" + item.typeName + "</td>");
                        sb.Append("<td align='right'> Rs </td>");
                        sb.Append("<td align='right'>" + item.amount + "</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("<br/>");


                    sb.Append("<div class='panel panel-default well'>");
                    sb.Append("<table class='table'>");
                    sb.Append("<tbody>");
                    sb.Append("<tr>");
                    sb.Append("<td>Due Date of Payment:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.DueDate + "</td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>Amount:</td>");
                    sb.Append("<td></td>");
                    sb.Append("<td align='right'>" + Challan.TotalAmount + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("</div>");
                    sb.Append("</div>");

                    sb.Append("</div>");

                   
                */


                    /*   StringReader sr = new StringReader(sb.ToString());




                       List<string> cssFiles = new List<string>();
                       cssFiles.Add(@"/Content/bootstrap.css");
                       cssFiles.Add(@"/Content/challanform.css");

                       var output = new MemoryStream();

                       var input = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

                       var document = new Document();
                       var writer = PdfWriter.GetInstance(document, output);
                       writer.CloseStream = false;

                       document.Open();
                       var htmlContext = new HtmlPipelineContext(null);
                       htmlContext.SetTagFactory(iTextSharp.tool.xml.html.Tags.GetHtmlTagProcessorFactory());

                       ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);
                       cssFiles.ForEach(i => cssResolver.AddCssFile(System.Web.HttpContext.Current.Server.MapPath(i), true));

                       var pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(document, writer)));
                       var worker = new XMLWorker(pipeline, true);
                       var p = new XMLParser(worker);
                       p.Parse(input);
                       document.Close();
                       output.Position = 0;


                       Response.Clear();
                       Response.ContentType = "application/pdf";
                       Response.AddHeader("Content-Disposition", "attachment; filename=myfile.pdf");
                       Response.BinaryWrite(output.ToArray());
                       // myMemoryStream.WriteTo(Response.OutputStream); //works too
                       Response.Flush();
                       Response.Close();
                       Response.End();


                   */

                    /*
                    
                      Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);


                      HTMLWorker htmlparser = new HTMLWorker(pdfDoc);


                      PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                      pdfDoc.Open();
                      htmlparser.Parse();
                      pdfDoc.Close();
                      Response.ContentType = "application/pdf";

                      Response.AddHeader("content-disposition", "attachment;filename=challanform.pdf");
                      Response.Cache.SetCacheability(HttpCacheability.NoCache);
                      Response.Write(pdfDoc);
                      Response.End();
                    */
                }

            }
        }
        


    }
}