using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using FinanzasPersonales.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class CategoriasController(FinanzasDbContext contexto) : Controller
{
    public async Task<IActionResult> Index(TipoCategoria? tipo)
    {
        var consulta = contexto.Categorias.AsNoTracking().Include(c => c.Transacciones).AsQueryable();
        if (tipo.HasValue) consulta = consulta.Where(c => c.Tipo == tipo);

        ViewData["TipoSeleccionado"] = tipo;
        return View(await consulta.OrderBy(c => c.Tipo).ThenBy(c => c.Nombre).ToListAsync());
    }

    public IActionResult Create() => View(new Categoria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Tipo,Descripcion")] Categoria categoria)
    {
        await ValidarNombreUnicoAsync(categoria);
        if (!ModelState.IsValid) return View(categoria);

        contexto.Categorias.Add(categoria);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La categoría \"{categoria.Nombre}\" se registró correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await contexto.Categorias.FindAsync(id);
        return categoria == null ? NotFound() : View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Tipo,Descripcion")] Categoria categoria)
    {
        if (id != categoria.Id) return NotFound();

        var tipoActual = await contexto.Categorias.Where(c => c.Id == id).Select(c => (TipoCategoria?)c.Tipo).FirstOrDefaultAsync();
        if (tipoActual == null) return NotFound();

        // Cambiar el tipo invertiría el efecto de las transacciones ya registradas sobre los saldos
        if (tipoActual != categoria.Tipo && await contexto.Transacciones.AnyAsync(t => t.CategoriaId == id))
        {
            ModelState.AddModelError(nameof(Categoria.Tipo), "No se puede cambiar el tipo de una categoría que ya tiene transacciones registradas.");
        }

        await ValidarNombreUnicoAsync(categoria);
        if (!ModelState.IsValid) return View(categoria);

        contexto.Categorias.Update(categoria);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La categoría \"{categoria.Nombre}\" se actualizó correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await contexto.Categorias.AsNoTracking().Include(c => c.Transacciones).FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(categoria);
        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await contexto.Categorias.Include(c => c.Transacciones).FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();

        var motivo = MotivoBloqueo(categoria);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Index));
        }

        contexto.Categorias.Remove(categoria);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La categoría \"{categoria.Nombre}\" fue eliminada.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarNombreUnicoAsync(Categoria categoria)
    {
        if (await contexto.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.Tipo == categoria.Tipo && c.Id != categoria.Id))
        {
            ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre y tipo.");
        }
    }

    private static string? MotivoBloqueo(Categoria categoria) =>
        categoria.Transacciones.Count > 0
            ? $"No se puede eliminar la categoría \"{categoria.Nombre}\" porque clasifica {categoria.Transacciones.Count} transacción(es)."
            : null;
}
