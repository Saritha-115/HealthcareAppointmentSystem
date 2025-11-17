using HealthcareAppointmentSystem.Models;
using HealthcareAppointmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace HealthcareAppointmentSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMongoDbService _mongoDbService;

        public PatientController(
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            IMongoDbService mongoDbService)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _mongoDbService = mongoDbService;
        }

        // Get current patient ID from claims
        private string GetCurrentPatientId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            var patientId = GetCurrentPatientId();
            var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);

            var upcomingAppointments = appointments
                .Where(a => a.AppointmentDate >= DateTime.Today && a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.UpcomingCount = upcomingAppointments.Count;
            ViewBag.CompletedCount = appointments.Count(a => a.Status == "Completed");

            return View(upcomingAppointments);
        }

        // ==================== PROFILE MANAGEMENT ====================

        // View Profile
        public async Task<IActionResult> Profile()
        {
            var patientId = GetCurrentPatientId();
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Patient patient)
        {
            var patientId = GetCurrentPatientId();

            if (ModelState.IsValid)
            {
                patient.Id = patientId;
                await _patientRepository.UpdatePatientAsync(patientId, patient);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        // ==================== SEARCH DOCTORS ====================

        [AllowAnonymous]
        public async Task<IActionResult> SearchDoctors(string specialty, string location)
        {
            var doctors = await _doctorRepository.GetAllDoctorsAsync();
            var specialties = await _doctorRepository.GetAllSpecialtiesAsync();

            if (!string.IsNullOrEmpty(specialty))
            {
                doctors = doctors.Where(d => d.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(location))
            {
                doctors = doctors.Where(d => d.Location != null && d.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Specialties = specialties;
            ViewBag.SelectedSpecialty = specialty;
            ViewBag.SelectedLocation = location;

            return View(doctors);
        }

        // ==================== DOCTOR DETAILS ====================

        [AllowAnonymous]
        public async Task<IActionResult> DoctorDetails(string id)
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            // Get feedback for this doctor
            var filter = Builders<Feedback>.Filter.Eq(f => f.DoctorId, id);
            var feedback = await _mongoDbService.Feedback
                .Find(filter)
                .ToListAsync();

            ViewBag.Feedback = feedback;
            ViewBag.AverageRating = feedback.Any() ? feedback.Average(f => f.Rating) : 0;

            return View(doctor);
        }

        // ==================== BOOK APPOINTMENT ====================

        // Show booking form
        public async Task<IActionResult> BookAppointment(string doctorId)
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var patientId = GetCurrentPatientId();
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);

            ViewBag.Doctor = doctor;
            ViewBag.Patient = patient;

            return View();
        }

        // Confirm Booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(string doctorId, DateTime appointmentDate, string appointmentTime)
        {
            var patientId = GetCurrentPatientId();
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);
            var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);

            if (patient == null || doctor == null)
            {
                return NotFound();
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                PatientName = patient.FullName,
                DoctorName = doctor.FullName,
                Specialty = doctor.Specialty,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                ConsultationFee = doctor.ConsultationFee,
                Status = "Pending",
                IsPaid = false,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepository.CreateAppointmentAsync(appointment);
            TempData["SuccessMessage"] = "Appointment booked successfully! Please proceed to payment.";

            return RedirectToAction("Payment", new { id = appointment.Id });
        }

        // ==================== PAYMENT ====================

        // Show payment page
        public async Task<IActionResult> Payment(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.PatientId != GetCurrentPatientId())
            {
                return NotFound();
            }

            return View(appointment);
        }

        // Process Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string id, string paymentMethod, string cardNumber, string cardName)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.PatientId == GetCurrentPatientId())
            {
                appointment.IsPaid = true;
                appointment.PaymentMethod = paymentMethod;
                appointment.PaymentDate = DateTime.Now;
                appointment.Status = "Confirmed";

                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Payment successful! Your appointment is confirmed.";

                return RedirectToAction("AppointmentConfirmation", new { id });
            }

            return RedirectToAction("Payment", new { id });
        }

        // Appointment Confirmation Page
        public async Task<IActionResult> AppointmentConfirmation(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.PatientId != GetCurrentPatientId())
            {
                return NotFound();
            }

            return View(appointment);
        }

        // ==================== MY APPOINTMENTS ====================

        // View all appointments
        public async Task<IActionResult> MyAppointments(string status = "All")
        {
            var patientId = GetCurrentPatientId();
            var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);

            if (status != "All")
            {
                appointments = appointments.Where(a => a.Status == status).ToList();
            }

            ViewBag.StatusFilter = status;
            return View(appointments);
        }

        // View single appointment details
        public async Task<IActionResult> AppointmentDetails(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.PatientId != GetCurrentPatientId())
            {
                return NotFound();
            }

            return View(appointment);
        }

        // Cancel Appointment
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.PatientId == GetCurrentPatientId())
            {
                appointment.Status = "Cancelled";
                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }

            return RedirectToAction("MyAppointments");
        }

        // ==================== MEDICAL RECORDS ====================

        public async Task<IActionResult> MedicalRecords()
        {
            var patientId = GetCurrentPatientId();
            var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);

            var completedAppointments = appointments
                .Where(a => a.Status == "Completed" && !string.IsNullOrEmpty(a.ConsultationNotes))
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(completedAppointments);
        }

        // ==================== FEEDBACK ====================

        // Show feedback form
        public async Task<IActionResult> LeaveFeedback(string appointmentId)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null || appointment.PatientId != GetCurrentPatientId())
            {
                return NotFound();
            }

            ViewBag.Appointment = appointment;
            return View();
        }

        // Submit feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(string appointmentId, int rating, string comments)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            var patientId = GetCurrentPatientId();
            var patient = await _patientRepository.GetPatientByIdAsync(patientId);

            if (appointment != null && patient != null)
            {
                var feedback = new Feedback
                {
                    PatientId = patientId,
                    PatientName = patient.FullName,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.DoctorName,
                    AppointmentId = appointmentId,
                    Rating = rating,
                    Comments = comments,
                    CreatedAt = DateTime.Now
                };

                await _mongoDbService.Feedback.InsertOneAsync(feedback);
                TempData["SuccessMessage"] = "Feedback submitted successfully!";
            }

            return RedirectToAction("MyAppointments");
        }
    }
}