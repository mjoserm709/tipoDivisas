using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

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

                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var dollarPanel = doc.DocumentNode.SelectSingleNode("//div[@id='dollar-panel-1']");
                var labels = dollarPanel?.SelectNodes(".//span[contains(@class, 'caps-label')]");

                string compra = null, venta = null;

                foreach (var label in labels)
                {
                    var texto = label.InnerText.Trim().ToLower();
                    var divPadre = label.ParentNode?.ParentNode;
                    var montoNode = divPadre?.SelectSingleNode(".//div[contains(@class, 'cell price')]//strong[2]");

                    if (texto == "venta" && montoNode != null)
                        venta = montoNode.InnerText.Trim();

                    if (texto == "compra" && montoNode != null)
                        compra = montoNode.InnerText.Trim();
                }

                return Ok(new { compra, venta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
