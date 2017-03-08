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
    public class AspNetCurriculumsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetCurriculums
        public ActionResult Index()
        {
            return View(db.AspNetCurriculums.ToList());
        }

        // GET: AspNetCurriculums/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetCurriculum aspNetCurriculum = db.AspNetCurriculums.Find(id);
            if (aspNetCurriculum == null)
            {
                return HttpNotFound();
            }
            return View(aspNetCurriculum);
        }

        // GET: AspNetCurriculums/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetCurriculums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CurriculumName")] AspNetCurriculum aspNetCurriculum)
        {
            if (ModelState.IsValid)
            {
                db.AspNetCurriculums.Add(aspNetCurriculum);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetCurriculum);
        }

        // GET: AspNetCurriculums/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetCurriculum aspNetCurriculum = db.AspNetCurriculums.Find(id);
            if (aspNetCurriculum == null)
            {
                return HttpNotFound();
            }
            return View(aspNetCurriculum);
        }

        // POST: AspNetCurriculums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CurriculumName")] AspNetCurriculum aspNetCurriculum)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetCurriculum).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetCurriculum);
        }

        // GET: AspNetCurriculums/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetCurriculum aspNetCurriculum = db.AspNetCurriculums.Find(id);
            if (aspNetCurriculum == null)
            {
                return HttpNotFound();
            }
            return View(aspNetCurriculum);
        }

        // POST: AspNetCurriculums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetCurriculum aspNetCurriculum = db.AspNetCurriculums.Find(id);
            db.AspNetCurriculums.Remove(aspNetCurriculum);
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
