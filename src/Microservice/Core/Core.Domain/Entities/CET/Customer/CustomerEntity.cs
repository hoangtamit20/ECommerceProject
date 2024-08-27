using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.Enums.Payments;
using Core.Domain.Extensions.JsonSerialized;

namespace Core.Domain
{
    [Table(name: "CET_Customer")]
    public class CustomerEntity
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        // Personal identification number
        public string PIN { get; set; } = string.Empty;
        public CPINType PINType { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Column(name: "Address")]
        private string _addressJson { get; set; } = string.Empty;
        [NotMapped]
        public List<AddressProperty> AddressProperties
        {
            get => _addressJson.FromJson<List<AddressProperty>>() ?? new();
            set => _addressJson = value.ToJson();
        }
        public DateOnly BirthDate { get; set; }
        private DateTimeOffset _createdDate = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate.ToLocalTime();
        }
    }
}