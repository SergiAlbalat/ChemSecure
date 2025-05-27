using System.ComponentModel.DataAnnotations;

namespace ChemSecureWeb.Model
{
    public class InsertTankDTO
    {
        
            public int Id { get; set; }
            
            [Required(ErrorMessage = "El volumen actual es requerido")]
            [Range(0, double.MaxValue, ErrorMessage = "El volumen actual debe ser mayor o igual a 0")]
            public double CurrentVolume { get; set; }
            
            [Required(ErrorMessage = "La capacidad es requerida")]
            [Range(0.1, double.MaxValue, ErrorMessage = "La capacidad debe ser mayor que 0")]
            public double Capacity { get; set; }
            
            public string? ClientId { get; set; }
            
            [Required(ErrorMessage = "El tipo de residuo es requerido")]
            public residusType Type { get; set; }
            
            public string? UserId { get; set; }
            
            public string? UserEmail { get; set; }
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
