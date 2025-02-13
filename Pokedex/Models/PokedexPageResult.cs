namespace Pokedex.Models;

public class PokedexPageResult<T>
{
    public int Count { get; set; }

    public T[] Results { get; set; }
}