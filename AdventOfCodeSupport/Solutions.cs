using System.Collections;

namespace AdventOfCodeSupport;

public class Solutions : IEnumerable<IAoC>
{
    private readonly List<IAoC> _list = new();

    public Solutions()
    {
        var baseType = typeof(AoCBase);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract);
        foreach (var type in types)
        {
            _list.Add((IAoC)Activator.CreateInstance(type)!);
        }
    }

    public IEnumerator<IAoC> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
