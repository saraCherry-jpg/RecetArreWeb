using RecetArreWeb.DTOs;
using System.Net.Http;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    public interface IRankingService
    {
        Task<List<RankingDto>> ObtenerTodos();
        Task<RankingDto?> ObtenerPorId(int id);
        Task<List<RankingDto>> ObtenerPorReceta(int recetaId);
        Task<RankingDto?> Crear(RankingCreacionDto rankingDto);
        Task<bool> Eliminar(int id);

    }//FIN DE LA INTERFAZ




    public class RankingService : IRankingService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Ranking"; // coincide con el controlador

        //Constructor
        public RankingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }// Fin del constructor

        

        //METODOS
        public async Task<List<RankingDto>> ObtenerTodos()
        {
            try
            {
                var rankings = await httpClient.GetFromJsonAsync<List<RankingDto>>(endpoint);
                return rankings ?? new List<RankingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener rankings: {ex.Message}");
                return new List<RankingDto>();
            }

        }

        public async Task<RankingDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<RankingDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ranking {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RankingDto>> ObtenerPorReceta(int recetaId)
        {
            try
            {
                var rankings = await httpClient.GetFromJsonAsync<List<RankingDto>>($"{endpoint}/receta/{recetaId}");
                return rankings ?? new List<RankingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener rankings de receta {recetaId}: {ex.Message}");
                return new List<RankingDto>();
            }
        }

        public async Task<RankingDto?> Crear(RankingCreacionDto rankingDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, rankingDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RankingDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear ranking: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear ranking: {ex.Message}");
                return null;
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
                Console.WriteLine($"Error al eliminar ranking {id}: {ex.Message}");
                return false;
            }

        }
    }//FIN DE LA CLASE
}
