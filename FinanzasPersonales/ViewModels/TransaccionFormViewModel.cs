using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels;

/// <summary>Formulario de creación/edición de transacciones con todas sus listas relacionadas.</summary>
public class TransaccionFormViewModel
{
    public Transaccion Transaccion { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Billeteras { get; set; } = [];

    [ValidateNever]
    public List<SelectListItem> Categorias { get; set; } = [];

    [ValidateNever]
    public List<SelectListItem> Personas { get; set; } = [];

    [ValidateNever]
    public List<SelectListItem> PrestamosDeudas { get; set; } = [];

    [ValidateNever]
    public List<SelectListItem> MetasAhorro { get; set; } = [];
}
