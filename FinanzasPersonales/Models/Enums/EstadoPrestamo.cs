using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Models.Enums;

/// <summary>Estado calculado de un préstamo o deuda (no se guarda en la base de datos).</summary>
public enum EstadoPrestamo
{
    [Display(Name = "Pendiente")]
    Pendiente = 1,

    [Display(Name = "Vencido")]
    Vencido = 2,

    [Display(Name = "Saldado")]
    Saldado = 3
}
