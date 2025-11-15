using System;
using System.Linq;
using System.Threading.Tasks;
using HealthcareAppointmentSystem.Models;
using HealthcareAppointmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace HealthcareAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMongoDbService _mongoDbService;

        public AdminController(
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository,
            IMongoDbService mongoDbService)
        {
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
            _mongoDbService = mongoDbService;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var doctors = await _doctorRepository.GetAllDoctorsAsync();
            var patients = await _patientRepository.GetAllPatientsAsync();
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();

            ViewBag.TotalDoctors = doctors.Count;
            ViewBag.TotalPatients = patients.Count;
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.PendingAppointments = appointments.Count(a => a.Status == "Pending");

            return View();
        }

        // ===== DOCTOR MANAGEMENT =====

        // GET: List all doctors
        public async Task<IActionResult> ManageDoctors()
        {
            var doctors = await _doctorRepository.GetAllDoctorsAsync();
            return View(doctors);
        }

        // GET: Add Doctor Form
        public async Task<IActionResult> AddDoctor()
        {
            var specialties = await _mongoDbService.Specialties.Find(_ => true).ToListAsync();
            ViewBag.Specialties = specialties;
            return View();
        }

        // POST: Add Doctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.CreatedAt = DateTime.Now;
                doctor.IsActive = true;
                await _doctorRepository.CreateDoctorAsync(doctor);
                TempData["SuccessMessage"] = "Doctor added successfully!";
                return RedirectToAction("ManageDoctors");
            }

            var specialties = await _mongoDbService.Specialties.Find(_ => true).ToListAsync();
            ViewBag.Specialties = specialties;
            return View(doctor);
        }

        // GET: Edit Doctor Form
        public async Task<IActionResult> EditDoctor(string id)
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            var specialties = await _mongoDbService.Specialties.Find(_ => true).ToListAsync();
            ViewBag.Specialties = specialties;
            return View(doctor);
        }

        // POST: Edit Doctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(string id, Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.Id = id;
                await _doctorRepository.UpdateDoctorAsync(id, doctor);
                TempData["SuccessMessage"] = "Doctor updated successfully!";
                return RedirectToAction("ManageDoctors");
            }

            var specialties = await _mongoDbService.Specialties.Find(_ => true).ToListAsync();
            ViewBag.Specialties = specialties;
            return View(doctor);
        }

        // POST: Delete Doctor
        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            await _doctorRepository.DeleteDoctorAsync(id);
            TempData["SuccessMessage"] = "Doctor deleted successfully!";
            return RedirectToAction("ManageDoctors");
        }

        // ===== SPECIALTY MANAGEMENT =====

        // GET: Manage Specialties
        public async Task<IActionResult> ManageSpecialties()
        {
            var specialties = await _mongoDbService.Specialties.Find(_ => true).ToListAsync();
            return View(specialties);
        }

        // POST: Add Specialty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecialty(string name, string description)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var specialty = new Specialty
                {
                    Name = name,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await _mongoDbService.Specialties.InsertOneAsync(specialty);
                TempData["SuccessMessage"] = "Specialty added successfully!";
            }

            return RedirectToAction("ManageSpecialties");
        }

        // POST: Delete Specialty
        [HttpPost]
        public async Task<IActionResult> DeleteSpecialty(string id)
        {
            await _mongoDbService.Specialties.DeleteOneAsync(s => s.Id == id);
            TempData["SuccessMessage"] = "Specialty deleted successfully!";
            return RedirectToAction("ManageSpecialties");
        }

        // ===== PATIENT MANAGEMENT =====

        // GET: View all patients
        public async Task<IActionResult> ViewPatients()
        {
            var patients = await _patientRepository.GetAllPatientsAsync();
            return View(patients);
        }

        // ===== APPOINTMENT MANAGEMENT =====

        // GET: View all appointments
        public async Task<IActionResult> ViewAppointments()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            return View(appointments);
        }

        // ===== REPORTS =====

        // GET: Generate Reports
        public async Task<IActionResult> Reports()
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
            var doctors = await _doctorRepository.GetAllDoctorsAsync();
            var patients = await _patientRepository.GetAllPatientsAsync();

            // Statistics
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.CompletedAppointments = appointments.Count(a => a.Status == "Completed");
            ViewBag.PendingAppointments = appointments.Count(a => a.Status == "Pending");
            ViewBag.CancelledAppointments = appointments.Count(a => a.Status == "Cancelled");
            ViewBag.TotalRevenue = appointments.Where(a => a.IsPaid).Sum(a => a.ConsultationFee);

            ViewBag.TotalDoctors = doctors.Count;
            ViewBag.TotalPatients = patients.Count;

            // Appointments by specialty
            var appointmentsBySpecialty = appointments
                .GroupBy(a => a.Specialty)
                .Select(g => new { Specialty = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.AppointmentsBySpecialty = appointmentsBySpecialty;

            return View(appointments);
        }
    }
}