namespace Unosquare.MemoryBlocks;

/// <summary>
/// Defines static method to perform various generic operations on memory.
/// </summary>
public interface IMemoryAllocator
{
    /// <summary>
    /// Allocates a given number of bytes in memory and
    /// sets all of the values to 0.
    /// </summary>
    /// <param name="byteLength">The number of bytes to allocate.</param>
    /// <returns>A memory address where the memory block was allocated.</returns>
    static abstract nint AllocateZeroed(int byteLength);

    /// <summary>
    /// Allocates a given number of bytes in memory.
    /// Contents of memory are not guaranteed to be all zeroes.
    /// </summary>
    /// <param name="byteLength">The number of bytes to allocate.</param>
    /// <returns>A memory address where the memory block was allocated.</returns>
    static abstract nint Allocate(int byteLength);

    /// <summary>
    /// Releases the memory allocated at the given base address.
    /// </summary>
    /// <param name="baseAddress">The zeroth adderss of the allocated memory block.</param>
    static abstract void Free(nint baseAddress);

    /// <summary>
    /// Copies the specified number of bytes from a source memory address to a
    /// target memory address.
    /// </summary>
    /// <param name="sourceAddress">The source address from which to start reading bytes.</param>
    /// <param name="targetAddress">The target address from which to start writing bytes.</param>
    /// <param name="byteLength">The number of bytes to copy.</param>
    static abstract void Copy(nint sourceAddress, nint targetAddress, int byteLength);

    /// <summary>
    /// Sets all bytes to zero from the start address and up to the given number of bytes.
    /// </summary>
    /// <param name="startAddress">The start address from which to start clearing bytes.</param>
    /// <param name="byteLength">The number of bytes to clear.</param>
    static abstract void Clear(nint startAddress, int byteLength);

    /// <summary>
    /// Sets all bytes to the specified value from the start address and up to
    /// the given number of bytes.
    /// </summary>
    /// <param name="startAddress">The start address from which to start clearing bytes.</param>
    /// <param name="byteLength">The number of bytes to clear.</param>
    /// <param name="value">The value to fill the memory segment with.</param>
    static abstract void Fill(nint startAddress, int byteLength, byte value);

    /// <summary>
    /// Grows or shinks the allocated memory block. This is often cheaper
    /// than creating a new block from scratch and copying the contents over to the
    /// new block to then dispose of the existing block. While efficiency is dependent
    /// on the implementation of thi method, it is expected that some form of
    /// memory realloc is called. Growing the block is expected to keep existing
    /// values. Additional room might have undeterminate values.
    /// Shrinking the block should preserve and truncate existing data.
    /// </summary>
    /// <param name="baseAddress">The base address of the memory block to reallocate.</param>
    /// <param name="newByteLength">The new size in bytes of the block.</param>
    /// <returns>The address where the memory block was reallocated.</returns>
    static abstract nint Resize(nint baseAddress, int newByteLength);
}
