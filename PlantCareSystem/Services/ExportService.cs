using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlantCareSystem.Services
{
    public class ExportService : IExportService
    {
        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                Encoding = System.Text.Encoding.UTF8
            };
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            await csv.WriteRecordsAsync(data);
        }

        public async Task ExportToPdfAsync(string title, IEnumerable<object> reportData, string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text(title)
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            // Определяем столбцы динамически по свойствам первого объекта
                            var first = reportData.FirstOrDefault();
                            if (first == null) return;

                            var props = first.GetType().GetProperties();
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var prop in props)
                                    columns.RelativeColumn();
                            });

                            // Заголовки
                            table.Header(header =>
                            {
                                foreach (var prop in props)
                                    header.Cell().Element(CellStyle).Text(prop.Name).SemiBold();
                            });

                            // Данные
                            foreach (var item in reportData)
                            {
                                foreach (var prop in props)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "";
                                    table.Cell().Element(CellStyle).Text(value);
                                }
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            });

            document.GeneratePdf(filePath);
            await Task.CompletedTask;
        }
    }
}