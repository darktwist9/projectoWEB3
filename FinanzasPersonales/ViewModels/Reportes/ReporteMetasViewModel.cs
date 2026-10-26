using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.ViewModels.Reportes;

/// <summary>Reporte 3: avance de las metas de ahorro.</summary>
public class ReporteMetasViewModel
{
    // ----- Filtros -----
    /// <summary>"en-curso", "cumplidas" o vacío para todas.</summary>
    [Display(Name = "Estado")]
    public string? Estado { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha objetivo hasta")]
    public DateTime? FechaObjetivoHasta { get; set; }

    // ----- Resultados -----
    public List<MetaAhorro> Metas { get; set; } = [];

    public decimal TotalObjetivo => Metas.Sum(m => m.MontoObjetivo);
    public decimal TotalAhorrado => Metas.Sum(m => m.MontoAhorrado);
    public decimal TotalRestante => Metas.Sum(m => m.MontoRestante);
    public decimal PorcentajeGlobal => TotalObjetivo == 0 ? 0 : Math.Min(Math.Round(TotalAhorrado * 100 / TotalObjetivo, 1), 100);
    public int CantidadCumplidas => Metas.Count(m => m.Cumplida);
}
