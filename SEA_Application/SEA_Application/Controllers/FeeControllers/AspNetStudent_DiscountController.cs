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
    public class AspNetStudent_DiscountController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetStudent_Discount
        public ActionResult Index()
        {
            var aspNetStudent_Discount = db.AspNetStudent_Discount.Include(a => a.AspNetDiscountType).Include(a => a.AspNetUser);
            return View(aspNetStudent_Discount.ToList());
        }

        // GET: AspNetStudent_Discount/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetStudent_Discount aspNetStudent_Discount = db.AspNetStudent_Discount.Find(id);
            if (aspNetStudent_Discount == null)
            {
                return HttpNotFound();
            }
            return View(aspNetStudent_Discount);
        }

        // GET: AspNetStudent_Discount/Create
        public ActionResult Create()
        {
            ViewBag.DiscountID = new SelectList(db.AspNetDiscountTypes, "Id", "TypeName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.StudentID = new SelectList(db.AspNetUsers, "Id", "Name");
            return View();
        }

        // POST: AspNetStudent_Discount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StudentID,DiscountID,Percentage")] AspNetStudent_Discount aspNetStudent_Discount)
        {
            if (ModelState.IsValid)
            {
                db.AspNetStudent_Discount.Add(aspNetStudent_Discount);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DiscountID = new SelectList(db.AspNetDiscountTypes, "Id", "TypeName", aspNetStudent_Discount.DiscountID);
            ViewBag.StudentID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetStudent_Discount.StudentID);
            return View(aspNetStudent_Discount);
        }

        // GET: AspNetStudent_Discount/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetStudent_Discount aspNetStudent_Discount = db.AspNetStudent_Discount.Find(id);
            if (aspNetStudent_Discount == null)
            {
                return HttpNotFound();
            }
            ViewBag.DiscountID = new SelectList(db.AspNetDiscountTypes, "Id", "TypeName", aspNetStudent_Discount.DiscountID);
            ViewBag.StudentID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetStudent_Discount.StudentID);
            return View(aspNetStudent_Discount);
        }

        // POST: AspNetStudent_Discount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StudentID,DiscountID,Percentage")] AspNetStudent_Discount aspNetStudent_Discount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetStudent_Discount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DiscountID = new SelectList(db.AspNetDiscountTypes, "Id", "TypeName", aspNetStudent_Discount.DiscountID);
            ViewBag.StudentID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetStudent_Discount.StudentID);
            return View(aspNetStudent_Discount);
        }

        // GET: AspNetStudent_Discount/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetStudent_Discount aspNetStudent_Discount = db.AspNetStudent_Discount.Find(id);
            if (aspNetStudent_Discount == null)
            {
                return HttpNotFound();
            }
            return View(aspNetStudent_Discount);
        }

        // POST: AspNetStudent_Discount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetStudent_Discount aspNetStudent_Discount = db.AspNetStudent_Discount.Find(id);
            db.AspNetStudent_Discount.Remove(aspNetStudent_Discount);
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

        public class student_discount
        {
            public string StudentID { get; set; }
            public int DiscountID { get; set; }
            public int Percent { get; set; }
        }
        [HttpPost]
        public ActionResult AddStudent_Discount(List<student_discount> student_discount)
        {
            foreach (var item in student_discount)
            {
                AspNetStudent_Discount studentdiscount = new AspNetStudent_Discount();
                studentdiscount.StudentID = item.StudentID;
                studentdiscount.Percentage = item.Percent;
                studentdiscount.DiscountID = item.DiscountID;
                db.AspNetStudent_Discount.Add(studentdiscount);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
