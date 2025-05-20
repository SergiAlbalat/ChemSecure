namespace ChemSecureWeb.Model
{
    public enum residusType { HalogenatedSolvents, NonHalogenatedSolvents, AqueousSolutions, Acids, Oils };
    public class TankDTO
    {
        public int Id { get; set; }
        public double CurrentVolume { get; set; }
        public double Capacity { get; set; }
        public residusType Type { get; set; }
        public double Percentage
        {
            get
            {
                return CalculatePercentage();
            }
            set
            {

            }
        }
        public double CalculatePercentage()
        {
            return CurrentVolume / Capacity * 100;
        }
    }
}
