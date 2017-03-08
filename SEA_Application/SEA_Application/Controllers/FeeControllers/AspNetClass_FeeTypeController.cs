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
    public class AspNetClass_FeeTypeController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetClass_FeeType
        public ActionResult Index()
        {
            var aspNetClass_FeeType = db.AspNetClass_FeeType.Include(a => a.AspNetClass).Include(a => a.AspNetFeeType);
            return View(aspNetClass_FeeType.ToList());
        }

        // GET: AspNetClass_FeeType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass_FeeType aspNetClass_FeeType = db.AspNetClass_FeeType.Find(id);
            if (aspNetClass_FeeType == null)
            {
                return HttpNotFound();
            }
            return View(aspNetClass_FeeType);
        }

        // GET: AspNetClass_FeeType/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.FeeTypeID = new SelectList(db.AspNetFeeTypes, "Id", "TypeName");
            return View();
        }

        // POST: AspNetClass_FeeType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClassID,FeeTypeID,Amount")] AspNetClass_FeeType aspNetClass_FeeType)
        {
            if (ModelState.IsValid)
            {
              //  db.AspNetClass_FeeType.Add(aspNetClass_FeeType);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetClass_FeeType.ClassID);
            ViewBag.FeeTypeID = new SelectList(db.AspNetFeeTypes, "Id", "TypeName", aspNetClass_FeeType.FeeTypeID);
            return View(aspNetClass_FeeType);
        }

        // GET: AspNetClass_FeeType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass_FeeType aspNetClass_FeeType = db.AspNetClass_FeeType.Find(id);
            if (aspNetClass_FeeType == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetClass_FeeType.ClassID);
            ViewBag.FeeTypeID = new SelectList(db.AspNetFeeTypes, "Id", "TypeName", aspNetClass_FeeType.FeeTypeID);
            return View(aspNetClass_FeeType);
        }

        // POST: AspNetClass_FeeType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClassID,FeeTypeID,Amount")] AspNetClass_FeeType aspNetClass_FeeType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetClass_FeeType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetClass_FeeType.ClassID);
            ViewBag.FeeTypeID = new SelectList(db.AspNetFeeTypes, "Id", "TypeName", aspNetClass_FeeType.FeeTypeID);
            return View(aspNetClass_FeeType);
        }

        // GET: AspNetClass_FeeType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass_FeeType aspNetClass_FeeType = db.AspNetClass_FeeType.Find(id);
            if (aspNetClass_FeeType == null)
            {
                return HttpNotFound();
            }
            return View(aspNetClass_FeeType);
        }

        // POST: AspNetClass_FeeType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetClass_FeeType aspNetClass_FeeType = db.AspNetClass_FeeType.Find(id);
            db.AspNetClass_FeeType.Remove(aspNetClass_FeeType);
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
        public class class_feetype
        {
            public int ClassId { get; set; }
            public int FeeTypeID { get; set; }
            public int Amount { get; set; }
        }
        [HttpPost]
        public ActionResult AddClass_FeeType(List<class_feetype> class_feeType)
        {
            foreach (var item in class_feeType)
            {
                AspNetClass_FeeType class_fee = new AspNetClass_FeeType();
                class_fee.ClassID = item.ClassId;
                class_fee.FeeTypeID = item.FeeTypeID;
                class_fee.Amount = item.Amount;
                db.AspNetClass_FeeType.Add(class_fee);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        
    }
}
