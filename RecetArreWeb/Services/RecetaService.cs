using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface IRecetaService
    {
        Task<List<RecetaDto>> ObtenerTodas();
        Task<RecetaDto?> ObtenerPorId(int id);
        Task<RecetaDto?> Crear(RecetaCreacionDto recetaDto);
        Task<bool> Actualizar(int id, RecetaModificacionDto recetaDto);
        Task<bool> Eliminar(int id);
    }

    public class RecetaService : IRecetaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Recetas";

        public RecetaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<RecetaDto>> ObtenerTodas()
        {
            try
            {
                var recetas = await httpClient.GetFromJsonAsync<List<RecetaDto>>(endpoint);
                return recetas ?? new List<RecetaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener recetas: {ex.Message}");
                return new List<RecetaDto>();
            }
        }

        public async Task<RecetaDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<RecetaDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener receta {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<RecetaDto?> Crear(RecetaCreacionDto recetaDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, recetaDto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RecetaDto>();
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear receta: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear receta: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, RecetaModificacionDto recetaDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", recetaDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar receta {id}: {ex.Message}");
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
                Console.WriteLine($"Error al eliminar receta {id}: {ex.Message}");
                return false;
            }
        }
    }
}
