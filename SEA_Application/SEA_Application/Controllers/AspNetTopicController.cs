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
    public class AspNetTopicController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetTopic
        public ActionResult Index()
        {
            var aspNetTopics = db.AspNetTopics.Include(a => a.AspNetSubject);
            return View(aspNetTopics.ToList());
        }

        // GET: AspNetTopic/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTopic aspNetTopic = db.AspNetTopics.Find(id);
            if (aspNetTopic == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTopic);
        }

        // GET: AspNetTopic/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName");
            return View();
        }

        // POST: AspNetTopic/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AspNetTopic aspNetTopic)
        {
            IEnumerable<string> topics = Request.Form["DynamicTextBox"].Split(',');
            if (ModelState.IsValid)
            {
                foreach(var item in topics)
                {
                    AspNetTopic aspnettopic = new AspNetTopic();
                    aspnettopic.SubjectID = aspNetTopic.SubjectID;
                    aspnettopic.TopicName = item;
                    db.AspNetTopics.Add(aspnettopic);
                    db.SaveChanges();
                }
               
                return RedirectToAction("Index");
            }
            
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTopic.SubjectID);
            return View(aspNetTopic);
        }

        // GET: AspNetTopic/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTopic aspNetTopic = db.AspNetTopics.Find(id);
            if (aspNetTopic == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTopic.SubjectID);
            return View(aspNetTopic);
        }

        // POST: AspNetTopic/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TopicName,SubjectID")] AspNetTopic aspNetTopic)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetTopic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTopic.SubjectID);
            return View(aspNetTopic);
        }

        // GET: AspNetTopic/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTopic aspNetTopic = db.AspNetTopics.Find(id);
            if (aspNetTopic == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTopic);
        }

        // POST: AspNetTopic/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetTopic aspNetTopic = db.AspNetTopics.Find(id);
            db.AspNetTopics.Remove(aspNetTopic);
            db.SaveChanges();
            return RedirectToAction("Index");
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
