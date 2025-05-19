using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChemSecureApi.Model
{
    public class Tank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public string Type { get; set; }
        public User Client { get; set; }
    }
}
