using BookTracker.Api.Wiring;

var builder = WebApplication.CreateBuilder(args);

builder.AddBookTracker();

var frontendOrigin = builder.Configuration["FrontendOrigin"]
    ?? "http://localhost:5173";
/*
http://localhost:5173 >>> Local Development ( nu weerkt react by Vite DUS (5173))
http://localhost:3000 >>> Docker  (nu weerkt react by Nginx DUS (3000))
https://booktracker.com >>> Production
*/

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseBookTracker();
app.Run();

public partial class Program;