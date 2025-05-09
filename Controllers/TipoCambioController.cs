using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace ApiScrapingTipoCambio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoCambioController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var url = "https://www.bancodeoccidente.hn/mas/internacional/divisas-personas";

                using var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var dollarPanel = doc.DocumentNode.SelectSingleNode("//div[@id='dollar-panel-1']");

                if (dollarPanel == null)
                    return Ok(new { mensaje = "❌ No se encontró la sección de tipo de cambio.", compra = "", venta = "" });

                var labels = dollarPanel.SelectNodes(".//span[contains(@class, 'caps-label')]");
                if (labels == null || labels.Count == 0)
                    return Ok(new { mensaje = "❌ No se encontraron etiquetas de compra/venta.", compra = "", venta = "" });

                string compra = "", venta = "";

                foreach (var label in labels)
                {
                    var texto = label.InnerText.Trim().ToLower();
                    var divPadre = label.ParentNode?.ParentNode;
                    var montoNode = divPadre?.SelectSingleNode(".//div[contains(@class, 'cell price')]//strong[2]");

                    if (montoNode != null)
                    {
                        if (texto == "venta")
                            venta = montoNode.InnerText.Trim();

                        if (texto == "compra")
                            compra = montoNode.InnerText.Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(compra) && string.IsNullOrWhiteSpace(venta))
                    return Ok(new { mensaje = "❌ No se encontraron valores de compra y venta.", compra = "", venta = "" });

                return Ok(new { compra, venta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❗ Error en la API: " + ex.Message });
            }
        }
    }
}
