using System.Collections;

namespace GerberParser.Helpers;

public class Indexed<T> : IEnumerable<T>
{
    private List<T> Elements { get; set; } = new();
    private Dictionary<T, ulong> Indices { get; set; } = new();

    public ulong Add(T element)
    {
        if (Indices.TryGetValue(element, out ulong index))
        {
            return index;
        }
        Elements.Add(element);
        index = (ulong)Elements.Count;
        Indices[element] = index;

        return index;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Elements.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
