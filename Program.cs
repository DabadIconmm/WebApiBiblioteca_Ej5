using Ejercicio_Sesión_1;
using Ejercicio_Sesión_1.Filtros;
using Ejercicio_Sesión_1.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApiAlmacen.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Para evitar ciclos infinitos en entidades relacionadas
builder.Services.AddControllers(
    opciones => opciones.Filters.Add(typeof(FiltroDeExcepcion))
    ).AddJsonOptions(opciones => opciones.JsonSerializerOptions.ReferenceHandler =
System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("defaultConnection"); //nos referimos a la conexion con appsetting.Development.json
// Registramos en el sistema de inyección de dependencias de la aplicación el ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseSqlServer(connectionString);
    //Con esto hacemos que no se realize el traking de los registros de una BBDD, así somos explícitos donde queremos hacer el tracking.
    opciones.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("webapi", x => { x.BaseAddress = new Uri("https://localhost:44381"); });

builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<TareaProgramadaService>();

// Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();
app.UseMiddleware<LogFilePathIPMiddleware>();
app.UseMiddleware<LogFileBodyHttpResponseMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
