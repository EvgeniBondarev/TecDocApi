using TecDocApi.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Добавляем все сервисы через расширения
builder.Services.AddApiServices(builder.Configuration);

// Настраиваем Kestrel через расширение
builder.WebHost.ConfigureKestrelLimits();

var app = builder.Build();

// Настраиваем pipeline через расширение
app.ConfigurePipeline();

app.Run();

