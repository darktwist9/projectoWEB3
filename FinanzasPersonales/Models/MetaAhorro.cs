using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Models;

/// <summary>DER: Meta de ahorro — recibe aportes de — Transacción.</summary>
public class MetaAhorro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la meta es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre de la meta")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Ingrese el monto objetivo.")]
    [Range(0.01, 999999999.99, ErrorMessage = "El monto objetivo debe ser mayor a 0.")]
    [Display(Name = "Monto objetivo (Bs)")]
    public decimal MontoObjetivo { get; set; }

    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha objetivo")]
    public DateTime? FechaObjetivo { get; set; }

    [Display(Name = "Aportes")]
    public ICollection<Transaccion> Aportes { get; set; } = new List<Transaccion>();

    // ----- Valores calculados (requieren cargar Aportes) -----

    public decimal MontoAhorrado => Aportes.Sum(t => t.Monto);

    public decimal MontoRestante => Math.Max(MontoObjetivo - MontoAhorrado, 0);

    public decimal PorcentajeAvance => MontoObjetivo == 0 ? 0 : Math.Min(Math.Round(MontoAhorrado * 100 / MontoObjetivo, 1), 100);

    public bool Cumplida => MontoAhorrado >= MontoObjetivo;

    /// <summary>Monto que habría que ahorrar cada mes para llegar a la meta en la fecha objetivo.</summary>
    public decimal? AporteMensualSugerido
    {
        get
        {
            if (!FechaObjetivo.HasValue || Cumplida) return null;
            var hoy = DateTime.Today;
            var mesesRestantes = (FechaObjetivo.Value.Year - hoy.Year) * 12 + FechaObjetivo.Value.Month - hoy.Month;
            return mesesRestantes <= 0 ? MontoRestante : Math.Round(MontoRestante / mesesRestantes, 2);
        }
    }
}
