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
                            .Include(b => b.User) // Include related entities if needed
                            .Include(b => b.Service) // Include related service information if needed
                            .FirstOrDefault(b => b.ID == bookingId);

            if (booking == null)
            {
                return HttpNotFound(); // Return 404 if the booking is not found
            }

            return View(booking); // Return the single booking to the view
        }



        [HttpPost]
        public ActionResult Success(int bookingId, string paymentMethod, string orderID)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Update booking status to "Confirmed" and mark it as paid
            booking.Status = "Confirmed";
            booking.IsPaid = true;

            // Calculate 10% of the booking price for the payment
            decimal paymentAmount = (booking.Service != null ? booking.Service.Price ?? 0 : 0) * 0.10M;

            // Save the payment record with the selected method and amount
            var payment = new Payment
            {
                BookingID = bookingId,
                Amount = paymentAmount,  // Only 10% of the service price
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Pending" // For admin approval
            };

            try
            {
                db.Payments.Add(payment);
                db.SaveChanges();

                // Send confirmation email
                SendConfirmationEmail(booking, paymentAmount);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return Json(new { success = false, message = "Error processing payment: " + ex.Message });
            }

            TempData["SuccessMessage"] = "Your booking has been successfully completed!";
            return Json(new { success = true });
        }

        private void SendConfirmationEmail(Booking booking, decimal paymentAmount)
        {
            var totalAmount = booking.Service != null ? booking.Service.Price ?? 0 : 0; // Total service price
            var remainingAmount = totalAmount - paymentAmount; // Calculate remaining amount (90%)

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("whiteaurateam@gmail.com", "qcig jkns ypcl hrnr"), // Consider using secure methods for credentials
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("whiteaurateam@gmail.com", "WhiteAura"),
                Subject = $"Confirmed booking for {booking.Service.ServiceName}", // Updated subject line
                Body = $"Dear {booking.User.FullName},\n\n" + // Include user's name in greeting
                       $"Your booking for **{booking.Service.ServiceName}** has been confirmed.\n" +
                       $"Date: {booking.BookingDate.Value.ToString("yyyy-MM-dd")}\n" + // Ensure BookingDate has a value
                       $"Time: {booking.ReservedHours}\n" +
                       $"Amount Paid: {paymentAmount:C} \n" + // Format amount as currency
                       $"Remaining Amount: {remainingAmount:C}\n" + // Include remaining amount
                       $"Please pay the remaining amount at {booking.Service.ServiceName} during your visit.\n\n" + // Add spacing for readability
                       "Thank you for your payment!\n\n" + // Thank the user
                       "Best regards,\n" +
                       "The WhiteAura Team\n" + // Closing statement
                       "Contact us: whiteaurateam@gmail.com", // WhiteAura email
                IsBodyHtml = false,
            };

            // Add user's email to the recipient list
            mailMessage.To.Add(booking.User.Email); // Assuming booking.User has an Email property

            smtpClient.Send(mailMessage);
        }

        public ActionResult ShowTestimonial(int bookingId)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Check if the booking date has passed
            if (booking.BookingDate.HasValue && booking.BookingDate.Value < DateTime.Now && !db.Testimonials.Any(t => t.BookingId == bookingId))
            {
                // This booking is eligible for a testimonial
                return PartialView("_TestimonialPopup", booking); // Return a partial view for the popup
            }

            return Json(new { showPopup = false }, JsonRequestBehavior.AllowGet); // No popup needed
        }

        // POST action to submit the testimonial
        [HttpPost]
        public ActionResult SubmitTestimonial(int bookingId, string testimonialText)
        {
            // Find the booking by ID
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Create a new testimonial
            var testimonial = new Testimonial
            {
                UserId = booking.UserID,
                BookingId = bookingId,
                Text = testimonialText,
                CreatedAt = DateTime.Now
            };

            try
            {
                // Add the testimonial to the database
                db.Testimonials.Add(testimonial);
                db.SaveChanges();

                // Optionally, you can set a success message or do additional processing
                TempData["SuccessMessage"] = "Thank you for your testimonial!";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
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
