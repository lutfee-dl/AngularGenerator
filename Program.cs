using AngularGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// -------------------------------------------------------------
// Register services - Now using Builder Pattern
// -------------------------------------------------------------
// Register DbSchemaServiceFactory as Singleton
builder.Services.AddSingleton<DbSchemaServiceFactory>();

// Register Database Configuration Service
builder.Services.AddScoped<DatabaseConfigService>();

// Register other services
builder.Services.AddScoped<AngularComponentFactory>();
builder.Services.AddScoped<FullStackGenerator>();

// Register HttpClient and JsonSchemaService for API/JSON schema detection
builder.Services.AddHttpClient<JsonSchemaService>();
builder.Services.AddScoped<JsonSchemaService>();

// Build App
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Disable HTTPS redirect for development
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Generator}/{action=Index}/{id?}");

// Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("===========================================");
logger.LogInformation("Application Started!");
logger.LogInformation("Navigate to: http://localhost:5180");
logger.LogInformation("===========================================");

app.Run();
