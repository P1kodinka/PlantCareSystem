using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public class ExportService : IExportService
    {
        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
        {
            // TODO: реализовать через CsvHelper
            await Task.CompletedTask;
        }

        public async Task ExportToPdfAsync(string title, object reportData, string filePath)
        {
            // TODO: реализовать через QuestPDF
            await Task.CompletedTask;
        }
    }
}