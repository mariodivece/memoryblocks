using System.Runtime.CompilerServices;
namespace Unosquare.MemoryBlocks;

/// <summary>
/// Provides a <see cref="IMemoryBlock"/> implementation
/// using the <see cref="System.Runtime.InteropServices.NativeMemory"/> API.
/// </summary>
public class NativeMemoryBlock : MemoryBlockBase<NativeMemoryAllocator>
{
    /// <summary>
    /// Creates a new instance of the <see cref="NativeMemoryBlock" /> class.
    /// </summary>
    /// <param name="byteLength">The number of bytes to allocate for the block.</param>
    /// <param name="zeroed">True if contents should be all set to zeroes.</param>
    public NativeMemoryBlock(int byteLength, bool zeroed)
        : base(byteLength, zeroed)
    {
        // placeholder
    }

    /// <summary>
    /// Creates a new instance of the <see cref="NativeMemoryBlock" /> class.
    /// By default, this method will fill the block with all zeroes.
    /// </summary>
    /// <param name="byteLength">The number of bytes to allocate for the block.</param>
    public NativeMemoryBlock(int byteLength)
        : this(byteLength, true)
    {
        // placholder
    }

    /// <summary>
    /// Allocates a block and writes the specified items to it.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The elements to write.</param>
    /// <returns>The newly allocated memory block.</returns>
    public static IMemoryBlock Allocate<T>(params T[] items)
        where T : unmanaged
    {
        if (items is null || items.Length == 0)
            return new NativeMemoryBlock(0, false);

        var byteLength = items.Length * Unsafe.SizeOf<T>();
        var block = new NativeMemoryBlock(byteLength, zeroed: true);
        block.Write<T>(0, items);
        return block;
    }

    /// <summary>
    /// Allocates a block of memory that can hold a maximum
    /// of the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="itemCount">The number of elements to make room for.</param>
    /// <returns>The newly allocated memory block.</returns>
    public static IMemoryBlock Allocate<T>(int itemCount)
        where T : unmanaged
    {
        if (itemCount <= 0)
            return new NativeMemoryBlock(0, false);

        var byteLength = itemCount * Unsafe.SizeOf<T>();
        return new NativeMemoryBlock(byteLength, zeroed: true);
    }
}
