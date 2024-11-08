using System.Collections;

namespace GerberParser.Helpers;

public class Indexed<T> : IEnumerable<T> where T : notnull
{
    private List<T> Elements { get; set; } = [];
    private Dictionary<T, ulong> Indices { get; set; } = [];

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
