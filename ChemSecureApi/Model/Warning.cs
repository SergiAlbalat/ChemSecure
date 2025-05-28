using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChemSecureApi.Model
{
    public class Warning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ClientName { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public DateTime CreationDate { get; set; }
        public int TankId { get; set; }
        public residusType Type { get; set; }
        public bool IsManaged { get; set; } = false;
        public DateTime? ManagedDate { get; set; } = null;
    }
}
