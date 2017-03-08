using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using OfficeOpenXml;

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AspNetClassController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetClass
        public ActionResult Index()
        {
            var aspNetClasses = db.AspNetClasses.Include(a => a.AspNetUser);
            return View(aspNetClasses.ToList());
        }

        // GET: AspNetClass/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass aspNetClass = db.AspNetClasses.Find(id);
            if (aspNetClass == null)
            {
                return HttpNotFound();
            }
            return View(aspNetClass);
        }

        // GET: AspNetClass/Create
        public ActionResult Create()
        {
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name");
            return View();
        }

        // POST: AspNetClass/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClassName,TeacherID")] AspNetClass aspNetClass)
        {
            if (ModelState.IsValid)
            {
                db.AspNetClasses.Add(aspNetClass);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name", aspNetClass.TeacherID);
            return View(aspNetClass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClassfromFile(AspNetClass aspNetClass)
        {
            // if (ModelState.IsValid)
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["classes"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
                var studentList = new List<RegisterViewModel>();
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var Class = new AspNetClass();
                        string teacherName = workSheet.Cells[rowIterator, 2].Value.ToString();
                        var Teacher = (from users in db.AspNetUsers
                                       where users.UserName == teacherName
                                       select users).First();
                        Class.ClassName = workSheet.Cells[rowIterator, 1].Value.ToString();
                        Class.TeacherID = Teacher.Id;
                        db.AspNetClasses.Add(Class);
                        db.SaveChanges();
                        
                    }
                }
                dbTransaction.Commit();
            }
            catch (Exception) { dbTransaction.Dispose(); }

           // ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", aspNetClass.TeacherID);
            return View(aspNetClass);
        }

        // GET: AspNetClass/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass aspNetClass = db.AspNetClasses.Find(id);
            if (aspNetClass == null)
            {
                return HttpNotFound();
            }
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", aspNetClass.TeacherID);
            return View(aspNetClass);
        }

        // POST: AspNetClass/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClassName,TeacherID")] AspNetClass aspNetClass)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetClass).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", aspNetClass.TeacherID);
            return View(aspNetClass);
        }

        // GET: AspNetClass/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetClass aspNetClass = db.AspNetClasses.Find(id);
            if (aspNetClass == null)
            {
                return HttpNotFound();
            }
            return View(aspNetClass);
        }

        // POST: AspNetClass/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetClass aspNetClass = db.AspNetClasses.Find(id);
            db.AspNetClasses.Remove(aspNetClass);
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
