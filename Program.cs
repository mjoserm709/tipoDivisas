var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();