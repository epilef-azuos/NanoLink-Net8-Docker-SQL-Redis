var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "NanoLink API is running on Docker! 🚀");

app.Run();
