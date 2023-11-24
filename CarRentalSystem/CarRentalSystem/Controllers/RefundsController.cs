using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class RefundsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Refunds
        public ActionResult Index()
        {
            var refunds = db.Refunds.Include(r => r.Booking);
            return View(refunds.ToList());
        }
        public ActionResult MyRefunds()
        {
            var refunds = db.Refunds.Include(r => r.Booking);
            return View(refunds.ToList());
        }
        public ActionResult Decline(int id, string reason)
        {
            try
            {
                var refund = db.Refunds.Find(id);
                refund.Status = "Declined";
                var car = db.Cars.Find(refund.Booking.Car.CarId);
                db.Entry(refund).State = EntityState.Modified;
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(refund.emailaddress);
                    email2.Subject = "Car Hire For " + car.Name + "Declined";
                    string emailBody = $"Dear {refund.Booking.Name} {refund.Booking.Surname},\n\n" +
                   $"We regret to inform you that your refund for car hire request for {car.Name} has been declined due to the following reason:\n" +
                   $"\n{reason}\n\n" +
                   $"We apologize for any inconvenience this may have caused. If you have any questions or need further assistance, please feel free to contact our customer support.\n" +
                   $"\nThank you for considering Durban Car Hire for your car rental needs.\n" +
                   $"\nWarm Regards,\n" +
                   $"Durban Car Hire";
                    email2.Body = emailBody;

                    //    email2.Body = "Dear" + booking.Name + " " + booking.Surname + "\n\nWe regret to inform you that your car hire request for " + car.Name + ", was declined due to "+ Reason+ ".\n" +
                    //"\n\n\n" +
                    //"Thank you." +
                    //"\n" +
                    //"\n\nWarm Regards,\n" + "Durban Car Hire";
                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();
                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email2);

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("Index");
                }



                db.SaveChanges();
                TempData["Message"] = "Booking declined successfully, email sent to client.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "Something went wrong, please try again later.";
                return RedirectToAction("Index");
            }
        }

        // GET: Refunds/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Refund refund = db.Refunds.Find(id);
            if (refund == null)
            {
                return HttpNotFound();
            }
            return View(refund);
        }

        // GET: Refunds/Create
        public ActionResult Create(int id)
        {
            var Book = db.Bookings.Find(id);
            double fee = Book.FinalCost * 0.15;
            double tobePaid = Book.FinalCost - fee;

            Refund b = new Refund()
            {
                RefundDate = DateTime.Now.Date,
                RefundFee = fee,
                tobePaid = tobePaid,
                InitialAmt = Book.FinalCost
            };
            Session["BookId"] = id.ToString();
            return View(b);
        }

        // POST: Refunds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RefundId,InitialAmt,RefundDate,RefundFee,tobePaid")] Refund refund)
        {
            if (ModelState.IsValid)
            {
                string BookID = Session["BookId"] as string;
                int bookId = int.Parse(BookID);
                refund.BookingId = bookId;
                var book = db.Bookings.Find(bookId);
                book.Status = "Cancelled+Refund";
                refund.Status = "Pending";
                refund.emailaddress = User.Identity.Name;
                db.Entry(book).State = EntityState.Modified;
                db.Refunds.Add(refund);
                db.SaveChanges();
                return RedirectToAction("MyRefunds");
            }

            
            return View(refund);
        }

        // GET: Refunds/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Refund refund = db.Refunds.Find(id);
            if (refund == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", refund.BookingId);
            return View(refund);
        }

        // POST: Refunds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RefundId,BookingId,Reason,DeclineReason,RefundDate,RefundStatus,emailaddress,RefundFee,tobePaid")] Refund refund)
        {
            if (ModelState.IsValid)
            {
                db.Entry(refund).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", refund.BookingId);
            return View(refund);
        }

        // GET: Refunds/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Refund refund = db.Refunds.Find(id);
            if (refund == null)
            {
                return HttpNotFound();
            }
            return View(refund);
        }

        // POST: Refunds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Refund refund = db.Refunds.Find(id);
            db.Refunds.Remove(refund);
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
