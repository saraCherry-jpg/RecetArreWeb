using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    public class RecetaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = default!;
        public string? Descripcion { get; set; }
        public string Instrucciones { get; set; } = default!;
        public int TiempoPreparacionMinutos { get; set; }
        public int TiempoCoccionMinutos { get; set; }
        public int Porciones { get; set; }
        public bool EstaPublicado { get; set; }
        public DateTime CreadoUtc { get; set; }
        public DateTime ModificadoUtc { get; set; }
        public string AutorId { get; set; } = default!;
        public List<int> CategoriaIds { get; set; } = new();
        public List<int> IngredienteIds { get; set; } = new();
    }

    public class RecetaCreacionDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres")]
        public string Titulo { get; set; } = default!;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Las instrucciones son requeridas")]
        [StringLength(15000)]
        public string Instrucciones { get; set; } = default!;

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoPreparacionMinutos { get; set; }

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoCoccionMinutos { get; set; }

        [Range(1, 100, ErrorMessage = "Las porciones deben estar entre 1 y 100")]
        public int Porciones { get; set; } = 1;

        public bool EstaPublicado { get; set; } = true;

        public List<int> CategoriaIds { get; set; } = new();
        public List<int> IngredienteIds { get; set; } = new();
    }

    public class RecetaModificacionDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres")]
        public string Titulo { get; set; } = default!;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Las instrucciones son requeridas")]
        [StringLength(15000)]
        public string Instrucciones { get; set; } = default!;

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoPreparacionMinutos { get; set; }

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoCoccionMinutos { get; set; }

        [Range(1, 100, ErrorMessage = "Las porciones deben estar entre 1 y 100")]
        public int Porciones { get; set; } = 1;

        public bool EstaPublicado { get; set; } = true;

        public List<int> CategoriaIds { get; set; } = new();
        public List<int> IngredienteIds { get; set; } = new();
    }
}
