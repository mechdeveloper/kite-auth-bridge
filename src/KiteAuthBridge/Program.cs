using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var config = builder.Configuration;

string? accessTokenGlobal = null;
string? apiKey = config["Kite:ApiKey"];
string? apiSecret = config["Kite:ApiSecret"];
string? baseUrl = config["Kite:BaseUrl"];
string? appSecret = config["App:Secret"];

if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(appSecret))
{
    throw new Exception("Configuration is missing");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Redirect to Kite login
app.MapGet("/", () => Results.Redirect($"https://kite.zerodha.com/connect/login?api_key={apiKey}&v=3"));

// Main Callback
app.MapGet("/callback", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var requestToken = context.Request.Query["request_token"].ToString();

    if (string.IsNullOrEmpty(requestToken))
    {
        return Results.BadRequest("Missing request_token");
    }

    // Generate checksum
    string input = apiKey + requestToken + apiSecret;

    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    var checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

    // Call Zerodha API
    using var httpClient = httpClientFactory.CreateClient();

    var formData = new Dictionary<string, string>
    {
        { "api_key", apiKey },
        { "request_token", requestToken },
        { "checksum", checksum }
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/session/token")
    {
        Content = new FormUrlEncodedContent(formData)
    };

    request.Headers.Add("X-Kite-Version", "3");

    var response = await httpClient.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Kite API error: {responseContent}");
        return Results.Problem("Failed to get access token");
    }

    // Parse access token   
    var json = JsonDocument.Parse(responseContent);

    if (!json.RootElement.TryGetProperty("data", out var data) ||
        !data.TryGetProperty("access_token", out var tokenEl))
    {
        return Results.Problem("Unexpected response format from Kite API");
    }

    accessTokenGlobal = tokenEl.GetString();

    return Results.Ok(new { message = "Login successful" });
});

// Get Token
app.MapGet("/token", (HttpContext context) =>
{
    var providedSecret = context.Request.Headers["X-Secret"].ToString();

    if (string.IsNullOrEmpty(providedSecret) || providedSecret != appSecret)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrEmpty(accessTokenGlobal))
    {
        return Results.NotFound(new
        {
            success = false,
            message = "Access token not available yet"
        });
    }

    return Results.Ok(new
    {
        success = true,
        data = new
        {
            api_key = apiKey,
            access_token = accessTokenGlobal,
        }
    });
});

app.Run();