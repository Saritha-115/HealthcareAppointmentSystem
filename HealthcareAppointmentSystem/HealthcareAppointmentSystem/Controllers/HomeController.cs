using Microsoft.AspNetCore.Mvc;
using HealthcareAppointmentSystem.Services;
using HealthcareAppointmentSystem.Models;

namespace HealthcareAppointmentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IMongoDbService _mongoDbService;

        public HomeController(IDoctorRepository doctorRepository, IMongoDbService mongoDbService)
        {
            _doctorRepository = doctorRepository;
            _mongoDbService = mongoDbService;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured doctors or specialties for home page
            var doctors = await _doctorRepository.GetAllDoctorsAsync();
            var specialties = await _doctorRepository.GetAllSpecialtiesAsync();

            ViewBag.Specialties = specialties;
            ViewBag.DoctorCount = doctors.Count;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchDoctors(string specialty, string location)
        {
            var doctors = await _doctorRepository.GetAllDoctorsAsync();

            if (!string.IsNullOrEmpty(specialty))
            {
                doctors = doctors.Where(d => d.Specialty.Contains(specialty, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(location))
            {
                doctors = doctors.Where(d => d.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View("SearchResults", doctors);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}