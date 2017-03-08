using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.IO;

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class AspNetAssignmentController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
       
         string TeacherID;


        public AspNetAssignmentController()
        {
            TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);
        }
        


        // GET: AspNetAssignment
        public ActionResult Index()
        {
           
            var aspNetAssignments = db.AspNetAssignments.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID==TeacherID);
            return View(aspNetAssignments.ToList());
        }



        public PartialViewResult View_Assignments(string id)
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s => s.TeacherID == TeacherID), "Id", "SubjectName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            var aspNetAssignments = db.AspNetAssignments.Include(a => a.AspNetClass).Include(a => a.AspNetSubject).Include(a => a.AspNetUser).Where(a => a.TeacherID == TeacherID);
            return PartialView("_View_Assignments");

        }


        // GET: AspNetAssignment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAssignment aspNetAssignment = db.AspNetAssignments.Find(id);
            if (aspNetAssignment == null)
            {
                return HttpNotFound();
            }
            return View(aspNetAssignment);
        }

        // GET: AspNetAssignment/Create
        public ActionResult Create()
        {

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(s=>s.TeacherID==TeacherID), "Id", "SubjectName");
            
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name");
            return View();
        }

        // POST: AspNetAssignment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubjectID,ClassID,PublishDate,DueDate,Description,TotalMarks,Weightage,Title,FileName,TeacherID")] AspNetAssignment aspNetAssignment)
        {
            IEnumerable<string> topics = Request.Form["TopicID"].Split(',');
            HttpPostedFileBase file =Request.Files["document"];
            if (ModelState.IsValid)
            {
                aspNetAssignment.TeacherID = TeacherID;
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/App_Data/Assignments"), fileName);
                    file.SaveAs(path);
                    aspNetAssignment.FileName= fileName;
                }
                db.AspNetAssignments.Add(aspNetAssignment);
                db.SaveChanges();

                int assignmentID = db.AspNetAssignments.Max(item => item.Id);
                List<string> StudentIDs = db.AspNetStudent_Subject.Where(s => s.SubjectID == aspNetAssignment.SubjectID).Select(s=>s.StudentID).ToList();
                foreach(var item in StudentIDs)
                {
                    Student_Assignment stu_assign = new Student_Assignment();
                    stu_assign.StudentID = item;
                    stu_assign.AssignmentID = assignmentID;
                    stu_assign.FileName = aspNetAssignment.FileName;
                    stu_assign.Status = "Not Submitted";
                    db.Student_Assignment.Add(stu_assign);
                    db.SaveChanges();
                }
                foreach(var item in topics)
                {
                    AspNetAssignment_Topic assign_topic = new AspNetAssignment_Topic();
                    assign_topic.TopicID = Convert.ToInt32(item);
                    assign_topic.AssignmentID = assignmentID;
                    db.AspNetAssignment_Topic.Add(assign_topic);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetAssignment.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetAssignment.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetAssignment.TeacherID);
            return View(aspNetAssignment);
        }

        public FileResult downloadFile(int id)
        {
            AspNetAssignment Assignment = db.AspNetAssignments.Find(id);

            var filepath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Assignments/"), Assignment.FileName);
            return File(filepath, MimeMapping.GetMimeMapping(filepath), Assignment.FileName);

        }

        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id && r.TeacherID == TeacherID).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult TopicsBySubject(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetTopic> topic = db.AspNetTopics.Where(r => r.SubjectID == id).OrderByDescending(r => r.Id).ToList();
           
            return Json(topic, JsonRequestBehavior.AllowGet);

        }

        // GET: AspNetAssignment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAssignment aspNetAssignment = db.AspNetAssignments.Find(id);
            if (aspNetAssignment == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetAssignment.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetAssignment.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetAssignment.TeacherID);
            return View(aspNetAssignment);
        }

        // POST: AspNetAssignment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubjectID,ClassID,PublishDate,DueDate,Description,Title,FileName,TeacherID")] AspNetAssignment aspNetAssignment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetAssignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetAssignment.ClassID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetAssignment.SubjectID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetAssignment.TeacherID);
            return View(aspNetAssignment);
        }

        // GET: AspNetAssignment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetAssignment aspNetAssignment = db.AspNetAssignments.Find(id);
            if (aspNetAssignment == null)
            {
                return HttpNotFound();
            }
            return View(aspNetAssignment);
        }

        // POST: AspNetAssignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetAssignment aspNetAssignment = db.AspNetAssignments.Find(id);
            db.AspNetAssignments.Remove(aspNetAssignment);
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
