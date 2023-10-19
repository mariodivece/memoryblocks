using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Unosquare.MemoryBlocks;

/// <summary>
/// Provides extension methods for classes implementing the <see cref="IMemoryBlock"/> interface.
/// </summary>
public static unsafe class MemoryBlockExtensions
{
    /// <summary>
    /// Used for converting integers to native unsigned integers
    /// where byte lengths are required by convention.
    /// Negative values are sumply clamped to zero.
    /// </summary>
    /// <remarks>DO NOT USE FOR POINTER OR ADDRESS CONVERSION.</remarks>
    /// <param name="value">The value to be converted.</param>
    /// <returns>The unsigned native integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static nuint ToNuint(this int value) => value < 0
        ? default
        : (nuint)value;

    /// <summary>
    /// Converts an integer pointer to a native integer for addressing purposes.
    /// </summary>
    /// <param name="value">The pointer value.</param>
    /// <returns>The native integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint ToNint(this IntPtr value) => new(value.ToPointer());

    /// <summary>
    /// Converts an integer pointer to a native integer for addressing purposes.
    /// </summary>
    /// <param name="value">The pointer value.</param>
    /// <returns>The native integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint ToNint(this UIntPtr value) => new(value.ToPointer());

    /// <summary>
    /// Converts a long value to an int. This is useful for
    /// working with interop buffers that handle byte lengths as longs
    /// but there is a need to simplify usage with <see cref="IMemoryBlock"/>
    /// byte lengths.
    /// </summary>
    /// <param name="value">The long value.</param>
    /// <returns>The integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this long value) => value < int.MinValue || value > int.MaxValue
        ? throw new OverflowException($"{typeof(long).Name} '{value}' does not fit in an {typeof(int).Name}.")
        : (int)value;

    /// <summary>
    /// Converts a long value to an int. This is useful for
    /// working with interop buffers that handle byte lengths as longs
    /// but there is a need to simplify usage with <see cref="IMemoryBlock"/>
    /// byte lengths.
    /// </summary>
    /// <param name="value">The long value.</param>
    /// <returns>The integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this ulong value) => value > int.MaxValue
        ? throw new OverflowException($"{typeof(ulong).Name} '{value}' does not fit in an {typeof(int).Name}.")
        : (int)value;

    /// <summary>
    /// Protects <see cref="IMemoryBlock"/> addressing from
    /// invalid user input by performing bounds check on
    /// offset and count of a memory segment.
    /// Passing a negative offset will clamp such offset to 0.
    /// Passing an offset greater than the last possible memory address, will
    /// make such offset 'wrap around'.
    /// Passing a negative count will clamp such count to 0.
    /// Passing a count greater than what is possible to read from the starting
    /// offset, will clamp such count to the maximum possible.
    /// </summary>
    /// <param name="block">The block that contains the segment to address.</param>
    /// <param name="addressOffset">The number of bytes to skip from the base memory address.</param>
    /// <param name="itemCount">The number of bytes to take starting from the base memory address.</param>
    /// <returns>A <see cref="SafeMemorySegment{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the block is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SafeMemorySegment<T> SafeSegment<T>(this IMemoryBlock block, int addressOffset, int itemCount)
        where T : unmanaged
    {
        if (block is null)
            throw new ArgumentNullException(nameof(block));

        if (block.IsDisposed)
            throw new ObjectDisposedException(nameof(block));

        var safeOffset = addressOffset < 0
            ? default
            : addressOffset >= block.ByteLength
            ? addressOffset % block.ByteLength
            : addressOffset;

        var itemSize = sizeof(T);
        var byteLength = itemSize * itemCount;

        var safeByteLength = Math.Max(default,
            Math.Min(block.ByteLength - safeOffset, byteLength));

        safeByteLength = (safeByteLength / itemSize) * itemSize;

        return new()
        {
            ByteLength = safeByteLength,
            Address = block.Address + safeOffset,
        };
    }

    /// <summary>
    /// Reads an item of the specified type, starting at the specified offset.
    /// Safe addressing is prepared with a <see cref="SafeSegment"/> call.
    /// </summary>
    /// <typeparam name="T">The type to read from the block.</typeparam>
    /// <param name="block"></param>
    /// <param name="addressOffset">
    /// The offset to the <see cref="IMemoryBlock.Address"/> from which to start reading.
    /// </param>
    /// <returns>The specified item.</returns>
    public static T Read<T>(this IMemoryBlock block, int addressOffset)
        where T : unmanaged
    {
        var span = AsSpan<T>(block, addressOffset, 1);
        return (span.IsEmpty)
            ? throw new ArgumentOutOfRangeException(nameof(addressOffset),
                $"Not enough room to read {sizeof(T)} bytes for a '{typeof(T).Name}' at the given '{nameof(addressOffset)}'.")
            : span[0];
    }

    /// <summary>
    /// Attempts to read an item of the specified type, starting at the specified offset.
    /// Safe addressing is prepared with a <see cref="SafeSegment"/> call.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="block">The block to read from.</param>
    /// <param name="addressOffset">
    /// The offset to the <see cref="IMemoryBlock.Address"/> from which to start reading.
    /// </param>
    /// <param name="value">The output value.</param>
    /// <returns>True if the read succeeds. False otherwise.</returns>
    public static bool TryRead<T>(this IMemoryBlock block, int addressOffset, out T value)
        where T : unmanaged
    {
        value = default;

        if (block is null || block.IsDisposed)
            return false;

        var span = AsSpan<T>(block, addressOffset, 1);

        if (span.IsEmpty)
            return false;

        value = span[0];
        return true;
    }

    /// <summary>
    /// Writes an item of the specified type, starting at the specified offset.
    /// Safe addressing is prepared with a <see cref="SafeSegment"/> call.
    /// </summary>
    /// <typeparam name="T">The type to write to the block.</typeparam>
    /// <param name="block">The block to write to.</param>
    /// <param name="items">The data structure to be written to the block.</param>
    /// <param name="addressOffset">
    /// The offset to the <see cref="IMemoryBlock.Address"/> from which to start reading.
    /// </param>
    /// <returns>
    /// The number of bytes written.
    /// </returns>
    public static int Write<T>(this IMemoryBlock block, int addressOffset, params T[] items)
        where T : unmanaged
    {
        if (items is null || items.Length == 0)
            return default;

        var safeSegment = SafeSegment<T>(block, addressOffset, items.Length);

        if (safeSegment.ItemCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(addressOffset),
                $"Not enough room to write {safeSegment.ItemSize} bytes for a '{typeof(T).Name}' at the given '{nameof(addressOffset)}'.");

        var itemIndex = 0;
        for (var address = safeSegment.Address;
            address < safeSegment.Address + safeSegment.ByteLength;
            address += safeSegment.ItemSize)
        {
            Marshal.StructureToPtr(items[itemIndex], address, true);
            itemIndex++;
        }
        
        return itemIndex * safeSegment.ItemSize;
    }

    /// <summary>
    /// Attempts to write an item of the specified type, starting at the specified offset.
    /// Safe addressing is prepared with a <see cref="SafeSegment"/> call.
    /// </summary>
    /// <typeparam name="T">The element type to be written.</typeparam>
    /// <param name="block">The block to write to.</param>
    /// <param name="addressOffset">
    /// The offset to the <see cref="IMemoryBlock.Address"/> from which to start reading.
    /// </param>
    /// <param name="items">The data elements to be written to the block.</param>
    /// <returns>True if the write succeeds. False otherwise.</returns>
    public static bool TryWrite<T>(this IMemoryBlock block, int addressOffset, params T[] items)
        where T : unmanaged
    {
        if (block is null || block.IsDisposed)
            return false;

        if (items is null || items.Length == 0)
            return true;

        var safeSegment = SafeSegment<T>(block, addressOffset, items.Length);
        if (safeSegment.ItemCount <= 0)
            return false;

        var itemIndex = 0;
        for (var address = safeSegment.Address;
            address < safeSegment.Address + safeSegment.ByteLength;
            address += safeSegment.ItemSize)
        {
            Marshal.StructureToPtr(items[itemIndex], address, true);
            itemIndex++;
        }

        return true;
    }

    /// <summary>
    /// Creates a span of elements over the specified memory segment.
    /// </summary>
    /// <typeparam name="T">The data element type.</typeparam>
    /// <param name="block">The block to create the span from.</param>
    /// <param name="addressOffset">The address from which to start mapping bytes.</param>
    /// <param name="itemCount">The maximum number of elements to map.</param>
    /// <returns>A span that maps the memory block segment as data elements of the specified type.</returns>
    public static Span<T> AsSpan<T>(this IMemoryBlock block, int addressOffset, int itemCount)
        where T : unmanaged
    {
        if (block is null)
            throw new ArgumentNullException(nameof(block));

        if (block.IsDisposed)
            throw new ObjectDisposedException(nameof(block));

        if (itemCount <= 0)
            return Span<T>.Empty;

        var safeSegment = SafeSegment<T>(block, addressOffset, itemCount);
        return new(safeSegment.Address.ToPointer(), safeSegment.ItemCount);
    }

    /// <summary>
    /// Creates a span of elements over the specified memory segment.
    /// </summary>
    /// <typeparam name="T">The data element type.</typeparam>
    /// <param name="block">The block to create the span from.</param>
    /// <param name="addressOffset">The address from which to start mapping bytes.</param>
    /// <returns>A span that maps the memory block segment as data elements of the specified type.</returns>
    public static Span<T> AsSpan<T>(this IMemoryBlock block, int addressOffset)
        where T : unmanaged => AsSpan<T>(block, addressOffset, ((block?.ByteLength ?? 0) - addressOffset) / sizeof(T));

    /// <summary>
    /// Creates a span of elements over the entire memory block.
    /// </summary>
    /// <typeparam name="T">The data element type.</typeparam>
    /// <param name="block">The block to create the span from.</param>
    /// <returns>A span that maps the memory block as data elements of the specified type.</returns>
    public static Span<T> AsSpan<T>(this IMemoryBlock block)
        where T : unmanaged => AsSpan<T>(block, default, (block?.ByteLength ?? 0) / sizeof(T));

    /// <summary>
    /// Copies as much data as possible (the minimum between the source's and target's byte length)
    /// to from the source block to the target memory block.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int CopyTo(this IMemoryBlock source, IMemoryBlock target) =>
        source?.CopyTo(default, target, default, Math.Min(source.ByteLength, target?.ByteLength ?? 0)) ?? 0;

    /// <summary>
    /// A wrapper method for <see cref="IMemoryBlock.CopyTo(int, nint, int)"/>.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="startOffset">The start address offset.</param>
    /// <param name="bufferPointer">The pointer at which writing begins.</param>
    /// <param name="byteLength">The maximum number of bytes to write.</param>
    /// <returns>The number of bytes that were effectively written to the buffer.</returns>
    public static int CopyTo(this IMemoryBlock block, int startOffset, void* bufferPointer, int byteLength) => block is null
        ? throw new ArgumentNullException(nameof(block))
        : block.CopyTo(startOffset, new(bufferPointer), byteLength);

    /// <summary>
    /// A wrapper method for <see cref="IMemoryBlock.CopyFrom(int, nint, int)"/>.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="startOffset">The start address offset.</param>
    /// <param name="bufferPointer">The pointer at which reading begins.</param>
    /// <param name="byteLength">The maximum number of bytes to read.</param>
    /// <returns>The number of bytes that were effectively copied from the buffer.</returns>
    public static int CopyFrom(this IMemoryBlock block, int startOffset, void* bufferPointer, int byteLength) => block is null
        ? throw new ArgumentNullException(nameof(block))
        : block.CopyFrom(startOffset, new(bufferPointer), byteLength);

    /// <summary>
    /// Creates a <see cref="Stream"/> based on the contents
    /// of block data. Use this method with caution, as other
    /// method calls may interfere with the contents of the
    /// data which may result in unexpected memory state and
    /// corruption. It is highly recommended that all stream
    /// operations have a narrow scope, are short-lived, and no
    /// other read or write operations are performed on the
    /// block. Always dispose of the resulting stream.
    /// </summary>
    /// <param name="block">The block to create the sream from.</param>
    /// <param name="mode">Specifies the access mode for the stream.</param>
    /// <returns>A stream to access the underlying block data.</returns>
    public static Stream ToStream(this IMemoryBlock block, FileAccess mode) =>
        block is null
        ? throw new ArgumentNullException(nameof(block))
        : block.IsDisposed
        ? throw new ObjectDisposedException(nameof(IMemoryBlock))
        : new UnmanagedMemoryStream(
            (byte*)block.Pointer,
            block.ByteLength,
            block.ByteLength,
            mode);
}
