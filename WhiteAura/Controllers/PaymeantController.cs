using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class PaymeantController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();


        [HttpGet]
        public ActionResult Payment(int bookingId)
        {
            var booking = db.Bookings
                            .Include(b => b.User) 
                            .Include(b => b.Service) 
                            .FirstOrDefault(b => b.ID == bookingId);

            if (booking == null)
            {
                return HttpNotFound(); 
            }

            return View(booking);
        }



        [HttpPost]
        public ActionResult Success(int bookingId, string paymentMethod, string orderID)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            booking.Status = "Confirmed";
            booking.IsPaid = true;

            decimal paymentAmount = (booking.Service != null ? booking.Service.Price ?? 0 : 0) * 0.10M;

            var payment = new Payment
            {
                BookingID = bookingId,
                Amount = paymentAmount, 
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Pending"
            };

            try
            {
                db.Payments.Add(payment);
                db.SaveChanges();

                SendConfirmationEmail(booking, paymentAmount);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing payment: " + ex.Message });
            }

            TempData["SuccessMessage"] = "Your booking has been successfully completed!";
            return Json(new { success = true });
        }

        private void SendConfirmationEmail(Booking booking, decimal paymentAmount)
        {
            var totalAmount = booking.Service != null ? booking.Service.Price ?? 0 : 0; 
            var remainingAmount = totalAmount - paymentAmount; 

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("whiteaurateam@gmail.com", "qcig jkns ypcl hrnr"), 
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("whiteaurateam@gmail.com", "WhiteAura"),
                Subject = $"Confirmed booking for {booking.Service.ServiceName}",
                Body = $"Dear {booking.User.FullName},\n\n" + 
                       $"Your booking for **{booking.Service.ServiceName}** has been confirmed.\n" +
                       $"Date: {booking.BookingDate.Value.ToString("yyyy-MM-dd")}\n" + 
                       $"Time: {booking.ReservedHours}\n" +
                       $"Amount Paid: {paymentAmount:C} \n" +
                       $"Remaining Amount: {remainingAmount:C}\n" + 
                       $"Please pay the remaining amount at {booking.Service.ServiceName} during your visit.\n\n" + 
                       "Thank you for your payment!\n\n" + 
                       "Best regards,\n" +
                       "The WhiteAura Team\n" + 
                       "Contact us: whiteaurateam@gmail.com", 
                IsBodyHtml = false,
            };

            mailMessage.To.Add(booking.User.Email); 

            smtpClient.Send(mailMessage);
        }

        public ActionResult ShowTestimonial(int bookingId)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            if (booking.BookingDate.HasValue && booking.BookingDate.Value < DateTime.Now && !db.Testimonials.Any(t => t.BookingId == bookingId))
            {
                return PartialView("_TestimonialPopup", booking); 
            }

            return Json(new { showPopup = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitTestimonial(int bookingId, string testimonialText)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            var testimonial = new Testimonial
            {
                UserId = booking.UserID,
                BookingId = bookingId,
                Text = testimonialText,
                CreatedAt = DateTime.Now
            };

            try
            {
                db.Testimonials.Add(testimonial);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thank you for your testimonial!";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving testimonial: " + ex.Message });
            }
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
