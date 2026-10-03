using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Middleware;
using SubastaYa.Api.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<PujaService>();
builder.Services.AddScoped<BilleteraService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<SubastaService>();
builder.Services.AddScoped<TransaccionLedgerService>();

// Worker que revisa y cierra automáticamente
// las subastas vencidas.
builder.Services.AddHostedService<CierreSubastasWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar datos de prueba si la base está vacía.
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    await SeedData.InicializarAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseMiddleware<ApiVersionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<SubastaHub>("/hubs/subastas");

app.Run();