using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using simple_agent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(
    "ollama-client",
    configClient =>
    {
        configClient.BaseAddress = new Uri("http://localhost:11434");
    }
);

var app = builder.Build();

var factory = app.Services.GetService<IHttpClientFactory>();

var olClient = factory?.CreateClient("ollama-client");

if (olClient is null)
    return;
var tools = """
Available tools:
- Add: Description: Add two numbers. Parameters: - a: integer - b: integer Returns: - integer
- Subtraction: Description: Subtract second parameter from the first parameter. Parameters: - a: integer - b: integer Returns: - integer
- Multiply: Description: Multiply two numbers. Parameters: - a: integer - b: integer Returns: - integer
- Division: Description: Divide first number with the second. Parameters: - a: integer - b: integer Returns: - integer
""";

var rules = """
Output format: JSON
Description: The JSON will suggest a list of sequential operations on the input numbers.
JSON Description:
- tool: Name of the tool (Add, Subtraction, Multiply, Division)
- On the root level,
- a: Initial integer value of the parameter a.
- b: Initial integer value of the parameter b.
Sample JSON object:
{ "tool": "Add", "a": 5, "b": 6 }
""";

while (true)
{
    Console.Write("Enter prompt: ");
    var prompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(prompt))
    {
        Console.WriteLine("Bye...");
        break;
    }
    var finalPrompt = $"""
          {prompt}

          {tools}

          {rules}
        """;
    var payload = new
    {
        model = "llama3",
        prompt = finalPrompt,
        stream = false,
        format = "json",
        options = new { temperature = 0.0 },
    };

    var payloadJson = JsonSerializer.Serialize(payload);

    var response = await olClient.PostAsync(
        "/api/generate",
        new StringContent(payloadJson, Encoding.UTF8, "application/json")
    );

    if (!response.IsSuccessStatusCode)
    {
        var errorDetails = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ollama API Error ({response.StatusCode}): {errorDetails}");
        continue;
    }

    try
    {
        var resp = await response.Content.ReadFromJsonAsync<PromptResponse>();
        if (resp == null)
        {
            Console.WriteLine("No response received!!!");
            continue;
        }
        Console.WriteLine("Response: ");
        Console.WriteLine(resp.response);
        var tool = JsonSerializer.Deserialize<ToolInvocation>(resp.response ?? "{}");
        var fn = tool?.tool ?? "";
        if (tool is null)
            continue;
        var result = 0;
        switch (fn)
        {
            case "Add":
                result = Tools.Add(tool.a, tool.b);
                break;
            case "Multiply":
                result = Tools.Multiply(tool.a, tool.b);
                break;
            case "Subtraction":
                result = Tools.Subtract(tool.a, tool.b);
                break;
            case "Division":
                result = Tools.Division(tool.a, tool.b);
                break;
            default:
                Console.WriteLine("Incorrect tool");
                break;
        }
        Console.WriteLine(result);
    }
    catch
    {
        var resp = await response.Content.ReadAsStringAsync();
        Console.WriteLine(response);
    }
}
