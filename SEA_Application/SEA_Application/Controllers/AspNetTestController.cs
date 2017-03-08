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
    public class AspNetTestController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        string TeacherID;


        public AspNetTestController()
        {
            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }
        // GET: AspNetTest
        public ActionResult Index()
        {
            var aspNetTests = db.AspNetTests.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser);
            return View(aspNetTests.ToList());
        }


        public PartialViewResult View_Test(string id)
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            var aspNetTests = db.AspNetTests.Include(a=>a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
            //var aspNetAssignments = db.AspNetAssignments.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
            return PartialView("_View_Tests");

        }



        // GET: AspNetTest/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTest aspNetTest = db.AspNetTests.Find(id);
            if (aspNetTest == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTest);
        }

        // GET: AspNetTest/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name");
            return View();
        }

        // POST: AspNetTest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubjectID,ClassID,Title,Description,Date,TotalMarks,Weightage,TeacherID")] AspNetTest aspNetTest)
        {
            IEnumerable<string> topics = Request.Form["TopicID"].Split(',');
            if (ModelState.IsValid)
            {
                var dbTransaction = db.Database.BeginTransaction();
                try {

                    db.AspNetTests.Add(aspNetTest);
                    db.SaveChanges();

                    int testID = db.AspNetTests.Max(item => item.Id);
                    List<string> StudentIDs = db.AspNetStudent_Subject.Where(s => s.SubjectID == aspNetTest.SubjectID).Select(s => s.StudentID).ToList();
                    foreach (var item in StudentIDs)
                    {
                        AspNetStudent_Test stu_test = new AspNetStudent_Test();
                        stu_test.StudentID = item;
                        stu_test.TestID = testID;
                        db.AspNetStudent_Test.Add(stu_test);
                        db.SaveChanges();
                    }

                    foreach (var item in topics)
                    {
                        AspNetTest_Topic test_topic = new AspNetTest_Topic();
                        test_topic.TopicID = Convert.ToInt32(item);
                        test_topic.TestID = testID;
                        db.AspNetTest_Topic.Add(test_topic);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                    dbTransaction.Commit();
                }
                catch (Exception) { dbTransaction.Dispose(); }
                }
            

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name");
            return View(aspNetTest);
        }

        // GET: AspNetTest/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTest aspNetTest = db.AspNetTests.Find(id);
            if (aspNetTest == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetTest.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName", aspNetTest.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name", aspNetTest.TeacherID);
            return View(aspNetTest);
        }

        // POST: AspNetTest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubjectID,ClassID,Title,Description,Date,TotalMarks,Weightage,TeacherID")] AspNetTest aspNetTest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetTest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetTest.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName", aspNetTest.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(s => s.Id == TeacherID), "Id", "Name", aspNetTest.TeacherID);
            return View(aspNetTest);
        }

        // GET: AspNetTest/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTest aspNetTest = db.AspNetTests.Find(id);
            if (aspNetTest == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTest);
        }

        // POST: AspNetTest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetTest aspNetTest = db.AspNetTests.Find(id);
            db.AspNetTests.Remove(aspNetTest);
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

        [HttpGet]
        public JsonResult TopicsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetTopic> topic = db.AspNetTopics.Where(r => r.SubjectID == id).OrderByDescending(r => r.Id).ToList();

            return Json(topic, JsonRequestBehavior.AllowGet);

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
