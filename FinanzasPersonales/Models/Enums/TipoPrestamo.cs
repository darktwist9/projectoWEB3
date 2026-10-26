using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Models.Enums;

public enum TipoPrestamo
{
    /// <summary>El usuario prestó dinero a una persona (le deben).</summary>
    [Display(Name = "Préstamo otorgado (me deben)")]
    PrestamoOtorgado = 1,

    /// <summary>El usuario recibió dinero prestado de una persona (debe).</summary>
    [Display(Name = "Deuda contraída (debo)")]
    DeudaContraida = 2
}
