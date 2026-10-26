using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Helpers;

/// <summary>
/// Mensajes de confirmación o error que sobreviven a una redirección (TempData).
/// Se muestran en Views/Shared/_Notificaciones.cshtml.
/// </summary>
public static class Notificaciones
{
    public const string ClaveExito = "NotificacionExito";
    public const string ClaveError = "NotificacionError";

    public static void NotificarExito(this Controller controlador, string mensaje) =>
        controlador.TempData[ClaveExito] = mensaje;

    public static void NotificarError(this Controller controlador, string mensaje) =>
        controlador.TempData[ClaveError] = mensaje;
}
