using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public interface IExportService
    {
        Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath);
        Task ExportToPdfAsync(string title, IEnumerable<object> reportData, string filePath);
    }
}