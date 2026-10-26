using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models.Enums;

namespace FinanzasPersonales.Models;

/// <summary>
/// DER: Usuario (1) — posee — (N) Billetera; Billetera (1) — registra — (N) Transacción.
/// </summary>
public class Billetera
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la billetera es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el tipo de billetera.")]
    [Display(Name = "Tipo de billetera")]
    public TipoBilletera Tipo { get; set; } = TipoBilletera.Efectivo;

    [Required(ErrorMessage = "Ingrese el saldo inicial (puede ser 0).")]
    [Range(0, 999999999.99, ErrorMessage = "El saldo inicial debe estar entre {1} y {2}.")]
    [Display(Name = "Saldo inicial (Bs)")]
    public decimal SaldoInicial { get; set; }

    [Required(ErrorMessage = "Seleccione el usuario dueño de la billetera.")]
    [Display(Name = "Usuario")]
    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

    /// <summary>Saldo inicial más ingresos menos gastos. Requiere cargar Transacciones con su Categoría.</summary>
    public decimal SaldoActual => SaldoInicial + Transacciones.Sum(t => t.MontoConSigno);
}
