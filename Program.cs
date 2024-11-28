using Microsoft.Extensions.Options;
using RestimController.Models;
using RestimController.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));


builder.Services.AddSingleton((IServiceProvider isp) => isp.GetService<IOptions<AppSettings>>().Value);
builder.Services.AddSingleton<SharedMemory>();
builder.Services.AddHostedService<CommandSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();

var authKey = builder.Configuration.GetValue<string>("AppSettings:AuthKey");


// Middleware to check AUTH_KEY in headers
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/static") || path.StartsWithSegments("/api/auth") ||path == "/" || app.Environment.IsDevelopment())
    {
        await next();
        return;
    }
    if (!context.Request.Headers.TryGetValue("AUTHKEY", out var extractedAuthKey) || extractedAuthKey != authKey)
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Unauthorized access. Invalid AUTHKEY header.");
        return;
    }

    await next(); // Continue to the next middleware or endpoint
});

// Serve `index.html` at the root URL (`/`)
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});
app.UseStaticFiles(); // Serves static files from `wwwroot`

// Serve static files under `/static` path
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static"
});

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

