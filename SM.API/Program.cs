using Microsoft.EntityFrameworkCore;
using SM.API.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS ekle - Android emülatör için gerekli
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON serialization cycle sorununu çöz
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Entity Framework yapýlandýrmasý
builder.Services.AddDbContext<SMDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// CORS'u kullan - HTTPS redirect'ten önce olmalý
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirect'i geliþtirme ortamýnda kapat - Android emülatör için
// app.UseHttpsRedirection(); // Bu satýrý yorum yap

app.UseAuthorization();
app.MapControllers();

// Tüm network interface'leri dinle
app.Run("http://0.0.0.0:5000");