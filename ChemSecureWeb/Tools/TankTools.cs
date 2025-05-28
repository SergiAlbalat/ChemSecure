using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChemSecureWeb.Tools
{
    public class TankTools
    {
        public static string TankImage(double percentage)
        {
            switch (percentage)
            {
                case > 99:
                    return "./img/FullTank.jpg";
                case > 89:
                    return "./img/Tank-90.jpg";
                case > 79:
                    return "./img/Tank-80.jpg";
                case > 69:
                    return "./img/Tank-70.jpg";
                case > 59:
                    return "./img/Tank-60.jpg";
                case > 49:
                    return "./img/Tank-50.jpg";
                case > 39:
                    return "./img/Tank-40.jpg";
                case > 29:
                    return "./img/Tank-30.jpg";
                case > 19:
                    return "./img/Tank-20.jpg";
                case > 9:
                    return "./img/Tank-10.jpg";
                default:
                    return "./img/EmptyTank.png";
            }
        }
        public static string TankSize(double maxSize)
        {
            switch (maxSize)
            {
                case < 5000:
                    return "100px";
                case > 12000:
                    return "400px";
                default :
                    return "200px";
            }
        }
    }
}
