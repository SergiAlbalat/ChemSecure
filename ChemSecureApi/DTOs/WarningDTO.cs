using ChemSecureApi.Model;

namespace ChemSecureApi.DTOs
{
    public class WarningDTO
    {
        public string ClientName { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
        public int TankId { get; set; }
        public residusType Type { get; set; }
    }
}
