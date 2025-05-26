namespace ChemSecureApi.DTOs
{
    public class WarningDTO
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public double Capacity { get; set; }
        public double CurrentVolume { get; set; }
    }
}
