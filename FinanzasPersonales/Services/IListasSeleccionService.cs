using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanzasPersonales.Services;

/// <summary>Arma las opciones de las listas desplegables mostrando nombres descriptivos en lugar de identificadores.</summary>
public interface IListasSeleccionService
{
    Task<List<SelectListItem>> UsuariosAsync();
    Task<List<SelectListItem>> BilleterasAsync();
    Task<List<SelectListItem>> CategoriasAsync();
    Task<List<SelectListItem>> PersonasAsync();
    Task<List<SelectListItem>> PrestamosDeudasAsync();
    Task<List<SelectListItem>> MetasAhorroAsync();
}
