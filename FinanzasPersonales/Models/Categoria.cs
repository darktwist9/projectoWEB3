using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Models.Enums;

namespace FinanzasPersonales.Models;

/// <summary>DER: Categoría (1) — clasifica — (N) Transacción.</summary>
public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el tipo de categoría.")]
    [Display(Name = "Tipo")]
    public TipoCategoria Tipo { get; set; } = TipoCategoria.Gasto;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}
