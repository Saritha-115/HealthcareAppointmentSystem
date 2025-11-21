using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HealthcareAppointmentSystem.Models
{
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Specialty { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string AppointmentTime { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public decimal ConsultationFee { get; set; }

        public bool IsPaid { get; set; } = false;

        public string PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string ConsultationNotes { get; set; }

        public string Prescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
