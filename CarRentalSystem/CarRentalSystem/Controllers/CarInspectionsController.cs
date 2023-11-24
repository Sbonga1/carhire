using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;

namespace CarRentalSystem.Controllers
{
    public class CarInspectionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CarInspections
        public ActionResult Index()
        {
            var carInspections = db.CarInspections.Include(c => c.Booking);
            return View(carInspections.ToList());
        }
        public ActionResult MyInspections()
        {
            var carInspections = db.CarInspections.Where(x=>x.InspEmail == User.Identity.Name).Include(c => c.Booking);
            return View(carInspections.ToList());
        }

        // GET: CarInspections/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarInspection carInspection = db.CarInspections.Find(id);
            if (carInspection == null)
            {
                return HttpNotFound();
            }
            return View(carInspection);
        }

        // GET: CarInspections/Create
        public ActionResult Create(int bookId,int assId)
        {
            Session["bookID"] = bookId.ToString();
            Session["AssID"] = assId.ToString();
            return View();
        }

        // POST: CarInspections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Dents,TireCondition,ExteriorCondition,InteriorCondition,TestDrive,CarMileage,FuelAmt,BookingId")] CarInspection carInspection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string BookId = Session["bookID"] as string;
                    string AssId = Session["AssID"] as string;
                    int assId = int.Parse(AssId);
                    int bookId = int.Parse(BookId);

                    var book = db.Bookings.Find(bookId);
                    var ass = db.AssignInspectors.Find(assId);
                    book.Status = "Inspection-Done";
                    ass.Status = "Settled";
                    var car = db.Cars.Find(book.CarId);
                   
                   
                    double bookKm = book.DistCost / book.Car.DistPrice;
                    double elapsedkm = carInspection.CarMileage - book.Car.Mileage;
                    if(elapsedkm > bookKm)
                    {
                        carInspection.extraKm = elapsedkm - bookKm;
                    }
                    else
                    {
                        carInspection.extraKm = 0;
                    }
                    carInspection.kmtravelled = elapsedkm;
                    var pickup = db.RentalHandovers.Where(x => x.BookingId == bookId).FirstOrDefault();
                    var carReturn = db.CarReturns.Where(x => x.BookingId == bookId).FirstOrDefault();
                    DateTime HandoverTime = DateTime.Parse(pickup.HandoverTime);
                    DateTime ReturnTime = DateTime.Parse(carReturn.Time);
                    DateTime combinedHandoverDateTime = pickup.HandoverDate.Date +HandoverTime.TimeOfDay;
                    DateTime combinedReturnDateTime = carReturn.Date.Date + ReturnTime.TimeOfDay;

                    TimeSpan timeDifference = combinedReturnDateTime - combinedHandoverDateTime;
                    double numberOfHours = timeDifference.TotalHours;
                    carInspection.timeTravelled = numberOfHours;

                    DateTime bookPickupDateTime = book.PickupDate.Date + book.PickupTime.TimeOfDay;
                    DateTime bookReturnDateTime = book.ReturnDate.Date + book.ReturnTime.TimeOfDay;

                    TimeSpan booktimeDifference = bookReturnDateTime - bookPickupDateTime;
                    double bookHours = booktimeDifference.TotalHours;


                    
                    if (numberOfHours> bookHours)
                    {
                        carInspection.Extratime = numberOfHours - bookHours;
                    }
                    else
                    {
                        carInspection.Extratime = 0;
                    }
                    if(carInspection.FuelAmt< book.Car.FuelAmt)
                    {
                        carInspection.ShortFuel = book.Car.FuelAmt - carInspection.FuelAmt;
                    }

                    carInspection.BookingId = bookId;
                    db.CarInspections.Add(carInspection);

                    car.Mileage = carInspection.CarMileage;
                    car.FuelAmt = carInspection.FuelAmt;
                    car.Status = "Available";
                    db.Entry(car).State = EntityState.Modified;
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(book.Email);
                    email2.Subject = "Car Inspection";
                    string emailBody = $"Dear {book.Name} {book.Surname},\n\n" +
                   $"Please note that we have completed vehicle inspection with the following results:\n" + 
                   $"Tire Condition\t:{carInspection.TireCondition}\n" + 
                   $"Dents\t:{carInspection.Dents}\n" + 
                   $"Exterior Condition\t:{carInspection.ExteriorCondition}\n" +
                   $"Interior Condition\t:{carInspection.InteriorCondition}\n" +
                   $"Test Drive\t:{carInspection.TestDrive}\n" +
                   $"Car Mileage\t:{carInspection.CarMileage}\n" +
                   $"Fuel amount\t:{carInspection.FuelAmt}\n" +

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
                    TempData["Message"] = "Inspection Completed successfully, email sent to client.";

                    return RedirectToAction("MyAssignments","AssignInspectors");
                }
                catch
                {
                    TempData["Message"] = "Something went wrong, please try again later.";

                    return RedirectToAction("MyAssignments", "AssignInspectors");
                }
                
            }

            return View(carInspection);
        }

        // GET: CarInspections/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarInspection carInspection = db.CarInspections.Find(id);
            if (carInspection == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", carInspection.BookingId);
            return View(carInspection);
        }

        // POST: CarInspections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Dents,TireCondition,ExteriorCondition,InteriorCondition,TestDrive,BookingId")] CarInspection carInspection)
        {
            if (ModelState.IsValid)
            {
                db.Entry(carInspection).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", carInspection.BookingId);
            return View(carInspection);
        }

        // GET: CarInspections/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CarInspection carInspection = db.CarInspections.Find(id);
            if (carInspection == null)
            {
                return HttpNotFound();
            }
            return View(carInspection);
        }

        // POST: CarInspections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CarInspection carInspection = db.CarInspections.Find(id);
            db.CarInspections.Remove(carInspection);
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
