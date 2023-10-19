using System.Runtime.InteropServices;

namespace Unosquare.MemoryBlocks;

/// <summary>
/// Uses the <see cref="NativeMemory"/> class to
/// perform various memory operations.
/// </summary>
public unsafe class NativeMemoryAllocator : IMemoryAllocator
{
    /// <inheritdoc />
    public static nint Allocate(int byteLength) =>
        new(NativeMemory.Alloc(byteLength.ToNuint()));

    /// <inheritdoc />
    public static nint AllocateZeroed(int byteLength) =>
        new(NativeMemory.AllocZeroed(byteLength.ToNuint()));

    /// <inheritdoc />
    public static void Clear(nint startAddress, int byteLength) =>
        NativeMemory.Clear(startAddress.ToPointer(), byteLength.ToNuint());

    /// <inheritdoc />
    public static void Copy(nint sourceAddress, nint targetAddress, int byteLength) =>
        NativeMemory.Copy(sourceAddress.ToPointer(), targetAddress.ToPointer(), byteLength.ToNuint());

    /// <inheritdoc />
    public static void Fill(nint startAddress, int byteLength, byte value) =>
        NativeMemory.Fill(startAddress.ToPointer(), byteLength.ToNuint(), value);

    /// <inheritdoc />
    public static void Free(nint baseAddress) =>
        NativeMemory.Free(baseAddress.ToPointer());

    /// <inheritdoc />
    public static nint Resize(nint baseAddress, int newByteLength) =>
        new(NativeMemory.Realloc(baseAddress.ToPointer(), newByteLength.ToNuint()));

}
