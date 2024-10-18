using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class LoginandSignupController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();

        // GET: LoginandSignup
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Password))
                {
                    ModelState.AddModelError("", "Password is required.");
                    return View(user);
                }

                var existingUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    if (existingUser.Password == user.Password)
                    {
                        Session["UserEmail"] = existingUser.Email;
                        Session["UserID"] = existingUser.ID;

                        var userId = existingUser.ID;
                        var pastBookings = db.Bookings
                            .Where(b => b.UserID == userId && b.BookingDate < DateTime.Now && !b.Testimonials.Any(t => t.UserId == userId))
                            .ToList();

                        // If there are past bookings, set TempData to show the testimonial popup
                        if (pastBookings.Any())
                        {
                            TempData["ShowTestimonialPopup"] = true;
                            TempData["PastBookingId"] = pastBookings.Select(b => b.ID).ToList();
                        }

                        if (TempData["ReturnUrl"] != null)
                        {
                            string returnUrl = TempData["ReturnUrl"].ToString();
                            return Redirect(returnUrl); 
                        }

                        return RedirectToAction("Index", "Home"); 
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The password you entered is incorrect.";
                        return View(user);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The email you entered does not exist.";
                    return View(user);
                }
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(user);
        }



        public ActionResult SingUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SingUp(User user)
        {
            if (user.Password != user.ConifrmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
            }

            if (ModelState.IsValid)
            {
                var existUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existUser == null)
                {

                    db.Users.Add(user);
                    db.SaveChanges();

                    db.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Email already exists.");
                }
            }

            return View(user);
        }


        [HttpGet]
        public ActionResult Profile()
        {
            // Retrieve user ID from session
            var userId = Session["UserID"] as int?;

            if (userId == null)
            {
                return RedirectToAction("Login", "LoginandSignup");
            }

            // Fetch the user information using the user ID and include bookings
            var user = db.Users
                .Include(u => u.Bookings.Select(b => b.Service)) // Include service details if needed
                .FirstOrDefault(u => u.ID == userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User user)
        {
            var userId = Session["UserID"] as int?;

            if (userId == null)
            {
                return RedirectToAction("Login", "LoginandSignup");
            }

            if (user == null)
            {
                return new HttpStatusCodeResult(400, "User data is missing or malformed.");
            }

            // Fetch the user from the database
            var userInDb = db.Users.FirstOrDefault(u => u.ID == userId);

            if (userInDb == null)
            {
                return HttpNotFound();
            }

            // Update the user's details
            userInDb.FullName = user.FullName;
            userInDb.Email = user.Email;
            userInDb.PhoneNumber = user.PhoneNumber;
            userInDb.Description = user.Description;

            // Save changes to the database
            db.Entry(userInDb).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Profile");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, errorMessage = "The new password and confirmation do not match." });
            }

            var userId = Session["UserID"] as int?;
            var user = db.Users.FirstOrDefault(u => u.ID == userId);
            if (user == null)
            {
                return Json(new { success = false, errorMessage = "User not found." });
            }

            if (user.Password != currentPassword)
            {
                return Json(new { success = false, errorMessage = "Current password is incorrect." });
            }

            user.Password = newPassword;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { success = true });
        }
        public ActionResult CheckLogin(int id)
        {
            bool isLoggedIn = Session["UserID"] != null;

            if (!isLoggedIn)
            {
                TempData["ReturnUrl"] = Url.Action("Book", "Services", new { id = id });
                TempData["ShowAlert"] = true;
                TempData["AlertMessage"] = "You need to log in to book a service.";
                return RedirectToAction("Login", "LoginandSignup");
            }

            // Proceed with booking logic or service details
            return RedirectToAction("Book", "Services", new { id = id });
        }







        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

    }
}