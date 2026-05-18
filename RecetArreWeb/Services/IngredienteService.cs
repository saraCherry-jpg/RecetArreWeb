using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface IIngredienteService
    {
        Task<List<IngredienteDto>> ObtenerTodos();
        Task<IngredienteDto?> ObtenerPorId(int id);
        Task<IngredienteDto?> Crear(IngredienteCreacionDto ingredienteDto);
        Task<bool> Actualizar(int id, IngredienteModificacionDto ingredienteDto);
        Task<bool> Eliminar(int id);
    }

    public class IngredienteService : IIngredienteService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Ingredientes";

        public IngredienteService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<IngredienteDto>> ObtenerTodos()
        {
            try
            {
                var ingredientes = await httpClient.GetFromJsonAsync<List<IngredienteDto>>(endpoint);
                return ingredientes ?? new List<IngredienteDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingredientes: {ex.Message}");
                return new List<IngredienteDto>();
            }
        }

        public async Task<IngredienteDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<IngredienteDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingrediente {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<IngredienteDto?> Crear(IngredienteCreacionDto ingredienteDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, ingredienteDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IngredienteDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear ingrediente: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear ingrediente: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, IngredienteModificacionDto ingredienteDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", ingredienteDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ingrediente {id}: {ex.Message}");
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
                Console.WriteLine($"Error al eliminar ingrediente {id}: {ex.Message}");
                return false;
            }
        }
    }
}
