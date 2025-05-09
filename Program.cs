var builder = WebApplication.CreateBuilder(args);

// Agregar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()      // Permitir cualquier origen (puedes restringirlo con .WithOrigins("http://localhost"))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Habilitar CORS antes de los controladores
app.UseCors();

app.UseAuthorization();

app.MapControllers(); // 👈 ¡NECESARIO!

app.Run();
