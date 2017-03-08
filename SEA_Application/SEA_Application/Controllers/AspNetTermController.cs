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
    public class AspNetTermController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetTerm
        public ActionResult Index()
        {
            var aspNetTerms = db.AspNetTerms.Include(a => a.AspNetSession);
            return View(aspNetTerms.ToList());
        }

        // GET: AspNetTerm/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTerm aspNetTerm = db.AspNetTerms.Find(id);
            if (aspNetTerm == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTerm);
        }

        // GET: AspNetTerm/Create
        public ActionResult Create()
        {
            ViewBag.SessionID = new SelectList(db.AspNetSessions, "Id", "SessionName");
            return View();
        }

        // POST: AspNetTerm/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TermName,SessionID,TermStartDate,TermEndDate,Status")] AspNetTerm aspNetTerm)
        {
            if (ModelState.IsValid)
            {
                db.AspNetTerms.Add(aspNetTerm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SessionID = new SelectList(db.AspNetSessions, "Id", "SessionName", aspNetTerm.SessionID);
            return View(aspNetTerm);
        }

        // GET: AspNetTerm/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTerm aspNetTerm = db.AspNetTerms.Find(id);
            if (aspNetTerm == null)
            {
                return HttpNotFound();
            }
            ViewBag.SessionID = new SelectList(db.AspNetSessions, "Id", "SessionName", aspNetTerm.SessionID);
            return View(aspNetTerm);
        }

        // POST: AspNetTerm/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TermName,SessionID,TermStartDate,TermEndDate,Status")] AspNetTerm aspNetTerm)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetTerm).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SessionID = new SelectList(db.AspNetSessions, "Id", "SessionName", aspNetTerm.SessionID);
            return View(aspNetTerm);
        }

        // GET: AspNetTerm/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTerm aspNetTerm = db.AspNetTerms.Find(id);
            if (aspNetTerm == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTerm);
        }

        // POST: AspNetTerm/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetTerm aspNetTerm = db.AspNetTerms.Find(id);
            db.AspNetTerms.Remove(aspNetTerm);
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
