using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Models.Enums;

/// <summary>Indica si los movimientos de una categoría suman (ingreso) o restan (gasto) al saldo.</summary>
public enum TipoCategoria
{
    [Display(Name = "Ingreso")]
    Ingreso = 1,

    [Display(Name = "Gasto")]
    Gasto = 2
}
