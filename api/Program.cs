using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

string discordBotToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

if (string.IsNullOrEmpty(discordBotToken))
{
    Console.WriteLine("DISCORD_BOT_TOKEN is not set.");
}

app.MapPost("/api/roblox", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

    if (data == null || !data.ContainsKey("message") || !data.ContainsKey("channel_id"))
    {
        return Results.BadRequest(new { status = "failure", message = "Invalid request payload" });
    }

    var message = data["message"];
    var channelId = data["channel_id"];

    var success = await PostToDiscord(channelId, message);
    return Results.Json(new { status = success ? "success" : "failure" });
});

async Task<bool> PostToDiscord(string channelId, string message)
{
    var client = new RestClient("https://discord.com/api/v10");
    var request = new RestRequest($"/channels/{channelId}/messages", Method.Post);
    request.AddHeader("Authorization", $"Bot {discordBotToken}");
    request.AddJsonBody(new { content = message });

    var response = await client.ExecuteAsync(request);
    Console.WriteLine($"Discord API response: {response.Content}");
    return response.IsSuccessful;
}

app.Run();
