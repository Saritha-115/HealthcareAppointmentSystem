using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HealthcareAppointmentSystem.Models
{
    public class Feedback
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string PatientId { get; set; }

        public string PatientName { get; set; }

        public string DoctorId { get; set; }

        public string DoctorName { get; set; }

        public string AppointmentId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
