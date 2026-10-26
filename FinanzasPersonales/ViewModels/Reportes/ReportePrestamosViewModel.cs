using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models;
using FinanzasPersonales.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels.Reportes;

/// <summary>Reporte 2: préstamos por cobrar y deudas por pagar con su saldo pendiente.</summary>
public class ReportePrestamosViewModel
{
    // ----- Filtros -----
    [Display(Name = "Tipo")]
    public TipoPrestamo? Tipo { get; set; }

    [Display(Name = "Persona")]
    public int? PersonaId { get; set; }

    /// <summary>Sin valor = solo los que no están saldados (pendientes y vencidos).</summary>
    [Display(Name = "Estado")]
    public EstadoPrestamo? Estado { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Vencen hasta")]
    public DateTime? VencimientoHasta { get; set; }

    public List<SelectListItem> Personas { get; set; } = [];

    // ----- Resultados -----
    public List<PrestamoDeuda> Prestamos { get; set; } = [];

    public decimal TotalPorCobrar => SaldoDe(TipoPrestamo.PrestamoOtorgado);
    public decimal TotalPorPagar => SaldoDe(TipoPrestamo.DeudaContraida);
    public decimal TotalMonto => Prestamos.Sum(p => p.Monto);
    public decimal TotalPagado => Prestamos.Sum(p => p.MontoPagado);
    public decimal TotalPendiente => Prestamos.Sum(p => p.SaldoPendiente);
    public int CantidadVencidos => Prestamos.Count(p => p.Estado == EstadoPrestamo.Vencido);

    private decimal SaldoDe(TipoPrestamo tipo) => Prestamos.Where(p => p.Tipo == tipo).Sum(p => p.SaldoPendiente);
}
