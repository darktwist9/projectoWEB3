using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class MetasAhorroController(FinanzasDbContext contexto) : Controller
{
    public async Task<IActionResult> Index()
    {
        var metas = await contexto.MetasAhorro
            .AsNoTracking()
            .Include(m => m.Aportes)
            .OrderBy(m => m.FechaObjetivo == null).ThenBy(m => m.FechaObjetivo)
            .ToListAsync();
        return View(metas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var meta = await contexto.MetasAhorro
            .AsNoTracking()
            .Include(m => m.Aportes).ThenInclude(t => t.Billetera)
            .FirstOrDefaultAsync(m => m.Id == id);

        return meta == null ? NotFound() : View(meta);
    }

    public IActionResult Create() => View(new MetaAhorro());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Descripcion,MontoObjetivo,FechaObjetivo")] MetaAhorro meta)
    {
        await ValidarNombreUnicoAsync(meta);
        if (!ModelState.IsValid) return View(meta);

        contexto.MetasAhorro.Add(meta);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La meta \"{meta.Nombre}\" se registró correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var meta = await contexto.MetasAhorro.FindAsync(id);
        return meta == null ? NotFound() : View(meta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,MontoObjetivo,FechaObjetivo")] MetaAhorro meta)
    {
        if (id != meta.Id) return NotFound();

        await ValidarNombreUnicoAsync(meta);
        if (!ModelState.IsValid) return View(meta);

        contexto.MetasAhorro.Update(meta);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La meta \"{meta.Nombre}\" se actualizó correctamente.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var meta = await contexto.MetasAhorro.AsNoTracking().Include(m => m.Aportes).FirstOrDefaultAsync(m => m.Id == id);
        if (meta == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(meta);
        return View(meta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var meta = await contexto.MetasAhorro.Include(m => m.Aportes).FirstOrDefaultAsync(m => m.Id == id);
        if (meta == null) return NotFound();

        var motivo = MotivoBloqueo(meta);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Index));
        }

        contexto.MetasAhorro.Remove(meta);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La meta \"{meta.Nombre}\" fue eliminada.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarNombreUnicoAsync(MetaAhorro meta)
    {
        if (await contexto.MetasAhorro.AnyAsync(m => m.Nombre == meta.Nombre && m.Id != meta.Id))
        {
            ModelState.AddModelError(nameof(MetaAhorro.Nombre), "Ya existe una meta de ahorro con ese nombre.");
        }
    }

    private static string? MotivoBloqueo(MetaAhorro meta) =>
        meta.Aportes.Count > 0
            ? $"No se puede eliminar la meta \"{meta.Nombre}\" porque tiene {meta.Aportes.Count} aporte(s) registrado(s)."
            : null;
}
