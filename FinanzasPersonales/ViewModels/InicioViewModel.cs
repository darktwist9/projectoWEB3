using FinanzasPersonales.Models;

namespace FinanzasPersonales.ViewModels;

/// <summary>Resumen general que se muestra en la página de inicio.</summary>
public class InicioViewModel
{
    public decimal SaldoTotal { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal GastosMes { get; set; }
    public decimal PorCobrar { get; set; }
    public decimal PorPagar { get; set; }
    public List<Transaccion> UltimasTransacciones { get; set; } = [];
}
