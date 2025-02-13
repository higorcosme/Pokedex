using Pokedex.Services;

namespace Pokedex;

public partial class PokemonDetailPage : ContentPage
{
	public PokemonDetailPage(Pokemon selectedPokemon)
	{
		InitializeComponent();
        BindingContext = selectedPokemon;
    }
}