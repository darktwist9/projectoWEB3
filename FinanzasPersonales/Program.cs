using System.Globalization;
using FinanzasPersonales.Data;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Mensajes de validación del enlace de datos (model binding) en español
    var mensajes = options.ModelBindingMessageProvider;
    mensajes.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    mensajes.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor del campo {campo}.");
    mensajes.SetMissingKeyOrValueAccessor(() => "Falta un valor obligatorio.");
    mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
    mensajes.SetUnknownValueIsInvalidAccessor(campo => $"El valor ingresado en {campo} no es válido.");
    mensajes.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    mensajes.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
    mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
});

// Conexión con la base de datos: SQL Server, cadena "FinanzasDb" definida en appsettings.json
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanzasDb")));

builder.Services.AddScoped<IListasSeleccionService, ListasSeleccionService>();

var app = builder.Build();

// Aplica las migraciones pendientes al iniciar (crea la base de datos si no existe)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FinanzasDbContext>().Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Cultura de Bolivia con punto decimal, compatible con los campos numéricos del navegador
var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-BO").Clone();
cultura.NumberFormat.NumberDecimalSeparator = ".";
cultura.NumberFormat.NumberGroupSeparator = ",";
cultura.NumberFormat.CurrencyDecimalSeparator = ".";
cultura.NumberFormat.CurrencyGroupSeparator = ",";
var opcionesCultura = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
};
opcionesCultura.RequestCultureProviders.Clear();
app.UseRequestLocalization(opcionesCultura);

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
