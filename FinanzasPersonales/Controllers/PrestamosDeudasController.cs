using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class PrestamosDeudasController(FinanzasDbContext contexto, IListasSeleccionService listas) : Controller
{
    public async Task<IActionResult> Index()
    {
        var prestamos = await ConsultaConRelaciones()
            .AsNoTracking()
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync();
        return View(prestamos);
    }

    public async Task<IActionResult> Details(int id)
    {
        var prestamo = await ConsultaConRelaciones()
            .AsNoTracking()
            .Include(p => p.Transacciones).ThenInclude(t => t.Billetera)
            .FirstOrDefaultAsync(p => p.Id == id);

        return prestamo == null ? NotFound() : View(prestamo);
    }

    public async Task<IActionResult> Create(int? personaId)
    {
        var modelo = new PrestamoDeudaFormViewModel();
        if (personaId.HasValue) modelo.PrestamoDeuda.PersonaId = personaId.Value;
        return View(await CargarListasAsync(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrestamoDeudaFormViewModel modelo)
    {
        await ValidarPersonaAsync(modelo.PrestamoDeuda);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        var prestamo = new PrestamoDeuda();
        CopiarDatos(modelo.PrestamoDeuda, prestamo);
        contexto.PrestamosDeudas.Add(prestamo);
        await contexto.SaveChangesAsync();
        this.NotificarExito("El préstamo/deuda se registró correctamente. Registre ahora la transacción del desembolso.");
        return RedirectToAction(nameof(Details), new { id = prestamo.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var prestamo = await contexto.PrestamosDeudas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        return View(await CargarListasAsync(new PrestamoDeudaFormViewModel { PrestamoDeuda = prestamo }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PrestamoDeudaFormViewModel modelo)
    {
        if (id != modelo.PrestamoDeuda.Id) return NotFound();

        var prestamo = await contexto.PrestamosDeudas
            .Include(p => p.Transacciones).ThenInclude(t => t.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        const string prefijo = nameof(PrestamoDeudaFormViewModel.PrestamoDeuda) + ".";
        if (prestamo.Transacciones.Count > 0)
        {
            // Cambiar tipo o persona dejaría inconsistentes las transacciones ya vinculadas
            if (modelo.PrestamoDeuda.Tipo != prestamo.Tipo)
                ModelState.AddModelError(prefijo + nameof(PrestamoDeuda.Tipo), "No se puede cambiar el tipo porque ya tiene transacciones vinculadas.");
            if (modelo.PrestamoDeuda.PersonaId != prestamo.PersonaId)
                ModelState.AddModelError(prefijo + nameof(PrestamoDeuda.PersonaId), "No se puede cambiar la persona porque ya tiene transacciones vinculadas.");
        }
        if (modelo.PrestamoDeuda.Monto < prestamo.MontoPagado)
        {
            ModelState.AddModelError(prefijo + nameof(PrestamoDeuda.Monto),
                $"El monto no puede ser menor a lo ya pagado ({prestamo.MontoPagado.ToMoneda()}).");
        }

        await ValidarPersonaAsync(modelo.PrestamoDeuda);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        CopiarDatos(modelo.PrestamoDeuda, prestamo);
        await contexto.SaveChangesAsync();
        this.NotificarExito("El préstamo/deuda se actualizó correctamente.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var prestamo = await ConsultaConRelaciones().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(prestamo);
        return View(prestamo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var prestamo = await ConsultaConRelaciones().FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        var motivo = MotivoBloqueo(prestamo);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Details), new { id });
        }

        contexto.PrestamosDeudas.Remove(prestamo);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"El registro \"{prestamo.Resumen}\" fue eliminado.");
        return RedirectToAction(nameof(Index));
    }

    private IQueryable<PrestamoDeuda> ConsultaConRelaciones() =>
        contexto.PrestamosDeudas
            .Include(p => p.Persona)
            .Include(p => p.Transacciones).ThenInclude(t => t.Categoria);

    private async Task ValidarPersonaAsync(PrestamoDeuda prestamo)
    {
        if (!await contexto.Personas.AnyAsync(p => p.Id == prestamo.PersonaId))
        {
            ModelState.AddModelError(nameof(PrestamoDeudaFormViewModel.PrestamoDeuda) + "." + nameof(PrestamoDeuda.PersonaId),
                "La persona seleccionada no existe.");
        }
    }

    private async Task<PrestamoDeudaFormViewModel> CargarListasAsync(PrestamoDeudaFormViewModel modelo)
    {
        modelo.Personas = await listas.PersonasAsync();
        return modelo;
    }

    /// <summary>Copia solo los campos editables para evitar que se asignen relaciones desde el formulario.</summary>
    private static void CopiarDatos(PrestamoDeuda origen, PrestamoDeuda destino)
    {
        destino.Tipo = origen.Tipo;
        destino.PersonaId = origen.PersonaId;
        destino.Monto = origen.Monto;
        destino.FechaInicio = origen.FechaInicio;
        destino.FechaVencimiento = origen.FechaVencimiento;
        destino.Descripcion = origen.Descripcion;
    }

    private static string? MotivoBloqueo(PrestamoDeuda prestamo) =>
        prestamo.Transacciones.Count > 0
            ? $"No se puede eliminar \"{prestamo.Resumen}\" porque tiene {prestamo.Transacciones.Count} transacción(es) vinculada(s). Elimine primero esas transacciones."
            : null;
}
