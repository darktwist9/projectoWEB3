using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanzasPersonales.Models.Enums;

namespace FinanzasPersonales.Models;

/// <summary>
/// Entidad central del DER. Toda transacción pertenece a una Billetera y a una Categoría.
/// Opcionalmente participa una Persona, se registra como parte de un Préstamo/Deuda
/// o es un aporte a una Meta de ahorro.
/// </summary>
public class Transaccion : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el monto.")]
    [Range(0.01, 999999999.99, ErrorMessage = "El monto debe ser mayor a 0.")]
    [Display(Name = "Monto (Bs)")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha.")]
    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Seleccione la billetera.")]
    [Display(Name = "Billetera")]
    public int BilleteraId { get; set; }

    public Billetera? Billetera { get; set; }

    [Required(ErrorMessage = "Seleccione la categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    [Display(Name = "Persona")]
    public int? PersonaId { get; set; }

    public Persona? Persona { get; set; }

    [Display(Name = "Préstamo / Deuda")]
    public int? PrestamoDeudaId { get; set; }

    public PrestamoDeuda? PrestamoDeuda { get; set; }

    [Display(Name = "Meta de ahorro")]
    public int? MetaAhorroId { get; set; }

    public MetaAhorro? MetaAhorro { get; set; }

    /// <summary>Monto positivo si es ingreso y negativo si es gasto. Requiere cargar la Categoría.</summary>
    public decimal MontoConSigno => Categoria?.Tipo == TipoCategoria.Gasto ? -Monto : Monto;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fecha.Date > DateTime.Today)
        {
            yield return new ValidationResult("La fecha de la transacción no puede ser futura.", [nameof(Fecha)]);
        }

        if (PrestamoDeudaId.HasValue && MetaAhorroId.HasValue)
        {
            yield return new ValidationResult(
                "Una transacción no puede ser a la vez parte de un préstamo/deuda y un aporte a una meta de ahorro.",
                [nameof(MetaAhorroId)]);
        }
    }
}
