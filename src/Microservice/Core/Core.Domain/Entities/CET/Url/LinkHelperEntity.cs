using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain
{
    [Table(name: "CET_LinkHelper")]
    public class LinkHelperEntity
    {
        [Key]
        public string Id { get; private set; } = string.Empty;
        public string DevelopmentEndpoint { get; set; } = string.Empty;
        public string ProductionEndpoint { get; set; } = string.Empty;
    }
}