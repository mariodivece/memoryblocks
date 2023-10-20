# Native Memory Management in the form of Blocks!

*A no-frills unmanaged memory block library*

[![NuGet](https://img.shields.io/nuget/dt/Unosquare.MemoryBlocks)](https://www.nuget.org/packages/Unosquare.MemoryBlocks)

---

## Description

This small library allows programmers to allocate, read and write unmanaged memory blocks.
It is ideal for stuff like image buffers or interop scenarios.

Currently, the only implementation available is the ```NativeMemoryBlock``` class but the
librarry can be easily extended by implementing the ```IMemoryAllocator``` interface and
inheriting from the ```MemoryBlockBase``` class.
