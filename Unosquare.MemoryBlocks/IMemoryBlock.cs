namespace Unosquare.MemoryBlocks;

/// <summary>
/// Represents an unmanaged section of memory.
/// </summary>
public unsafe interface IMemoryBlock : IDisposable
{
    /// <summary>
    /// Gets the base (zeroth) memory address where this this
    /// <see cref="IMemoryBlock"/> is allocated.
    /// </summary>
    nint Address { get; }

    /// <summary>
    /// Gets the unmanged pointer to the base (zeroth) memory
    /// location of where this <see cref="IMemoryBlock"/> is
    /// allocated.
    /// </summary>
    void* Pointer { get; }

    /// <summary>
    /// Gets the total number of bytes that have been allocated
    /// to this block of memory.
    /// </summary>
    int ByteLength { get; }

    /// <summary>
    /// Gets or sets an object as means to store data that is
    /// closely related to this block.
    /// </summary>
    object? Tag { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Fills the memory values starting at the given offset and
    /// up to the specified number of bytes with the specified value.
    /// </summary>
    /// <param name="startOffset">The byte offset from which to start filling values.</param>
    /// <param name="byteLength">The number of bytes to fill with the specified value.</param>
    /// <param name="value">The value to write to the memory segment.</param>
    /// <returns>The number of bytes that were written.</returns>
    int Fill(int startOffset, int byteLength, byte value);

    /// <summary>
    /// Reallocates the memory block. This may or may not
    /// change the <see cref="Address"/> of this block.
    /// </summary>
    /// <param name="byteLength">The new allocation size in bytes.</param>
    /// <returns>The number of bytes that were allocated.</returns>
    int Resize(int byteLength);

    /// <summary>
    /// Fills the memory values starting at the given offset and
    /// up to the specified number of bytes with zeroes.
    /// </summary>
    /// <param name="startOffset">The byte offset from which to start filling values.</param>
    /// <param name="byteLength">The number of bytes to fill with zeroes.</param>
    /// <returns>The number of bytes that were written.</returns>
    int Clear(int startOffset, int byteLength);

    /// <summary>
    /// Copies data from the given start offset and on to another memory block,
    /// starting at the target offset and up to the specified number of bytes.
    /// </summary>
    /// <param name="startOffset">The offset at which data starts to be read at the source.</param>
    /// <param name="target">The target memory block.</param>
    /// <param name="targetOffset">The offset at which data starts to get written.</param>
    /// <param name="byteLength">The number of bytes to copy.</param>
    /// <returns>The number of bytes that were effectively copied.</returns>
    int CopyTo(int startOffset, IMemoryBlock target, int targetOffset, int byteLength);

    /// <summary>
    /// Writes data to this memory block from the speicifed source.
    /// </summary>
    /// <param name="startOffset">The offset at which to start writing data.</param>
    /// <param name="source">The memory block to read data from.</param>
    /// <param name="sourceOffset">The offset at which to start reading data.</param>
    /// <param name="byteLength">The number of bytes to copy.</param>
    /// <returns>The number of bytes that were effectively copied.</returns>
    int CopyFrom(int startOffset, IMemoryBlock source, int sourceOffset, int byteLength);

    /// <summary>
    /// Copies data from the given start offset and on to a given memory address,
    /// and up to the specified number of bytes.
    /// </summary>
    /// <param name="startOffset">The offset at which data starts to be read at the source.</param>
    /// <param name="bufferAddress">The buffer address at which data starts to be written.</param>
    /// <param name="byteLength">The number of bytes to copy.</param>
    /// <returns>The number of bytes that were effectively copied.</returns>
    int CopyTo(int startOffset, nint bufferAddress, int byteLength);

    /// <summary>
    /// Copies data from a memory address, and on to this memory block
    /// starting at the specified start offset.
    /// </summary>
    /// <param name="startOffset">The offset at which data starts to be written.</param>
    /// <param name="bufferAddress">The memory address from which to start reading data.</param>
    /// <param name="byteLength">The number of bytes to copy.</param>
    /// <returns>The number of bytes that were effectively copied.</returns>
    int CopyFrom(int startOffset, nint bufferAddress, int byteLength);

}
