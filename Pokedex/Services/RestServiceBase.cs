using System.Reflection;
using System.Text.Json;
using System.Text;

namespace Pokedex.Services;

/// <summary>
/// Classe base para implementações de serviço HTTP Rest.
/// </summary>
public abstract class RestServiceBase
{
    private const string MEDIA_TYPE = "application/json";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public RestServiceBase(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Envia uma requisição GET para o URI especificado e retorna a resposta no tipo especificado.
    /// </summary>
    /// <typeparam name="TRequest">O tipo do objeto de requisição.</typeparam>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="uri">O URI para o qual enviar a requisição GET.</param>
    /// <param name="request">O objeto de requisição a ser incluído na string de consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>A resposta recebida da requisição GET.</returns>
    protected async Task<TResponse> GetAsync<TRequest, TResponse>(string uri, TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var queryString = BuildQueryString(request);
            var requestUri = string.IsNullOrWhiteSpace(queryString) ? uri : $"{uri}?{queryString}";
            var httpResponse = await _httpClient.GetAsync(requestUri, cancellationToken);

            return await GetResponse<TResponse>(httpResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Envia uma requisição GET (GET Request) para o URI especificado e retorna a resposta no tipo especificado.
    /// </summary>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="uri">O URI para o qual enviar a requisição GET.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>A resposta recebida da requisição GET.</returns>
    protected async Task<TResponse> GetAsync<TResponse>(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var httpResponse = await _httpClient.GetAsync(uri, cancellationToken);

            return await GetResponse<TResponse>(httpResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Envia uma requisição POST para o URI especificado com o objeto de requisição fornecido e retorna a resposta no tipo especificado.
    /// </summary>
    /// <typeparam name="TRequest">O tipo do objeto de requisição.</typeparam>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="uri">O URI para o qual enviar a requisição POST.</param>
    /// <param name="request">O objeto de requisição a ser enviado na requisição POST.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>A resposta recebida da requisição POST.</returns>
    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var serializedRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            var content = new StringContent(serializedRequest, Encoding.UTF8, MEDIA_TYPE);
            var httpResponse = await _httpClient.PostAsync(uri, content, cancellationToken);

            return await GetResponse<TResponse>(httpResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Envia uma requisição PUT para o URI especificado com o objeto de requisição fornecido e retorna a resposta no tipo especificado.
    /// </summary>
    /// <typeparam name="TRequest">O tipo do objeto de requisição.</typeparam>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="uri">O URI para o qual enviar a requisição PUT.</param>
    /// <param name="request">O objeto de requisição a ser enviado na requisição PUT.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>A resposta recebida da requisição PUT.</returns>
    protected async Task<TResponse> PutAsync<TRequest, TResponse>(string uri, TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var serializedRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            var content = new StringContent(serializedRequest, Encoding.UTF8, MEDIA_TYPE);

            var httpResponse = await _httpClient.PutAsync(uri, content, cancellationToken);

            return await GetResponse<TResponse>(httpResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Envia uma requisição DELETE para o URI especificado e retorna a resposta no tipo especificado.
    /// </summary>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="uri">O URI para o qual enviar a requisição DELETE.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>A resposta recebida da requisição DELETE.</returns>
    protected async Task<TResponse> DeleteAsync<TResponse>(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var httpResponse = await _httpClient.DeleteAsync(uri, cancellationToken);

            return await GetResponse<TResponse>(httpResponse, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Lê o conteúdo da mensagem de resposta HTTP e o desserializa no tipo de resposta especificado.
    /// </summary>
    /// <typeparam name="TResponse">O tipo do objeto de resposta.</typeparam>
    /// <param name="httpResponseMessage">A mensagem de resposta HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento para permitir a interrupção do processamento, se necessário.</param>
    /// <returns>O objeto de resposta desserializado.</returns>
    protected async Task<TResponse> GetResponse<TResponse>(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
        try
        {
            var streamContent = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<TResponse>(streamContent, _jsonSerializerOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    /// <summary>
    /// Constrói uma string de consulta a partir das propriedades do objeto de requisição fornecido.
    /// </summary>
    /// <typeparam name="TRequest">O tipo do objeto de requisição.</typeparam>
    /// <param name="request">O objeto de requisição.</param>
    /// <returns>A string de consulta construída.</returns>
    private static string BuildQueryString<TRequest>(TRequest request)
    {
        var queryParameters = new List<string>();
        var properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(request);

            if (value is not null)
            {
                var encodedValue = Uri.EscapeDataString(value.ToString()!);
                var parameter = $"{property.Name}={encodedValue}";

                queryParameters.Add(parameter);
            }
        }

        return string.Join('&', queryParameters);
    }
}