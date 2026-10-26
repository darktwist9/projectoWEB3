using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels;

/// <summary>Listado de transacciones con filtros de búsqueda.</summary>
public class TransaccionesIndexViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime? FechaDesde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime? FechaHasta { get; set; }

    [Display(Name = "Billetera")]
    public int? BilleteraId { get; set; }

    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    public List<Transaccion> Transacciones { get; set; } = [];
    public List<SelectListItem> Billeteras { get; set; } = [];
    public List<SelectListItem> Categorias { get; set; } = [];

    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
}
