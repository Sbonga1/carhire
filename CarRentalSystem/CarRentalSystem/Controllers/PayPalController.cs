using CarRentalSystem.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CarRentalSystem.Controllers
{
    public class PayPalController : Controller
    {
        private static readonly Random random = new Random();
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult CreatePayment(string Payment,double cost, string refundId = "none")
        {
            if (Payment == "Initial")
            {
                Session["Payment"] = "Initial";


                //double usdAmount = await currencyConverter.ConvertCurrencyAsync("ZAR");
                double convertedTot = cost * 0.06965;

                var CurrentUser = User.Identity.Name;
                CultureInfo cultureInfo = new CultureInfo("en-US");
                string Cost = convertedTot.ToString("0.00", cultureInfo);

                // Set up the PayPal API context
                var apiContext = PayPalConfig.GetAPIContext();

                // Retrieve the API credentials from configuration
                var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
                var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
                apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
                var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
                apiContext.AccessToken = accessToken;

                // Create a new payment object
                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Consultation Payment"
            }
        },
                    redirect_urls = new RedirectUrls
                    {
                        return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                        cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                    }
                };

                // Create the payment and get the approval URL
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                // Redirect the user to the PayPal approval URL
                return Redirect(approvalUrl);
            }
            else if (Payment == "Refund")
            {
                if (refundId != "none")
                {
                    Session["RefundPaymentAppID"] = refundId;
                }
                Session["Payment"] = "Refund";

                //double usdAmount = await currencyConverter.ConvertCurrencyAsync("ZAR");
                double convertedTot = cost * 0.06965;

                var CurrentUser = User.Identity.Name;
                CultureInfo cultureInfo = new CultureInfo("en-US");
                string Cost = convertedTot.ToString("0.00", cultureInfo);

                // Set up the PayPal API context
                var apiContext = PayPalConfig.GetAPIContext();

                // Retrieve the API credentials from configuration
                var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
                var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
                apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
                var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
                apiContext.AccessToken = accessToken;

                // Create a new payment object
                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Refund Payment"
            }
        },
                    redirect_urls = new RedirectUrls
                    {
                        return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                        cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                    }
                };

                // Create the payment and get the approval URL
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                // Redirect the user to the PayPal approval URL
                return Redirect(approvalUrl);
            }
            else
            {
                var CurrentUser = User.Identity.Name;
                double convertedTot = Math.Round(cost / 14.357);
                int Rem = (int)(cost % 14.357);
                string Cost = convertedTot.ToString() + "." + Rem;

                // Set up the PayPal API context
                var apiContext = PayPalConfig.GetAPIContext();

                // Retrieve the API credentials from configuration
                var clientId = ConfigurationManager.AppSettings["PayPalClientId"];
                var clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
                apiContext.Config = new Dictionary<string, string> { { "mode", "sandbox" } };
                var accessToken = new OAuthTokenCredential(clientId, clientSecret, apiContext.Config).GetAccessToken();
                apiContext.AccessToken = accessToken;

                // Create a new payment object
                var payment = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                {
            new Transaction
            {
                amount = new Amount
                {

                    total = Cost,
                    currency = "USD"
                },

                description = "Penalty Invoice Payment"
            }
        },
                    redirect_urls = new RedirectUrls
                    {
                        return_url = Url.Action("CompletePayment", "PayPal", null, Request.Url.Scheme),
                        cancel_url = Url.Action("CancelPayment", "PayPal", null, Request.Url.Scheme)
                    }
                };

                // Create the payment and get the approval URL
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url")?.href;

                // Redirect the user to the PayPal approval URL
                return Redirect(approvalUrl);
            }
        }


        public ActionResult CompletePayment(string paymentId, string token, string PayerID)
        {
            // Set up the PayPal API context
            var apiContext = PayPalConfig.GetAPIContext();

            // Execute the payment
            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var executedPayment = new Payment { id = paymentId }.Execute(apiContext, paymentExecution);

            // Process the payment completion
            // You can save the transaction details or perform other necessary actions

            // Redirect the user to a success page
            return RedirectToAction("PaymentSuccess");
        }

        public ActionResult CancelPayment()
        {

            return RedirectToAction("PaymentCancelled");
        }

        public ActionResult PaymentSuccess()
        {
            string pay = Session["Payment"] as string;
            if (pay == "Initial")
            {

                try
                {
                    var booking = db.Bookings.Where(x => x.Email == User.Identity.Name && x.Status == "Approved").FirstOrDefault();
                    booking.Status = "Payment-Settled";
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(User.Identity.Name);
                    email2.Subject = "Rental Payment";

                    string emailBody = $"Dear {booking.Name} {booking.Surname},\n\n" +
                   $"Please note that we have received your payment for car hire request you can now proceed to pickup car on {booking.PickupDate.ToShortDateString()} at {booking.PickupTime.ToShortTimeString()}\n" +
                   $"If you have any questions or need further assistance, please feel free to contact our customer support.\n" +
                   $"\nThank you for considering Durban Car Hire for your car rental needs.\n" +
                   $"\nWarm Regards,\n" +
                   $"Durban Car Hire";
                    email2.Body = emailBody;
                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();

                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email2);

                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    // Display a success toast notification
                    TempData["Message"] = "Your Payment for your car hire has been Successfully Completed, Please check your Emails for more info.";
                    Session["Payment"] = null;
                    return RedirectToAction("MyBookings", "Bookings");
                }
                catch
                {
                    // Display a error toast notification
                    TempData["Message"] = "We could not process your payment at the moment, Please try again later.";
                    return RedirectToAction("MyBookings", "Bookings");
                }
            }
            else if (pay == "Refund")
            {
                try
                {

                    string refundId = Session["RefundPaymentAppID"] as string;
                    int id = int.Parse(refundId);
                    var refund = db.Refunds.Find(id);
                    var booking = db.Bookings.Find(refund.BookingId);
                    booking.Status = "Settled with Refund";
                    refund.Status = "Settled";
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(User.Identity.Name);
                    email2.Subject = "Refund Payment";
                    //
                    

                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();
                    string emailBody = $"Dear {refund.Booking.Name} {refund.Booking.Surname},\n\n" +
                  $"Please note that your refund request has been approved, Payment has been made to your account\n" +
                  $"If you have any questions or need further assistance, please feel free to contact our customer support.\n" +
                  $"\nThank you for considering Durban Car Hire for your car rental needs.\n" +
                  $"\nWarm Regards,\n" +
                  $"Durban Car Hire";
                    email2.Body = emailBody;
                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email2);

                    db.Entry(refund).State = EntityState.Modified;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();

                    // Display a success toast notification
                    TempData["Message"] = "Refund Payment has been Successfully Completed,Email sent to client.";
                    Session["RefundPaymentAppID"] = null;
                    return RedirectToAction("Index", "Refunds");
                }
                catch
                {
                    // Display a error toast notification
                    TempData["Message"] = "We could not process your payment at the moment, Please try again later.";
                    return RedirectToAction("Index", "Refunds");
                }

            }
            else
            {
                try
                {
                    var invoice = db.Invoices.Where(x => x.Booking.Email == User.Identity.Name && x.Status== "Awaiting Payment").FirstOrDefault();
                    var booking = db.Bookings.Find(invoice.BookingId);
                  
                    // Prepare email message
                    var email2 = new MailMessage();
                    email2.From = new MailAddress("SnapDrive2023@outlook.com");
                    email2.To.Add(User.Identity.Name);
                    email2.Subject = "Penalty Invoice Payment";
                  
                    
                    // Use the SMTP settings from web.config
                    var smtpClient = new SmtpClient();
                    string emailBody = $"Dear {invoice.Booking.Name} {invoice.Booking.Surname},\n\n" +
                 $"Please note that we have successfully received your final penalty payment\n" +
                 $"If you have any questions or need further assistance, please feel free to contact our customer support.\n" +
                 $"\nThank you for considering Durban Car Hire for your car rental needs.\n" +
                 $"\nWarm Regards,\n" +
                 $"Durban Car Hire";
                    // The SmtpClient will automatically use the settings from web.config
                    smtpClient.Send(email2);
                    booking.Status = "Settled";
                    invoice.Status = "Settled";
                    db.Entry(invoice).State = EntityState.Modified;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    // Display a success toast notification

                    TempData["Message"] = "Your Penalty invoice payment was processed success fully, Please check your Emails for more info.";

                    Session["Payment"] = null;
                    return RedirectToAction("MyInvoices", "Invoices");
                }
                catch
                {
                    // Display a error toast notification

                    TempData["Message"] = "We could not process your payment at the moment, Please try again later.";

                    return RedirectToAction("MyInvoices", "Invoices");
                }
            }

        }

    }

}