using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    public class RankingDto
    {
        public int Id { get; set; }
        public string ComentariosRanking { get; set; } = default!;
        public int RecetaId { get; set; }
        public DateTime CreadoUtc { get; set; }
        //opcional el titulo de la receta..
    }

    //METODO
    public class RankingCreacionDto
    {
        [Required(ErrorMessage = "El comentario es obligatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 500 caracteres")]
        public string ComentariosRanking { get; set; } = default!;

        [Required]
        public int RecetaId { get; set; }
    }
    // No hay ModificacionDto porque el backend no tiene PUT
}
