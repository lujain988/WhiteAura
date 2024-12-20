﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography; // Import for SHA256
using System.Text; // Import for string encoding
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
                    if (VerifyPassword(user.Password, existingUser.Password))
                    {
                        Session["UserEmail"] = existingUser.Email;
                        Session["UserID"] = existingUser.ID;

                        var userId = existingUser.ID;
                        var pastBookings = db.Bookings
                            .Where(b => b.UserID == userId && b.BookingDate < DateTime.Now && !b.Testimonials.Any(t => t.UserId == userId))
                            .ToList();

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

            if (!IsValidPassword(user.Password))
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long, contain at least one uppercase letter, one special character (e.g., @), and one number.");
            }

            if (ModelState.IsValid)
            {
                var existUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existUser == null)
                {
                    user.Password = HashPassword(user.Password);
                    db.Users.Add(user);
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

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(ch => "!@#$%^&*()_-+=<>?/|~`.,".Contains(ch));

            return hasUpperCase && hasNumber && hasSpecialChar;
        }

        [HttpGet]
        public ActionResult Profile()
        {
            var userId = Session["UserID"] as int?;

            if (userId == null)
            {
                return RedirectToAction("Login", "LoginandSignup");
            }

            var user = db.Users
                .Include(u => u.Bookings.Select(b => b.Service))
                .FirstOrDefault(u => u.ID == userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var unavailableDates = db.Bookings
                .Where(b => (b.Status == "Confirmed" || b.IsPaid == true) && b.UserID != userId)
                .Select(b => new
                {
                    b.ServiceID,
                    BookingDate = DbFunctions.TruncateTime(b.BookingDate),
                    b.ReservedHours
                })
                .ToList();

            foreach (var booking in user.Bookings)
            {
                booking.IsDateAvailable = !unavailableDates.Any(b =>
                    b.ServiceID == booking.ServiceID &&
                    b.BookingDate.Value.Date == booking.BookingDate.Value.Date &&
                    booking.ReservedHours.HasValue &&
                    b.ReservedHours.HasValue &&
                    booking.ReservedHours.Value == b.ReservedHours.Value); 
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

            if (!ModelState.IsValid)
            {
                return View(user); 
            }

            var userInDb = db.Users.FirstOrDefault(u => u.ID == userId);

            if (userInDb == null)
            {
                return HttpNotFound();
            }

            userInDb.FullName = user.FullName;
            userInDb.Email = user.Email;
            userInDb.PhoneNumber = user.PhoneNumber;
            userInDb.Description = user.Description;

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

            if (user.Password != HashPassword(currentPassword)) 
            {
                return Json(new { success = false, errorMessage = "Current password is incorrect." });
            }

            user.Password = HashPassword(newPassword); 
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

            return RedirectToAction("Book", "Services", new { id = id });
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); 
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var hashOfEnteredPassword = HashPassword(enteredPassword);
            return hashOfEnteredPassword.Equals(storedHash);
        }
    }
}
