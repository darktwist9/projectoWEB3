# Guía de ubicaciones: Finanzas Personales (ASP.NET Core MVC)

Mapa rápido del código para encontrar **dónde se guardan los datos**, **qué botón dispara cada INSERT**, **los modelos importantes** y **la conexión con la base de datos**.
Todas las rutas son relativas a la raíz del repositorio y los enlaces abren el archivo en la línea indicada.

---

## 1. Conexión con la base de datos

| Qué | Dónde | Detalle |
|---|---|---|
| **Cadena de conexión** | [appsettings.json:3](FinanzasPersonales/appsettings.json#L3) | Clave `ConnectionStrings:FinanzasDb`. SQL Server LocalDB, base `FinanzasPersonalesDb`, autenticación de Windows (sin contraseñas). |
| **Registro del DbContext** | [Program.cs:26-27](FinanzasPersonales/Program.cs#L26-L27) | `AddDbContext<FinanzasDbContext>(... UseSqlServer(GetConnectionString("FinanzasDb")))`. Aquí se conecta EF Core con SQL Server. |
| **Creación automática de la BD** | [Program.cs:36](FinanzasPersonales/Program.cs#L36) | `Database.Migrate()` aplica las migraciones pendientes al arrancar. |
| **DbContext (tablas y relaciones)** | [Data/FinanzasDbContext.cs](FinanzasPersonales/Data/FinanzasDbContext.cs) | `DbSet` de las 7 tablas ([líneas 12-18](FinanzasPersonales/Data/FinanzasDbContext.cs#L12-L18)), relaciones del DER con `DeleteBehavior.Restrict`, índices únicos y restricciones `CHECK`. |
| **Datos iniciales (semilla)** | [Data/DatosIniciales.cs](FinanzasPersonales/Data/DatosIniciales.cs) | Usuarios, billeteras, categorías, personas, préstamos, metas y 19 transacciones de ejemplo (`HasData`). |
| **Migraciones** | [Data/Migrations/](FinanzasPersonales/Data/Migrations/) | `MigracionInicial`: el script que crea todas las tablas. |

**Cómo llega un dato a la BD (flujo MVC):**

```
Vista (botón "Guardar ...", type="submit")
   └─► POST al Controlador (acción Create con [HttpPost] y [ValidateAntiForgeryToken])
         ├─ Validaciones: atributos del modelo + reglas de negocio (ModelState)
         ├─ contexto.<Tabla>.Add(entidad)
         └─ await contexto.SaveChangesAsync()   ──►  INSERT en SQL Server
   ◄── Redirección + mensaje de confirmación (TempData → _Notificaciones.cshtml)
```

El controlador recibe el `FinanzasDbContext` por inyección de dependencias en su constructor, por ejemplo [UsuariosController.cs:9](FinanzasPersonales/Controllers/UsuariosController.cs#L9).

---

## 2. Botones que AGREGAN datos a la base de datos (INSERT)

Cada fila es un botón **"Guardar ..."** que envía el formulario. La acción POST valida, ejecuta el `Add` y luego `SaveChangesAsync()`.

| Tabla | Botón | Vista (botón) | Acción POST que lo recibe | `Add` + `SaveChangesAsync` | URL del formulario |
|---|---|---|---|---|---|
| Usuarios | **Guardar usuario** | [Usuarios/Create.cshtml:11](FinanzasPersonales/Views/Usuarios/Create.cshtml#L11) | [UsuariosController.cs:25](FinanzasPersonales/Controllers/UsuariosController.cs#L25) | [líneas 30-31](FinanzasPersonales/Controllers/UsuariosController.cs#L30-L31) | `/Usuarios/Create` |
| Billeteras | **Guardar billetera** | [Billeteras/Create.cshtml:11](FinanzasPersonales/Views/Billeteras/Create.cshtml#L11) | [BilleterasController.cs:49](FinanzasPersonales/Controllers/BilleterasController.cs#L49) | [líneas 56-57](FinanzasPersonales/Controllers/BilleterasController.cs#L56-L57) | `/Billeteras/Create` |
| Categorias | **Guardar categoría** | [Categorias/Create.cshtml:11](FinanzasPersonales/Views/Categorias/Create.cshtml#L11) | [CategoriasController.cs:25](FinanzasPersonales/Controllers/CategoriasController.cs#L25) | [líneas 30-31](FinanzasPersonales/Controllers/CategoriasController.cs#L30-L31) | `/Categorias/Create` |
| Personas | **Guardar persona** | [Personas/Create.cshtml:11](FinanzasPersonales/Views/Personas/Create.cshtml#L11) | [PersonasController.cs:32](FinanzasPersonales/Controllers/PersonasController.cs#L32) | [líneas 36-37](FinanzasPersonales/Controllers/PersonasController.cs#L36-L37) | `/Personas/Create` |
| Transacciones | **Guardar transacción** | [Transacciones/Create.cshtml:11](FinanzasPersonales/Views/Transacciones/Create.cshtml#L11) | [TransaccionesController.cs:64](FinanzasPersonales/Controllers/TransaccionesController.cs#L64) | [líneas 71-72](FinanzasPersonales/Controllers/TransaccionesController.cs#L71-L72) | `/Transacciones/Create` |
| PrestamosDeudas | **Guardar préstamo / deuda** | [PrestamosDeudas/Create.cshtml:11](FinanzasPersonales/Views/PrestamosDeudas/Create.cshtml#L11) | [PrestamosDeudasController.cs:41](FinanzasPersonales/Controllers/PrestamosDeudasController.cs#L41) | [líneas 48-49](FinanzasPersonales/Controllers/PrestamosDeudasController.cs#L48-L49) | `/PrestamosDeudas/Create` |
| MetasAhorro | **Guardar meta** | [MetasAhorro/Create.cshtml:11](FinanzasPersonales/Views/MetasAhorro/Create.cshtml#L11) | [MetasAhorroController.cs:35](FinanzasPersonales/Controllers/MetasAhorroController.cs#L35) | [líneas 40-41](FinanzasPersonales/Controllers/MetasAhorroController.cs#L40-L41) | `/MetasAhorro/Create` |

Los campos de cada formulario están en el parcial `_Formulario.cshtml` de su carpeta (por ejemplo [Transacciones/_Formulario.cshtml](FinanzasPersonales/Views/Transacciones/_Formulario.cshtml)). `Create` y `Edit` lo reutilizan para no duplicar código.

### 2.1 Botones que ABREN esos formularios de alta

No guardan nada por sí mismos: llevan al formulario y, en algunos casos, lo dejan **prellenado**.

| Botón | Dónde está | Abre |
|---|---|---|
| **Nueva transacción** (barra de navegación, visible en todas las páginas) | [Shared/_Layout.cshtml:63](FinanzasPersonales/Views/Shared/_Layout.cshtml#L63) | Formulario de transacción |
| **Registrar transacción** (inicio) | [Home/Index.cshtml:11](FinanzasPersonales/Views/Home/Index.cshtml#L11) | Formulario de transacción |
| **Nueva transacción** | [Transacciones/Index.cshtml:8](FinanzasPersonales/Views/Transacciones/Index.cshtml#L8) | Formulario de transacción |
| **+** (por fila) y **Registrar transacción** | [Billeteras/Index.cshtml:57](FinanzasPersonales/Views/Billeteras/Index.cshtml#L57), [Billeteras/Details.cshtml:12](FinanzasPersonales/Views/Billeteras/Details.cshtml#L12) | Transacción con la billetera ya elegida |
| **Registrar desembolso** | [PrestamosDeudas/Details.cshtml:15](FinanzasPersonales/Views/PrestamosDeudas/Details.cshtml#L15) | Transacción vinculada al préstamo, con persona y monto completos |
| **Registrar cobro / pago** | [PrestamosDeudas/Details.cshtml:21](FinanzasPersonales/Views/PrestamosDeudas/Details.cshtml#L21) | Transacción vinculada al préstamo, con el saldo pendiente como monto |
| **Aportar** y **Registrar aporte** | [MetasAhorro/Index.cshtml:41](FinanzasPersonales/Views/MetasAhorro/Index.cshtml#L41), [MetasAhorro/Details.cshtml:12](FinanzasPersonales/Views/MetasAhorro/Details.cshtml#L12) | Transacción vinculada a la meta |
| **Nuevo usuario** | [Usuarios/Index.cshtml:8](FinanzasPersonales/Views/Usuarios/Index.cshtml#L8) | Formulario de usuario |
| Ícono de billetera (por usuario) | [Usuarios/Index.cshtml:42](FinanzasPersonales/Views/Usuarios/Index.cshtml#L42) | Billetera con el usuario ya elegido |
| **Nueva billetera** | [Billeteras/Index.cshtml:10](FinanzasPersonales/Views/Billeteras/Index.cshtml#L10) | Formulario de billetera |
| **Nueva categoría** | [Categorias/Index.cshtml:9](FinanzasPersonales/Views/Categorias/Index.cshtml#L9) | Formulario de categoría |
| **Nueva persona** | [Personas/Index.cshtml:8](FinanzasPersonales/Views/Personas/Index.cshtml#L8) | Formulario de persona |
| Ícono de dinero (por persona) | [Personas/Index.cshtml:52](FinanzasPersonales/Views/Personas/Index.cshtml#L52) | Préstamo/deuda con la persona ya elegida |
| **Nuevo préstamo / deuda** | [PrestamosDeudas/Index.cshtml:10](FinanzasPersonales/Views/PrestamosDeudas/Index.cshtml#L10) | Formulario de préstamo/deuda |
| **Nueva meta** | [MetasAhorro/Index.cshtml:10](FinanzasPersonales/Views/MetasAhorro/Index.cshtml#L10) | Formulario de meta |

### 2.2 Botones que MODIFICAN o ELIMINAN (UPDATE / DELETE)

| Tabla | Botón "Guardar cambios" (UPDATE) | Acción `Edit` POST | Acción `DeleteConfirmed` (DELETE) |
|---|---|---|---|
| Usuarios | [Usuarios/Edit.cshtml:12](FinanzasPersonales/Views/Usuarios/Edit.cshtml#L12) | [UsuariosController.cs:44](FinanzasPersonales/Controllers/UsuariosController.cs#L44) | [UsuariosController.cs:68](FinanzasPersonales/Controllers/UsuariosController.cs#L68) |
| Billeteras | [Billeteras/Edit.cshtml:12](FinanzasPersonales/Views/Billeteras/Edit.cshtml#L12) | [BilleterasController.cs:72](FinanzasPersonales/Controllers/BilleterasController.cs#L72) | [BilleterasController.cs:99](FinanzasPersonales/Controllers/BilleterasController.cs#L99) |
| Categorias | [Categorias/Edit.cshtml:12](FinanzasPersonales/Views/Categorias/Edit.cshtml#L12) | [CategoriasController.cs:44](FinanzasPersonales/Controllers/CategoriasController.cs#L44) | [CategoriasController.cs:77](FinanzasPersonales/Controllers/CategoriasController.cs#L77) |
| Personas | [Personas/Edit.cshtml:12](FinanzasPersonales/Views/Personas/Edit.cshtml#L12) | [PersonasController.cs:50](FinanzasPersonales/Controllers/PersonasController.cs#L50) | [PersonasController.cs:72](FinanzasPersonales/Controllers/PersonasController.cs#L72) |
| Transacciones | [Transacciones/Edit.cshtml:12](FinanzasPersonales/Views/Transacciones/Edit.cshtml#L12) | [TransaccionesController.cs:87](FinanzasPersonales/Controllers/TransaccionesController.cs#L87) | [TransaccionesController.cs:119](FinanzasPersonales/Controllers/TransaccionesController.cs#L119) |
| PrestamosDeudas | [PrestamosDeudas/Edit.cshtml:12](FinanzasPersonales/Views/PrestamosDeudas/Edit.cshtml#L12) | [PrestamosDeudasController.cs:64](FinanzasPersonales/Controllers/PrestamosDeudasController.cs#L64) | [PrestamosDeudasController.cs:108](FinanzasPersonales/Controllers/PrestamosDeudasController.cs#L108) |
| MetasAhorro | [MetasAhorro/Edit.cshtml:12](FinanzasPersonales/Views/MetasAhorro/Edit.cshtml#L12) | [MetasAhorroController.cs:54](FinanzasPersonales/Controllers/MetasAhorroController.cs#L54) | [MetasAhorroController.cs:78](FinanzasPersonales/Controllers/MetasAhorroController.cs#L78) |

El botón **"Sí, eliminar"** es único y compartido por todas las vistas `Delete`: [Shared/_ConfirmarEliminacion.cshtml:18](FinanzasPersonales/Views/Shared/_ConfirmarEliminacion.cshtml#L18). Si el registro tiene datos relacionados, el botón no aparece y en su lugar se muestra el motivo del bloqueo.

---

## 3. Modelos importantes

### 3.1 Entidades del DER (`Models/`)

| Entidad (tabla) | Archivo | Relaciones (DER) | Validaciones y lógica destacada |
|---|---|---|---|
| **Usuario** (`Usuarios`) | [Models/Usuario.cs](FinanzasPersonales/Models/Usuario.cs) | 1 → N Billetera (*posee*) | Nombre y correo obligatorios, formato de correo, **correo único**. |
| **Billetera** (`Billeteras`) | [Models/Billetera.cs](FinanzasPersonales/Models/Billetera.cs) | N → 1 Usuario · 1 → N Transacción (*registra*) | Saldo inicial ≥ 0, nombre único por usuario. Calcula `SaldoActual` ([línea 36](FinanzasPersonales/Models/Billetera.cs#L36)). |
| **Categoria** (`Categorias`) | [Models/Categoria.cs](FinanzasPersonales/Models/Categoria.cs) | 1 → N Transacción (*clasifica*) | `Tipo` Ingreso/Gasto define si la transacción suma o resta. Nombre + tipo únicos. |
| **Transaccion** (`Transacciones`): entidad central | [Models/Transaccion.cs](FinanzasPersonales/Models/Transaccion.cs) | N → 1 Billetera · N → 1 Categoría · N → 0..1 Persona (*participa en*) · N → 0..1 Préstamo/Deuda (*se registra como*) · N → 0..1 Meta (*recibe aportes de*) | Monto > 0, fecha no futura, no puede ser préstamo y aporte a la vez ([`Validate`, línea 61](FinanzasPersonales/Models/Transaccion.cs#L61)). `MontoConSigno` ([línea 59](FinanzasPersonales/Models/Transaccion.cs#L59)). |
| **Persona** (`Personas`) | [Models/Persona.cs](FinanzasPersonales/Models/Persona.cs) | 1 → N Transacción · 1 → N Préstamo/Deuda | Teléfono y correo con formato válido. |
| **PrestamoDeuda** (`PrestamosDeudas`) | [Models/PrestamoDeuda.cs](FinanzasPersonales/Models/PrestamoDeuda.cs) | N → 1 Persona · 1 → N Transacción | Vencimiento ≥ inicio ([`Validate`, línea 71](FinanzasPersonales/Models/PrestamoDeuda.cs#L71)). Calcula `MontoPagado`, `SaldoPendiente` y `Estado` (Pendiente/Vencido/Saldado) ([líneas 55-67](FinanzasPersonales/Models/PrestamoDeuda.cs#L55-L67)). |
| **MetaAhorro** (`MetasAhorro`) | [Models/MetaAhorro.cs](FinanzasPersonales/Models/MetaAhorro.cs) | 1 → N Transacción (`Aportes`) | Monto objetivo > 0, nombre único. Calcula `MontoAhorrado`, `PorcentajeAvance` y `AporteMensualSugerido`. |

Enumeraciones en [Models/Enums/](FinanzasPersonales/Models/Enums/): `TipoCategoria`, `TipoBilletera`, `TipoPrestamo`, `EstadoPrestamo`.

### 3.2 ViewModels (`ViewModels/`)

| ViewModel | Uso |
|---|---|
| [BilleteraFormViewModel](FinanzasPersonales/ViewModels/BilleteraFormViewModel.cs), [TransaccionFormViewModel](FinanzasPersonales/ViewModels/TransaccionFormViewModel.cs), [PrestamoDeudaFormViewModel](FinanzasPersonales/ViewModels/PrestamoDeudaFormViewModel.cs) | Formularios con listas desplegables: muestran **nombres** en lugar de IDs. |
| [TransaccionesIndexViewModel](FinanzasPersonales/ViewModels/TransaccionesIndexViewModel.cs) | Listado de transacciones con filtros y totales. |
| [InicioViewModel](FinanzasPersonales/ViewModels/InicioViewModel.cs) | Resumen de la página de inicio. |
| [Reportes/ReporteCategoriasViewModel](FinanzasPersonales/ViewModels/Reportes/ReporteCategoriasViewModel.cs), [Reportes/ReportePrestamosViewModel](FinanzasPersonales/ViewModels/Reportes/ReportePrestamosViewModel.cs), [Reportes/ReporteMetasViewModel](FinanzasPersonales/ViewModels/Reportes/ReporteMetasViewModel.cs) | Filtros, filas y totales de cada reporte. |

### 3.3 Servicios y helpers

| Archivo | Para qué sirve |
|---|---|
| [Services/ListasSeleccionService.cs](FinanzasPersonales/Services/ListasSeleccionService.cs) | Arma las listas desplegables (usuarios, billeteras, categorías agrupadas por tipo, etc.). Se registra en [Program.cs:29](FinanzasPersonales/Program.cs#L29). |
| [Helpers/FormatoExtensions.cs](FinanzasPersonales/Helpers/FormatoExtensions.cs) | `ToMoneda()` (Bs 1,234.50), `ToFecha()` (dd/MM/yyyy), `ObtenerNombre()` para enums. |
| [Helpers/Notificaciones.cs](FinanzasPersonales/Helpers/Notificaciones.cs) | `NotificarExito` / `NotificarError`: mensajes de confirmación que muestra [Shared/_Notificaciones.cshtml](FinanzasPersonales/Views/Shared/_Notificaciones.cshtml). |

---

## 4. Validaciones de negocio (en los controladores)

| Regla | Ubicación |
|---|---|
| Un gasto no puede superar el saldo de la billetera · un pago no puede superar el saldo del préstamo · la persona debe coincidir con la del préstamo · los aportes a metas deben ser de tipo Gasto | [TransaccionesController.cs:131](FinanzasPersonales/Controllers/TransaccionesController.cs#L131) (`ValidarReglasDeNegocioAsync`) |
| Correo de usuario único | [UsuariosController.cs:86](FinanzasPersonales/Controllers/UsuariosController.cs#L86) |
| Nombre de billetera único por usuario | [BilleterasController.cs:117](FinanzasPersonales/Controllers/BilleterasController.cs#L117) |
| Categoría única · no cambiar el tipo si ya tiene transacciones | [CategoriasController.cs:44](FinanzasPersonales/Controllers/CategoriasController.cs#L44), [línea 95](FinanzasPersonales/Controllers/CategoriasController.cs#L95) |
| No cambiar tipo/persona de un préstamo con transacciones · el monto no puede ser menor a lo pagado | [PrestamosDeudasController.cs:64](FinanzasPersonales/Controllers/PrestamosDeudasController.cs#L64) |
| No eliminar registros con datos relacionados | Método `MotivoBloqueo` al final de cada controlador |
| Mensajes de error del model binding en español | [Program.cs:9-23](FinanzasPersonales/Program.cs#L9-L23) |

---

## 5. Reportes

| Reporte | Acción | Vista | URL |
|---|---|---|---|
| Ingresos y gastos por categoría | [ReportesController.cs:15](FinanzasPersonales/Controllers/ReportesController.cs#L15) | [Reportes/MovimientosPorCategoria.cshtml](FinanzasPersonales/Views/Reportes/MovimientosPorCategoria.cshtml) | `/Reportes/MovimientosPorCategoria` |
| Préstamos y deudas pendientes | [ReportesController.cs:54](FinanzasPersonales/Controllers/ReportesController.cs#L54) | [Reportes/PrestamosPendientes.cshtml](FinanzasPersonales/Views/Reportes/PrestamosPendientes.cshtml) | `/Reportes/PrestamosPendientes` |
| Progreso de metas de ahorro (adicional) | [ReportesController.cs:83](FinanzasPersonales/Controllers/ReportesController.cs#L83) | [Reportes/ProgresoMetas.cshtml](FinanzasPersonales/Views/Reportes/ProgresoMetas.cshtml) | `/Reportes/ProgresoMetas` |

> Los números de línea corresponden a la versión actual del código. Si se edita un archivo, pueden moverse unas líneas.
