using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class CarReturnsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CarReturns
        public ActionResult Index()
        {
            var carReturns = db.CarReturns.Include(c => c.Booking);
            return View(carReturns.ToList());
        }


        public ActionResult ReturnCar(int id,int assId)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                booking.Status = "Vehicle-Returned";
                var ass = db.AssignInspectors.Find(assId);
                ass.Status = "Vehicle-Returned";
                db.Entry(booking).State = EntityState.Modified;
                db.Entry(ass).State = EntityState.Modified;
                CarReturn returns = new CarReturn()
                {
                    BookingId = id,
                    Date = DateTime.Now.Date,
                    Time = DateTime.Now.ToShortTimeString(),
                };
                db.CarReturns.Add(returns);
                var email2 = new MailMessage();
                email2.From = new MailAddress("SnapDrive2023@outlook.com");
                email2.To.Add(booking.Email);
                email2.Subject = "Car Returned";
                string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
               $"Please note that we have received the vehicle, we still have to run some inspections to check if all is fine.\n" +
               $"If you have any questions or need further assistance, please feel free to contact our customer support.\n" +
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
                db.SaveChanges();
                TempData["Message"] = "Vehicle marked as returned, email sent to client.";

                return RedirectToAction("MyAssignments", "AssignInspectors");
            }
            catch{
                TempData["Message"] = "Something went wrong, please try again later.";

                return RedirectToAction("MyAssignments", "AssignInspectors");
            }
            
        }

        // GET: CarReturns/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarReturn carReturn = db.CarReturns.Find(id);
            if (carReturn == null)
            {
                return HttpNotFound();
            }
            return View(carReturn);
        }

        // GET: CarReturns/Create
        public ActionResult Create()
        {
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name");
            return View();
        }

        // POST: CarReturns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReturnId,BookingId,Date,Time")] CarReturn carReturn)
        {
            if (ModelState.IsValid)
            {
                db.CarReturns.Add(carReturn);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", carReturn.BookingId);
            return View(carReturn);
        }

        // GET: CarReturns/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarReturn carReturn = db.CarReturns.Find(id);
            if (carReturn == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", carReturn.BookingId);
            return View(carReturn);
        }

        // POST: CarReturns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReturnId,BookingId,Date,Time")] CarReturn carReturn)
        {
            if (ModelState.IsValid)
            {
                db.Entry(carReturn).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", carReturn.BookingId);
            return View(carReturn);
        }

        // GET: CarReturns/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarReturn carReturn = db.CarReturns.Find(id);
            if (carReturn == null)
            {
                return HttpNotFound();
            }
            return View(carReturn);
        }

        // POST: CarReturns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CarReturn carReturn = db.CarReturns.Find(id);
            db.CarReturns.Remove(carReturn);
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
