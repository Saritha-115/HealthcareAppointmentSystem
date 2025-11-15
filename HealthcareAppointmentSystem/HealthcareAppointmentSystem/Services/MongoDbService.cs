using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using HealthcareAppointmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var connectionString = configuration.GetConnectionString("MongoDbConnection");
            var databaseName = configuration.GetConnectionString("DatabaseName");

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
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
            return await _doctors.Find(d => d.IsActive).ToListAsync();
        }

        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            return await _doctors.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Doctor> GetDoctorByEmailAsync(string email)
        {
            return await _doctors.Find(d => d.Email == email).FirstOrDefaultAsync();
        }

        public async Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            return await _doctors.Find(d => d.Specialty == specialty && d.IsActive).ToListAsync();
        }

        public async Task CreateDoctorAsync(Doctor doctor)
        {
            await _doctors.InsertOneAsync(doctor);
        }

        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            await _doctors.ReplaceOneAsync(d => d.Id == id, doctor);
        }

        public async Task DeleteDoctorAsync(string id)
        {
            var filter = Builders<Doctor>.Filter.Eq(d => d.Id, id);
            var update = Builders<Doctor>.Update.Set(d => d.IsActive, false);
            await _doctors.UpdateOneAsync(filter, update);
        }

        public async Task<List<string>> GetAllSpecialtiesAsync()
        {
            return await _doctors.Distinct(d => d.Specialty, d => d.IsActive).ToListAsync();
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
            return await _patients.Find(_ => true).ToListAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(string id)
        {
            return await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Patient> GetPatientByEmailAsync(string email)
        {
            return await _patients.Find(p => p.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreatePatientAsync(Patient patient)
        {
            await _patients.InsertOneAsync(patient);
        }

        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            await _patients.ReplaceOneAsync(p => p.Id == id, patient);
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
            return await _appointments.Find(_ => true)
                .SortByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(string id)
        {
            return await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _appointments.Find(a => a.PatientId == patientId)
                .SortByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            return await _appointments.Find(a => a.DoctorId == doctorId)
                .SortByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task CreateAppointmentAsync(Appointment appointment)
        {
            await _appointments.InsertOneAsync(appointment);
        }

        public async Task UpdateAppointmentAsync(string id, Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.Now;
            await _appointments.ReplaceOneAsync(a => a.Id == id, appointment);
        }

        public async Task DeleteAppointmentAsync(string id)
        {
            await _appointments.DeleteOneAsync(a => a.Id == id);
        }
    }
}