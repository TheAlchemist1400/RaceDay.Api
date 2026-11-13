using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using Azure.Storage.Blobs;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddDbContext<RaceDayDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session configuration
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var azureConnectionString =
    builder.Configuration["AzureBlobStorage:ConnectionString"];

if (string.IsNullOrWhiteSpace(azureConnectionString))
{
    throw new InvalidOperationException(
        "Azure Blob Storage connection string is missing.");
}

builder.Services.AddSingleton(
    new BlobServiceClient(azureConnectionString)
);

builder.Services.AddScoped<BlobStorageService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
