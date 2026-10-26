using FinanzasPersonales.Models;
using FinanzasPersonales.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Data;

/// <summary>Datos de ejemplo que se insertan con la migración inicial para poder probar la aplicación y los reportes.</summary>
public static class DatosIniciales
{
    public static void Cargar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nombre = "Ana Pérez", Email = "ana.perez@ejemplo.com", FechaRegistro = new DateTime(2026, 7, 1) },
            new Usuario { Id = 2, Nombre = "Luis Rojas", Email = "luis.rojas@ejemplo.com", FechaRegistro = new DateTime(2026, 9, 15) });

        modelBuilder.Entity<Billetera>().HasData(
            new Billetera { Id = 1, Nombre = "Efectivo", Tipo = TipoBilletera.Efectivo, SaldoInicial = 2000, UsuarioId = 1 },
            new Billetera { Id = 2, Nombre = "Cuenta de ahorro", Tipo = TipoBilletera.CuentaBancaria, SaldoInicial = 3000, UsuarioId = 1 },
            new Billetera { Id = 3, Nombre = "Billetera digital", Tipo = TipoBilletera.BilleteraDigital, SaldoInicial = 800, UsuarioId = 2 });

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Salario", Tipo = TipoCategoria.Ingreso, Descripcion = "Sueldo mensual" },
            new Categoria { Id = 2, Nombre = "Ingresos extra", Tipo = TipoCategoria.Ingreso, Descripcion = "Ventas, trabajos ocasionales" },
            new Categoria { Id = 3, Nombre = "Préstamos y cobros", Tipo = TipoCategoria.Ingreso, Descripcion = "Dinero recibido en préstamo o cobros de préstamos otorgados" },
            new Categoria { Id = 4, Nombre = "Alimentación", Tipo = TipoCategoria.Gasto },
            new Categoria { Id = 5, Nombre = "Transporte", Tipo = TipoCategoria.Gasto },
            new Categoria { Id = 6, Nombre = "Servicios básicos", Tipo = TipoCategoria.Gasto, Descripcion = "Luz, agua, internet" },
            new Categoria { Id = 7, Nombre = "Entretenimiento", Tipo = TipoCategoria.Gasto },
            new Categoria { Id = 8, Nombre = "Préstamos y pagos de deuda", Tipo = TipoCategoria.Gasto, Descripcion = "Dinero prestado a otros o pagos de deudas" },
            new Categoria { Id = 9, Nombre = "Ahorro", Tipo = TipoCategoria.Gasto, Descripcion = "Aportes a metas de ahorro" });

        modelBuilder.Entity<Persona>().HasData(
            new Persona { Id = 1, Nombre = "Carlos Mendoza", Telefono = "70000001" },
            new Persona { Id = 2, Nombre = "María Quispe", Telefono = "70000002", Email = "maria.quispe@ejemplo.com" });

        modelBuilder.Entity<PrestamoDeuda>().HasData(
            new PrestamoDeuda
            {
                Id = 1, Tipo = TipoPrestamo.PrestamoOtorgado, PersonaId = 1, Monto = 1000,
                FechaInicio = new DateTime(2026, 8, 15), FechaVencimiento = new DateTime(2026, 10, 15),
                Descripcion = "Préstamo para reparación de moto"
            },
            new PrestamoDeuda
            {
                Id = 2, Tipo = TipoPrestamo.DeudaContraida, PersonaId = 2, Monto = 1500,
                FechaInicio = new DateTime(2026, 9, 1), FechaVencimiento = new DateTime(2026, 12, 31),
                Descripcion = "Préstamo para compra de laptop"
            });

        modelBuilder.Entity<MetaAhorro>().HasData(
            new MetaAhorro { Id = 1, Nombre = "Fondo de emergencia", MontoObjetivo = 5000, FechaObjetivo = new DateTime(2026, 12, 31) },
            new MetaAhorro { Id = 2, Nombre = "Viaje de vacaciones", MontoObjetivo = 3000, FechaObjetivo = new DateTime(2027, 6, 30) });

        modelBuilder.Entity<Transaccion>().HasData(
            new Transaccion { Id = 1, BilleteraId = 2, CategoriaId = 1, Monto = 4500, Fecha = new DateTime(2026, 8, 1), Descripcion = "Sueldo de agosto" },
            new Transaccion { Id = 2, BilleteraId = 1, CategoriaId = 4, Monto = 350, Fecha = new DateTime(2026, 8, 5), Descripcion = "Compras del mercado" },
            new Transaccion { Id = 3, BilleteraId = 1, CategoriaId = 5, Monto = 120, Fecha = new DateTime(2026, 8, 10), Descripcion = "Pasajes" },
            new Transaccion { Id = 4, BilleteraId = 2, CategoriaId = 8, Monto = 1000, Fecha = new DateTime(2026, 8, 15), Descripcion = "Préstamo a Carlos", PersonaId = 1, PrestamoDeudaId = 1 },
            new Transaccion { Id = 5, BilleteraId = 2, CategoriaId = 6, Monto = 280, Fecha = new DateTime(2026, 8, 20), Descripcion = "Luz e internet" },
            new Transaccion { Id = 6, BilleteraId = 2, CategoriaId = 9, Monto = 600, Fecha = new DateTime(2026, 8, 31), Descripcion = "Aporte mensual", MetaAhorroId = 1 },
            new Transaccion { Id = 7, BilleteraId = 2, CategoriaId = 1, Monto = 4500, Fecha = new DateTime(2026, 9, 1), Descripcion = "Sueldo de septiembre" },
            new Transaccion { Id = 8, BilleteraId = 2, CategoriaId = 3, Monto = 1500, Fecha = new DateTime(2026, 9, 1), Descripcion = "Préstamo recibido de María", PersonaId = 2, PrestamoDeudaId = 2 },
            new Transaccion { Id = 9, BilleteraId = 1, CategoriaId = 4, Monto = 420, Fecha = new DateTime(2026, 9, 7), Descripcion = "Compras del mercado" },
            new Transaccion { Id = 10, BilleteraId = 1, CategoriaId = 7, Monto = 200, Fecha = new DateTime(2026, 9, 14), Descripcion = "Cine y cena" },
            new Transaccion { Id = 11, BilleteraId = 2, CategoriaId = 3, Monto = 400, Fecha = new DateTime(2026, 9, 20), Descripcion = "Abono de Carlos", PersonaId = 1, PrestamoDeudaId = 1 },
            new Transaccion { Id = 12, BilleteraId = 2, CategoriaId = 9, Monto = 800, Fecha = new DateTime(2026, 9, 30), Descripcion = "Aporte mensual", MetaAhorroId = 1 },
            new Transaccion { Id = 13, BilleteraId = 2, CategoriaId = 1, Monto = 4500, Fecha = new DateTime(2026, 10, 1), Descripcion = "Sueldo de octubre" },
            new Transaccion { Id = 14, BilleteraId = 2, CategoriaId = 8, Monto = 500, Fecha = new DateTime(2026, 10, 5), Descripcion = "Primer pago a María", PersonaId = 2, PrestamoDeudaId = 2 },
            new Transaccion { Id = 15, BilleteraId = 1, CategoriaId = 4, Monto = 390, Fecha = new DateTime(2026, 10, 8), Descripcion = "Compras del mercado" },
            new Transaccion { Id = 16, BilleteraId = 2, CategoriaId = 9, Monto = 300, Fecha = new DateTime(2026, 10, 10), Descripcion = "Ahorro para viaje", MetaAhorroId = 2 },
            new Transaccion { Id = 17, BilleteraId = 3, CategoriaId = 2, Monto = 1200, Fecha = new DateTime(2026, 10, 12), Descripcion = "Venta de artículos usados" },
            new Transaccion { Id = 18, BilleteraId = 3, CategoriaId = 5, Monto = 150, Fecha = new DateTime(2026, 10, 15), Descripcion = "Taxi y pasajes" },
            new Transaccion { Id = 19, BilleteraId = 3, CategoriaId = 4, Monto = 260, Fecha = new DateTime(2026, 10, 18), Descripcion = "Supermercado" });
    }
}
