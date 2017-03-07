using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OfficeOpenXml;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Admin")]
    public class Admin_DashboardController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        // GET: Admin_Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public Admin_DashboardController()
        {
            
        }

        public Admin_DashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ActionResult Admin_Dashboard()
        {
            return View("Admin_Dashboard");
        }

        /*******************************************************************************************************************
         * 
         *                                    Teacher's Functions
         *                                    
         *******************************************************************************************************************/
        public ActionResult TeacherRegister()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TeacherRegister(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name=model.Name  };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Teacher");
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TeacherfromFile(RegisterViewModel model)
        {
           // if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["teachers"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
                var teacherList = new List<RegisterViewModel>();
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var teacher = new RegisterViewModel();
                        teacher.Email= workSheet.Cells[rowIterator, 1].Value.ToString();
                        teacher.Name= workSheet.Cells[rowIterator, 2].Value.ToString();
                        teacher.UserName= workSheet.Cells[rowIterator, 3].Value.ToString();
                        teacher.Password= workSheet.Cells[rowIterator, 4].Value.ToString();
                        teacher.ConfirmPassword= workSheet.Cells[rowIterator, 5].Value.ToString();
                        var checkUserName= await UserManager.FindByNameAsync(teacher.UserName);
                        if(checkUserName != null)
                        {
                            var localTeacher = new ApplicationUser { UserName = teacher.UserName, Email = teacher.Email, Name = teacher.Name };
                            var localresult = await UserManager.CreateAsync(localTeacher, teacher.Password);
                            AddErrors(localresult);
                            return View("TeacherRegister",model);
                            
                        }
                        else
                        {
                            teacherList.Add(teacher);
                        }
                       
                    }
                }
                ApplicationDbContext context = new ApplicationDbContext();
                foreach (var item in teacherList)
                {
                    var user = new ApplicationUser { UserName = item.UserName, Email = item.Email, Name = item.Name };
                    var result = await UserManager.CreateAsync(user, item.Password);
                    if (result.Succeeded)
                    {
                        var roleStore = new RoleStore<IdentityRole>(context);
                        var roleManager = new RoleManager<IdentityRole>(roleStore);
                        var userStore = new UserStore<ApplicationUser>(context);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        userManager.AddToRole(user.Id, "Teacher");
                    }
                    else
                    {
                        AddErrors(result);
                        return View("TeacherRegister", model);
                    }     
                }
            }
            return RedirectToAction("TeacherIndex", "AspNetUser");
        }

       
        /*******************************************************************************************************************
         * 
         *                                    Student's Functions
         *                                    
         *******************************************************************************************************************/
        public ActionResult StudentRegister()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentRegister(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                IEnumerable<string> selectedsubjects = Request.Form["subjects"].Split(',');
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Student");
                
                   
                    foreach (var item in selectedsubjects)
                    {
                        AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                        stu_sub.StudentID = user.Id;
                        stu_sub.SubjectID = Convert.ToInt32(item);
                        db.AspNetStudent_Subject.Add(stu_sub);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentfromFile(RegisterViewModel model)
        {
            // if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["students"];
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
                        var student = new RegisterViewModel();
                        student.Email = workSheet.Cells[rowIterator, 1].Value.ToString();
                        student.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                        student.UserName = workSheet.Cells[rowIterator, 3].Value.ToString();
                        student.Password = workSheet.Cells[rowIterator, 4].Value.ToString();
                        student.ConfirmPassword = workSheet.Cells[rowIterator, 5].Value.ToString();
                        var checkUserName = await UserManager.FindByNameAsync(student.UserName);
                        if (checkUserName != null)
                        {
                            var localTeacher = new ApplicationUser { UserName = student.UserName, Email = student.Email, Name = student.Name };
                            var localresult = await UserManager.CreateAsync(localTeacher, student.Password);
                            AddErrors(localresult);
                            return View("StudentRegister", model);

                        }
                        else
                        {
                            studentList.Add(student);
                        }
                    }
                }
                ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
                ApplicationDbContext context = new ApplicationDbContext();
                int rowIteratortemp = 2;
                foreach (var item in studentList)
                {
                    var user = new ApplicationUser { UserName = item.UserName, Email = item.Email, Name = item.Name };
                    var result = await UserManager.CreateAsync(user, item.Password);
                    if (result.Succeeded)
                    {
                        var roleStore = new RoleStore<IdentityRole>(context);
                        var roleManager = new RoleManager<IdentityRole>(roleStore);
                        var userStore = new UserStore<ApplicationUser>(context);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        userManager.AddToRole(user.Id, "Student");

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            var subjects = new List<string>();
                           
                                var Class= workSheet.Cells[rowIteratortemp, 6].Value.ToString();
                                subjects.Add(workSheet.Cells[rowIteratortemp, 7].Value.ToString());
                                subjects.Add(workSheet.Cells[rowIteratortemp, 8].Value.ToString());
                                subjects.Add(workSheet.Cells[rowIteratortemp, 9].Value.ToString());
                                subjects.Add(workSheet.Cells[rowIteratortemp, 10].Value.ToString());
                                subjects.Add(workSheet.Cells[rowIteratortemp, 11].Value.ToString());

                                var d = (from subject in db.AspNetSubjects
                                         join Classes in db.AspNetClasses on subject.ClassID equals Classes.Id
                                         where Classes.ClassName == Class && subjects.Contains(subject.SubjectName)
                                         select subject).ToList();

                                foreach(var subjectid in d)
                                {
                                    AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                                    stu_sub.StudentID = user.Id;
                                    stu_sub.SubjectID = subjectid.Id;
                                    db.AspNetStudent_Subject.Add(stu_sub);
                                    db.SaveChanges();
                                }

                             
                        }
                    }
                    else
                    {
                        AddErrors(result);
                        return View("StudentRegister",model);
                    }
                    rowIteratortemp++;
                }
            }
            return RedirectToAction("StudentsIndex", "AspNetUser");
           // return View("StudentIndex","Asp);
        }

        /**********************************************************************************************************************************/


        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion



    }

}