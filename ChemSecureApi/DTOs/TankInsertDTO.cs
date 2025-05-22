using ChemSecureApi.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChemSecureApi.DTOs
{
    public class TankInsertDTO
    {
        public int Id { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public residusType Type { get; set; }
        public string ClientId { get; set; }
    }
}
