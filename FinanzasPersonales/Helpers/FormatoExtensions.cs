using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FinanzasPersonales.Helpers;

/// <summary>Formatos comunes usados en las vistas.</summary>
public static class FormatoExtensions
{
    public static string ToMoneda(this decimal valor) => $"Bs {valor:N2}";

    public static string ToFecha(this DateTime fecha) => fecha.ToString("dd/MM/yyyy");

    public static string ToFecha(this DateTime? fecha) => fecha?.ToString("dd/MM/yyyy") ?? "—";

    /// <summary>Devuelve el texto del atributo [Display(Name = ...)] de un valor de enumeración.</summary>
    public static string ObtenerNombre(this Enum valor) =>
        valor.GetType().GetMember(valor.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? valor.ToString();
}
