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
    [Authorize(Roles = "Admin")]
    public class AspNetUserController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        /*************************************************************Student List Functions*******************************************************/
        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult SubjectsByStudent(string id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            //List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).ToList();
            //ViewBag.Subjects = sub;
            var subs = (from sub in db.AspNetStudent_Subject
                        where sub.StudentID == id
                        select new { sub.AspNetSubject.Id, sub.AspNetSubject.SubjectName, sub.AspNetSubject.ClassID }).ToList();


            return Json(subs, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult StudentsBySubject(int id)
        {

            var students = (from student in db.AspNetStudent_Subject
                            where student.SubjectID == id
                            select new { student.AspNetUser.Id, student.AspNetUser.Email, student.AspNetUser.UserName, student.AspNetUser.Name, student.AspNetUser.PhoneNumber }).ToList();


            return Json(students, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public JsonResult StudentClass(string id)
        {

            //var students = (from student in db.AspNetStudent_Subject
            //                where student.SubjectID == id
            //                select new { student.AspNetUser.Id, student.AspNetUser.Email, student.AspNetUser.UserName, student.AspNetUser.Name, student.AspNetUser.PhoneNumber }).ToList();
            //var Studclass = (from Stuclas in db.AspNetSubjects
            //where Stuclas.Id == id
            //select Stuclas.ClassID);

            var subID = db.AspNetStudent_Subject.FirstOrDefault(x => x.StudentID == id);
            var classidcheck = db.AspNetSubjects.FirstOrDefault(x => x.Id == subID.SubjectID);
            var ClassIDscheck = db.AspNetClasses.FirstOrDefault(x => x.Id == classidcheck.ClassID);
            //ViewBag.ClassIDNAME = ClassIDscheck.ClassName;
            var ClassIDNAMEv = ClassIDscheck.Id;

            return Json(ClassIDNAMEv, JsonRequestBehavior.AllowGet);
        }

        // GET: AspNetUser/Edit/5
        public ActionResult EditStudent(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            //ViewBag.SubjectID = new SelectList(db.AspNetStudent_Subject.Where(x=>x.StudentID==id), "Id", "SubjectID");
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudent([Bind(Include = "Id,Email,PasswordHash,SecurityStamp,PhoneNumber,UserName,Name")] AspNetUser aspNetUser)
        {
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    IEnumerable<string> selectedsubjects = Request.Form["subjects"].Split(',');
                    AspNetStudent_Subject stu_sub_rem = new AspNetStudent_Subject();
                    do
                    {
                        stu_sub_rem = db.AspNetStudent_Subject.FirstOrDefault(x => x.StudentID == aspNetUser.Id);
                        try
                        {
                            db.AspNetStudent_Subject.Remove(stu_sub_rem);
                            db.SaveChanges();
                        }
                        catch
                        {

                        }
                    }
                    while (stu_sub_rem != null);


                    foreach (var item in selectedsubjects)
                    {
                        AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                        stu_sub.StudentID = aspNetUser.Id;
                        stu_sub.SubjectID = Convert.ToInt32(item);
                        db.AspNetStudent_Subject.Add(stu_sub);
                        db.SaveChanges();
                    }

                    db.Entry(aspNetUser).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("StudentsIndex");
                }
                dbTransaction.Commit();
            }
            catch (Exception) { dbTransaction.Dispose(); }

            return View("StudentsIndex");
        }
    

        /**********************************************************************************************************************************************/
        // GET: AspNetUser
        public ActionResult StudentsIndex()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            //return View("Index",db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Student")).ToList());
            return View();

        }

        public ActionResult TeacherIndex()
        {

            return View("Index",db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")).ToList());
        }

        // GET: AspNetUser/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // GET: AspNetUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,Name")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.AspNetUsers.Add(aspNetUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetUser);
        }

        // GET: AspNetUser/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectID");
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,Name")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetUser);
        }

        // GET: AspNetUser/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            db.AspNetUsers.Remove(aspNetUser);
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
