using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = default!;
        public DateTime CreadoUtc { get; set; }
        public int RecetaId { get; set; }
        public string UsuarioId { get; set; } = default!;
    }

    public class ComentarioCreacionDto
    {
        [Required(ErrorMessage = "El comentario es requerido")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 1000 caracteres")]
        public string Contenido { get; set; } = default!;

        [Required]
        public int RecetaId { get; set; }
    }

    public class ComentarioModificacionDto
    {
        [Required(ErrorMessage = "El comentario es requerido")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 1000 caracteres")]
        public string Contenido { get; set; } = default!;
    }
}
