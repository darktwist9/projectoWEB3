using FinanzasPersonales.Data;
using FinanzasPersonales.Models.Enums;
using FinanzasPersonales.Services;
using FinanzasPersonales.ViewModels.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class ReportesController(FinanzasDbContext contexto, IListasSeleccionService listas) : Controller
{
    public IActionResult Index() => View();

    /// <summary>Reporte 1: ingresos y gastos por categoría en un período.</summary>
    public async Task<IActionResult> MovimientosPorCategoria(ReporteCategoriasViewModel filtro)
    {
        var hoy = DateTime.Today;
        filtro.FechaDesde ??= new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-2);
        filtro.FechaHasta ??= hoy;
        filtro.Usuarios = await listas.UsuariosAsync();
        filtro.Billeteras = await listas.BilleterasAsync();

        if (filtro.FechaDesde > filtro.FechaHasta)
        {
            ModelState.AddModelError(nameof(filtro.FechaHasta), "La fecha \"Hasta\" debe ser posterior o igual a la fecha \"Desde\".");
            return View(filtro);
        }

        var consulta = contexto.Transacciones
            .AsNoTracking()
            .Where(t => t.Fecha >= filtro.FechaDesde && t.Fecha <= filtro.FechaHasta);

        if (filtro.UsuarioId.HasValue) consulta = consulta.Where(t => t.Billetera!.UsuarioId == filtro.UsuarioId);
        if (filtro.BilleteraId.HasValue) consulta = consulta.Where(t => t.BilleteraId == filtro.BilleteraId);
        if (filtro.Tipo.HasValue) consulta = consulta.Where(t => t.Categoria!.Tipo == filtro.Tipo);

        // La agrupación y las sumas se resuelven en SQL Server (GROUP BY)
        var filas = await consulta
            .GroupBy(t => new { t.Categoria!.Nombre, t.Categoria.Tipo })
            .Select(g => new FilaReporteCategoria
            {
                Categoria = g.Key.Nombre,
                Tipo = g.Key.Tipo,
                Cantidad = g.Count(),
                Total = g.Sum(t => t.Monto)
            })
            .ToListAsync();

        filtro.Filas = filas.OrderBy(f => f.Tipo).ThenByDescending(f => f.Total).ToList();
        return View(filtro);
    }

    /// <summary>Reporte 2: préstamos por cobrar y deudas por pagar.</summary>
    public async Task<IActionResult> PrestamosPendientes(ReportePrestamosViewModel filtro)
    {
        filtro.Personas = await listas.PersonasAsync();

        var consulta = contexto.PrestamosDeudas
            .AsNoTracking()
            .Include(p => p.Persona)
            .Include(p => p.Transacciones).ThenInclude(t => t.Categoria)
            .AsQueryable();

        if (filtro.Tipo.HasValue) consulta = consulta.Where(p => p.Tipo == filtro.Tipo);
        if (filtro.PersonaId.HasValue) consulta = consulta.Where(p => p.PersonaId == filtro.PersonaId);
        if (filtro.VencimientoHasta.HasValue) consulta = consulta.Where(p => p.FechaVencimiento <= filtro.VencimientoHasta);

        var prestamos = await consulta.ToListAsync();

        // El estado se calcula a partir de los pagos, por eso se filtra después de consultar
        prestamos = filtro.Estado.HasValue
            ? prestamos.Where(p => p.Estado == filtro.Estado).ToList()
            : prestamos.Where(p => p.Estado != EstadoPrestamo.Saldado).ToList();

        filtro.Prestamos = prestamos
            .OrderBy(p => p.Estado == EstadoPrestamo.Vencido ? 0 : 1)
            .ThenBy(p => p.FechaVencimiento ?? DateTime.MaxValue)
            .ToList();
        return View(filtro);
    }

    /// <summary>Reporte 3: avance de las metas de ahorro.</summary>
    public async Task<IActionResult> ProgresoMetas(ReporteMetasViewModel filtro)
    {
        var consulta = contexto.MetasAhorro.AsNoTracking().Include(m => m.Aportes).AsQueryable();
        if (filtro.FechaObjetivoHasta.HasValue) consulta = consulta.Where(m => m.FechaObjetivo <= filtro.FechaObjetivoHasta);

        var metas = await consulta.ToListAsync();
        metas = filtro.Estado switch
        {
            "en-curso" => metas.Where(m => !m.Cumplida).ToList(),
            "cumplidas" => metas.Where(m => m.Cumplida).ToList(),
            _ => metas
        };

        filtro.Metas = metas.OrderByDescending(m => m.PorcentajeAvance).ToList();
        return View(filtro);
    }
}
