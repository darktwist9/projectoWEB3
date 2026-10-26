using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.ViewModels;

/// <summary>Formulario de creación/edición de billeteras con la lista de usuarios.</summary>
public class BilleteraFormViewModel
{
    public Billetera Billetera { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Usuarios { get; set; } = [];
}
