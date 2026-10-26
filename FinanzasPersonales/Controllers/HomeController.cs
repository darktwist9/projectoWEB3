using System.Diagnostics;
using FinanzasPersonales.Data;
using FinanzasPersonales.Models;
using FinanzasPersonales.Models.Enums;
using FinanzasPersonales.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Controllers;

public class HomeController(FinanzasDbContext contexto) : Controller
{
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var transaccionesMes = contexto.Transacciones.Where(t => t.Fecha >= inicioMes && t.Fecha <= hoy);
        var prestamos = await contexto.PrestamosDeudas
            .AsNoTracking()
            .Include(p => p.Transacciones).ThenInclude(t => t.Categoria)
            .ToListAsync();

        var modelo = new InicioViewModel
        {
            SaldoTotal = await contexto.Billeteras.SumAsync(b => b.SaldoInicial)
                + await contexto.Transacciones.SumAsync(t => t.Categoria!.Tipo == TipoCategoria.Ingreso ? t.Monto : -t.Monto),
            IngresosMes = await transaccionesMes.Where(t => t.Categoria!.Tipo == TipoCategoria.Ingreso).SumAsync(t => t.Monto),
            GastosMes = await transaccionesMes.Where(t => t.Categoria!.Tipo == TipoCategoria.Gasto).SumAsync(t => t.Monto),
            PorCobrar = prestamos.Where(p => p.Tipo == TipoPrestamo.PrestamoOtorgado).Sum(p => p.SaldoPendiente),
            PorPagar = prestamos.Where(p => p.Tipo == TipoPrestamo.DeudaContraida).Sum(p => p.SaldoPendiente),
            UltimasTransacciones = await contexto.Transacciones
                .AsNoTracking()
                .Include(t => t.Billetera)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Id)
                .Take(5)
                .ToListAsync()
        };

        return View(modelo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
