using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using CarRentalSystem.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Ajax.Utilities;

namespace CarRentalSystem.Controllers
{
    public class InvoicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        public ActionResult Index()
        {
            var invoices = db.Invoices.Include(i => i.Booking);
            return View(invoices.ToList());
        }
        public ActionResult MyInvoices()
        {
            var invoices = db.Invoices.Where(x=>x.Email == User.Identity.Name).Include(i => i.Booking);
            return View(invoices.ToList());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // GET: Invoices/Create
        public ActionResult Create(int bookId)
        {
            Session["bookId"] = bookId.ToString();
            
            
            return View();
        }

        // POST: Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvId,BookingId,Date,DueDate,Description,Penalty,LatePaymentFee")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string bookID = Session["bookId"] as string;
                    int bookId = int.Parse(bookID);
                    invoice.BookingId = bookId;
                    var book = db.Bookings.Find(bookId);
                    book.Status = "Awaiting INV Payment";
                    invoice.Status = "Awaiting Payment";
                    invoice.Email = book.Email;
                    var inspection = db.CarInspections.Where(x => x.BookingId == bookId).FirstOrDefault();
                    invoice.Date = DateTime.Now.Date;
                    db.Entry(book).State = EntityState.Modified;
                    db.Invoices.Add(invoice);

                    MemoryStream memoryStream = new MemoryStream();
                    Document document = new Document(PageSize.A5, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();



                    // Add a title
                    Font titleFont = new Font(Font.FontFamily.HELVETICA, 24, Font.BOLD);
                    Paragraph title = new Paragraph("Penalty Invoice", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);

                    // Create the heading paragraph with the headig font
                    PdfPTable table1 = new PdfPTable(1);
                    PdfPTable table2 = new PdfPTable(5);
                    PdfPTable table3 = new PdfPTable(1);

                    iTextSharp.text.pdf.draw.VerticalPositionMark seperator = new iTextSharp.text.pdf.draw.LineSeparator();
                    seperator.Offset = -6f;
                    // Remove table cell
                    table1.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    table3.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                    table1.WidthPercentage = 80;
                    table1.SetWidths(new float[] { 100 });
                    table2.WidthPercentage = 80;
                    table3.SetWidths(new float[] { 100 });
                    table3.WidthPercentage = 80;

                    PdfPCell subtitleCell = new PdfPCell();
                    subtitleCell.AddElement(new Paragraph("Invoice Details"));
                    subtitleCell.HorizontalAlignment = Element.ALIGN_CENTER;

                    PdfPCell PenaltyTitleCell = new PdfPCell();
                    PenaltyTitleCell.AddElement(new Paragraph("Penalty Details"));
                    PenaltyTitleCell.HorizontalAlignment = Element.ALIGN_CENTER;


                    PdfPCell cell = new PdfPCell(new Phrase(""));
                    cell.Colspan = 3;
                    table1.AddCell("\n");
                    table1.AddCell(cell);
                    table1.AddCell("\n\n");
                    table1.AddCell(
                        "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t" +
                        "Rental \n" +
                        "Email :SnapDrive2023@outlook.com" + "\n" +
                        "\n" + "\n");
                    table1.AddCell(subtitleCell);
                    table1.AddCell("INV NO : \t" + invoice.InvId);
                    table1.AddCell("Client Name : \t" + book.Name);
                    table1.AddCell("Client Surname : \t" + book.Surname);
                    table1.AddCell("Client Email : \t" + book.Email);
                    table2.AddCell("Penalty : \t" + invoice.Penalty);
                    table1.AddCell("LatePaymentFee : \t" + invoice.LatePaymentFee);
                    table1.AddCell("Due Date : \t" + invoice.DueDate.Date);
                    table1.AddCell("Date : \t" + DateTime.Now.Date);
                    table1.AddCell("\n\n");
                    table1.AddCell(PenaltyTitleCell);
                    table1.AddCell("Extra kilometers travelled : \t" + inspection.extraKm);
                    table1.AddCell("Extra hours spent with vehicle : \t" + inspection.Extratime);
                    table1.AddCell("Short fuel in liters : \t" + inspection.ShortFuel);
                    table1.AddCell("Car Dents? : \t" + inspection.Dents);
                    table1.AddCell("Tires are in good condition? : \t" + inspection.TireCondition);
                    table1.AddCell("Exterior in good condition? : \t" + inspection.ExteriorCondition);
                    table1.AddCell("Interior in good condition? : \t" + inspection.InteriorCondition);
                    table1.AddCell("Test Drive passed? : \t" + inspection.InteriorCondition);
                    table1.AddCell("Car mileage after rental : \t" + inspection.CarMileage);
                    table1.AddCell("Fuel amount after rental : \t" + inspection.FuelAmt);

                    
                    // table1.AddCell("Vehicle: \t" + book.Car.Name + " " + book.Car.Model);
                    //table1.AddCell("Pickup: \t" + book.PickupDate.ToLongDateString() + " " + book.PickupTime.ToLongTimeString());
                    //table1.AddCell("Return: \t" + book.ReturnDate.ToLongDateString() + " " + book.ReturnTime.ToLongTimeString());
                    // table1.AddCell("Distance Cost: \tR " + book.DistCost);
                    //double dist = book.DistCost / book.Car.DistPrice;
                    // table1.AddCell("Distance : \t" + dist.ToString() + " km");
                    // table1.AddCell("Time Cost : \tR " + book.Cost);
                    // DateTime combinedPickupDateTime = book.PickupDate.Date + book.PickupTime.TimeOfDay;
                    //DateTime combinedReturnDateTime = book.ReturnDate.Date + book.ReturnTime.TimeOfDay;

                    //TimeSpan timeDifference = combinedReturnDateTime - combinedPickupDateTime;
                    //double numberOfHours = timeDifference.TotalHours;

                    //table1.AddCell("Time: \t" + numberOfHours.ToString() + " hr");
                    //table1.AddCell("Total: \tR " + book.FinalCost);



                    //table1.AddCell("\n\n");
                    //table1.AddCell(signatureTitleCell);






                    // Convert the signature image to a byte array
                    // var CustcleanerBase64 = rentalHandover.Signature.Substring(22);
                    // byte[] CustsignatureBytes = Convert.FromBase64String(CustcleanerBase64);

                    // Create an image from the byte array
                    // iTextSharp.text.Image CustsignatureImage = iTextSharp.text.Image.GetInstance(CustsignatureBytes);

                    // Scale the image to fit within the cell
                    // CustsignatureImage.ScaleToFit(100, 100);

                    // Add the image to the cell
                    // PdfPCell CustsignatureCell = new PdfPCell(CustsignatureImage);
                    // CustsignatureCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    // table1.AddCell(CustsignatureCell);

                    table1.AddCell("\n");





                    table1.AddCell(cell);
                    document.Add(table1);

                    document.Add(table3);
                    document.Close();

                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();


                    var attachments = new List<Attachment>();
                    attachments.Add(new Attachment(new MemoryStream(bytes), "Penalty Invoice", "application/pdf"));
                    var email = new MailMessage();
                    email.From = new MailAddress("SnapDrive2023@outlook.com");
                    email.To.Add(book.Email);
                    email.Subject = "Penalty Invoice";
                    email.Body = "Dear " + book.Name + " " + book.Surname + "\n\nPlease see the attached Invoice." +
                 "\n\n---------------------------------------------------\n" +
                 "Thank you. Have a safe journey" +
                 "\n---------------------------------------------------" +
                 "\n\nKind Regards,\nCar Rental";
                    // Attach the files to the email
                    foreach (var attachment in attachments)
                    {
                        email.Attachments.Add(attachment);
                    }
                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();

                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email);
                    db.SaveChanges();
                    // Specify the file path and name
                    string filePath = Server.MapPath("~/") + "Invoice" + invoice.InvId + ".pdf";

                    // Write the PDF bytes to the file
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    TempData["Message"] = "Invoice Create successfully, email sent to client";





                    
                    return RedirectToAction("Index","Bookings");
                }
                catch
                {
                    TempData["Message"] = "Something went wrong, please try again later.";

                    return RedirectToAction("Index", "Bookings");

                }

            }

            
            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", invoice.BookingId);
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InvId,BookingId,Date,DueDate,Description,Penalty,LatePaymentFee,Car")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", invoice.BookingId);
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            db.Invoices.Remove(invoice);
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
