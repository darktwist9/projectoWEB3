using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class PersonasController(FinanzasDbContext contexto) : Controller
{
    public async Task<IActionResult> Index(string? buscar)
    {
        var consulta = contexto.Personas
            .AsNoTracking()
            .Include(p => p.Transacciones)
            .Include(p => p.PrestamosDeudas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            consulta = consulta.Where(p => p.Nombre.Contains(buscar));
        }

        ViewData["Buscar"] = buscar;
        return View(await consulta.OrderBy(p => p.Nombre).ToListAsync());
    }

    public IActionResult Create() => View(new Persona());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Telefono,Email,Notas")] Persona persona)
    {
        if (!ModelState.IsValid) return View(persona);

        contexto.Personas.Add(persona);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La persona \"{persona.Nombre}\" se registró correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var persona = await contexto.Personas.FindAsync(id);
        return persona == null ? NotFound() : View(persona);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Telefono,Email,Notas")] Persona persona)
    {
        if (id != persona.Id) return NotFound();
        if (!ModelState.IsValid) return View(persona);

        contexto.Personas.Update(persona);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"Los datos de \"{persona.Nombre}\" se actualizaron correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var persona = await BuscarConRelacionesAsync(id);
        if (persona == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(persona);
        return View(persona);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var persona = await BuscarConRelacionesAsync(id);
        if (persona == null) return NotFound();

        var motivo = MotivoBloqueo(persona);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Index));
        }

        contexto.Personas.Remove(persona);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La persona \"{persona.Nombre}\" fue eliminada.");
        return RedirectToAction(nameof(Index));
    }

    private Task<Persona?> BuscarConRelacionesAsync(int id) =>
        contexto.Personas
            .Include(p => p.Transacciones)
            .Include(p => p.PrestamosDeudas)
            .FirstOrDefaultAsync(p => p.Id == id);

    private static string? MotivoBloqueo(Persona persona) =>
        persona.Transacciones.Count > 0 || persona.PrestamosDeudas.Count > 0
            ? $"No se puede eliminar a \"{persona.Nombre}\" porque participa en {persona.Transacciones.Count} transacción(es) y {persona.PrestamosDeudas.Count} préstamo(s)/deuda(s)."
            : null;
}
