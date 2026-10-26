using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class UsuariosController(FinanzasDbContext contexto) : Controller
{
    public async Task<IActionResult> Index()
    {
        var usuarios = await contexto.Usuarios
            .AsNoTracking()
            .Include(u => u.Billeteras)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
        return View(usuarios);
    }

    public IActionResult Create() => View(new Usuario());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Email,FechaRegistro")] Usuario usuario)
    {
        await ValidarEmailUnicoAsync(usuario);
        if (!ModelState.IsValid) return View(usuario);

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"El usuario \"{usuario.Nombre}\" se registró correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await contexto.Usuarios.FindAsync(id);
        return usuario == null ? NotFound() : View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Email,FechaRegistro")] Usuario usuario)
    {
        if (id != usuario.Id) return NotFound();

        await ValidarEmailUnicoAsync(usuario);
        if (!ModelState.IsValid) return View(usuario);

        contexto.Usuarios.Update(usuario);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"Los datos de \"{usuario.Nombre}\" se actualizaron correctamente.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await contexto.Usuarios.AsNoTracking().Include(u => u.Billeteras).FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return NotFound();

        ViewData["MotivoBloqueo"] = MotivoBloqueo(usuario);
        return View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await contexto.Usuarios.Include(u => u.Billeteras).FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return NotFound();

        var motivo = MotivoBloqueo(usuario);
        if (motivo != null)
        {
            this.NotificarError(motivo);
            return RedirectToAction(nameof(Index));
        }

        contexto.Usuarios.Remove(usuario);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"El usuario \"{usuario.Nombre}\" fue eliminado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarEmailUnicoAsync(Usuario usuario)
    {
        if (await contexto.Usuarios.AnyAsync(u => u.Email == usuario.Email && u.Id != usuario.Id))
        {
            ModelState.AddModelError(nameof(Usuario.Email), "Ya existe un usuario registrado con ese correo electrónico.");
        }
    }

    private static string? MotivoBloqueo(Usuario usuario) =>
        usuario.Billeteras.Count > 0
            ? $"No se puede eliminar a \"{usuario.Nombre}\" porque tiene {usuario.Billeteras.Count} billetera(s) registrada(s). Elimine primero sus billeteras."
            : null;
}
