using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.Extensions.JsonSerialized;
using Microsoft.AspNetCore.Identity;

namespace Core.Domain
{
    public class UserEntity : IdentityUser
    {
        
        public string PIN { get; set; } = string.Empty;
        public CPINType PINType { get; set; }
        public DateOnly? BirthDate { get; set; }
        [Column(name: "Image")]
        private string _imageJson { get; set; } = string.Empty;
        [NotMapped]
        public List<ImageProperty> ImageProperties
        {
            get => _imageJson.FromJson<List<ImageProperty>>() ?? new();
            set => _imageJson = value.ToJson();
        }

        [Column(name: "Address")]
        private string _addressJson { get; set; } = string.Empty;
        [NotMapped]
        public List<AddressProperty> AddressProperties
        {
            get => _addressJson.FromJson<List<AddressProperty>>() ?? new();
            set => _addressJson = value.ToJson();
        }
    }

    #region address property
    public class AddressProperty
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public string ProvinceId { get; set; } = string.Empty;
        public string DistrictId { get; set; } = string.Empty;
        public string WardId { get; set; } = string.Empty;
        public int UsageCount { get; set; } = 0;
        public string FullAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;

        private DateTimeOffset _createdDate = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate.ToLocalTime();
            private set => _createdDate = value;
        }

        private DateTimeOffset? _modifiedDate;
        public DateTimeOffset? ModifiedDate
        {
            get => _modifiedDate?.ToLocalTime();
            set => _modifiedDate = value;
        }
    }
    #endregion address property

    #region image info
    public class ImageProperty
    {
        public string BlobId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        public CBlobType BlobType { get; set; }
        public CImageType ImageType { get; set; }
        public CFileExtensionType FileExtensionType { get; set; }
        private DateTimeOffset _createdDate = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate.ToLocalTime();
            private set => _createdDate = value;
        }
    }
    #endregion image info

    
}