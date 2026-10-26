using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Models;

/// <summary>
/// Persona externa con la que el usuario tiene movimientos (DER: Persona (1) — participa en — (N) Transacción)
/// y préstamos o deudas (Persona (1) — (N) Préstamo/Deuda).
/// </summary>
public class Persona
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los {1} caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los {1} caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [StringLength(250, ErrorMessage = "Las notas no pueden superar los {1} caracteres.")]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

    public ICollection<PrestamoDeuda> PrestamosDeudas { get; set; } = new List<PrestamoDeuda>();
}
