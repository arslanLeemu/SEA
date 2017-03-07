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

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Student")]
    public class Student_DashboardController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        private string StudentID;

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
           Student_Assignment assignment = db.Student_Assignment.Include(x=>x.AspNetAssignment.AspNetAssignment_Topic).Where(x => x.AssignmentID == id && x.StudentID == StudentID).FirstOrDefault();
            if(assignment.SubmittedFileName==null)
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
                                      select new { student_assignment.AspNetAssignment.Title,student_assignment.AspNetAssignment.DueDate, student_assignment.AspNetAssignment.TotalMarks, student_assignment.MarksGot }).ToList();
            foreach(var item in student_Assignment)
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
        
    }
}