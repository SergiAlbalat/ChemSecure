using ChemSecureApi.Model;

namespace ChemSecureApi.DTOs
{
    public class TankGetDTO
    {
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public residusType Type { get; set; }
    }
}
