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
    public class AspNetSessionController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetSession
        public ActionResult Index()
        {
            return View(db.AspNetSessions.ToList());
        }

        // GET: AspNetSession/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }

        // GET: AspNetSession/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetSession/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SessionName,SessionStartDate,SessionEndDate,Status")] AspNetSession aspNetSession)
        {
            if (ModelState.IsValid)
            {
                db.AspNetSessions.Add(aspNetSession);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetSession);
        }

        // GET: AspNetSession/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }

        // POST: AspNetSession/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SessionName,SessionStartDate,SessionEndDate,Status")] AspNetSession aspNetSession)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetSession).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetSession);
        }

        // GET: AspNetSession/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }

        // POST: AspNetSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            db.AspNetSessions.Remove(aspNetSession);
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
