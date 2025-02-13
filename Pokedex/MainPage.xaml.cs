using System.Collections.ObjectModel;
using Pokedex.Services;

namespace Pokedex;

public partial class MainPage : ContentPage
{
    private int _currentPage = 0;
    private const int _pageSize = 20; // Número de Pokémons por página
    private readonly PokedexService _pokedexService;
    private ObservableCollection<Pokemon> _pokemons;
    private List<Pokemon> _allPokemons; // Lista para manter todos os pokémons carregados
    private bool _isLoading;
    private bool _isSearching; // Variável para interromper o carregamento durante a busca

    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _pokedexService = Shell.Current.Handler.MauiContext.Services.GetService<PokedexService>();
        _pokemons = new ObservableCollection<Pokemon>();
        _allPokemons = new List<Pokemon>(); // Inicializa a lista de todos os pokémons
        collectionPokemons.ItemsSource = _pokemons;
        collectionPokemons.SelectionChanged += OnPokemonSelected;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_pokemons.Count == 0) // Carregar apenas uma vez
        {
            LoadPokemons(); // Carrega a primeira página de pokémons
        }
    }

    private async void LoadPokemons()
    {
        if (_isLoading || _isSearching) // Verifica se está buscando por pokémons
            return;

        _isLoading = true;

        try
        {
            // Carrega a próxima página de pokémon
            var pokemons = await _pokedexService.GetPokemonsPageAsync(_currentPage * _pageSize, _pageSize, CancellationToken.None);

            foreach (var pokemon in pokemons)
            {
                if (!_allPokemons.Contains(pokemon))
                {
                    _pokemons.Add(pokemon);
                    _allPokemons.Add(pokemon); // Adiciona à lista completa
                }
            }

            _currentPage++; // Incrementa para próxima página
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Failed to load Pokémon data.", "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (!_isSearching && _pokemons.Count <= _currentPage * _pageSize + 4) // Verifica se a contagem de pokémons é menor ou igual ao limite
        {
            LoadPokemons();
        }
    }

    // Função de pesquisa para a barra de busca
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? string.Empty;
        // Define o Searching como True e não deixa a lista continuar a ser carregada
        _isSearching = !string.IsNullOrWhiteSpace(searchText);
        FilterPokemons(searchText);
    }

    private void FilterPokemons(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            UpdateDisplayedPokemons(_allPokemons); // Mostra todos os pokémons se não houver texto de busca
            _isSearching = false; // Retoma o carregamento contínuo quando não está buscando
        }
        else
        {
            var filteredPokemons = _allPokemons
                .Where(p => p.Name.ToLower().StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            UpdateDisplayedPokemons(filteredPokemons);
        }   
    }

    private void UpdateDisplayedPokemons(List<Pokemon> pokemons)
    {
        _pokemons.Clear();

        foreach (var pokemon in pokemons)
        {
            _pokemons.Add(pokemon);
        }
    }

    private void OnPokemonSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            // Navega para a página de detalhes, passando o Pokémon selecionado
            var selectedPokemon = e.CurrentSelection[0] as Pokemon;

            // Navega para a página de detalhes, passando o Pokémon completo
            Navigation.PushAsync(new PokemonDetailPage(selectedPokemon));
        }

        // Limpe a seleção para permitir que o mesmo item seja selecionado novamente
        ((CollectionView)sender).SelectedItem = null;
    }
}