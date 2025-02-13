using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Pokedex.Services;
using System.Text.Json;

namespace Pokedex
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient("PokedexClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
                httpClient.Timeout = TimeSpan.FromSeconds(40);
            });

            builder.Services.TryAddSingleton(_ =>
            {
                var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    WriteIndented = false,
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                return jsonSerializerOptions;
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddScoped<PokedexService>();

            return builder.Build();
        }
    }
}