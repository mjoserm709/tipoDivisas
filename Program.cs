var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("https://tipo-divisa-app.onrender.com") // Tu dominio exacto
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

app.UseCors("PermitirFrontend"); // Activar la política de CORS antes de Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();