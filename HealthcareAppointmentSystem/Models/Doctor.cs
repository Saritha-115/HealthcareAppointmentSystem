using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HealthcareAppointmentSystem.Models
{
    public class Doctor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string Qualification { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Location { get; set; }

        [Required]
        public decimal ConsultationFee { get; set; }

        public List<Availability> AvailabilitySchedule { get; set; } = new List<Availability>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
