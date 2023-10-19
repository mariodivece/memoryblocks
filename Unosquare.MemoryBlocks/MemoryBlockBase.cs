namespace Unosquare.MemoryBlocks;

/// <summary>
/// Provides a base implementation for <see cref="IMemoryBlock"/>
/// which uses the <see cref="IMemoryAllocator"/> interface to perform
/// basic memory management tasks.
/// </summary>
public unsafe abstract class MemoryBlockBase<TAllocator> : IMemoryBlock
    where TAllocator : IMemoryAllocator
{
    private static long LastBlockId;
    private long m_IsDisposed;

    /// <summary>
    /// Creates a new instance of the <see cref="MemoryBlockBase{TAllocator}"/> class.
    /// </summary>
    /// <param name="byteLength">The number of bytes to allocate.</param>
    /// <param name="zeroed">Whether the allocated bytes should be zeroed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when byte length is less than zero.</exception>
    protected MemoryBlockBase(int byteLength, bool zeroed)
    {
        if (byteLength < 0)
            throw new ArgumentOutOfRangeException(nameof(byteLength));

        Address = zeroed && byteLength > 0
            ? TAllocator.AllocateZeroed(byteLength)
            : TAllocator.Allocate(byteLength);

        ByteLength = byteLength;
        BlockId = Interlocked.Increment(ref LastBlockId);
    }

    /// <inheridoc />
    public nint Address { get; private set; }

    /// <inheridoc />
    public void* Pointer => Address.ToPointer();

    /// <summary>
    /// Gets the unique identifier within the owning
    /// used as means to keep track and manage the lifecycle
    /// of this block of allocated memory.
    /// </summary>
    public long BlockId { get; }

    /// <inheridoc />
    public int ByteLength { get; private set; }

    /// <inheridoc />
    public object? Tag { get; set; }

    /// <inheridoc />
    public bool IsDisposed => Interlocked.Read(ref m_IsDisposed) != 0;

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(alsoManaged: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual int Fill(int startOffset, int byteLength, byte value)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        var safeSegment = this.SafeSegment<byte>(startOffset, byteLength);
        TAllocator.Fill(safeSegment.Address, safeSegment.ByteLength, value);
        return safeSegment.ByteLength;
    }

    /// <inheritdoc />
    public virtual int Clear(int startOffset, int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        var safeSegment = this.SafeSegment<byte>(startOffset, byteLength);
        TAllocator.Clear(safeSegment.Address, safeSegment.ByteLength);
        return safeSegment.ByteLength;
    }

    /// <inheritdoc />
    public virtual int Resize(int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        if (byteLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(byteLength));

        var originalLength = ByteLength;
        Address = TAllocator.Resize(Address, byteLength);
        ByteLength = byteLength;

        // ensure byte contents are zero for the newly allocated size.
        if (byteLength > originalLength)
            TAllocator.Clear(Address + originalLength, byteLength - originalLength);

        return byteLength;
    }

    /// <inheritdoc />
    public virtual int CopyTo(int startOffset, IMemoryBlock target, int targetOffset, int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.IsDisposed)
            throw new ObjectDisposedException(nameof(target));

        if (byteLength <= 0)
            return default;

        var sourceSegment = this.SafeSegment<byte>(startOffset, byteLength);
        var targetSegment = target.SafeSegment<byte>(targetOffset, byteLength);
        var safeByteLength = Math.Min(sourceSegment.ByteLength, targetSegment.ByteLength);

        if (safeByteLength > 0)
            TAllocator.Copy(
                sourceSegment.Address,
                targetSegment.Address,
                safeByteLength);

        return safeByteLength;
    }

    /// <inheritdoc />
    public virtual int CopyTo(int startOffset, nint bufferAddress, int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        if (bufferAddress == nint.Zero)
            throw new ArgumentNullException(nameof(bufferAddress));

        if (byteLength <= 0)
            return default;

        var sourceSegment = this.SafeSegment<byte>(startOffset, byteLength);

        TAllocator.Copy(sourceSegment.Address,
            bufferAddress,
            sourceSegment.ByteLength);

        return sourceSegment.ByteLength;
    }

    /// <inheritdoc />
    public virtual int CopyFrom(int startOffset, IMemoryBlock source, int sourceOffset, int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (source.IsDisposed)
            throw new ObjectDisposedException(nameof(source));

        if (byteLength <= 0)
            return default;

        return source.CopyTo(sourceOffset, this, startOffset, byteLength);
    }

    /// <inheritdoc />
    public virtual int CopyFrom(int startOffset, nint bufferAddress, int byteLength)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(IMemoryBlock));

        if (bufferAddress == nint.Zero)
            throw new ArgumentNullException(nameof(bufferAddress));

        if (byteLength <= 0)
            return default;

        var sourceSegment = this.SafeSegment<byte>(startOffset, byteLength);
        TAllocator.Copy(sourceSegment.Address,
            bufferAddress,
            sourceSegment.ByteLength);

        return sourceSegment.ByteLength;
    }

    /// <inheritdoc />
    protected virtual void Dispose(bool alsoManaged)
    {
        if (Interlocked.Increment(ref m_IsDisposed) > 1)
            return;

        if (alsoManaged)
        {
            if (Address != nint.Zero)
                TAllocator.Free(Address);

            Address = nint.Zero;
        }

        ByteLength = 0;
        Tag = null;
    }
}
