using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanzasPersonales.Models.Enums;

namespace FinanzasPersonales.Models;

/// <summary>
/// DER: Transacción — se registra como — Préstamo/Deuda.
/// El desembolso y cada pago son transacciones vinculadas a este registro.
/// </summary>
public class PrestamoDeuda : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione si es un préstamo otorgado o una deuda.")]
    [Display(Name = "Tipo")]
    public TipoPrestamo Tipo { get; set; } = TipoPrestamo.PrestamoOtorgado;

    [Required(ErrorMessage = "Seleccione la persona involucrada.")]
    [Display(Name = "Persona")]
    public int PersonaId { get; set; }

    public Persona? Persona { get; set; }

    [Required(ErrorMessage = "Ingrese el monto.")]
    [Range(0.01, 999999999.99, ErrorMessage = "El monto debe ser mayor a 0.")]
    [Display(Name = "Monto (Bs)")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha de inicio.")]
    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime FechaInicio { get; set; } = DateTime.Today;

    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [StringLength(200, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

    // ----- Valores calculados (requieren cargar Transacciones con su Categoría) -----

    /// <summary>
    /// Tipo de categoría que cuenta como pago: si presté, los pagos que recibo son ingresos;
    /// si me prestaron, los pagos que hago son gastos.
    /// </summary>
    public TipoCategoria TipoDePago => Tipo == TipoPrestamo.PrestamoOtorgado ? TipoCategoria.Ingreso : TipoCategoria.Gasto;

    public decimal MontoPagado => Transacciones.Where(t => t.Categoria?.Tipo == TipoDePago).Sum(t => t.Monto);

    public decimal SaldoPendiente => Math.Max(Monto - MontoPagado, 0);

    public EstadoPrestamo Estado
    {
        get
        {
            if (SaldoPendiente == 0) return EstadoPrestamo.Saldado;
            if (FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today) return EstadoPrestamo.Vencido;
            return EstadoPrestamo.Pendiente;
        }
    }

    public string Resumen => $"{(Tipo == TipoPrestamo.PrestamoOtorgado ? "Préstamo a" : "Deuda con")} {Persona?.Nombre}";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaVencimiento.HasValue && FechaVencimiento.Value.Date < FechaInicio.Date)
        {
            yield return new ValidationResult(
                "La fecha de vencimiento no puede ser anterior a la fecha de inicio.",
                [nameof(FechaVencimiento)]);
        }
    }
}
