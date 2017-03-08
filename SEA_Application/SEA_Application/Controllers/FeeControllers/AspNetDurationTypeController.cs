using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;

namespace SEA_Application.Controllers.FeeControllers
{
    public class AspNetDurationTypeController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetDurationType
        public ActionResult Index()
        {
            return View(db.AspNetDurationTypes.ToList());
        }

        // GET: AspNetDurationType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetDurationType aspNetDurationType = db.AspNetDurationTypes.Find(id);
            if (aspNetDurationType == null)
            {
                return HttpNotFound();
            }
            return View(aspNetDurationType);
        }

        // GET: AspNetDurationType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetDurationType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TypeName")] AspNetDurationType aspNetDurationType)
        {
            if (ModelState.IsValid)
            {
                db.AspNetDurationTypes.Add(aspNetDurationType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetDurationType);
        }

        // GET: AspNetDurationType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetDurationType aspNetDurationType = db.AspNetDurationTypes.Find(id);
            if (aspNetDurationType == null)
            {
                return HttpNotFound();
            }
            return View(aspNetDurationType);
        }

        // POST: AspNetDurationType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TypeName")] AspNetDurationType aspNetDurationType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetDurationType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetDurationType);
        }

        // GET: AspNetDurationType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetDurationType aspNetDurationType = db.AspNetDurationTypes.Find(id);
            if (aspNetDurationType == null)
            {
                return HttpNotFound();
            }
            return View(aspNetDurationType);
        }

        // POST: AspNetDurationType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetDurationType aspNetDurationType = db.AspNetDurationTypes.Find(id);
            db.AspNetDurationTypes.Remove(aspNetDurationType);
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
