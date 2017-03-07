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
    public class AspNetSubjectController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetSubject
        public ActionResult Index()
        {
            var aspNetSubjects = db.AspNetSubjects.Include(a => a.AspNetClass).Include(a => a.AspNetUser);
            return View(aspNetSubjects.ToList());
        }

        // GET: AspNetSubject/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSubject);
        }

        // GET: AspNetSubject/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name");
            return View();
        }

        // POST: AspNetSubject/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubjectName,ClassID,TeacherID")] AspNetSubject aspNetSubject)
        {
            if (ModelState.IsValid)
            {
                db.AspNetSubjects.Add(aspNetSubject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name");
            return View(aspNetSubject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubjectfromFile(AspNetSubject aspNetSubject)
        {
            // if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["subjects"];
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
                        var Subject = new AspNetSubject();
                        string TeacherName = workSheet.Cells[rowIterator, 2].Value.ToString();
                        var Teacher = (from users in db.AspNetUsers
                                       where users.UserName == TeacherName
                                       select users).First();

                        var ClassName = workSheet.Cells[rowIterator, 3].Value.ToString();
                        var Class = (from classes in db.AspNetClasses
                                       where classes.ClassName == ClassName
                                     select classes).First();

                        Subject.SubjectName = workSheet.Cells[rowIterator, 1].Value.ToString();
                        Subject.TeacherID = Teacher.Id;
                        Subject.ClassID = Class.Id;
                        db.AspNetSubjects.Add(Subject);
                        db.SaveChanges();

                    }
                }

            }

            // ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", aspNetClass.TeacherID);
            return View(aspNetSubject);
        }

        // GET: AspNetSubject/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetSubject.TeacherID);
            return View(aspNetSubject);
        }

        // POST: AspNetSubject/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubjectName,ClassID,TeacherID")] AspNetSubject aspNetSubject)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetSubject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetSubject.TeacherID);
            return View(aspNetSubject);
        }

        // GET: AspNetSubject/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSubject);
        }

        // POST: AspNetSubject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            db.AspNetSubjects.Remove(aspNetSubject);
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
