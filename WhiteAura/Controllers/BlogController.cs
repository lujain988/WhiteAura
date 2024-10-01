using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WhiteAura.Models;

namespace WhiteAura.Controllers
{
    public class BlogController : Controller
    {
        private WhiteAuraEntities db = new WhiteAuraEntities();
        public ActionResult Index()
        {
            var blogs = db.Blogs.ToList(); 
            return View(blogs); 
        }

        public ActionResult Details(string title)
        {
            var blog = db.Blogs.FirstOrDefault(b => b.Title == title);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }
    }
}