var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "BlijvenLeren",
    mode = "bootstrap"
}));

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
