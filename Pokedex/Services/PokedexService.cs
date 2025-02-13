using System.Text.Json.Serialization;
using System.Text.Json;
using Pokedex.Models;

namespace Pokedex.Services;

public class PokedexService : RestServiceBase
{
    public PokedexService(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonSerializerOptions)
        : base(httpClientFactory.CreateClient("PokedexClient"), jsonSerializerOptions)
    {
    }

    public async Task<Pokemon[]> GetPokemonsAsync(CancellationToken cancellationToken)
    {
        const byte limit = 10;
        var currentPage = 0;
        PokedexPageResult<PokemonBasicData> pagePokemons;
        List<Task<Pokemon>> pokemonTasks = [];

        do
        {
            pagePokemons = await GetAsync<PokedexPageResult<PokemonBasicData>>(
                $"pokemon?limit={limit}&offset={currentPage}",
                cancellationToken);

            foreach (var pokemonBasicDetails in pagePokemons.Results)
            {
                var pokemonTask = GetAsync<Pokemon>(pokemonBasicDetails.Url, cancellationToken);
                pokemonTasks.Add(pokemonTask);
            }

            currentPage += limit;

        } while (currentPage < pagePokemons.Count);

        var allPokemons = await Task.WhenAll(pokemonTasks);

        return allPokemons;
    }

    public async Task<Pokemon[]> GetPokemonsPageAsync(int offset, int limit, CancellationToken cancellationToken)
    {
        var pagePokemons = await GetAsync<PokedexPageResult<PokemonBasicData>>(
           $"pokemon?limit={limit}&offset={offset}",
           cancellationToken);

        var pokemonTasks = pagePokemons.Results.Select(async pokemonBasicDetails =>
        {
            var pokemon = await GetAsync<Pokemon>(pokemonBasicDetails.Url, cancellationToken);
            return pokemon;
        }).ToList();

        return await Task.WhenAll(pokemonTasks);
    }
}

public class Pokemon
{
    public string Name { get; set; }

    public Sprite Sprites { get; set; }

    public List<PokemonType> Types { get; set; }

    public string FormattedName => char.ToUpper(Name[0]) + Name.Substring(1);

    public bool HasSecondType => Types != null && Types.Count > 1;
}

public class Sprite
{
    [JsonPropertyName("front_default")]
    public string FrontDefault { get; set; }
}

public class PokemonType
{
    [JsonPropertyName("type")]
    public TypeDetails Type { get; set; }

    public string TypeColor
    {
        get
        {
            return Type.Name.ToLower() switch
            {
                "normal" => "#A8A878", // Cinza
                "fighting" => "#C03028", // Vermelho
                "flying" => "#A890F0", // Ciano
                "poison" => "#A040A0", // Roxo claro
                "ground" => "#E0C068", // Marrom
                "rock" => "#B8A038", // Bege
                "bug" => "#A8B820", // Verde claro
                "ghost" => "#705898", // Roxo escuro
                "steel" => "#B8B8D0", // Azul escuro
                "fire" => "#F08030", // Laranja
                "water" => "#6890F0", // Azul
                "grass" => "#78C850", // Verde escuro
                "electric" => "#F8D030", // Amarelo
                "psychic" => "#F85888", // Rosa forte
                "ice" => "#98D8D8", // Azul claro
                "dragon" => "#7038F8", // Azul meio roxo
                "dark" => "#705848", // Preto
                "fairy" => "#EE99AC", // Rosa claro
                "stellar" => "#78C850", // Verde azulado
                "unknown" => "#A8A878", // Cinza
                _ => "#A8A878" // Default para Cinza
            };
        }
    }
}

public class TypeDetails
{
    public string Name { get; set; }

    public string FormattedTypeName => char.ToUpper(Name[0]) + Name.Substring(1);
}