using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Car);
            return View(bookings.ToList());
        }

        public ActionResult MyBookings()
        {
            var bookings = db.Bookings.Where(x=>x.Email == User.Identity.Name).Include(b => b.Car);
            return View(bookings.ToList());
        }
        public ActionResult DownloadIdPdf(int id)
        {
            var pdfFile = db.Bookings.Find(id);
            if (pdfFile != null)
            {
                return File(pdfFile.IdContent, "application/pdf", pdfFile.IdFile);
            }
            return HttpNotFound();
        }
        public ActionResult DownloadLicensePdf(int id)
        {
            var pdfFile = db.Bookings.Find(id);
            if (pdfFile != null)
            {
                return File(pdfFile.LicenseContent, "application/pdf", pdfFile.LicenseFile);
            }
            return HttpNotFound();
        } 
        public ActionResult DownloadBankStatementPdf(int id)
        {
            var pdfFile = db.Bookings.Find(id);
            if (pdfFile != null)
            {
                return File(pdfFile.BankStatContent, "application/pdf", pdfFile.BankStatName);
            }
            return HttpNotFound();
        }
        public ActionResult Settle(int id)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                booking.Status = "Settled";
                var car = db.Cars.Find(booking.Car.CarId);
                db.Entry(booking).State = EntityState.Modified;
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(booking.Email);
                    email2.Subject = "Car Hire For " + car.Name + "Settled";
                    string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"Please note that your car hire request for {car.Name} has been settled please ensure that you settle any outstanding payments.\n" +
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

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("Index");
                }



                db.SaveChanges();
                TempData["Message"] = "Booking completed successfully, email sent to client.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "Something went wrong, please try again later.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Approve(int id)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                booking.Status = "Approved";
                var car = db.Cars.Find(booking.Car.CarId);
                db.Entry(booking).State = EntityState.Modified;
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(booking.Email);
                    email2.Subject = "Car Hire For " + car.Name + "Approved";
                    string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"Congratulations your car hire request for {car.Name} has been approved please ensure that you make your payment 2 days before pickup\n" +
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

                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " + ex.Message;
                    return RedirectToAction("Index");
                }



                db.SaveChanges();
                TempData["Message"] = "Booking Approved successfully, email sent to client.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Message"] = "Something went wrong, please try again later.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Decline(int id, string reason)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                booking.Status = "Declined";
                var car = db.Cars.Find(booking.Car.CarId);
                car.Status = "Available";
                db.Entry(car).State = EntityState.Modified;
                db.Entry(booking).State = EntityState.Modified;
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(booking.Email);
                    email2.Subject = "Car Hire For " + car.Name + "Declined";
                    string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"We regret to inform you that your car hire request for {car.Name} has been declined due to the following reason:\n" +
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

        public ActionResult Cancel(int id) {
            try
            {
                var booking = db.Bookings.Find(id);
                booking.Status = "Cancelled";
                var car = db.Cars.Find(booking.Car.CarId);
                car.Status = "Available";
                db.Entry(car).State = EntityState.Modified;
                db.Entry(booking).State = EntityState.Modified;
                try
                {
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(booking.Email);
                    email2.Subject = "Car Hire For " + car.Name +"Cancelled";
                    string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"We received your request to cancel your car hire reservation for {car.Name}.\n" +
                   $"\nYour request has been processed, and the reservation has been canceled. If you did not initiate this cancellation or have any concerns, please contact our customer support immediately.\n" +
                   $"\nThank you for choosing Durban Car Hire. We hope to serve you in the future.\n" +
                   $"\nWarm Regards,\n" +
                   $"Durban Car Hire";
                    email2.Body = emailBody;

                    //    email2.Body = "Dear" + booking.Name + " " + booking.Surname + "\n\nYou have cancelled your car hire request for " + car.Name  + ".\n" +
                    //"\n\n\n" +
                    //"Thank you." +
                    //"\n" +
                    //"\n\nWarm Regards,\n" + "Durban Car Hire";
                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();
                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email2);

                }
                catch(Exception ex)
                {
                    TempData["Message"] = "Failed to send email due to, " +ex.Message;
                    return RedirectToAction("MyBookings");
                }



                db.SaveChanges();
                TempData["Message"] = "Booking cancelled successfully, please check your emails for more info.";
                return RedirectToAction("MyBookings");
            }
            catch
            {
                TempData["Message"] = "Something went wrong, please try again later.";
                return RedirectToAction("MyBookings");
            }
            

            
        }


        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create(int CarId)
        {
            Session["CarId"] = CarId.ToString();
            Booking b = new Booking()
            {
                Email = User.Identity.Name
            };
            return View(b);
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookingId,CarId,Name,Surname,Email,IdNumber,PickupDate,PickupTime,ReturnTime,ReturnDate,Destination,Cost,DistCost,FinalCost,LicenseFile,LicenseContent,IdFile,IdContent,BankStatName,BankStatContent")] Booking booking,HttpPostedFileBase LicenseFile, HttpPostedFileBase IdFile,HttpPostedFileBase BankStatFile)
        {
            if (ModelState.IsValid)
            {
                string CarID = Session["CarId"] as string;
                if(CarID!=null)
                {
                    try
                    {
                        booking.CarId = int.Parse(CarID);
                        var car = db.Cars.Find(booking.CarId);
                        booking.Status = "Awaiting Approval";
                        car.Status = "Booked";
                        db.Entry(car).State = EntityState.Modified;
                        if (LicenseFile != null && LicenseFile.ContentLength > 0)
                        {
                            byte[] pdfData;
                            using (var binaryReader = new BinaryReader(LicenseFile.InputStream))
                            {
                                pdfData = binaryReader.ReadBytes(LicenseFile.ContentLength);
                            }

                            // Create a PdfFile object and save it to the database

                            booking.LicenseFile = "License.pdf";
                            booking.LicenseContent = pdfData;
                        }
                        if (IdFile != null && IdFile.ContentLength > 0)
                        {
                            byte[] pdfData;
                            using (var binaryReader = new BinaryReader(IdFile.InputStream))
                            {
                                pdfData = binaryReader.ReadBytes(IdFile.ContentLength);
                            }

                            // Create a PdfFile object and save it to the database

                            booking.IdFile = "ID Copy.pdf";
                            booking.IdContent = pdfData;
                        }
                        if (BankStatFile != null && BankStatFile.ContentLength > 0)
                        {
                            byte[] pdfData;
                            using (var binaryReader = new BinaryReader(BankStatFile.InputStream))
                            {
                                pdfData = binaryReader.ReadBytes(BankStatFile.ContentLength);
                            }

                            // Create a PdfFile object and save it to the database

                            booking.BankStatName = "Bank Statement.pdf";
                            booking.BankStatContent = pdfData;
                        }
                        db.Bookings.Add(booking);
                        try
                        {
                            // Prepare email message
                            var email2 = new MailMessage();
                            email2.From = new MailAddress("SnapDrive2023@outlook.com");
                            email2.To.Add(booking.Email);
                            email2.Subject = "Car Hire For " + car.Name;
                            string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"We want to inform you that we have received your car hire request for {car.Name}. Our team will review your request, and once approved, you will receive a confirmation.\n" +
                   $"\nThank you for choosing Durban Car Hire. We appreciate your business and look forward to serving you.\n" +
                   $"\nWarm Regards,\n" +
                   $"Durban Car Hire";
                            email2.Body = emailBody;

                            //    email2.Body = "Dear" + booking.Name + " " + booking.Surname + "\n\n Please note that we have received your car hire request for "+ Car.Name +" one of our staff will review and approve ASP"  + ".\n" +
                            //"\n\n\n" +
                            //"Thank you." +
                            //"\n" +
                            //"\n\nWarm Regards,\n" + "Durban Car Hire";
                            // Use the SMTP settings from web.config
                            var smtpClient = new SmtpClient();
                            // The SmtpClient will automatically use the settings from web.config
                            smtpClient.Send(email2);
                        }
                        catch(Exception ex)
                        {

                            TempData["Message"] = "Failed to send email due to, "+ex.Message + ".";
                            return View(booking);
                        }
                       
                        db.SaveChanges();
                        TempData["Message"] = "Your booking has been successfully submitted, Please check your emails for more details.";
                        return RedirectToAction("MyBookings");

                    }
                    catch
                    {
                        TempData["Message"] = "Something went wrong, please try again later.";
                        return View(booking);
                    }





                }


                else
                {
                    TempData["Message"] = "Sorry, your Session has expired.";
                    return RedirectToAction("Brochure","Cars");
                }
                
            }

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.CarId = new SelectList(db.Cars, "CarId", "Name", booking.CarId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingId,CarId,Name,Surname,Email,IdNumber,PickupDate,PickupTime,ReturnTime,ReturnDate,Cost,LicenseFile,LicenseContent,IdFile,IdContent,BankStatName,BankStatContent")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CarId = new SelectList(db.Cars, "CarId", "Name", booking.CarId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
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
