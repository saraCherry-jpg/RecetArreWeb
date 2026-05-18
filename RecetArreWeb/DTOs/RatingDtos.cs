using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
        public class RatingDto
        {
            public int Id { get; set; }
            public int Calificacion { get; set; } // 1 a 5 estrellas
            public string? UsuarioId { get; set; }
            public int RecetaId { get; set; }
            // Opcional: si quieres mostrar información adicional de navegación
            // public string? NombreReceta { get; set; }
            // public string? NombreUsuario { get; set; }
        }

        // DTO para CREAR un nuevo rating (POST)
        public class RatingCreacionDto
        {
            [Required(ErrorMessage = "La calificación es requerida")]
            [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5 estrellas")]
            public int Calificacion { get; set; }

            [Required(ErrorMessage = "El ID del usuario es requerido")]
            public string UsuarioId { get; set; } = default!;

            [Required(ErrorMessage = "El ID de la receta es requerido")]
            public int RecetaId { get; set; }
        }

        // DTO para MODIFICAR un rating existente (PUT)
        public class RatingModificacionDto
        {
            [Required(ErrorMessage = "La calificación es requerida")]
            [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5 estrellas")]
            public int Calificacion { get; set; }
        }

        // DTO adicional: Para el promedio de calificaciones de una receta
        public class RatingPromedioDto
        {
            public int RecetaId { get; set; }
            public double Promedio { get; set; }
            public int TotalCalificaciones { get; set; }
        }
}
