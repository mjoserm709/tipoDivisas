using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using ClosedXML.Excel;
using System.Globalization;

namespace ApiTipoCambio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoCambioBCHController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var page = await browser.NewPageAsync();

                await page.GotoAsync("https://www.bch.hn/estadisticas-y-publicaciones-economicas/tipo-de-cambio-nominal", new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                var enlace = await page.Locator("a[href$='.xlsx']").First.GetAttributeAsync("href");

                if (string.IsNullOrEmpty(enlace))
                    return Ok(new { mensaje = "❌ No se encontró enlace .xlsx", fecha = "", compra = "", venta = "" });

                var urlExcel = enlace.StartsWith("http") ? enlace : "https://www.bch.hn" + enlace;

                var tempPath = Path.Combine(Path.GetTempPath(), "tipo_cambio_bch.xlsx");
                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(urlExcel);

                if (bytes == null || bytes.Length == 0)
                    return Ok(new { mensaje = "❌ El archivo Excel está vacío o no se pudo descargar.", fecha = "", compra = "", venta = "" });

                await System.IO.File.WriteAllBytesAsync(tempPath, bytes);

                using var workbook = new XLWorkbook(tempPath);
                var sheet = workbook.Worksheet("Tipo de Cambio Diario");

                // Buscar la fecha del día o la más cercana anterior
                var fechaHoy = DateTime.Today;
                var fechasOrdenadas = sheet.RowsUsed()
                    .Select(row => new
                    {
                        Row = row,
                        Fecha = DateTime.TryParse(
                            row.Cell(1).GetString().Trim(),
                            new CultureInfo("es-HN"),
                            DateTimeStyles.None,
                            out var parsedDate
                        ) ? parsedDate : (DateTime?)null
                    })
                    .Where(x => x.Fecha.HasValue && x.Fecha.Value <= fechaHoy)
                    .OrderByDescending(x => x.Fecha.Value)
                    .ToList();

                var rowSeleccionada = fechasOrdenadas.FirstOrDefault();

                if (rowSeleccionada == null)
                    return Ok(new { mensaje = "❌ No se encontró una fecha válida para hoy o días anteriores.", fecha = "", compra = "", venta = "" });

                var fecha = rowSeleccionada.Fecha.Value.ToString("dd/MM/yyyy");
                var compra = rowSeleccionada.Row.Cell(2).GetString().Trim();
                var venta = rowSeleccionada.Row.Cell(3).GetString().Trim();

                return Ok(new { fecha, compra, venta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❗ Error en la API: " + ex.Message });
            }
        }
    }
}
