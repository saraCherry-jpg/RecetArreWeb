using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> ObtenerTodas();
        Task<CategoriaDto?> ObtenerPorId(int id);
        Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto);
        Task<bool> Actualizar(int id, CategoriaModificacionDto categoriaDto);
        Task<bool> Eliminar(int id);
    }

    public class CategoriaService : ICategoriaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Categorias";

        public CategoriaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<CategoriaDto>> ObtenerTodas()
        {
            try
            {
                var categorias = await httpClient.GetFromJsonAsync<List<CategoriaDto>>(endpoint);
                return categorias ?? new List<CategoriaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categorías: {ex.Message}");
                return new List<CategoriaDto>();
            }
        }

        public async Task<CategoriaDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<CategoriaDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categoría {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, categoriaDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CategoriaDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear categoría: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear categoría: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, CategoriaModificacionDto categoriaDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", categoriaDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar categoría {id}: {ex.Message}");
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
                Console.WriteLine($"Error al eliminar categoría {id}: {ex.Message}");
                return false;
            }
        }
    }
}
