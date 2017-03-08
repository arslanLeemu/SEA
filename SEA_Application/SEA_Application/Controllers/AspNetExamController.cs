using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspNetExamController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        

        string TeacherID;


        public AspNetExamController()
        {
            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }
        // GET: AspNetExam
        public ActionResult Index()
        {
            var aspNetExams = db.AspNetExams.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser);
            return View(aspNetExams.ToList());
        }

        public PartialViewResult View_Exam(string id)
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            var aspNetExams = db.AspNetExams.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser);
            //var aspNetTests = db.AspNetTests.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
            //var aspNetAssignments = db.AspNetAssignments.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
            return PartialView("_View_Exam");

        }

        [HttpGet]
        public JsonResult ExamsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            //List<AspNetTest> tests = (from test in db.AspNetTests
            //                          where test.SubjectID == id
            //                          select test).ToList();
            List<AspNetExam> exams = (from exam in db.AspNetExams
                                      where exam.SubjectID == id
                                      select exam).ToList();

            return Json(exams, JsonRequestBehavior.AllowGet);
        }


        // GET: AspNetExam/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetExam aspNetExam = db.AspNetExams.Find(id);
            if (aspNetExam == null)
            {
                return HttpNotFound();
            }
            return View(aspNetExam);
        }

        // GET: AspNetExam/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s=>s.Id==TeacherID), "Id", "Name");
            return View();
        }

        // POST: AspNetExam/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubjectID,ClassID,Title,Description,Date,TotalMarks,Weightage,TeacherID")] AspNetExam aspNetExam)
        {
            var dbTransection = db.Database.BeginTransaction();
            try {
                if (ModelState.IsValid)
                {
                    db.AspNetExams.Add(aspNetExam);
                    db.SaveChanges();

                    int examID = db.AspNetExams.Max(item => item.Id);
                    List<string> StudentIDs = db.AspNetStudent_Subject.Where(s => s.SubjectID == aspNetExam.SubjectID).Select(s => s.StudentID).ToList();
                    foreach (var item in StudentIDs)
                    {
                        AspNetStudent_Exam stu_exams = new AspNetStudent_Exam();
                        stu_exams.StudentID = item;
                        stu_exams.ExamID = examID;
                        db.AspNetStudent_Exam.Add(stu_exams);
                        db.SaveChanges();
                    }
                }
                    dbTransection.Commit();
                }
            catch (Exception) { dbTransection.Dispose(); }

            return RedirectToAction("Index");
            

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name");
            return View(aspNetExam);
        }

        // GET: AspNetExam/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetExam aspNetExam = db.AspNetExams.Find(id);
            if (aspNetExam == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetExam.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName", aspNetExam.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name", aspNetExam.TeacherID);
            return View(aspNetExam);
        }

        // POST: AspNetExam/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubjectID,ClassID,Title,Description,Date,TotalMarks,Weightage,TeacherID")] AspNetExam aspNetExam)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetExam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName",aspNetExam.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName", aspNetExam.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name", aspNetExam.TeacherID);
            
            return View(aspNetExam);
        }



        //public PartialViewResult Student_Exams(string id)
        //{
        //    ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName");
        //    ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
        //    var aspNetExams = db.AspNetExams.Include(a => a.AspNetSubject).Include(a => a.AspNetUser);
        //    //var aspNetAssignments = db.AspNetAssignments.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
        //    return PartialView("_Student_Exams");

        //}





        // GET: AspNetExam/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetExam aspNetExam = db.AspNetExams.Find(id);
            if (aspNetExam == null)
            {
                return HttpNotFound();
            }
            return View(aspNetExam);
        }

        // POST: AspNetExam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetExam aspNetExam = db.AspNetExams.Find(id);
            db.AspNetExams.Remove(aspNetExam);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id && r.TeacherID == TeacherID).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
