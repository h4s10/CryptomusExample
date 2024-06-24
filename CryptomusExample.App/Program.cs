using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptomusExample.App.Models.Bot;
using CryptomusExample.App.Models.Cryptomus.Requests;
using CryptomusExample.App.Models.Settings;
using CryptomusExample.App.ThirdPartyApis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Refit;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CryptomusSettings>(builder.Configuration.GetSection(nameof(CryptomusSettings)));
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(builder.Configuration["TelegramSettings:ApiKey"]));
builder.Services.AddRefitClient<ICryptomusClient>(s => new RefitSettings()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        })
    })
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.cryptomus.com/v1"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapPost("/", async ([FromBody] UpdateDto update, [FromServices] ILogger<Program> logger,
        [FromServices] ICryptomusClient client, [FromServices] ITelegramBotClient telegramBotClient,
        [FromServices] IOptions<CryptomusSettings> cryptomusSettings) =>
    {
        if (update.Message?.Text == "/start")
        {
            logger.LogDebug("Start command has received");
            using (var md5 = MD5.Create())
            {
                var request = new CreateStaticWalletRequest()
                {
                    Currency = "USDT",
                    Network = "tron",
                    OrderId = Guid.NewGuid().ToString()
                };
                var rawJson = JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawJson));
                var inputBytes = Encoding.UTF8.GetBytes(base64String + cryptomusSettings.Value.ApiKey);
                var sign = Convert.ToHexString(md5.ComputeHash(inputBytes)).ToLower();
                var response = await client.CreateStaticWalletAsync(request, sign, cryptomusSettings.Value.MerchantId);
                if (response.State == 0)
                {
                    await telegramBotClient.SendTextMessageAsync(new ChatId(update.Message.Chat.Id),
                        response.Result.Url);
                }
            }
        }
    })
    .WithName("WebHook handler method")
    .WithOpenApi();

app.Run();