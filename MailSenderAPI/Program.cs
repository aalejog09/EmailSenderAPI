using MailSenderAPI.Context;
using MailSenderAPI.Middleware;
using MailSenderAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Envío de Correos", Version = "v1" });

});



// Leer la configuración de URLs desde appsettings.json
var urls = builder.Configuration["Urls"];

builder.WebHost.ConfigureKestrel(options =>
{
    if (!string.IsNullOrEmpty(urls))
    {
        options.ListenAnyIP(int.Parse(urls.Split(':')[2])); // Configura el puerto
    }
});

// Configurar la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("email_db")));


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<EmailExtensionService>();  // Registro del servicio de Extensiones
builder.Services.AddScoped<EmailService>();  // Registro del servicio de correo
builder.Services.AddScoped<SmtpSettingsService>();  // Registro del servicio de SMTP Settings
builder.Services.AddScoped<EncryptionService>();  // Registro del servicio de encryptacion
builder.Services.AddSingleton<ErrorService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
