using FinanzasPersonales.Data;
using FinanzasPersonales.Helpers;
using FinanzasPersonales.Models;
using FinanzasPersonales.Models.Enums;
using FinanzasPersonales.Services;
using FinanzasPersonales.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class TransaccionesController(FinanzasDbContext contexto, IListasSeleccionService listas) : Controller
{
    public async Task<IActionResult> Index(TransaccionesIndexViewModel filtro)
    {
        var consulta = contexto.Transacciones
            .AsNoTracking()
            .Include(t => t.Billetera).ThenInclude(b => b!.Usuario)
            .Include(t => t.Categoria)
            .Include(t => t.Persona)
            .Include(t => t.PrestamoDeuda).ThenInclude(p => p!.Persona)
            .Include(t => t.MetaAhorro)
            .AsQueryable();

        if (filtro.FechaDesde.HasValue) consulta = consulta.Where(t => t.Fecha >= filtro.FechaDesde.Value);
        if (filtro.FechaHasta.HasValue) consulta = consulta.Where(t => t.Fecha <= filtro.FechaHasta.Value);
        if (filtro.BilleteraId.HasValue) consulta = consulta.Where(t => t.BilleteraId == filtro.BilleteraId);
        if (filtro.CategoriaId.HasValue) consulta = consulta.Where(t => t.CategoriaId == filtro.CategoriaId);

        filtro.Transacciones = await consulta.OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Id).ToListAsync();
        filtro.TotalIngresos = filtro.Transacciones.Where(t => t.Categoria!.Tipo == TipoCategoria.Ingreso).Sum(t => t.Monto);
        filtro.TotalGastos = filtro.Transacciones.Where(t => t.Categoria!.Tipo == TipoCategoria.Gasto).Sum(t => t.Monto);
        filtro.Billeteras = await listas.BilleterasAsync();
        filtro.Categorias = await listas.CategoriasAsync();
        return View(filtro);
    }

    /// <summary>Los parámetros opcionales permiten abrir el formulario ya vinculado (por ejemplo, "Registrar pago" desde un préstamo).</summary>
    public async Task<IActionResult> Create(int? billeteraId, int? prestamoDeudaId, int? metaAhorroId)
    {
        var transaccion = new Transaccion { MetaAhorroId = metaAhorroId };
        if (billeteraId.HasValue) transaccion.BilleteraId = billeteraId.Value;

        if (prestamoDeudaId.HasValue)
        {
            var prestamo = await contexto.PrestamosDeudas
                .AsNoTracking()
                .Include(p => p.Transacciones).ThenInclude(t => t.Categoria)
                .FirstOrDefaultAsync(p => p.Id == prestamoDeudaId);

            if (prestamo != null)
            {
                transaccion.PrestamoDeudaId = prestamo.Id;
                transaccion.PersonaId = prestamo.PersonaId;
                transaccion.Monto = prestamo.Transacciones.Count == 0 ? prestamo.Monto : prestamo.SaldoPendiente;
            }
        }

        return View(await CargarListasAsync(new TransaccionFormViewModel { Transaccion = transaccion }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransaccionFormViewModel modelo)
    {
        await ValidarReglasDeNegocioAsync(modelo.Transaccion);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        var transaccion = new Transaccion();
        CopiarDatos(modelo.Transaccion, transaccion);
        contexto.Transacciones.Add(transaccion);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La transacción de {transaccion.Monto.ToMoneda()} se registró correctamente.");
        return RedirigirDespuesDeGuardar(transaccion);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var transaccion = await contexto.Transacciones.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (transaccion == null) return NotFound();

        return View(await CargarListasAsync(new TransaccionFormViewModel { Transaccion = transaccion }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TransaccionFormViewModel modelo)
    {
        if (id != modelo.Transaccion.Id) return NotFound();

        var transaccion = await contexto.Transacciones.FindAsync(id);
        if (transaccion == null) return NotFound();

        await ValidarReglasDeNegocioAsync(modelo.Transaccion);
        if (!ModelState.IsValid) return View(await CargarListasAsync(modelo));

        CopiarDatos(modelo.Transaccion, transaccion);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La transacción de {transaccion.Monto.ToMoneda()} se actualizó correctamente.");
        return RedirigirDespuesDeGuardar(transaccion);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var transaccion = await contexto.Transacciones
            .AsNoTracking()
            .Include(t => t.Billetera).ThenInclude(b => b!.Usuario)
            .Include(t => t.Categoria)
            .Include(t => t.Persona)
            .Include(t => t.PrestamoDeuda).ThenInclude(p => p!.Persona)
            .Include(t => t.MetaAhorro)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaccion == null ? NotFound() : View(transaccion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var transaccion = await contexto.Transacciones.FindAsync(id);
        if (transaccion == null) return NotFound();

        contexto.Transacciones.Remove(transaccion);
        await contexto.SaveChangesAsync();
        this.NotificarExito($"La transacción de {transaccion.Monto.ToMoneda()} del {transaccion.Fecha.ToFecha()} fue eliminada.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Reglas que dependen de otros registros de la base de datos (las simples están en el modelo).</summary>
    private async Task ValidarReglasDeNegocioAsync(Transaccion transaccion)
    {
        const string prefijo = nameof(TransaccionFormViewModel.Transaccion) + ".";

        if (!await contexto.Billeteras.AnyAsync(b => b.Id == transaccion.BilleteraId))
        {
            ModelState.AddModelError(prefijo + nameof(Transaccion.BilleteraId), "La billetera seleccionada no existe.");
        }

        var categoria = await contexto.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == transaccion.CategoriaId);
        if (categoria == null)
        {
            ModelState.AddModelError(prefijo + nameof(Transaccion.CategoriaId), "La categoría seleccionada no existe.");
            return;
        }

        if (transaccion.MetaAhorroId.HasValue && categoria.Tipo != TipoCategoria.Gasto)
        {
            ModelState.AddModelError(prefijo + nameof(Transaccion.CategoriaId),
                "Un aporte a una meta de ahorro sale de la billetera: seleccione una categoría de tipo Gasto (por ejemplo, \"Ahorro\").");
        }

        // Un gasto no puede dejar la billetera con saldo negativo (sin contar esta misma transacción al editar)
        if (categoria.Tipo == TipoCategoria.Gasto)
        {
            var saldoDisponible = await contexto.Billeteras
                .Where(b => b.Id == transaccion.BilleteraId)
                .Select(b => b.SaldoInicial + b.Transacciones
                    .Where(t => t.Id != transaccion.Id)
                    .Sum(t => t.Categoria!.Tipo == TipoCategoria.Ingreso ? t.Monto : -t.Monto))
                .FirstOrDefaultAsync();

            if (transaccion.Monto > saldoDisponible)
            {
                ModelState.AddModelError(prefijo + nameof(Transaccion.Monto),
                    $"Saldo insuficiente: la billetera seleccionada tiene {saldoDisponible.ToMoneda()} disponibles.");
            }
        }

        if (!transaccion.PrestamoDeudaId.HasValue) return;

        var prestamo = await contexto.PrestamosDeudas
            .AsNoTracking()
            .Include(p => p.Persona)
            .Include(p => p.Transacciones).ThenInclude(t => t.Categoria)
            .FirstOrDefaultAsync(p => p.Id == transaccion.PrestamoDeudaId);

        if (prestamo == null)
        {
            ModelState.AddModelError(prefijo + nameof(Transaccion.PrestamoDeudaId), "El préstamo o deuda seleccionado no existe.");
            return;
        }

        // La persona de la transacción debe ser la misma del préstamo; si no se indicó, se toma la del préstamo
        if (!transaccion.PersonaId.HasValue)
        {
            transaccion.PersonaId = prestamo.PersonaId;
        }
        else if (transaccion.PersonaId != prestamo.PersonaId)
        {
            ModelState.AddModelError(prefijo + nameof(Transaccion.PersonaId),
                $"La persona debe coincidir con la del préstamo/deuda ({prestamo.Persona!.Nombre}).");
        }

        // Un pago no puede superar lo que falta pagar (sin contar esta misma transacción al editar)
        if (categoria.Tipo == prestamo.TipoDePago)
        {
            var pagadoPorOtras = prestamo.Transacciones
                .Where(t => t.Id != transaccion.Id && t.Categoria!.Tipo == prestamo.TipoDePago)
                .Sum(t => t.Monto);
            var saldoDisponible = prestamo.Monto - pagadoPorOtras;

            if (transaccion.Monto > saldoDisponible)
            {
                ModelState.AddModelError(prefijo + nameof(Transaccion.Monto),
                    $"El pago supera el saldo pendiente del préstamo/deuda ({saldoDisponible.ToMoneda()}).");
            }
        }
    }

    private async Task<TransaccionFormViewModel> CargarListasAsync(TransaccionFormViewModel modelo)
    {
        modelo.Billeteras = await listas.BilleterasAsync();
        modelo.Categorias = await listas.CategoriasAsync();
        modelo.Personas = await listas.PersonasAsync();
        modelo.PrestamosDeudas = await listas.PrestamosDeudasAsync();
        modelo.MetasAhorro = await listas.MetasAhorroAsync();
        return modelo;
    }

    /// <summary>Copia solo los campos editables para evitar que se asignen relaciones desde el formulario.</summary>
    private static void CopiarDatos(Transaccion origen, Transaccion destino)
    {
        destino.Monto = origen.Monto;
        destino.Fecha = origen.Fecha;
        destino.Descripcion = origen.Descripcion;
        destino.BilleteraId = origen.BilleteraId;
        destino.CategoriaId = origen.CategoriaId;
        destino.PersonaId = origen.PersonaId;
        destino.PrestamoDeudaId = origen.PrestamoDeudaId;
        destino.MetaAhorroId = origen.MetaAhorroId;
    }

    /// <summary>Si la transacción pertenece a un préstamo o meta, vuelve a su detalle para ver el saldo actualizado.</summary>
    private IActionResult RedirigirDespuesDeGuardar(Transaccion transaccion)
    {
        if (transaccion.PrestamoDeudaId.HasValue)
            return RedirectToAction("Details", "PrestamosDeudas", new { id = transaccion.PrestamoDeudaId });
        if (transaccion.MetaAhorroId.HasValue)
            return RedirectToAction("Details", "MetasAhorro", new { id = transaccion.MetaAhorroId });
        return RedirectToAction(nameof(Index));
    }
}
