using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels.Reportes;

/// <summary>Reporte 1: ingresos y gastos agrupados por categoría en un período.</summary>
public class ReporteCategoriasViewModel
{
    // ----- Filtros -----
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime? FechaDesde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime? FechaHasta { get; set; }

    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }

    [Display(Name = "Billetera")]
    public int? BilleteraId { get; set; }

    [Display(Name = "Tipo")]
    public TipoCategoria? Tipo { get; set; }

    public List<SelectListItem> Usuarios { get; set; } = [];
    public List<SelectListItem> Billeteras { get; set; } = [];

    // ----- Resultados -----
    public List<FilaReporteCategoria> Filas { get; set; } = [];

    public decimal TotalIngresos => Filas.Where(f => f.Tipo == TipoCategoria.Ingreso).Sum(f => f.Total);
    public decimal TotalGastos => Filas.Where(f => f.Tipo == TipoCategoria.Gasto).Sum(f => f.Total);
    public decimal Balance => TotalIngresos - TotalGastos;
    public int CantidadTransacciones => Filas.Sum(f => f.Cantidad);

    /// <summary>Porcentaje que representa la fila dentro del total de su mismo tipo (ingresos o gastos).</summary>
    public decimal Porcentaje(FilaReporteCategoria fila)
    {
        var totalTipo = fila.Tipo == TipoCategoria.Ingreso ? TotalIngresos : TotalGastos;
        return totalTipo == 0 ? 0 : Math.Round(fila.Total * 100 / totalTipo, 1);
    }
}

public class FilaReporteCategoria
{
    public string Categoria { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public decimal Promedio => Cantidad == 0 ? 0 : Math.Round(Total / Cantidad, 2);
}
