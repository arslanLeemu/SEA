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
    [Authorize(Roles = "Teacher")]

    public class AspNetSubject_CurriculumController : Controller
    {
        string TeacherID;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public AspNetSubject_CurriculumController()
        {
            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }


        // GET: AspNetSubject_Curriculum
        public ActionResult Index()
        {
            var aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Include(a => a.AspNetCurriculum).Include(a => a.AspNetSubject);
            return View(aspNetSubject_Curriculum.ToList());
        }

        // GET: AspNetSubject_Curriculum/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject_Curriculum aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Find(id);
            if (aspNetSubject_Curriculum == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSubject_Curriculum);
        }

        // GET: AspNetSubject_Curriculum/Create
        public ActionResult Create()
        {
            ViewBag.CurriculumID = new SelectList(db.AspNetCurriculums, "Id", "CurriculumName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            return View();
        }

        // POST: AspNetSubject_Curriculum/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,WeightageValue,CurriculumID,SubjectID")] AspNetSubject_Curriculum aspNetSubject_Curriculum)
        {
            if (ModelState.IsValid)
            {
                db.AspNetSubject_Curriculum.Add(aspNetSubject_Curriculum);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CurriculumID = new SelectList(db.AspNetCurriculums, "Id", "CurriculumName", aspNetSubject_Curriculum.CurriculumID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetSubject_Curriculum.SubjectID);
            return View(aspNetSubject_Curriculum);
        }

        [HttpGet]
        public ActionResult CurriculumAdd(int RecWeightage, int RecCurric, int RecSubject)
        {

            AspNetSubject_Curriculum curricObjRem = db.AspNetSubject_Curriculum.FirstOrDefault(x => x.SubjectID == RecSubject && x.CurriculumID == RecCurric);
            if (curricObjRem != null)
            curricObjRem.WeightageValue = RecWeightage;
                

            if (curricObjRem == null) { 
            AspNetSubject_Curriculum curricObj = new AspNetSubject_Curriculum();
            curricObj.WeightageValue = RecWeightage;
            curricObj.SubjectID = RecSubject;
            curricObj.CurriculumID = RecCurric;
            db.AspNetSubject_Curriculum.Add(curricObj);
            }
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: AspNetSubject_Curriculum/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject_Curriculum aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Find(id);
            if (aspNetSubject_Curriculum == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurriculumID = new SelectList(db.AspNetCurriculums, "Id", "CurriculumName", aspNetSubject_Curriculum.CurriculumID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetSubject_Curriculum.SubjectID);
            return View(aspNetSubject_Curriculum);
        }

        // POST: AspNetSubject_Curriculum/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,WeightageValue,CurriculumID,SubjectID")] AspNetSubject_Curriculum aspNetSubject_Curriculum)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetSubject_Curriculum).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurriculumID = new SelectList(db.AspNetCurriculums, "Id", "CurriculumName", aspNetSubject_Curriculum.CurriculumID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetSubject_Curriculum.SubjectID);
            return View(aspNetSubject_Curriculum);
        }

        // GET: AspNetSubject_Curriculum/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject_Curriculum aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Find(id);
            if (aspNetSubject_Curriculum == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSubject_Curriculum);
        }

        // POST: AspNetSubject_Curriculum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetSubject_Curriculum aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Find(id);
            db.AspNetSubject_Curriculum.Remove(aspNetSubject_Curriculum);
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
        public PartialViewResult View_Curriculum()
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            var aspNetSubject_Curriculum = db.AspNetSubject_Curriculum.Include(a => a.AspNetCurriculum).Include(a => a.AspNetSubject);
            return PartialView("_View_Curriculum");

            //return View(aspNetSubject_Curriculum.ToList());

        }


        [HttpGet]
        public JsonResult CurriculumBySubject(int subjectID)
        {



            var curriculums = (from curriculum in db.AspNetSubject_Curriculum
                               where curriculum.SubjectID == subjectID
                               select new { curriculum.Id, curriculum.AspNetCurriculum.CurriculumName, curriculum.AspNetSubject.SubjectName, curriculum.WeightageValue }).ToList();
            //ViewBag.curriculums = curriculums;
            return Json(curriculums, JsonRequestBehavior.AllowGet);
        }





    }
}
