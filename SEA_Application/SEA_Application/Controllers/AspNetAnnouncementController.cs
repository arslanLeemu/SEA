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
    [Authorize(Roles = "Teacher")]
    public class AspNetAnnouncementController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        string TeacherID;


        public AspNetAnnouncementController()
        {
            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }
        // GET: AspNetAnnouncement
        public ActionResult Index()
        {
            return View(db.AspNetAnnouncements.ToList());
        }

        // GET: AspNetAnnouncement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAnnouncement aspNetAnnouncement = db.AspNetAnnouncements.Find(id);
            if (aspNetAnnouncement == null)
            {
                return HttpNotFound();
            }
            return View(aspNetAnnouncement);
        }

        // GET: AspNetAnnouncement/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID).Select(s => s.AspNetClass), "Id", "ClassName").ToList();
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName");
            return View();
        }

        // POST: AspNetAnnouncement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description")] AspNetAnnouncement aspNetAnnouncement)
        {
            string subjects = Request.Form["subjects"];
            IEnumerable<string> selectedsubjects = subjects.Split(',');
            if (ModelState.IsValid)
            {
                // announcement.SubjectID=
                db.AspNetAnnouncements.Add(aspNetAnnouncement);
                db.SaveChanges();
                int announcementID = db.AspNetAnnouncements.Max(item => item.Id);
                List<int> SubjectIDs = new List<int>();
                foreach (var item in selectedsubjects)
                {
                    int subjectID = Convert.ToInt32(item);
                    SubjectIDs.Add(subjectID);
                }
                foreach (var item in SubjectIDs)
                {
                    AspNetAnnouncement_Subject ann_sub = new AspNetAnnouncement_Subject();
                    ann_sub.SubjectID = item;
                    ann_sub.AnnouncementID = announcementID;
                    db.AspNetAnnouncement_Subject.Add(ann_sub);
                    db.SaveChanges();
                }
                List<string> student = db.AspNetStudent_Subject.Where(x => SubjectIDs.Contains(x.SubjectID)).Select(s => s.StudentID).ToList();
                foreach (var item in student)
                {
                   
                    AspNetStudent_Announcement stu_ann = new AspNetStudent_Announcement();
                    stu_ann.StudentID = item;
                    stu_ann.AnnouncementID = announcementID;
                    stu_ann.Seen = false;
                    db.AspNetStudent_Announcement.Add(stu_ann);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            return View(aspNetAnnouncement);
        }

        // GET: AspNetAnnouncement/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAnnouncement aspNetAnnouncement = db.AspNetAnnouncements.Find(id);
            if (aspNetAnnouncement == null)
            {
                return HttpNotFound();
            }
            return View(aspNetAnnouncement);
        }

        // POST: AspNetAnnouncement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description")] AspNetAnnouncement aspNetAnnouncement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetAnnouncement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetAnnouncement);
        }

        // GET: AspNetAnnouncement/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAnnouncement aspNetAnnouncement = db.AspNetAnnouncements.Find(id);
            if (aspNetAnnouncement == null)
            {
                return HttpNotFound();
            }
            return View(aspNetAnnouncement);
        }

        // POST: AspNetAnnouncement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetAnnouncement aspNetAnnouncement = db.AspNetAnnouncements.Find(id);
            db.AspNetAnnouncements.Remove(aspNetAnnouncement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult SubjectsByClass(string[] bdoIds)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<int> ids = new List<int>();

            foreach (var item in bdoIds)
            {
                int a = Convert.ToInt32(item);
                ids.Add(a);
            }

            var Subjects = db.AspNetSubjects.Where(x => ids.Contains(x.AspNetClass.Id) && x.TeacherID == TeacherID).ToList();
            ViewBag.Subjects = Subjects;
            return Json(Subjects, JsonRequestBehavior.AllowGet);
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
