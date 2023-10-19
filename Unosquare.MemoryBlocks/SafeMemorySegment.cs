using System.Runtime.CompilerServices;

namespace Unosquare.MemoryBlocks;

/// <summary>
/// Represents a segment of memory computed from an offset and a count.
/// </summary>
public readonly ref struct SafeMemorySegment<T>
    where T : unmanaged
{
    /// <summary>
    /// Creates a new instance of the <see cref="SafeMemorySegment{T}"/> structure.
    /// </summary>
    public SafeMemorySegment()
    {
        ItemSize = Unsafe.SizeOf<T>();
    }

    /// <summary>
    /// Gets the zeroth memory address of this segment.
    /// </summary>
    public readonly nint Address { get; init; }

    /// <summary>
    /// Gets the total number of bytes that are safe to address.
    /// </summary>
    public readonly int ByteLength { get; init; }

    /// <summary>
    /// Gets the size in bytes of each of the addressable items.
    /// </summary>
    public readonly int ItemSize { get; }

    /// <summary>
    /// Gets the total number of items that are addressable.
    /// </summary>
    public readonly int ItemCount => ByteLength / ItemSize;

    /// <summary>
    /// Gets whether the size of this segment zero.
    /// </summary>
    public bool IsEmpty => ByteLength <= 0;
}