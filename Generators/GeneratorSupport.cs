#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace DotNetSourceGeneratorToolkit.Generators;

/// <summary>
/// A value-equatable wrapper around <see cref="ImmutableArray{T}"/>. Incremental generators
/// compare pipeline outputs structurally to decide whether to re-run downstream steps;
/// <see cref="ImmutableArray{T}"/> compares by reference, which silently defeats caching.
/// This wrapper restores element-wise equality so cached steps stay cached.
/// </summary>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array) => _array = array;

    public int Count => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];

    public bool Equals(EquatableArray<T> other)
    {
        if (_array.IsDefault || other._array.IsDefault)
            return _array.IsDefault && other._array.IsDefault;

        if (_array.Length != other._array.Length)
            return false;

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (_array.IsDefault)
            return 0;

        var hash = new HashCode();
        foreach (var item in _array)
            hash.Add(item);
        return hash.ToHashCode();
    }

    public IEnumerator<T> GetEnumerator() =>
        (_array.IsDefault ? ImmutableArray<T>.Empty : _array).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Minimal indentation-aware string builder used to keep generated source readable
/// without pulling in a formatting dependency.
/// </summary>
internal sealed class IndentedWriter
{
    private readonly StringBuilder _sb = new();
    private int _depth;

    public void Indent() => _depth++;

    public void Outdent() => _depth = Math.Max(0, _depth - 1);

    public void Blank() => _sb.AppendLine();

    public void Line(string text)
    {
        if (text.Length > 0)
            _sb.Append(new string(' ', _depth * 4));
        _sb.AppendLine(text);
    }

    public override string ToString() => _sb.ToString();
}
