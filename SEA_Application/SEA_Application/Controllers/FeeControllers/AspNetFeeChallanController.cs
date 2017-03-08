using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using System.IO;
using System.Web.UI;
using System.Text;
using System.IO;
using System.Text;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

namespace SEA_Application.Controllers.FeeControllers
{
    public class AspNetFeeChallanController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetFeeChallan
        public ActionResult Index()
        {
            var aspNetFeeChallans = db.AspNetFeeChallans.Include(a => a.AspNetClass).Include(a => a.AspNetDurationType);
            return View(aspNetFeeChallans);
        }

        // GET: AspNetFeeChallan/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetFeeChallan aspNetFeeChallan = db.AspNetFeeChallans.Find(id);
            if (aspNetFeeChallan == null)
            {
                return HttpNotFound();
            }
            return View(aspNetFeeChallan);
        }

        // GET: AspNetFeeChallan/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.DurationTypeID = new SelectList(db.AspNetDurationTypes, "Id", "TypeName");
            return View();
        }


        public List<AspNetStudent_Payment> getStudentPayAbleFee(AspNetFeeChallan aspNetFeeChallan)
        {
            int challanformId = db.AspNetFeeChallans.Max(item => item.Id);
            List<AspNetStudent_Payment> studentsPayments = new List<AspNetStudent_Payment>();
            List<AspNetUser> students = (from student in db.AspNetUsers
                                         join student_subject in db.AspNetStudent_Subject on student.Id equals student_subject.StudentID
                                         join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                                         where subject.ClassID == aspNetFeeChallan.ClassID
                                         select student).Distinct().ToList();

            foreach (var item in students)
            {
                AspNetStudent_Payment student_payment = new AspNetStudent_Payment();
                student_payment.FeeChallanID = challanformId;
                student_payment.StudentID = item.Id;

                int? discountSum = db.AspNetStudent_Discount.Where(x => x.StudentID == item.Id).Sum(x => x.Percentage);
                if (discountSum == null)
                {
                    discountSum = 0;
                }
                int? tuitionFee = db.AspNetClass_FeeType.Where(x => x.AspNetFeeType.TypeName == "Tuition Fee").Select(x => x.Amount).FirstOrDefault();
                int? discount = tuitionFee * discountSum / 100;
                int payableAmount = Convert.ToInt32(tuitionFee - discount);
                string FeeDurationTypeName = db.AspNetDurationTypes.Where(x => x.Id == aspNetFeeChallan.DurationTypeID).Select(x => x.TypeName).FirstOrDefault();
                int? totalAmount = (from class_fee in db.AspNetClass_FeeType
                                    where class_fee.ClassID == aspNetFeeChallan.ClassID
                                    select class_fee.Amount).Sum();
                if (FeeDurationTypeName == "Monthly")
                {


                    int others = Convert.ToInt32(totalAmount - tuitionFee);
                    payableAmount = payableAmount + others;


                }
                else if (FeeDurationTypeName == "Quarterly")
                {


                    int others = Convert.ToInt32(totalAmount - tuitionFee);
                    payableAmount = payableAmount + others;
                    payableAmount = payableAmount * 4;
                }
                else if (FeeDurationTypeName == "6 Months")
                {


                    int others = Convert.ToInt32(totalAmount - tuitionFee);
                    payableAmount = payableAmount + others;
                    payableAmount = payableAmount * 6;
                }
                else if (FeeDurationTypeName == "Yearly")
                {
                    int others = Convert.ToInt32(totalAmount - tuitionFee);
                    payableAmount = payableAmount + others;
                    payableAmount = payableAmount * 12;

                }

                student_payment.PaymentAmount = payableAmount;
                studentsPayments.Add(student_payment);
            }
            return studentsPayments;
        }

        // POST: AspNetFeeChallan/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClassID,DueDate,DurationTypeID,TotalAmount,StartDate,EndDate")] AspNetFeeChallan aspNetFeeChallan)
        {
            if (ModelState.IsValid)
            {
               db.AspNetFeeChallans.Add(aspNetFeeChallan);
               db.SaveChanges();
               var studenstPayments = getStudentPayAbleFee(aspNetFeeChallan);
               foreach(var item in studenstPayments)
                {
                    db.AspNetStudent_Payment.Add(item);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetFeeChallan.ClassID);
            ViewBag.DurationTypeID = new SelectList(db.AspNetDurationTypes, "Id", "TypeName", aspNetFeeChallan.DurationTypeID);
            return View(aspNetFeeChallan);
        }

        // GET: AspNetFeeChallan/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetFeeChallan aspNetFeeChallan = db.AspNetFeeChallans.Find(id);
            if (aspNetFeeChallan == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetFeeChallan.ClassID);
            ViewBag.DurationTypeID = new SelectList(db.AspNetDurationTypes, "Id", "TypeName", aspNetFeeChallan.DurationTypeID);
            return View(aspNetFeeChallan);
        }

        // POST: AspNetFeeChallan/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClassID,DueDate,DurationTypeID,TotalAmount,StartDate,EndDate")] AspNetFeeChallan aspNetFeeChallan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetFeeChallan).State = EntityState.Modified;
                db.SaveChanges();

                var student_payments = (from studentpayment in db.AspNetStudent_Payment
                                        where studentpayment.FeeChallanID == aspNetFeeChallan.Id
                                        select studentpayment).ToList();
                var studentspayments = getStudentPayAbleFee(aspNetFeeChallan);
                int x = 0;
                foreach(var item in student_payments)
                {
                    AspNetStudent_Payment studentpayment = (from student_payment in db.AspNetStudent_Payment
                                         where student_payment.Id == item.Id
                                         select student_payment).SingleOrDefault();
                    
                    studentpayment.FeeChallanID = item.FeeChallanID;
                    studentpayment.Id = item.Id;
                    if(item.PaymentDate==null)
                    {
                        studentpayment.PaymentAmount = studentspayments[x].PaymentAmount;
                    }
                    else
                    {
                        studentpayment.PaymentAmount = studentspayments[x].PaymentAmount - item.PaymentAmount;
                        if(studentpayment.PaymentAmount<0)
                        {
                            studentpayment.PaymentAmount = studentpayment.PaymentAmount * (-1);
                        }
                        studentpayment.PaymentDate = null;
                    }
                    studentpayment.StudentID = item.StudentID;
                    x++;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetFeeChallan.ClassID);
            ViewBag.DurationTypeID = new SelectList(db.AspNetDurationTypes, "Id", "TypeName", aspNetFeeChallan.DurationTypeID);
            return View(aspNetFeeChallan);
        }

        
        // GET: AspNetFeeChallan/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetFeeChallan aspNetFeeChallan = db.AspNetFeeChallans.Find(id);
            if (aspNetFeeChallan == null)
            {
                return HttpNotFound();
            }
            return View(aspNetFeeChallan);
        }

        // POST: AspNetFeeChallan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetFeeChallan aspNetFeeChallan = db.AspNetFeeChallans.Find(id);
            db.AspNetFeeChallans.Remove(aspNetFeeChallan);
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

        public class fee_challan
        {
            public int? amount { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }

        [HttpGet]
        public JsonResult ChallanByDuration(int classId, int durationId)
        {
            string duration = db.AspNetDurationTypes.Where(x => x.Id == durationId).Select(x => x.TypeName).FirstOrDefault();
            int? amount = (from class_fee in db.AspNetClass_FeeType
                           where class_fee.ClassID == classId
                           select class_fee.Amount).Sum();
            DateTime startDate = Convert.ToDateTime(db.AspNetFeeChallans.Where(x => x.ClassID == classId).OrderByDescending(x => x.Id).Select(x => x.EndDate).FirstOrDefault());
            if (startDate == Convert.ToDateTime("1/1/0001 12:00:00 AM"))
            {
                startDate = Convert.ToDateTime(db.AspNetSessions.OrderByDescending(x => x.Id).Select(x => x.SessionStartDate).FirstOrDefault());
            }
            DateTime endDate = startDate.AddMonths(1);


            if (duration == "Monthly")
            {
                endDate = startDate.AddMonths(1);
               
            }
            else if (duration == "Quarterly")
            {
                endDate = startDate.AddMonths(4);
                amount = amount * 4;
            }
            else if (duration == "6 Months")
            {
                endDate = startDate.AddMonths(6);
                amount = amount * 6;
            }
            else if (duration == "Yearly")
            {
                endDate = startDate.AddMonths(12);
                amount = amount * 12;
            }
            fee_challan challan = new fee_challan();
            challan.amount = amount;
            challan.endDate = endDate;
            challan.startDate = startDate;
            return Json(challan, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult ChallanByDurationEdit(int classId, int durationId)
        {
            string duration = db.AspNetDurationTypes.Where(x => x.Id == durationId).Select(x => x.TypeName).FirstOrDefault();
            int? amount = (from class_fee in db.AspNetClass_FeeType
                           where class_fee.ClassID == classId
                           select class_fee.Amount).Sum();
            DateTime startDate = Convert.ToDateTime(db.AspNetFeeChallans.Where(x => x.ClassID == classId).OrderByDescending(x => x.Id).Select(x => x.StartDate).FirstOrDefault());
            if (startDate == Convert.ToDateTime("1/1/0001 12:00:00 AM"))
            {
                startDate = Convert.ToDateTime(db.AspNetSessions.OrderByDescending(x => x.Id).Select(x => x.SessionStartDate).FirstOrDefault());
            }
            DateTime endDate = startDate.AddMonths(1);


            if (duration == "Monthly")
            {
                endDate = startDate.AddMonths(1);

            }
            else if (duration == "Quarterly")
            {
                endDate = startDate.AddMonths(4);
                amount = amount * 4;
            }
            else if (duration == "6 Months")
            {
                endDate = startDate.AddMonths(6);
                amount = amount * 6;
            }
            else if (duration == "Yearly")
            {
                endDate = startDate.AddMonths(12);
                amount = amount * 12;
            }
            fee_challan challan = new fee_challan();
            challan.amount = amount;
            challan.endDate = endDate;
            challan.startDate = startDate;
            return Json(challan, JsonRequestBehavior.AllowGet);
        }
        

       public void GenerateInvoicePDF(object sender, EventArgs e)
        {
            //Dummy data for Invoice (Bill).
            string companyName = "ASPSnippets";
            int orderNo = 2303;
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[5] {
                            new DataColumn("ProductId", typeof(string)),
                            new DataColumn("Product", typeof(string)),
                            new DataColumn("Price", typeof(int)),
                            new DataColumn("Quantity", typeof(int)),
                            new DataColumn("Total", typeof(int))});
            dt.Rows.Add(101, "Sun Glasses", 200, 5, 1000);
            dt.Rows.Add(102, "Jeans", 400, 2, 800);
            dt.Rows.Add(103, "Trousers", 300, 3, 900);
            dt.Rows.Add(104, "Shirts", 550, 2, 1100);

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    StringBuilder sb = new StringBuilder();

                    
                    //Generate Invoice (Bill) Header.
                    sb.Append("<div class='container'><div class='row'> <div class=' well col - md - 4'> <div class='row'> <div class='col - xs - 6 col - sm - 6 col - md - 6'> <address> <strong>Elf Cafe</strong> <br> 2135 Sunset Blvd <br> Los Angeles, CA 90026 <br> <abbr title='Phone'>P:</abbr> (213) 484-6829 </address> </div> <div class='col - xs - 6 col - sm - 6 col - md - 6 text - right'> <p> <em>Date: 1st November, 2013</em> </p> <p> <em>Receipt #: 34522677W</em> </p> </div> </div> <div class='row'> <div class='text - center'> <h1>Receipt</h1> </div> <table class='table table - hover'> <thead> <tr> <th>Product</th> <th>#</th> <th class='text - center'>Price</th> <th class='text - center'>Total</th> </tr> </thead> <tbody> <tr> <td class='col - md - 9'><em>Baked Tart with Thyme and Garlic</em></h4></td> <td class='col - md - 1' style='text - align: center'> 3 </td> <td class='col - md - 1 text - center'>$16</td> <td class='col - md - 1 text - center'>$48</td> </tr> <tr> <td> </td> <td> </td> <td class='text - right'> <p> <strong>Subtotal: </strong> </p> <p> <strong>Tax: </strong> </p></td> <td class='text - center'> <p> <strong>$6.94</strong> </p> <p> <strong>$6.94</strong> </p></td> </tr> <tr> <td> </td> <td> </td> <td class='text - right'><h4><strong>Total: </strong></h4></td> <td class='text - center text - danger'><h4><strong>$31.53</strong></h4></td> </tr> </tbody> </table> </td> </div> </div> </div> </div> ");
                   /* sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
                    sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td></tr>");
                    sb.Append("<tr><td colspan = '2'></td></tr>");
                    sb.Append("<tr><td><b>Order No: </b>");
                    sb.Append(orderNo);
                    sb.Append("</td><td align = 'right'><b>Date: </b>");
                    sb.Append(DateTime.Now);
                    sb.Append(" </td></tr>");
                    sb.Append("<tr><td colspan = '2'><b>Company Name: </b>");
                    sb.Append(companyName);
                    sb.Append("</td></tr>");
                    sb.Append("</table>");
                    sb.Append("<br />");

                    //Generate Invoice (Bill) Items Grid.
                    sb.Append("<table border = '1'>");
                    sb.Append("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append(column.ColumnName);
                        sb.Append("</th>");
                    }
                    sb.Append("</tr>");
                    foreach (DataRow row in dt.Rows)
                    {
                        sb.Append("<tr>");
                        foreach (DataColumn column in dt.Columns)
                        {
                            sb.Append("<td>");
                            sb.Append(row[column]);
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }
                    sb.Append("<tr><td align = 'right' colspan = '");
                    sb.Append(dt.Columns.Count - 1);
                    sb.Append("'>Total</td>");
                    sb.Append("<td>");
                    sb.Append(dt.Compute("sum(Total)", ""));
                    sb.Append("</td>");
                    sb.Append("</tr></table>");
                    */
                    //Export HTML String as PDF.
                    StringReader sr = new StringReader(sb.ToString());
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                   
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    
                    
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                    pdfDoc.Open();
                    htmlparser.Parse(sr);
                    pdfDoc.Close();
                    Response.ContentType = "application/pdf";
                   
                    Response.AddHeader("content-disposition", "attachment;filename=Invoice_" + orderNo + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(pdfDoc);
                    Response.End();
                }
            }
        }
    }
}
