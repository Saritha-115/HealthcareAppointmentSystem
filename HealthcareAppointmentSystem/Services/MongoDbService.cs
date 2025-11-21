using HealthcareAppointmentSystem.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HealthcareAppointmentSystem.Services
{
    // ==================== MONGODB SERVICE ====================
    public interface IMongoDbService
    {
        IMongoCollection<Admin> Admins { get; }
        IMongoCollection<Doctor> Doctors { get; }
        IMongoCollection<Patient> Patients { get; }
        IMongoCollection<Appointment> Appointments { get; }
        IMongoCollection<Feedback> Feedback { get; }
        IMongoCollection<Specialty> Specialties { get; }
    }

    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("MongoDbConnection");
                var databaseName = configuration.GetConnectionString("DatabaseName");

                Console.WriteLine($"Connecting to MongoDB: {connectionString}");
                Console.WriteLine($"Database: {databaseName}");

                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);

                Console.WriteLine("MongoDB connection established successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB connection error: {ex.Message}");
                throw;
            }
        }

        public IMongoCollection<Admin> Admins =>
            _database.GetCollection<Admin>("Admins");

        public IMongoCollection<Doctor> Doctors =>
            _database.GetCollection<Doctor>("Doctors");

        public IMongoCollection<Patient> Patients =>
            _database.GetCollection<Patient>("Patients");

        public IMongoCollection<Appointment> Appointments =>
            _database.GetCollection<Appointment>("Appointments");

        public IMongoCollection<Feedback> Feedback =>
            _database.GetCollection<Feedback>("Feedback");

        public IMongoCollection<Specialty> Specialties =>
            _database.GetCollection<Specialty>("Specialties");
    }

    // ==================== DOCTOR REPOSITORY ====================
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<Doctor> GetDoctorByIdAsync(string id);
        Task<Doctor> GetDoctorByEmailAsync(string email);
        Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
        Task CreateDoctorAsync(Doctor doctor);
        Task UpdateDoctorAsync(string id, Doctor doctor);
        Task DeleteDoctorAsync(string id);
        Task<List<string>> GetAllSpecialtiesAsync();
    }

    public class DoctorRepository : IDoctorRepository
    {
        private readonly IMongoCollection<Doctor> _doctors;

        public DoctorRepository(IMongoDbService mongoDbService)
        {
            _doctors = mongoDbService.Doctors;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            try
            {
                return await _doctors.Find(d => d.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctors: {ex.Message}");
                return new List<Doctor>();
            }
        }

        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            try
            {
                return await _doctors.Find(d => d.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctor by id: {ex.Message}");
                return null;
            }
        }

        public async Task<Doctor> GetDoctorByEmailAsync(string email)
        {
            try
            {
                return await _doctors.Find(d => d.Email == email).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctor by email: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            try
            {
                return await _doctors.Find(d => d.Specialty == specialty && d.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctors by specialty: {ex.Message}");
                return new List<Doctor>();
            }
        }

        public async Task CreateDoctorAsync(Doctor doctor)
        {
            try
            {
                await _doctors.InsertOneAsync(doctor);
                Console.WriteLine($"Doctor created: {doctor.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating doctor: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            try
            {
                await _doctors.ReplaceOneAsync(d => d.Id == id, doctor);
                Console.WriteLine($"Doctor updated: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating doctor: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDoctorAsync(string id)
        {
            try
            {
                var filter = Builders<Doctor>.Filter.Eq(d => d.Id, id);
                var update = Builders<Doctor>.Update.Set(d => d.IsActive, false);
                await _doctors.UpdateOneAsync(filter, update);
                Console.WriteLine($"Doctor deleted: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting doctor: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetAllSpecialtiesAsync()
        {
            try
            {
                return await _doctors.Distinct(d => d.Specialty, d => d.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting specialties: {ex.Message}");
                return new List<string>();
            }
        }
    }

    // ==================== PATIENT REPOSITORY ====================
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllPatientsAsync();
        Task<Patient> GetPatientByIdAsync(string id);
        Task<Patient> GetPatientByEmailAsync(string email);
        Task CreatePatientAsync(Patient patient);
        Task UpdatePatientAsync(string id, Patient patient);
    }

    public class PatientRepository : IPatientRepository
    {
        private readonly IMongoCollection<Patient> _patients;

        public PatientRepository(IMongoDbService mongoDbService)
        {
            _patients = mongoDbService.Patients;
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                return await _patients.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting patients: {ex.Message}");
                return new List<Patient>();
            }
        }

        public async Task<Patient> GetPatientByIdAsync(string id)
        {
            try
            {
                return await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting patient by id: {ex.Message}");
                return null;
            }
        }

        public async Task<Patient> GetPatientByEmailAsync(string email)
        {
            try
            {
                return await _patients.Find(p => p.Email == email).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting patient by email: {ex.Message}");
                return null;
            }
        }

        public async Task CreatePatientAsync(Patient patient)
        {
            try
            {
                await _patients.InsertOneAsync(patient);
                Console.WriteLine($"Patient created: {patient.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating patient: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            try
            {
                await _patients.ReplaceOneAsync(p => p.Id == id, patient);
                Console.WriteLine($"Patient updated: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating patient: {ex.Message}");
                throw;
            }
        }
    }

    // ==================== APPOINTMENT REPOSITORY ====================
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment> GetAppointmentByIdAsync(string id);
        Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId);
        Task CreateAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(string id, Appointment appointment);
        Task DeleteAppointmentAsync(string id);
    }

    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly IMongoCollection<Appointment> _appointments;

        public AppointmentRepository(IMongoDbService mongoDbService)
        {
            _appointments = mongoDbService.Appointments;
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                return await _appointments.Find(_ => true)
                    .SortByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting appointments: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(string id)
        {
            try
            {
                return await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting appointment by id: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            try
            {
                return await _appointments.Find(a => a.PatientId == patientId)
                    .SortByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting appointments by patient: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            try
            {
                return await _appointments.Find(a => a.DoctorId == doctorId)
                    .SortByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting appointments by doctor: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                await _appointments.InsertOneAsync(appointment);
                Console.WriteLine($"Appointment created for patient: {appointment.PatientId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating appointment: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAppointmentAsync(string id, Appointment appointment)
        {
            try
            {
                appointment.UpdatedAt = DateTime.Now;
                await _appointments.ReplaceOneAsync(a => a.Id == id, appointment);
                Console.WriteLine($"Appointment updated: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appointment: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAppointmentAsync(string id)
        {
            try
            {
                await _appointments.DeleteOneAsync(a => a.Id == id);
                Console.WriteLine($"Appointment deleted: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting appointment: {ex.Message}");
                throw;
            }
        }
    }
}
