using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChemSecureApi.Model
{
    public enum residusType {HalogenatedSolvents, NonHalogenatedSolvents, AqueousSolutions, Acids, Oils};
    public class Tank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public residusType Type { get; set; }
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public User Client { get; set; }
    }
}
