# Finanzas Personales

Aplicación web en **ASP.NET Core MVC** con **Entity Framework Core** y **SQL Server** para administrar las finanzas personales: billeteras (efectivo, cuentas, tarjetas), ingresos y gastos clasificados por categoría, préstamos otorgados y deudas con otras personas, y metas de ahorro. Incluye reportes con filtros y totales que ayudan a ver en qué se gasta, quién debe dinero y cuánto falta para cada meta.

## Funcionalidades

- **CRUD completo** de Usuarios, Billeteras, Categorías, Personas, Transacciones, Préstamos/Deudas y Metas de ahorro.
- Las relaciones muestran **nombres descriptivos** (no IDs) en listados y listas desplegables.
- **Validaciones** con mensajes en español: campos obligatorios, formatos, montos mayores a 0, fechas coherentes y datos únicos (correo, nombre de categoría, nombre de billetera por usuario).
- **Reglas de negocio**:
  - Un gasto no puede dejar una billetera con saldo negativo.
  - Un pago no puede superar el saldo pendiente del préstamo/deuda.
  - Los aportes a metas deben registrarse con una categoría de tipo Gasto.
  - No se puede eliminar un registro que tiene datos relacionados (por ejemplo, una billetera con transacciones). La aplicación explica el motivo.
- Saldo actual por billetera, estado calculado de cada préstamo (Pendiente / Vencido / Saldado) y avance de cada meta.
- Mensajes de confirmación después de cada operación.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server LocalDB**: se instala con Visual Studio (carga de trabajo "Desarrollo de ASP.NET y web"). También funciona con SQL Server Express o Developer cambiando la cadena de conexión.
- Opcional, para gestionar migraciones desde la terminal: `dotnet tool install --global dotnet-ef`

## Cómo ejecutar

```bash
git clone <url-del-repositorio>
cd <carpeta-del-repositorio>/FinanzasPersonales
dotnet run
```

Abrir `http://localhost:5104` en el navegador. En Visual Studio basta con abrir `FinanzasPersonales.slnx` y presionar **F5**.

## Base de datos

La cadena de conexión está en `FinanzasPersonales/appsettings.json` (clave `ConnectionStrings:FinanzasDb`). Por defecto usa LocalDB con autenticación de Windows, por lo que **no contiene usuarios ni contraseñas**:

```
Server=(localdb)\MSSQLLocalDB;Database=FinanzasPersonalesDb;Trusted_Connection=True;...
```

**Crear la base de datos:**

- **Automático:** al iniciar, la aplicación aplica las migraciones pendientes (`Database.Migrate()` en `Program.cs`). Crea la base `FinanzasPersonalesDb` con datos de ejemplo.
- **Manual con migraciones** (desde la carpeta `FinanzasPersonales`):
  ```bash
  dotnet ef database update
  ```
  En la Consola del Administrador de paquetes de Visual Studio: `Update-Database`.
- **Reiniciar con los datos de ejemplo:** `dotnet ef database drop --force` y luego `dotnet ef database update`.

La migración inicial está en `FinanzasPersonales/Data/Migrations/` e incluye datos de ejemplo (2 usuarios, 3 billeteras, 9 categorías, 2 personas, 1 préstamo, 1 deuda, 2 metas y 19 transacciones) para probar los reportes de inmediato.

## Reportes

### 1. Ingresos y gastos por categoría (`/Reportes/MovimientosPorCategoria`)
Agrupa las transacciones de un período por categoría y muestra cantidad, total, promedio y el porcentaje que representa cada categoría dentro de los ingresos o de los gastos, además del total de ingresos, el total de gastos y el balance.
- **Filtros:** rango de fechas (por defecto, los últimos 3 meses), usuario, billetera y tipo (ingreso/gasto).
- **Utilidad:** muestra en qué se va el dinero y de dónde viene, para decidir dónde recortar gastos y comprobar si el período cerró con ahorro o con déficit.

### 2. Préstamos y deudas pendientes (`/Reportes/PrestamosPendientes`)
Lista los préstamos otorgados y las deudas contraídas con su monto, lo pagado, el saldo pendiente, el vencimiento (días restantes o días de atraso) y el estado. Los vencidos aparecen primero.
- **Filtros:** tipo (préstamo/deuda), persona, estado (por defecto, pendientes y vencidos) y fecha de vencimiento máxima.
- **Resumen:** total por cobrar, total por pagar, cantidad de vencidos y totales de la tabla.
- **Utilidad:** saber quién debe dinero y a quién se le debe, y priorizar cobros y pagos según el vencimiento.

### 3. Progreso de metas de ahorro (`/Reportes/ProgresoMetas`), adicional
Compara lo ahorrado con el objetivo de cada meta, muestra el porcentaje de avance y calcula el **aporte mensual sugerido** para cumplir la meta en su fecha objetivo.
- **Filtros:** estado (en curso / cumplidas) y fecha objetivo máxima.

Los reportes obtienen sus datos de la base de datos (el reporte 1 agrupa y suma con `GROUP BY` en SQL Server) y se pueden imprimir con el botón **Imprimir**.

## DER

- **DER original:** `DER.pdf`.
- **Cambios durante la implementación:** no se agregaron ni se eliminaron entidades. Las 7 entidades y sus relaciones se implementaron así:

| Relación del DER | Implementación |
|---|---|
| Usuario **posee** Billetera | `Billetera.UsuarioId` (obligatoria), 1 a N |
| Billetera **registra** Transacción | `Transaccion.BilleteraId` (obligatoria), 1 a N |
| Categoría **clasifica** Transacción | `Transaccion.CategoriaId` (obligatoria), 1 a N |
| Persona **participa en** Transacción | `Transaccion.PersonaId` (opcional), 1 a N |
| Transacción **se registra como** Préstamo/Deuda | `Transaccion.PrestamoDeudaId` (opcional): el desembolso y cada pago son transacciones del préstamo |
| Meta de ahorro **recibe aportes de** Transacción | `Transaccion.MetaAhorroId` (opcional), 1 a N |
| Persona — Préstamo/Deuda (línea compartida en el DER) | `PrestamoDeuda.PersonaId` (obligatoria): la persona a quien se prestó o de quien se recibió el préstamo |

Las claves foráneas usan `ON DELETE RESTRICT` para no perder historial. Si el equipo considera que la relación directa Persona — Préstamo/Deuda es un cambio respecto al DER original, debe entregar un DER actualizado que la muestre explícitamente.

## Estructura del proyecto

```
FinanzasPersonales/
├── Controllers/     Un controlador por entidad + ReportesController y HomeController
├── Data/            FinanzasDbContext (relaciones y restricciones), DatosIniciales y Migrations/
├── Helpers/         Formato de moneda/fechas y notificaciones (TempData)
├── Models/          Entidades del DER y enumeraciones (Models/Enums)
├── Services/        ListasSeleccionService (listas desplegables reutilizables)
├── ViewModels/      Modelos de formularios, listados y reportes
├── Views/           Vistas Razor; cada entidad reutiliza un _Formulario.cshtml en Create/Edit
└── wwwroot/         CSS, JS y librerías (Bootstrap, jQuery Validation)
```

`GUIA_UBICACIONES.md` indica en qué archivo y línea está cada botón que guarda datos, cada modelo y la conexión con la base de datos.
