using RecetArreWeb.DTOs;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    // Interfaz que debes crear (posiblemente en otro archivo)
    public interface IRatingService
    {
        Task<List<RatingDto>> ObtenerTodos();
        Task<RatingDto?> ObtenerPorId(int id);
        Task<List<RatingDto>> ObtenerPorReceta(int recetaId);
        Task<List<RatingDto>> ObtenerPorUsuario(string usuarioId);
        Task<RatingPromedioDto?> ObtenerPromedioPorReceta(int recetaId);
        Task<RatingDto?> Crear(RatingCreacionDto ratingDto);
        Task<bool> Actualizar(int id, RatingModificacionDto ratingDto);
        Task<bool> Eliminar(int id);
    }

    // Solo UNA clase RatingService
    public class RatingService : IRatingService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Ratings";

        public RatingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<RatingDto>> ObtenerTodos()
        {
            try
            {
                var ratings = await httpClient.GetFromJsonAsync<List<RatingDto>>(endpoint);
                return ratings ?? new List<RatingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ratings: {ex.Message}");
                return new List<RatingDto>();
            }
        }

        public async Task<RatingDto?> ObtenerPorId(int id)
        {
            try
            {
                var rating = await httpClient.GetFromJsonAsync<RatingDto>($"{endpoint}/{id}");
                return rating;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener rating {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RatingDto>> ObtenerPorReceta(int recetaId)
        {
            try
            {
                var ratings = await httpClient.GetFromJsonAsync<List<RatingDto>>($"{endpoint}/receta/{recetaId}");
                return ratings ?? new List<RatingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ratings por receta {recetaId}: {ex.Message}");
                return new List<RatingDto>();
            }
        }

        public async Task<List<RatingDto>> ObtenerPorUsuario(string usuarioId)
        {
            try
            {
                var ratings = await httpClient.GetFromJsonAsync<List<RatingDto>>($"{endpoint}/usuario/{usuarioId}");
                return ratings ?? new List<RatingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ratings por usuario {usuarioId}: {ex.Message}");
                return new List<RatingDto>();
            }
        }

        public async Task<RatingPromedioDto?> ObtenerPromedioPorReceta(int recetaId)
        {
            try
            {
                var promedio = await httpClient.GetFromJsonAsync<RatingPromedioDto>($"{endpoint}/receta/{recetaId}/promedio");
                return promedio;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener promedio de receta {recetaId}: {ex.Message}");
                return null;
            }
        }

        public async Task<RatingDto?> Crear(RatingCreacionDto ratingDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, ratingDto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RatingDto>();
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear rating: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear rating: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, RatingModificacionDto ratingDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", ratingDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar rating {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar rating {id}: {ex.Message}");
                return false;
            }
        }
    }
}