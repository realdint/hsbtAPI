using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System.IO;
using System.Threading.Tasks;
using vars;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/api/roblox", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine($"Received body: {body}");
    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
    var message = data != null && data.ContainsKey("message") ? data["message"] : "No message provided";

    var success = await PostToDiscord(message);
    return Results.Json(new { status = success ? "success" : "failure" });
});

async Task<bool> PostToDiscord(string message)
{
    var client = new RestClient("https://discord.com/api/v10");
    var request = new RestRequest(Vars.channel, Method.Post);
    request.AddHeader("Authorization", Vars.token);
    request.AddJsonBody(new { content = message });

    var response = await client.ExecuteAsync(request);
    return response.IsSuccessful;
}

app.Run();
