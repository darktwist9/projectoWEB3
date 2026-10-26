using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Services;

public class ListasSeleccionService(FinanzasDbContext contexto) : IListasSeleccionService
{
    public Task<List<SelectListItem>> UsuariosAsync() =>
        contexto.Usuarios
            .OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString()))
            .ToListAsync();

    public Task<List<SelectListItem>> BilleterasAsync() =>
        contexto.Billeteras
            .OrderBy(b => b.Usuario!.Nombre).ThenBy(b => b.Nombre)
            .Select(b => new SelectListItem($"{b.Nombre} — {b.Usuario!.Nombre}", b.Id.ToString()))
            .ToListAsync();

    public async Task<List<SelectListItem>> CategoriasAsync()
    {
        var grupos = Enum.GetValues<TipoCategoria>()
            .ToDictionary(tipo => tipo, tipo => new SelectListGroup { Name = tipo.ObtenerNombre() });

        var categorias = await contexto.Categorias
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nombre)
            .Select(c => new { c.Id, c.Nombre, c.Tipo })
            .ToListAsync();

        return categorias
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()) { Group = grupos[c.Tipo] })
            .ToList();
    }

    public Task<List<SelectListItem>> PersonasAsync() =>
        contexto.Personas
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString()))
            .ToListAsync();

    public async Task<List<SelectListItem>> PrestamosDeudasAsync()
    {
        var prestamos = await contexto.PrestamosDeudas
            .Include(pd => pd.Persona)
            .OrderBy(pd => pd.Persona!.Nombre).ThenBy(pd => pd.FechaInicio)
            .ToListAsync();

        return prestamos
            .Select(pd => new SelectListItem($"{pd.Resumen} — {pd.Monto.ToMoneda()} ({pd.FechaInicio.ToFecha()})", pd.Id.ToString()))
            .ToList();
    }

    public Task<List<SelectListItem>> MetasAhorroAsync() =>
        contexto.MetasAhorro
            .OrderBy(m => m.Nombre)
            .Select(m => new SelectListItem(m.Nombre, m.Id.ToString()))
            .ToListAsync();
}
