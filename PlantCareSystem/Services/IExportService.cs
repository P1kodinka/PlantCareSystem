using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public interface IExportService
    {
        Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath);
        Task ExportToPdfAsync(string title, object reportData, string filePath);
    }
}