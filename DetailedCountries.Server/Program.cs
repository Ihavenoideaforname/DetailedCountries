using DetailedCountries.Server.Models.BackendModels;
using DetailedCountries.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();

builder.Services.Configure<DetailedCountriesDBSettings>(builder.Configuration.GetSection("DetailedCountriesDB"));

builder.Services.AddSingleton<IRESTCountriesAPIService, RESTCountriesAPIService>();
builder.Services.AddSingleton<IOpenMeteoAPIService, OpenMeteoAPIService>();
builder.Services.AddSingleton<ICountryService, CountryService>();

builder.Services.AddHostedService<CountrySyncService>();

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy => policy
            .WithOrigins("https://localhost:61230", "http://localhost:61230")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowAngularClient");

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "https://localhost:61230");
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/browser/index.html");

app.Run();
