using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthcareAppointmentSystem.Models;
using HealthcareAppointmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareAppointmentSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public DoctorController(
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        // Get current doctor ID from claims
        private string GetCurrentDoctorId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var doctorId = GetCurrentDoctorId();
            var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId);

            var today = DateTime.Today;
            var todayAppointments = appointments.Where(a => a.AppointmentDate.Date == today).ToList();
            var upcomingAppointments = appointments.Where(a => a.AppointmentDate.Date > today && a.Status != "Cancelled").ToList();

            ViewBag.TodayAppointments = todayAppointments.Count;
            ViewBag.UpcomingAppointments = upcomingAppointments.Count;
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.PendingAppointments = appointments.Count(a => a.Status == "Pending");

            return View(todayAppointments);
        }

        // View Profile
        public async Task<IActionResult> Profile()
        {
            var doctorId = GetCurrentDoctorId();
            var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // Edit Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Doctor doctor)
        {
            var doctorId = GetCurrentDoctorId();

            if (ModelState.IsValid)
            {
                var existingDoctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);
                if (existingDoctor != null)
                {
                    // Update only allowed fields
                    existingDoctor.FullName = doctor.FullName;
                    existingDoctor.PhoneNumber = doctor.PhoneNumber;
                    existingDoctor.Location = doctor.Location;
                    existingDoctor.Qualification = doctor.Qualification;

                    await _doctorRepository.UpdateDoctorAsync(doctorId, existingDoctor);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
            }

            return RedirectToAction("Profile");
        }

        // Update Availability
        public async Task<IActionResult> ManageAvailability()
        {
            var doctorId = GetCurrentDoctorId();
            var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // Save Availability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAvailability(List<Availability> availabilitySchedule)
        {
            var doctorId = GetCurrentDoctorId();
            var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);

            if (doctor != null)
            {
                doctor.AvailabilitySchedule = availabilitySchedule;
                await _doctorRepository.UpdateDoctorAsync(doctorId, doctor);
                TempData["SuccessMessage"] = "Availability updated successfully!";
            }

            return RedirectToAction("ManageAvailability");
        }

        // View All Appointments
        public async Task<IActionResult> Appointments(string status = "All")
        {
            var doctorId = GetCurrentDoctorId();
            var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId);

            if (status != "All")
            {
                appointments = appointments.Where(a => a.Status == status).ToList();
            }

            ViewBag.StatusFilter = status;
            return View(appointments);
        }

        // View Appointment Details
        public async Task<IActionResult> AppointmentDetails(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Verify this appointment belongs to current doctor
            var doctorId = GetCurrentDoctorId();
            if (appointment.DoctorId != doctorId)
            {
                return Forbid();
            }

            return View(appointment);
        }

        // Confirm Appointment
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.DoctorId == GetCurrentDoctorId())
            {
                appointment.Status = "Confirmed";
                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Appointment confirmed successfully!";
            }

            return RedirectToAction("Appointments");
        }

        // Cancel Appointment
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(string id, string reason)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.DoctorId == GetCurrentDoctorId())
            {
                appointment.Status = "Cancelled";
                appointment.ConsultationNotes = $"Cancelled by doctor. Reason: {reason}";
                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }

            return RedirectToAction("Appointments");
        }

        // Update Consultation Notes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConsultation(string id, string consultationNotes, string prescription)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.DoctorId == GetCurrentDoctorId())
            {
                appointment.ConsultationNotes = consultationNotes;
                appointment.Prescription = prescription;
                appointment.Status = "Completed";
                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Consultation notes updated successfully!";
            }

            return RedirectToAction("AppointmentDetails", new { id });
        }

        // Reschedule Appointment
        public async Task<IActionResult> Reschedule(string id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment == null || appointment.DoctorId != GetCurrentDoctorId())
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(string id, DateTime newDate, string newTime)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

            if (appointment != null && appointment.DoctorId == GetCurrentDoctorId())
            {
                appointment.AppointmentDate = newDate;
                appointment.AppointmentTime = newTime;
                appointment.Status = "Confirmed";
                await _appointmentRepository.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Appointment rescheduled successfully!";
            }

            return RedirectToAction("Appointments");
        }
    }
}