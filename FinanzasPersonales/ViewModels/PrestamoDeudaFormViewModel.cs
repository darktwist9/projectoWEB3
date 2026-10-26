using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels;

/// <summary>Formulario de creación/edición de préstamos y deudas con la lista de personas.</summary>
public class PrestamoDeudaFormViewModel
{
    public PrestamoDeuda PrestamoDeuda { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Personas { get; set; } = [];
}
