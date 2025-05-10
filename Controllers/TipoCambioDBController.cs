using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Globalization;
using ApiTipoCambio.Models; // 👉 Importa el modelo

namespace ApiTipoCambio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoCambioDBController : ControllerBase
    {
        private readonly string _connectionString;

        public TipoCambioDBController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetPorFecha([FromQuery] string fecha)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand(
                    "SELECT TOP 1 * FROM TipoCambioBCH WHERE Fecha = @Fecha", connection);
                cmd.Parameters.AddWithValue("@Fecha", DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture));

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Ok(new
                    {
                        fecha = ((DateTime)reader["Fecha"]).ToString("yyyy-MM-dd"),
                        compra = reader["Compra"].ToString(),
                        venta = reader["Venta"].ToString()
                    });
                }

                return NotFound(new { mensaje = "❌ No existe un registro para esa fecha." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GET: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] TipoCambioRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Fecha))
                    return BadRequest(new { error = "❌ El campo 'fecha' es obligatorio." });

                string fecha = model.Fecha;
                string compra = model.Compra;
                string venta = model.Venta;

                Console.WriteLine($"📅 Fecha: {fecha}, 💵 Compra: {compra}, 💶 Venta: {venta}");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand(@"
                    INSERT INTO TipoCambioBCH (Fecha, Compra, Venta)
                    VALUES (@Fecha, @Compra, @Venta)", connection);

                cmd.Parameters.AddWithValue("@Fecha", DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@Compra", compra);
                cmd.Parameters.AddWithValue("@Venta", venta);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { mensaje = "✅ Registro insertado con éxito." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en POST: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("SELECT Fecha, Compra, Venta FROM TipoCambioBCH ORDER BY Fecha DESC", connection);

                using var reader = await cmd.ExecuteReaderAsync();
                var historial = new List<object>();

                while (await reader.ReadAsync())
                {
                    historial.Add(new
                    {
                        fecha = ((DateTime)reader["Fecha"]).ToString("yyyy-MM-dd"),
                        compra = reader["Compra"].ToString(),
                        venta = reader["Venta"].ToString()
                    });
                }

                return Ok(historial);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GET Historial: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
