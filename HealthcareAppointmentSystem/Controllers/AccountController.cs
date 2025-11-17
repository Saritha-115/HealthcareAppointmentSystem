using HealthcareAppointmentSystem.Models;
using HealthcareAppointmentSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using System.Security.Claims;

namespace HealthcareAppointmentSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        public AccountController(IMongoDbService mongoDbService,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _mongoDbService = mongoDbService;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        // GET: Login
        public IActionResult Login(string userType = "patient")
        {
            ViewBag.UserType = userType;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string userType)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                ViewBag.UserType = userType;
                return View();
            }

            ClaimsPrincipal? principal = null;
            string redirectUrl = "/";

            switch ((userType ?? "patient").ToLower())
            {
                case "admin":
                    var adminFilter = Builders<Admin>.Filter.And(
                        Builders<Admin>.Filter.Eq(a => a.Email, email),
                        Builders<Admin>.Filter.Eq(a => a.Password, password)
                    );

                    var adminCursor = await _mongoDbService.Admins.FindAsync(adminFilter);
                    Admin admin = null;
                    if (await adminCursor.MoveNextAsync())
                    {
                        admin = adminCursor.Current.FirstOrDefault();
                    }

                    if (admin != null)
                    {
                        principal = CreateClaimsPrincipal(admin.Id, admin.Email, admin.FullName, "Admin");
                        redirectUrl = "/Admin/Dashboard";
                    }
                    break;

                case "doctor":
                    var doctor = await _doctorRepository.GetDoctorByEmailAsync(email);
                    if (doctor != null && doctor.Password == password)
                    {
                        principal = CreateClaimsPrincipal(doctor.Id, doctor.Email, doctor.FullName, "Doctor");
                        redirectUrl = "/Doctor/Dashboard";
                    }
                    break;

                case "patient":
                    var patient = await _patientRepository.GetPatientByEmailAsync(email);
                    if (patient != null && patient.Password == password)
                    {
                        principal = CreateClaimsPrincipal(patient.Id, patient.Email, patient.FullName, "Patient");
                        redirectUrl = "/Patient/Dashboard";
                    }
                    break;
            }

            if (principal != null)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return Redirect(redirectUrl);
            }

            ModelState.AddModelError("", "Invalid email or password.");
            ViewBag.UserType = userType;
            return View();
        }

        // GET: Register Patient
        public IActionResult RegisterPatient()
        {
            return View();
        }

        // POST: Register Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(Patient patient)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingPatient = await _patientRepository.GetPatientByEmailAsync(patient.Email);
                if (existingPatient != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(patient);
                }

                patient.CreatedAt = DateTime.Now;
                await _patientRepository.CreatePatientAsync(patient);

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login", new { userType = "patient" });
            }

            return View(patient);
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Access Denied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to create claims principal
        private ClaimsPrincipal CreateClaimsPrincipal(string userId, string email, string name, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}