using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {

        private WeddingContext dbContext;

        public HomeController(WeddingContext context)
        {
            dbContext = context;
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/Dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            int curUser = (int)HttpContext.Session.GetInt32("curUser");
            User user = dbContext.Users.Include(u => u.CreatedWeddings)
                .Include(u => u.Attending)
                .ThenInclude(u => u.Wedding)
                .FirstOrDefault(u => u.UserId == curUser);
            ViewBag.user = user;
            List<Wedding> AllWeddings = dbContext.Weddings
                .Include(w => w.Attendees)
                .ToList();
            List<Wedding> attending = new List<Wedding>();
            List<Wedding> notAttending = new List<Wedding>();
            foreach(Attendee attend in user.Attending) {
                attending.Add(attend.Wedding);
            }
            foreach(Wedding wedding in AllWeddings) {
                if(!attending.Contains(wedding)) {
                    notAttending.Add(wedding);
                }
            }
            ViewBag.notAttending = notAttending;
            return View(AllWeddings);
        }

        [HttpGet]
        [Route("/Detail/{weddingId}")]
        public IActionResult Detail(int weddingId)
        {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId)) {
                return RedirectToAction("Dashboard");
            }
            Wedding selectedWedding = dbContext.Weddings
                .Include(w => w.Attendees)
                .ThenInclude(a => a.User)
                .FirstOrDefault(w => w.WeddingId == weddingId);
            return View(selectedWedding);
        }

        [HttpGet]
        [Route("/wedding/new")]
        public IActionResult Plan()
        {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [Route("/wedding/create")]
        public IActionResult CreateWedding(Wedding newWedding) {
            newWedding.UserId = (int)HttpContext.Session.GetInt32("curUser");
            if(ModelState.IsValid){
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("Plan");
        }

        [HttpGet]
        [Route("/delete/{weddingId}")]
        public IActionResult Delete(int weddingId) {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId)) {
                return RedirectToAction("Dashboard");
            }
            Wedding selectedWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            int curUser = (int)HttpContext.Session.GetInt32("curUser");
            if(selectedWedding.UserId != curUser) {
                return RedirectToAction("Dashboard");
            }
            dbContext.Weddings.Remove(selectedWedding);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("/rsvp/{weddingId}")]
        public IActionResult RSVP(int weddingId) {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId)) {
                return RedirectToAction("Dashboard");
            }
            int curUser = (int)HttpContext.Session.GetInt32("curUser");
            Attendee selectedAttendee = new Attendee();
            selectedAttendee.UserId = curUser;
            selectedAttendee.WeddingId = weddingId;
            dbContext.Add(selectedAttendee);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
            
        }

        [HttpGet]
        [Route("/cancel/{weddingId}")]
        public IActionResult Cancel(int weddingId) {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            if(!dbContext.Weddings.Any(u => u.WeddingId == weddingId)) {
                return RedirectToAction("Dashboard");
            }
            int curUser = (int)HttpContext.Session.GetInt32("curUser");
            Attendee selectedAttendee = dbContext.Attendees.FirstOrDefault(a => a.WeddingId == weddingId && a.UserId == curUser);
            if(selectedAttendee.Equals(default(Attendee))) {
                return RedirectToAction("Dashboard");
            }
            dbContext.Attendees.Remove(selectedAttendee);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [Route("/register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid) {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("curUser", user.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpPost]
        [Route("/login")]
        public IActionResult ProcessLogin(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "There is no user with this email address!");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Incorrect Password!");
                    return View("Login");
                }
                HttpContext.Session.SetInt32("curUser", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpGet]
        [Route("/logout")]
        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
