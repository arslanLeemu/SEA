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

        public PartialViewResult MultipleAssignment_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_MultipleAssignment_Report");
        }
        public PartialViewResult Test_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Test_Report");
        }
        public PartialViewResult MultipleTest_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_MultipleTest_Report");
        }
        public PartialViewResult Exam_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Exam_Report");
        }
        public PartialViewResult MultipleExam_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_MultipleExam_Report");
        }

        public PartialViewResult Student_Report()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return PartialView("_Student_Report");
        }
        public class pieChartReport
        {
            public string result { get; set; }
            public int number { get; set; }
            public string color { get; set; }
        }
        public class bands
        {
            public string color { get; set; }
            public int endValue { get; set; }
            public int startValue { get; set; }
        }
        public class average
        {
            public bands[] band { get; set; }
            public int average_marks { get; set; }
        }
        public class TableResultReport
        {
            public string UserName { get; set; }
            public string Name { get; set; }
            public double? MarksGot { get; set; }
            public double? Percentage { get; set; }
            public string Status { get; set; }
        }
        public class barChartReport
        {
            public string Assignment { get; set; }
            public string Exam { get; set; }
            public string Test { get; set; }
            public int Pass { get; set; }
            public int Fail { get; set; }
        }


        [HttpGet]
        public JsonResult ReportByAssignment(int assignment, int percentage)
        {
            List<double?> stu_assign = (from stu_assignment in db.Student_Assignment
                                        where stu_assignment.AssignmentID == assignment
                                        select stu_assignment.MarksGot).ToList();
            double? totalMarks = db.AspNetAssignments.Where(x => x.Id == assignment).Select(x => x.TotalMarks).FirstOrDefault();
            int passcount = 0;
            int failcount = 0;
            foreach (var item in stu_assign)
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
            List<pieChartReport> reportAssignment = new List<pieChartReport>();
            pieChartReport passreport = new pieChartReport();
            passreport.result = "Pass";
            passreport.number = passcount;
            passreport.color = "#4F52BA";
            pieChartReport failreport = new pieChartReport();
            failreport.result = "Fail";
            failreport.number = failcount;
            failreport.color = "#ef553a";
            reportAssignment.Add(passreport);
            reportAssignment.Add(failreport);
            return Json(reportAssignment, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult ReportByMultipleAssignment(int subject, int percentage)
        {
            List<barChartReport> reportMultipleAssignment = new List<barChartReport>();
            List<AspNetAssignment> assignments = db.AspNetAssignments.Where(x => x.SubjectID == subject).ToList();
            foreach (var assignment in assignments)
            {
                int passcount = 0;
                int failcount = 0;
                List<double?> stu_assign = db.Student_Assignment.Where(x => x.AssignmentID == assignment.Id).Select(x => x.MarksGot).ToList();
                foreach (var student_assignment in stu_assign)
                {
                    double? perc = (student_assignment / assignment.TotalMarks * 100);
                    if (perc < percentage)
                    {
                        failcount++;
                    }
                    else
                    {
                        passcount++;
                    }
                }
                barChartReport multipleAssignment = new barChartReport();
                multipleAssignment.Assignment = assignment.Title;
                multipleAssignment.Pass = passcount;
                multipleAssignment.Fail = failcount;
                reportMultipleAssignment.Add(multipleAssignment);
            }

            return Json(reportMultipleAssignment, JsonRequestBehavior.AllowGet);

        }
        public JsonResult AverageByAssignment(int assignment, int percentage)
        {
            bands start = new bands();
            double? totalMarks = db.AspNetAssignments.Where(x => x.Id == assignment).Select(x => x.TotalMarks).FirstOrDefault();
            int passignmarks = Convert.ToInt32(totalMarks * percentage / 100);
            start.startValue = 0;
            start.endValue = passignmarks;
            start.color = "#cc4748";
            bands middle = new bands();
            middle.startValue = start.endValue;
            middle.endValue = Convert.ToInt32(totalMarks * 80 / 100);
            middle.color = "#fdd400";
            bands end = new bands();
            end.startValue = middle.endValue;
            end.endValue = Convert.ToInt32(totalMarks);
            end.color = "#84b761";

            average averageAssignment = new average();
            averageAssignment.band = new bands[3];
            averageAssignment.band[0] = start;
            averageAssignment.band[1] = middle;
            averageAssignment.band[2] = end;

            List<double?> stu_assignment = (from student_assignment in db.Student_Assignment
                                            where student_assignment.AssignmentID == assignment
                                            select student_assignment.MarksGot).ToList();
            double? count = 0;
            int n = 0;
            foreach (var item in stu_assignment)
            {
                count = count + item;
                n++;
            }
            averageAssignment.average_marks = Convert.ToInt32(count / n);
            return Json(averageAssignment, JsonRequestBehavior.AllowGet);
        }



        public JsonResult AssignmentResult_Report(int assignment, int percentage)
        {
            List<Student_Assignment> stu_assignment = db.Student_Assignment.Where(x => x.AssignmentID == assignment).ToList();
            List<TableResultReport> assignment_report = new List<TableResultReport>();
            foreach (var item in stu_assignment)
            {
                TableResultReport assignmentreport = new TableResultReport();
                assignmentreport.MarksGot = item.MarksGot;
                assignmentreport.Name = item.AspNetUser.Name;
                assignmentreport.UserName = item.AspNetUser.UserName;
                assignmentreport.Percentage = item.MarksGot / item.AspNetAssignment.TotalMarks * 100;
                if (assignmentreport.Percentage < percentage)
                {
                    assignmentreport.Status = "Fail";
                }
                else
                {
                    assignmentreport.Status = "Pass";
                }
                assignment_report.Add(assignmentreport);

            }
            return Json(assignment_report, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult ReportByTest(int test, int percentage)
        {
            List<double?> stu_test = (from student_test in db.AspNetStudent_Test
                                      where student_test.TestID == test
                                      select student_test.MarksGot).ToList();
            double? totalMarks = db.AspNetTests.Where(x => x.Id == test).Select(x => x.TotalMarks).FirstOrDefault();
            int passcount = 0;
            int failcount = 0;
            foreach (var item in stu_test)
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
            List<pieChartReport> reportbyTest = new List<pieChartReport>();
            pieChartReport passreport = new pieChartReport();
            passreport.result = "Pass";
            passreport.number = passcount;
            passreport.color = "#4F52BA";
            pieChartReport failreport = new pieChartReport();
            failreport.result = "Fail";
            failreport.number = failcount;
            failreport.color = "#ef553a";
            reportbyTest.Add(passreport);
            reportbyTest.Add(failreport);
            return Json(reportbyTest, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public JsonResult ReportByMultipleTest(int subject, int percentage)
        {
            List<barChartReport> reportMultipleTest = new List<barChartReport>();
            List<AspNetTest> tests = db.AspNetTests.Where(x => x.SubjectID == subject).ToList();
            foreach (var test in tests)
            {
                int passcount = 0;
                int failcount = 0;
                List<double?> stu_test = db.AspNetStudent_Test.Where(x => x.TestID == test.Id).Select(x => x.MarksGot).ToList();
                foreach (var student_test in stu_test)
                {
                    double? perc = (student_test / test.TotalMarks * 100);
                    if (perc < percentage)
                    {
                        failcount++;
                    }
                    else
                    {
                        passcount++;
                    }
                }
                barChartReport multipleTest = new barChartReport();
                multipleTest.Test = test.Title;
                multipleTest.Pass = passcount;
                multipleTest.Fail = failcount;
                reportMultipleTest.Add(multipleTest);
            }

            return Json(reportMultipleTest, JsonRequestBehavior.AllowGet);

        }

        public JsonResult AverageByTest(int test, int percentage)
        {
            bands start = new bands();
            double? totalMarks = db.AspNetTests.Where(x => x.Id == test).Select(x => x.TotalMarks).FirstOrDefault();
            int passignmarks = Convert.ToInt32(totalMarks * percentage / 100);
            start.startValue = 0;
            start.endValue = passignmarks;
            start.color = "#cc4748";
            bands middle = new bands();
            middle.startValue = start.endValue;
            middle.endValue = Convert.ToInt32(totalMarks * 80 / 100);
            middle.color = "#fdd400";
            bands end = new bands();
            end.startValue = middle.endValue;
            end.endValue = Convert.ToInt32(totalMarks);
            end.color = "#84b761";

            average averageTest = new average();
            averageTest.band = new bands[3];
            averageTest.band[0] = start;
            averageTest.band[1] = middle;
            averageTest.band[2] = end;

            List<double?> stu_test = (from student_test in db.AspNetStudent_Test
                                      where student_test.TestID == test
                                      select student_test.MarksGot).ToList();
            double? count = 0;
            int n = 0;
            foreach (var item in stu_test)
            {
                count = count + item;
                n++;
            }
            averageTest.average_marks = Convert.ToInt32(count / n);
            return Json(averageTest, JsonRequestBehavior.AllowGet);
        }



        public JsonResult TestResult_Report(int test, int percentage)
        {
            List<AspNetStudent_Test> stu_test = db.AspNetStudent_Test.Where(x => x.TestID == test).ToList();
            List<TableResultReport> test_report = new List<TableResultReport>();
            foreach (var item in stu_test)
            {
                TableResultReport testreport = new TableResultReport();
                testreport.MarksGot = item.MarksGot;
                testreport.Name = item.AspNetUser.Name;
                testreport.UserName = item.AspNetUser.UserName;
                testreport.Percentage = item.MarksGot / item.AspNetTest.TotalMarks * 100;
                if (testreport.Percentage < percentage)
                {
                    testreport.Status = "Fail";
                }
                else
                {
                    testreport.Status = "Pass";
                }
                test_report.Add(testreport);

            }
            return Json(test_report, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult ReportByExam(int exam, int percentage)
        {
            List<double?> stu_exam = (from student_exam in db.AspNetStudent_Exam
                                      where student_exam.ExamID == exam
                                      select student_exam.MarksGot).ToList();
            double? totalMarks = db.AspNetExams.Where(x => x.Id == exam).Select(x => x.TotalMarks).FirstOrDefault();
            int passcount = 0;
            int failcount = 0;
            foreach (var item in stu_exam)
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
            List<pieChartReport> reportbyExam = new List<pieChartReport>();
            pieChartReport passreport = new pieChartReport();
            passreport.result = "Pass";
            passreport.number = passcount;
            passreport.color = "#4F52BA";
            pieChartReport failreport = new pieChartReport();
            failreport.result = "Fail";
            failreport.number = failcount;
            failreport.color = "#ef553a";
            reportbyExam.Add(passreport);
            reportbyExam.Add(failreport);
            return Json(reportbyExam, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public JsonResult ReportByMultipleExam(int subject, int percentage)
        {
            List<barChartReport> reportMultipleExam = new List<barChartReport>();
            List<AspNetExam> exams = db.AspNetExams.Where(x => x.SubjectID == subject).ToList();
            foreach (var exam in exams)
            {
                int passcount = 0;
                int failcount = 0;
                List<double?> stu_exam = db.AspNetStudent_Exam.Where(x => x.ExamID == exam.Id).Select(x => x.MarksGot).ToList();
                foreach (var student_exam in stu_exam)
                {
                    double? perc = (student_exam / exam.TotalMarks * 100);
                    if (perc < percentage)
                    {
                        failcount++;
                    }
                    else
                    {
                        passcount++;
                    }
                }
                barChartReport multipleExam = new barChartReport();
                multipleExam.Exam = exam.Title;
                multipleExam.Pass = passcount;
                multipleExam.Fail = failcount;
                reportMultipleExam.Add(multipleExam);
            }

            return Json(reportMultipleExam, JsonRequestBehavior.AllowGet);

        }


        public JsonResult AverageByExam(int exam, int percentage)
        {
            bands start = new bands();
            double? totalMarks = db.AspNetExams.Where(x => x.Id == exam).Select(x => x.TotalMarks).FirstOrDefault();
            int passignmarks = Convert.ToInt32(totalMarks * percentage / 100);
            start.startValue = 0;
            start.endValue = passignmarks;
            start.color = "#cc4748";
            bands middle = new bands();
            middle.startValue = start.endValue;
            middle.endValue = Convert.ToInt32(totalMarks * 80 / 100);
            middle.color = "#fdd400";
            bands end = new bands();
            end.startValue = middle.endValue;
            end.endValue = Convert.ToInt32(totalMarks);
            end.color = "#84b761";

            average averageExam = new average();
            averageExam.band = new bands[3];
            averageExam.band[0] = start;
            averageExam.band[1] = middle;
            averageExam.band[2] = end;

            List<double?> stu_exam = (from student_exam in db.AspNetStudent_Exam
                                      where student_exam.ExamID == exam
                                      select student_exam.MarksGot).ToList();
            double? count = 0;
            int n = 0;
            foreach (var item in stu_exam)
            {
                count = count + item;
                n++;
            }
            averageExam.average_marks = Convert.ToInt32(count / n);
            return Json(averageExam, JsonRequestBehavior.AllowGet);
        }



        public JsonResult ExamResult_Report(int exam, int percentage)
        {
            List<AspNetStudent_Exam> stu_exam = db.AspNetStudent_Exam.Where(x => x.ExamID == exam).ToList();
            List<TableResultReport> exam_report = new List<TableResultReport>();
            foreach (var item in stu_exam)
            {
                TableResultReport examreport = new TableResultReport();
                examreport.MarksGot = item.MarksGot;
                examreport.Name = item.AspNetUser.Name;
                examreport.UserName = item.AspNetUser.UserName;
                examreport.Percentage = item.MarksGot / item.AspNetExam.TotalMarks * 100;
                if (examreport.Percentage < percentage)
                {
                    examreport.Status = "Fail";
                }
                else
                {
                    examreport.Status = "Pass";
                }
                exam_report.Add(examreport);

            }
            return Json(exam_report, JsonRequestBehavior.AllowGet);
        }

        public class stockChartReport
        {
            public String date { get; set; }
            public double? value { get; set; }
            public string ballontext { get; set; }
        }

        public JsonResult StudentAssignment_Report(string studentID, int subjectID)
        {
            var student_assignments = (from student_assignment in db.Student_Assignment
                                       join assignment in db.AspNetAssignments on student_assignment.AssignmentID equals assignment.Id
                                       where assignment.SubjectID == subjectID && student_assignment.StudentID == studentID
                                       orderby assignment.DueDate ascending
                                       select student_assignment).ToList();
            List<stockChartReport> student_assignment_chart = new List<stockChartReport>();
            foreach (var item in student_assignments)
            {
                stockChartReport student_assignment = new stockChartReport();
                student_assignment.date = item.AspNetAssignment.DueDate.ToString();
                if(item.MarksGot==null)
                {
                    item.MarksGot = 0;
                }
                student_assignment.value = item.MarksGot/item.AspNetAssignment.TotalMarks*100;
                student_assignment.ballontext = item.AspNetAssignment.Title;
                student_assignment_chart.Add(student_assignment);

            }
            return Json(student_assignment_chart, JsonRequestBehavior.AllowGet);
        }


        public JsonResult StudentTest_Report(string studentID, int subjectID)
        {
            var student_tests = (from student_test in db.AspNetStudent_Test
                                       join test in db.AspNetTests on student_test.TestID equals test.Id
                                       where test.SubjectID == subjectID && student_test.StudentID == studentID
                                       orderby test.Date ascending
                                       select student_test).ToList();
            List<stockChartReport> student_test_chart = new List<stockChartReport>();
            foreach (var item in student_tests)
            {
                stockChartReport student_test = new stockChartReport();
                student_test.date = item.AspNetTest.Date.ToString();
                if (item.MarksGot == null)
                {
                    item.MarksGot = 0;
                }
                student_test.value = item.MarksGot / item.AspNetTest.TotalMarks * 100;
                student_test.ballontext = item.AspNetTest.Title;
                student_test_chart.Add(student_test);

            }
            return Json(student_test_chart, JsonRequestBehavior.AllowGet);
        }

        public JsonResult StudentExam_Report(string studentID, int subjectID)
        {
            var student_exams = (from student_exam in db.AspNetStudent_Exam
                                 join exam in db.AspNetExams on student_exam.ExamID equals exam.Id
                                 where exam.SubjectID == subjectID && student_exam.StudentID == studentID
                                 orderby exam.Date ascending
                                 select student_exam).ToList();
            List<stockChartReport> student_exam_chart = new List<stockChartReport>();
            foreach (var item in student_exams)
            {
                stockChartReport student_exam = new stockChartReport();
                student_exam.date = item.AspNetExam.Date.ToString();
                if (item.MarksGot == null)
                {
                    item.MarksGot = 0;
                }
                student_exam.value = item.MarksGot / item.AspNetExam.TotalMarks * 100;
                student_exam.ballontext = item.AspNetExam.Title;
                student_exam_chart.Add(student_exam);

            }
            return Json(student_exam_chart, JsonRequestBehavior.AllowGet);
        }

        public JsonResult StudentAll_Report(string studentID, int subjectID)
        {
            var student_exams = (from student_exam in db.AspNetStudent_Exam
                                 join exam in db.AspNetExams on student_exam.ExamID equals exam.Id
                                 where exam.SubjectID == subjectID && student_exam.StudentID == studentID
                                 orderby exam.Date ascending
                                 select student_exam).ToList();

            var student_tests = (from student_test in db.AspNetStudent_Test
                                 join test in db.AspNetTests on student_test.TestID equals test.Id
                                 where test.SubjectID == subjectID && student_test.StudentID == studentID
                                 orderby test.Date ascending
                                 select student_test).ToList();

            var student_assignments = (from student_assignment in db.Student_Assignment
                                       join assignment in db.AspNetAssignments on student_assignment.AssignmentID equals assignment.Id
                                       where assignment.SubjectID == subjectID && student_assignment.StudentID == studentID
                                       orderby assignment.DueDate ascending
                                       select student_assignment).ToList();

            List<stockChartReport> student_all_chart = new List<stockChartReport>();

            foreach (var item in student_exams)
            {
                stockChartReport student_exam = new stockChartReport();
                student_exam.date = item.AspNetExam.Date.ToString();
                if (item.MarksGot == null)
                {
                    item.MarksGot = 0;
                }
                student_exam.value = item.MarksGot / item.AspNetExam.TotalMarks * 100 ;
                student_exam.ballontext = item.AspNetExam.Title;
                student_all_chart.Add(student_exam);

            }

            foreach (var item in student_tests)
            {
                stockChartReport student_test = new stockChartReport();
                student_test.date = item.AspNetTest.Date.ToString();
                if (item.MarksGot == null)
                {
                    item.MarksGot = 0;
                }
                student_test.value = item.MarksGot / item.AspNetTest.TotalMarks * 100 ;
                student_test.ballontext = item.AspNetTest.Title;
                student_all_chart.Add(student_test);

            }

            foreach (var item in student_assignments)
            {
                stockChartReport student_assignment = new stockChartReport();
                student_assignment.date = item.AspNetAssignment.DueDate.ToString();
                if (item.MarksGot == null)
                {
                    item.MarksGot = 0;
                }
                student_assignment.value = item.MarksGot / item.AspNetAssignment.TotalMarks * 100;
                student_assignment.ballontext = item.AspNetAssignment.Title;
                student_all_chart.Add(student_assignment);

            }
            return Json(student_all_chart, JsonRequestBehavior.AllowGet);
        }
    }
}