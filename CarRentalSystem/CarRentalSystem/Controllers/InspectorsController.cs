using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class InspectorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Inspectors
        public ActionResult Index(string bookId = "none")
        {
            if(bookId!="none")
            {
                Session["bookID"] = bookId;
            }
            return View(db.Inspectors.ToList());
        }
        public ActionResult AssignInspector (int inspId)
        {
            try
            {
                string bookID = Session["bookID"] as string;
                int bookId = int.Parse(bookID);
                var inspector = db.Inspectors.Find(inspId);
                var booking = db.Bookings.Find(bookId);
                booking.Status = "Awaiting-Collection";
                db.Entry(booking).State = EntityState.Modified;
                AssignInspector assign = new AssignInspector()
                {
                    BookingId = bookId,
                    InspId = inspId,
                    Name = inspector.Name,
                    Surname = inspector.Surname,
                    Status = "Assigned",
                    Email = inspector.Email
                };
                db.AssignInspectors.Add(assign);
                var email2 = new MailMessage();
                email2.From = new MailAddress("SnapDrive2023@outlook.com");
                email2.To.Add(inspector.Email);
                email2.Subject = "NEW TASK ASSIGNED";
                string emailBody = $"Dear {inspector.Name} {inspector.Surname},\n\n" +
               $"Please note that you have been assigned to a task to handover a vehicle to a client on {booking.PickupDate} at {booking.PickupTime}\n" +
               $"\nWarm Regards,\n" +
               $"Durban Car Hire";
                email2.Body = emailBody;

                
                var smtpClient = new SmtpClient();
                smtpClient.Send(email2);
                db.SaveChanges();
                TempData["Message"] = "Inspector assigned successfully, email sent to inspector.";

                return RedirectToAction("Index", "Bookings");

            }
            catch
            {
                TempData["Message"] = "Something went wrong, please try again later.";
                return RedirectToAction("Index", "Bookings");
            }

        }

        // GET: Inspectors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inspector inspector = db.Inspectors.Find(id);
            if (inspector == null)
            {
                return HttpNotFound();
            }
            return View(inspector);
        }

        // GET: Inspectors/Create
        public ActionResult Create()
        {
            Inspector b = new Inspector()
            {
                Email = User.Identity.Name
            };
            return View(b);
        }

        // POST: Inspectors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InspId,Name,Surname,Email,Picture")] Inspector inspector, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Save the picture file on the server
                string pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string picturePath = Path.Combine(Server.MapPath("~/"), pictureFileName);
                file.SaveAs(picturePath);

                // Set the picture path in the record
                inspector.Picture = pictureFileName;
                db.Inspectors.Add(inspector);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(inspector);
        }

        // GET: Inspectors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inspector inspector = db.Inspectors.Find(id);
            if (inspector == null)
            {
                return HttpNotFound();
            }
            return View(inspector);
        }

        // POST: Inspectors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InspId,Name,Surname,Email,Picture")] Inspector inspector)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inspector).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inspector);
        }

        // GET: Inspectors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inspector inspector = db.Inspectors.Find(id);
            if (inspector == null)
            {
                return HttpNotFound();
            }
            return View(inspector);
        }

        // POST: Inspectors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inspector inspector = db.Inspectors.Find(id);
            db.Inspectors.Remove(inspector);
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
