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

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class Teacher_DashboardController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        private string TeacherID;
        public Teacher_DashboardController()
        {

            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }
        // GET: Teacher_Dashboard
        public ActionResult Teacher_Dashboard()
        {
            return View("Teacher_Dashboard");
        }

        public PartialViewResult Teacher_Subject()
        {
            var subjects = db.AspNetSubjects.Include(s => s.AspNetClass).Include(s => s.AspNetUser).Where(s => s.TeacherID == TeacherID);
            return PartialView("_Teacher_Subject", subjects);
        }

        public PartialViewResult Teacher_Announcement()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Teacher_Announcement");
        }

        public PartialViewResult Attendance()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Attendance");
        }
        public PartialViewResult Topics()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Topics");
        }
        

        /******************************************************************************************************************
         * 
         *                                       Common Classes
         *                                       
         ******************************************************************************************************************/
        public class Marks
        {
            public int Id { get; set; }
            public int GotMark { get; set; }

        }
        public class Students
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Reason { get; set; }
            public int SubjectID { get; set; }

        }



        /******************************************************************************************************************
         * 
         *                                       Common Filter Functions
         *                                       
         ******************************************************************************************************************/
        [HttpGet]
        public JsonResult ClassByTeacher()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetClass> classes = db.AspNetClasses.ToList();
            ViewBag.Subjects = classes;
            return Json(classes, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id && r.TeacherID == TeacherID).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult AssignmentsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetAssignment> assignments = (from assignment in db.AspNetAssignments
                                                  where assignment.SubjectID == id
                                                  select assignment).ToList();

            return Json(assignments, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SubmissionByAssignment(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var assignments = (from assignmentsubmission in db.Student_Assignment
                               where assignmentsubmission.AssignmentID == id
                               select new { assignmentsubmission, assignmentsubmission.AspNetUser.Name }).ToList();
           
            return Json(assignments, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult StudentBySubject(int id)
        {
            ApplicationDbContext d = new ApplicationDbContext();
            db.Configuration.ProxyCreationEnabled = false;
            List<string> studentsID = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Student")).Select(s => s.Id).ToList();
            var stu = (from t in db.AspNetUsers
                       join t1 in db.AspNetStudent_Subject on t.Id equals t1.StudentID
                       where studentsID.Contains(t.Id) && t1.SubjectID == id
                       select new { t.Id, t.Name }).ToList();
            return Json(stu, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult AnnouncementBySubject(int id)
        {
            ApplicationDbContext d = new ApplicationDbContext();

            db.Configuration.ProxyCreationEnabled = false;
            var Announcements = (from announcement_subject in db.AspNetAnnouncement_Subject
                                 where announcement_subject.SubjectID == id
                                 select new { announcement_subject.AspNetAnnouncement.Id, announcement_subject.AspNetAnnouncement.Title, announcement_subject.AspNetSubject.SubjectName }).ToList();

            return Json(Announcements, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult ExamsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetExam> exams = (from exam in db.AspNetExams
                                                  where exam.SubjectID == id
                                                  select exam).ToList();

            return Json(exams, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SubmissionByExam(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var exams = (from exam in db.AspNetStudent_Exam
                               where exam.ExamID == id
                               select new { exam, exam.AspNetUser.Name }).ToList();

            return Json(exams, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TestBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetTest> tests = (from test in db.AspNetTests
                                      where test.SubjectID == id
                                      select test).ToList();

            return Json(tests, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SubmissionByTest(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var tests = (from test in db.AspNetStudent_Test
                         where test.TestID == id
                         select new { test, test.AspNetUser.Name }).ToList();

            return Json(tests, JsonRequestBehavior.AllowGet);
        }
        /******************************************************************************************************************
         * 
         *                                       Assignments Function
         *                                       
         ******************************************************************************************************************/
        public PartialViewResult Assignment_Marks()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Assignments_Marks");
        }

        public PartialViewResult Submission_Students_Assignments(string id)
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Assignments_Submissions");
        }

        public FileResult Student_Assignment_Submitted_File(int id)
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

        [HttpPost]
        public void SaveAssignmentMarks(List<Marks> marks)
        {
            foreach (var item in marks)
            {
                Student_Assignment stu_assign = new Student_Assignment();
                stu_assign.Id = item.Id;
                stu_assign.MarksGot = item.GotMark;
                var check = db.Student_Assignment.Any(x => x.Id == stu_assign.Id);
                if (check)
                {
                    Student_Assignment student_assignment = (from x in db.Student_Assignment
                                                             where x.Id == stu_assign.Id
                                                             select x).First();
                    student_assignment.MarksGot = stu_assign.MarksGot;
                }
                db.SaveChanges();
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AssignmentMarksFromFile(Student_Assignment Stu_Assign)
        {
            HttpPostedFileBase file = Request.Files["AssignmentMarks"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            }

            List<Marks> MarkList = new List<Marks>();
            using (var package = new ExcelPackage(file.InputStream))
            {
                var currentSheet = package.Workbook.Worksheets;
                var workSheet = currentSheet.First();
                var noOfCol = workSheet.Dimension.End.Column;
                var noOfRow = workSheet.Dimension.End.Row;
                int AssignmentID = Convert.ToInt32(Request.Form["assignment"]);
                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    string UserName = workSheet.Cells[rowIterator, 1].Value.ToString();
                    Student_Assignment stu_assign;
                    try
                    {
                        stu_assign = (from student_assignment in db.Student_Assignment
                                      where student_assignment.AspNetUser.UserName == UserName && student_assignment.AssignmentID == AssignmentID
                                      select student_assignment).First();
                    }
                    catch
                    {
                        ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
                        ViewBag.Error = "There is a problem at row" + rowIterator;

                        return View("_Assignments_Marks", Stu_Assign);
                    }



                    Marks mark = new Marks();
                    mark.Id = stu_assign.Id;
                    mark.GotMark = Convert.ToInt32(workSheet.Cells[rowIterator, 2].Value.ToString());
                    MarkList.Add(mark);

                }
                foreach (var item in MarkList)
                {
                    Student_Assignment stu_assign = new Student_Assignment();
                    stu_assign.Id = item.Id;
                    stu_assign.MarksGot = item.GotMark;


                    var check = db.Student_Assignment.Any(x => x.Id == stu_assign.Id);
                    if (check)
                    {
                        Student_Assignment student_assignment = (from x in db.Student_Assignment
                                                                 where x.Id == stu_assign.Id
                                                                 select x).First();
                        student_assignment.MarksGot = stu_assign.MarksGot;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Teacher_Dashboard");
        }

        /******************************************************************************************************************
         * 
         *                                       Exams Function
         *                                       
         ******************************************************************************************************************/

        public PartialViewResult Exam_Marks()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Exam_Marks");
        }

        [HttpPost]
        public void SaveExamMarks(List<Marks> marks)
        {
            foreach (var item in marks)
            {
                AspNetStudent_Exam stu_exam = new AspNetStudent_Exam();
                stu_exam.Id = item.Id;
                stu_exam.MarksGot = item.GotMark;
                var check = db.AspNetStudent_Exam.Any(x => x.Id == stu_exam.Id);
                if (check)
                {
                    AspNetStudent_Exam student_exam = (from x in db.AspNetStudent_Exam
                                                       where x.Id == stu_exam.Id
                                                             select x).First();
                    student_exam.MarksGot = stu_exam.MarksGot;
                }
                db.SaveChanges();
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExamMarksFromFile(AspNetStudent_Exam Stu_Exam)
        {
            HttpPostedFileBase file = Request.Files["ExamMarks"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            }

            List<Marks> MarkList = new List<Marks>();
            using (var package = new ExcelPackage(file.InputStream))
            {
                var currentSheet = package.Workbook.Worksheets;
                var workSheet = currentSheet.First();
                var noOfCol = workSheet.Dimension.End.Column;
                var noOfRow = workSheet.Dimension.End.Row;
                int ExamID = Convert.ToInt32(Request.Form["exam"]);
                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    string UserName = workSheet.Cells[rowIterator, 1].Value.ToString();
                    AspNetStudent_Exam stu_exam;
                    try
                    {
                        stu_exam = (from student_exam in db.AspNetStudent_Exam
                                    where student_exam.AspNetUser.UserName == UserName && student_exam.ExamID == ExamID
                                    select student_exam).First();
                    }
                    catch
                    {
                        ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
                        ViewBag.Error = "There is a problem at row" + rowIterator;

                        return View("_Exam_Marks", Stu_Exam);
                    }



                    Marks mark = new Marks();
                    mark.Id = stu_exam.Id;
                    mark.GotMark = Convert.ToInt32(workSheet.Cells[rowIterator, 2].Value.ToString());
                    MarkList.Add(mark);

                }
                foreach (var item in MarkList)
                {
                    AspNetStudent_Exam stu_exam = new AspNetStudent_Exam();
                    stu_exam.Id = item.Id;
                    stu_exam.MarksGot = item.GotMark;


                    var check = db.AspNetStudent_Exam.Any(x => x.Id == stu_exam.Id);
                    if (check)
                    {
                        AspNetStudent_Exam student_exam = (from x in db.AspNetStudent_Exam
                                                                 where x.Id == stu_exam.Id
                                                                 select x).First();
                        student_exam.MarksGot = stu_exam.MarksGot;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Teacher_Dashboard");
        }
        /******************************************************************************************************************
         * 
         *                                       Test Function
         *                                       
         ******************************************************************************************************************/

        public PartialViewResult Test_Marks()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Test_Marks");
        }

        [HttpPost]
        public void SaveTestMarks(List<Marks> marks)
        {
            foreach (var item in marks)
            {
                AspNetStudent_Test stu_test = new AspNetStudent_Test();
                stu_test.Id = item.Id;
                stu_test.MarksGot = item.GotMark;
                var check = db.AspNetStudent_Exam.Any(x => x.Id == stu_test.Id);
                if (check)
                {
                    AspNetStudent_Test student_test = (from x in db.AspNetStudent_Test
                                                       where x.Id == stu_test.Id
                                                       select x).First();
                    student_test.MarksGot = stu_test.MarksGot;
                }
                db.SaveChanges();
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult TestMarksFromFile(AspNetStudent_Test Stu_Test)
        {
            HttpPostedFileBase file = Request.Files["TestMarks"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            }

            List<Marks> MarkList = new List<Marks>();
            using (var package = new ExcelPackage(file.InputStream))
            {
                var currentSheet = package.Workbook.Worksheets;
                var workSheet = currentSheet.First();
                var noOfCol = workSheet.Dimension.End.Column;
                var noOfRow = workSheet.Dimension.End.Row;
                int TestID = Convert.ToInt32(Request.Form["test"]);
                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    string UserName = workSheet.Cells[rowIterator, 1].Value.ToString();
                    AspNetStudent_Test stu_test;
                    try
                    {
                        stu_test = (from student_test in db.AspNetStudent_Test
                                    where student_test.AspNetUser.UserName == UserName && student_test.TestID == TestID
                                    select student_test).First();
                    }
                    catch
                    {
                        ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
                        ViewBag.Error = "There is a problem at row" + rowIterator;

                        return View("_Test_Marks", Stu_Test);
                    }



                    Marks mark = new Marks();
                    mark.Id = stu_test.Id;
                    mark.GotMark = Convert.ToInt32(workSheet.Cells[rowIterator, 2].Value.ToString());
                    MarkList.Add(mark);

                }
                foreach (var item in MarkList)
                {
                    AspNetStudent_Test stu_test = new AspNetStudent_Test();
                    stu_test.Id = item.Id;
                    stu_test.MarksGot = item.GotMark;


                    var check = db.AspNetStudent_Test.Any(x => x.Id == stu_test.Id);
                    if (check)
                    {
                        AspNetStudent_Test student_test = (from x in db.AspNetStudent_Test
                                                           where x.Id == stu_test.Id
                                                           select x).First();
                        student_test.MarksGot = stu_test.MarksGot;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Teacher_Dashboard");
        }
        /******************************************************************************************************************
         * 
         *                                       Attendance Function
         *                                       
         ******************************************************************************************************************/

        [HttpPost]
        public void Attendance(List<Students> stu)
        {
            foreach (var item in stu)
            {
                AspNetAttendance attendance = new AspNetAttendance();
                attendance.StudentID = item.Id;
                attendance.SubjectID = item.SubjectID;
                attendance.Status = item.Status;
                attendance.Reason = item.Reason;
                attendance.Date = DateTime.Now.Date;
                var check = db.AspNetAttendances.Any(x => x.StudentID == attendance.StudentID && x.SubjectID == attendance.SubjectID && x.Date == attendance.Date);
                if (check)
                {
                    AspNetAttendance attend = (from x in db.AspNetAttendances
                                               where x.StudentID == attendance.StudentID && x.SubjectID == attendance.SubjectID && x.Date == attendance.Date
                                               select x).First();
                    attend.Status = attendance.Status;
                    attend.Reason = attendance.Reason;


                }
                else
                {
                    db.AspNetAttendances.Add(attendance);
                }
                db.SaveChanges();
            }
        }
        
       

    }
}