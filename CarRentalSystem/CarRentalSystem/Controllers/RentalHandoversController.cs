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
using CarRentalSystem.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.SqlServer.Server;
using static iTextSharp.text.pdf.PdfStructTreeController;

namespace CarRentalSystem.Controllers
{
    public class RentalHandoversController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: RentalHandovers
        public ActionResult Index()
        {
            var rentalHandovers = db.RentalHandovers.Include(r => r.Booking).Include(r => r.Inspector);
            return View(rentalHandovers.ToList());
        }

        // GET: RentalHandovers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RentalHandover rentalHandover = db.RentalHandovers.Find(id);
            if (rentalHandover == null)
            {
                return HttpNotFound();
            }
            return View(rentalHandover);
        }

        // GET: RentalHandovers/Create
        public ActionResult Create(int bookId,int inspId, int assId)
        {
            Session["AssId"] = assId.ToString();
            Session["BookingId"] = bookId.ToString();
            Session["InspId"] = inspId.ToString();

            RentalHandover b = new RentalHandover()
            {
                HandoverDate = DateTime.Now.Date,
                HandoverTime = DateTime.Now.ToShortTimeString()
            };
            return View(b);
        }

        // POST: RentalHandovers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HandoverID,BookingId,InspId,HandoverDate,HandoverTime,Status,Signature")] RentalHandover rentalHandover)
        {
            if (ModelState.IsValid)
            {
                string bookID = Session["BookingId"] as string;
                string InspID = Session["InspId"] as string;
                string assId = Session["AssId"] as string;

                int assID = int.Parse(assId);
                int bookId = int.Parse(bookID);
                int InspId = int.Parse(InspID);

                var Ass = db.AssignInspectors.Find(assID);
                var book = db.Bookings.Find(bookId);
                book.Status = "Vehicle-Received";
                Ass.Status = "Vehicle-Handed";

                rentalHandover.InspId = InspId;
                rentalHandover.BookingId = bookId;
                db.Entry(book).State = EntityState.Modified;
                db.Entry(Ass).State = EntityState.Modified;

                try
                {


                    MemoryStream memoryStream = new MemoryStream();
                    Document document = new Document(PageSize.A5, 0, 0, 0, 0);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();



                    // Add a title
                    Font titleFont = new Font(Font.FontFamily.HELVETICA, 24, Font.BOLD);
                    Paragraph title = new Paragraph("Vehicle Receival Record", titleFont);
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
                    subtitleCell.AddElement(new Paragraph("Rental Details"));
                    subtitleCell.HorizontalAlignment = Element.ALIGN_CENTER;

                    PdfPCell signatureTitleCell = new PdfPCell();
                    signatureTitleCell.AddElement(new Paragraph("Client Signature"));
                    signatureTitleCell.HorizontalAlignment = Element.ALIGN_CENTER;


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
                    table1.AddCell("Client Name : \t" + book.Name);
                    table1.AddCell("Client Surname : \t" + book.Surname);
                    table1.AddCell("Client Email : \t" + book.Email);
                    table2.AddCell("Inspector Name : \t" + Ass.Inspector.Name);
                    table1.AddCell("Inspector Email : \t" + Ass.Inspector.Surname);
                    table1.AddCell("Vehicle: \t" + book.Car.Name +" " + book.Car.Model);
                    table1.AddCell("Pickup: \t" + book.PickupDate.ToLongDateString() + " " + book.PickupTime.ToLongTimeString());
                    table1.AddCell("Return: \t" + book.ReturnDate.ToLongDateString() + " " + book.ReturnTime.ToLongTimeString());
                    table1.AddCell("Distance Cost: \tR " + book.DistCost);
                    double dist = book.DistCost / book.Car.DistPrice;
                    table1.AddCell("Distance : \t" + dist.ToString() + " km");
                    table1.AddCell("Time Cost : \tR " + book.Cost);
                    DateTime combinedPickupDateTime = book.PickupDate.Date + book.PickupTime.TimeOfDay;
                    DateTime combinedReturnDateTime = book.ReturnDate.Date + book.ReturnTime.TimeOfDay;

                    TimeSpan timeDifference = combinedReturnDateTime - combinedPickupDateTime;
                    double numberOfHours = timeDifference.TotalHours;

                    table1.AddCell("Time: \t" + numberOfHours.ToString()+" hr");
                    table1.AddCell("Total: \tR " + book.FinalCost);



                    table1.AddCell("\n\n");
                    table1.AddCell(signatureTitleCell);





                   
                    // Convert the signature image to a byte array
                    var CustcleanerBase64 = rentalHandover.Signature.Substring(22);
                    byte[] CustsignatureBytes = Convert.FromBase64String(CustcleanerBase64);

                    // Create an image from the byte array
                    iTextSharp.text.Image CustsignatureImage = iTextSharp.text.Image.GetInstance(CustsignatureBytes);

                    // Scale the image to fit within the cell
                    CustsignatureImage.ScaleToFit(100, 100);

                    // Add the image to the cell
                    PdfPCell CustsignatureCell = new PdfPCell(CustsignatureImage);
                    CustsignatureCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    table1.AddCell(CustsignatureCell);

                    table1.AddCell("\n");





                    table1.AddCell(cell);
                    document.Add(table1);

                    document.Add(table3);
                    document.Close();

                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();


                    var attachments = new List<Attachment>();
                    attachments.Add(new Attachment(new MemoryStream(bytes), "Rental Handover", "application/pdf"));
                    var email = new MailMessage();
                    email.From = new MailAddress("SnapDrive2023@outlook.com");
                    email.To.Add(book.Email);
                    email.Subject = "Vehicle Handed";
                    email.Body = "Dear " + book.Name + " " + book.Surname + "\n\nPlease see the attached PDF for your vehicle rental." +
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

                    db.RentalHandovers.Add(rentalHandover);
                    db.SaveChanges();
                    // Specify the file path and name
                    string filePath = Server.MapPath("~/") + "RentalHandover"  + rentalHandover.HandoverID + ".pdf";

                    // Write the PDF bytes to the file
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    TempData["Message"] = "Vehicle handover success, email sent to client";
                    return RedirectToAction("MyAssignments", "AssignInspectors");

                }
                catch
                {
                    TempData["Message"] = "Something went wrong, please try again later.";
                    return View(rentalHandover);
                }



               
               
            }

            
            return View(rentalHandover);
        }

        // GET: RentalHandovers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RentalHandover rentalHandover = db.RentalHandovers.Find(id);
            if (rentalHandover == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", rentalHandover.BookingId);
            ViewBag.InspId = new SelectList(db.Inspectors, "InspId", "Name", rentalHandover.InspId);
            return View(rentalHandover);
        }

        // POST: RentalHandovers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "HandoverID,BookingId,InspId,CustEmail,HandoverDate,HandoverTime,Status,Signature")] RentalHandover rentalHandover)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rentalHandover).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingId = new SelectList(db.Bookings, "BookingId", "Name", rentalHandover.BookingId);
            ViewBag.InspId = new SelectList(db.Inspectors, "InspId", "Name", rentalHandover.InspId);
            return View(rentalHandover);
        }

        // GET: RentalHandovers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RentalHandover rentalHandover = db.RentalHandovers.Find(id);
            if (rentalHandover == null)
            {
                return HttpNotFound();
            }
            return View(rentalHandover);
        }

        // POST: RentalHandovers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RentalHandover rentalHandover = db.RentalHandovers.Find(id);
            db.RentalHandovers.Remove(rentalHandover);
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
