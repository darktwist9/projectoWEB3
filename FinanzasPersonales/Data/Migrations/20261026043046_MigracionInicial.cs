using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanzasPersonales.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetasAhorro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MontoObjetivo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaObjetivo = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasAhorro", x => x.Id);
                    table.CheckConstraint("CK_MetasAhorro_MontoObjetivo", "[MontoObjetivo] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrestamosDeudas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "date", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrestamosDeudas", x => x.Id);
                    table.CheckConstraint("CK_PrestamosDeudas_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_PrestamosDeudas_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Billeteras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billeteras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billeteras_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BilleteraId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: true),
                    PrestamoDeudaId = table.Column<int>(type: "int", nullable: true),
                    MetaAhorroId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.Id);
                    table.CheckConstraint("CK_Transacciones_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_Transacciones_Billeteras_BilleteraId",
                        column: x => x.BilleteraId,
                        principalTable: "Billeteras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_MetasAhorro_MetaAhorroId",
                        column: x => x.MetaAhorroId,
                        principalTable: "MetasAhorro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_PrestamosDeudas_PrestamoDeudaId",
                        column: x => x.PrestamoDeudaId,
                        principalTable: "PrestamosDeudas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre", "Tipo" },
                values: new object[,]
                {
                    { 1, "Sueldo mensual", "Salario", 1 },
                    { 2, "Ventas, trabajos ocasionales", "Ingresos extra", 1 },
                    { 3, "Dinero recibido en préstamo o cobros de préstamos otorgados", "Préstamos y cobros", 1 },
                    { 4, null, "Alimentación", 2 },
                    { 5, null, "Transporte", 2 },
                    { 6, "Luz, agua, internet", "Servicios básicos", 2 },
                    { 7, null, "Entretenimiento", 2 },
                    { 8, "Dinero prestado a otros o pagos de deudas", "Préstamos y pagos de deuda", 2 },
                    { 9, "Aportes a metas de ahorro", "Ahorro", 2 }
                });

            migrationBuilder.InsertData(
                table: "MetasAhorro",
                columns: new[] { "Id", "Descripcion", "FechaObjetivo", "MontoObjetivo", "Nombre" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5000m, "Fondo de emergencia" },
                    { 2, null, new DateTime(2027, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 3000m, "Viaje de vacaciones" }
                });

            migrationBuilder.InsertData(
                table: "Personas",
                columns: new[] { "Id", "Email", "Nombre", "Notas", "Telefono" },
                values: new object[,]
                {
                    { 1, null, "Carlos Mendoza", null, "70000001" },
                    { 2, "maria.quispe@ejemplo.com", "María Quispe", null, "70000002" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaRegistro", "Nombre" },
                values: new object[,]
                {
                    { 1, "ana.perez@ejemplo.com", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ana Pérez" },
                    { 2, "luis.rojas@ejemplo.com", new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Luis Rojas" }
                });

            migrationBuilder.InsertData(
                table: "Billeteras",
                columns: new[] { "Id", "Nombre", "SaldoInicial", "Tipo", "UsuarioId" },
                values: new object[,]
                {
                    { 1, "Efectivo", 2000m, 1, 1 },
                    { 2, "Cuenta de ahorro", 3000m, 2, 1 },
                    { 3, "Billetera digital", 800m, 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "PrestamosDeudas",
                columns: new[] { "Id", "Descripcion", "FechaInicio", "FechaVencimiento", "Monto", "PersonaId", "Tipo" },
                values: new object[,]
                {
                    { 1, "Préstamo para reparación de moto", new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1000m, 1, 1 },
                    { 2, "Préstamo para compra de laptop", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1500m, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Transacciones",
                columns: new[] { "Id", "BilleteraId", "CategoriaId", "Descripcion", "Fecha", "MetaAhorroId", "Monto", "PersonaId", "PrestamoDeudaId" },
                values: new object[,]
                {
                    { 1, 2, 1, "Sueldo de agosto", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4500m, null, null },
                    { 2, 1, 4, "Compras del mercado", new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 350m, null, null },
                    { 3, 1, 5, "Pasajes", new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 120m, null, null },
                    { 4, 2, 8, "Préstamo a Carlos", new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1000m, 1, 1 },
                    { 5, 2, 6, "Luz e internet", new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 280m, null, null },
                    { 6, 2, 9, "Aporte mensual", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 600m, null, null },
                    { 7, 2, 1, "Sueldo de septiembre", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4500m, null, null },
                    { 8, 2, 3, "Préstamo recibido de María", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1500m, 2, 2 },
                    { 9, 1, 4, "Compras del mercado", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 420m, null, null },
                    { 10, 1, 7, "Cine y cena", new DateTime(2026, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 200m, null, null },
                    { 11, 2, 3, "Abono de Carlos", new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 400m, 1, 1 },
                    { 12, 2, 9, "Aporte mensual", new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 800m, null, null },
                    { 13, 2, 1, "Sueldo de octubre", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 4500m, null, null },
                    { 14, 2, 8, "Primer pago a María", new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 500m, 2, 2 },
                    { 15, 1, 4, "Compras del mercado", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 390m, null, null },
                    { 16, 2, 9, "Ahorro para viaje", new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 300m, null, null },
                    { 17, 3, 2, "Venta de artículos usados", new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1200m, null, null },
                    { 18, 3, 5, "Taxi y pasajes", new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 150m, null, null },
                    { 19, 3, 4, "Supermercado", new DateTime(2026, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 260m, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billeteras_UsuarioId_Nombre",
                table: "Billeteras",
                columns: new[] { "UsuarioId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre_Tipo",
                table: "Categorias",
                columns: new[] { "Nombre", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetasAhorro_Nombre",
                table: "MetasAhorro",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrestamosDeudas_PersonaId",
                table: "PrestamosDeudas",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_BilleteraId",
                table: "Transacciones",
                column: "BilleteraId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_CategoriaId",
                table: "Transacciones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_MetaAhorroId",
                table: "Transacciones",
                column: "MetaAhorroId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_PersonaId",
                table: "Transacciones",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_PrestamoDeudaId",
                table: "Transacciones",
                column: "PrestamoDeudaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "Billeteras");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "MetasAhorro");

            migrationBuilder.DropTable(
                name: "PrestamosDeudas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}
