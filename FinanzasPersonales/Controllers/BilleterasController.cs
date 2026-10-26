using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class BilleterasController(FinanzasDbContext contexto, IListasSeleccionService listas) : Controller
{
    public async Task<IActionResult> Index(int? usuarioId)
    {
        var consulta = contexto.Billeteras
            .AsNoTracking()
            .Include(b => b.Usuario)
            .Include(b => b.Transacciones).ThenInclude(t => t.Categoria)
            .AsQueryable();

        if (usuarioId.HasValue) consulta = consulta.Where(b => b.UsuarioId == usuarioId);

        ViewData["Usuarios"] = await listas.UsuariosAsync();
        ViewData["UsuarioSeleccionado"] = usuarioId;
        return View(await consulta.OrderBy(b => b.Usuario!.Nombre).ThenBy(b => b.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var billetera = await contexto.Billeteras
            .AsNoTracking()
            .Include(b => b.Usuario)
            .Include(b => b.Transacciones).ThenInclude(t => t.Categoria)
            .Include(b => b.Transacciones).ThenInclude(t => t.Persona)
            .FirstOrDefaultAsync(b => b.Id == id);

        return billetera == null ? NotFound() : View(billetera);
    }

    public async Task<IActionResult> Create(int? usuarioId)
    {
        var modelo = new BilleteraFormViewModel();
        if (usuarioId.HasValue) modelo.Billetera.UsuarioId = usuarioId.Value;
        return View(await CargarListasAsync(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BilleteraFormViewModel modelo)
    {
        await ValidarAsync(modelo.Billetera);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        var billetera = new Billetera();
        CopiarDatos(modelo.Billetera, billetera);
        contexto.Billeteras.Add(billetera);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La billetera \"{billetera.Nombre}\" se registró correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var billetera = await contexto.Billeteras.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (billetera == null) return NotFound();

        return View(await CargarListasAsync(new BilleteraFormViewModel { Billetera = billetera }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BilleteraFormViewModel modelo)
    {
        if (id != modelo.Billetera.Id) return NotFound();

        var billetera = await contexto.Billeteras.FindAsync(id);
        if (billetera == null) return NotFound();

        await ValidarAsync(modelo.Billetera);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        CopiarDatos(modelo.Billetera, billetera);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La billetera \"{billetera.Nombre}\" se actualizó correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var billetera = await BuscarConRelacionesAsync(id);
        if (billetera == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(billetera);
        return View(billetera);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var billetera = await BuscarConRelacionesAsync(id);
        if (billetera == null) return NotFound();

        var motivo = MotivoBloqueo(billetera);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Index));
        }

        contexto.Billeteras.Remove(billetera);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La billetera \"{billetera.Nombre}\" fue eliminada.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Billetera billetera)
    {
        if (!await contexto.Usuarios.AnyAsync(u => u.Id == billetera.UsuarioId))
        {
            ModelState.AddModelError("Billetera.UsuarioId", "El usuario seleccionado no existe.");
        }
        else if (await contexto.Billeteras.AnyAsync(b => b.UsuarioId == billetera.UsuarioId && b.Nombre == billetera.Nombre && b.Id != billetera.Id))
        {
            ModelState.AddModelError("Billetera.Nombre", "Este usuario ya tiene una billetera con ese nombre.");
        }
    }

    private async Task<BilleteraFormViewModel> CargarListasAsync(BilleteraFormViewModel modelo)
    {
        modelo.Usuarios = await listas.UsuariosAsync();
        return modelo;
    }

    /// <summary>Copia solo los campos editables para evitar que se asignen relaciones desde el formulario.</summary>
    private static void CopiarDatos(Billetera origen, Billetera destino)
    {
        destino.Nombre = origen.Nombre;
        destino.Tipo = origen.Tipo;
        destino.SaldoInicial = origen.SaldoInicial;
        destino.UsuarioId = origen.UsuarioId;
    }

    private Task<Billetera?> BuscarConRelacionesAsync(int id) =>
        contexto.Billeteras
            .Include(b => b.Usuario)
            .Include(b => b.Transacciones).ThenInclude(t => t.Categoria)
            .FirstOrDefaultAsync(b => b.Id == id);

    private static string? MotivoBloqueo(Billetera billetera) =>
        billetera.Transacciones.Count > 0
            ? $"No se puede eliminar la billetera \"{billetera.Nombre}\" porque tiene {billetera.Transacciones.Count} transacción(es) registrada(s)."
            : null;
}
