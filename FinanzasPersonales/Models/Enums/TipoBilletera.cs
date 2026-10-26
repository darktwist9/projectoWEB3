using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Models.Enums;

public enum TipoBilletera
{
    [Display(Name = "Efectivo")]
    Efectivo = 1,

    [Display(Name = "Cuenta bancaria")]
    CuentaBancaria = 2,

    [Display(Name = "Tarjeta de débito")]
    TarjetaDebito = 3,

    [Display(Name = "Billetera digital")]
    BilleteraDigital = 4,

    [Display(Name = "Otro")]
    Otro = 5
}
