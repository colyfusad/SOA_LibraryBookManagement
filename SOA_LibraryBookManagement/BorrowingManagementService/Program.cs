using BorrowingManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BorrowingManagementService.Data;
using BorrowingManagementService.Interface;
using CustomerManagementService.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null; // Loại bỏ Preserve
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Bỏ qua các giá trị null (tuỳ chọn)
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddScoped<IBorrowingService, BorrowingService>();

builder.Services.AddDbContext<BorrowingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnStr")));

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Đăng ký JwtAuthorizationHandler
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddHttpClient("BookManagementService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7025/api/");
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient("CustomerManagementService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7247/api/");
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<Authentication>();

app.UseAuthorization();

app.MapControllers();

app.Run();
