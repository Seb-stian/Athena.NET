﻿using System.Runtime.InteropServices;

namespace Athena.NET.Compilation.DataHolders;

public unsafe class NativeMemoryList<T> : IDisposable where T : struct //TODO: Implement an ICollection<T> interface
{
    protected Span<T> memoryBuffer => new Span<T>(MemoryPointer, allocationLength);
    private nuint memoryAlignment;
    private int dataSize;

    protected int allocationLength { get; private set; }

    public Span<T> Span => memoryBuffer[..Count];
    public void* MemoryPointer { get; internal set; }
    public int Count { get; protected set; }

    public NativeMemoryList(int allocationLength = 4)
    {
        dataSize = Marshal.SizeOf<T>();

        this.allocationLength = allocationLength;
        memoryAlignment = (nuint)Math.Pow(2, dataSize);
        MemoryPointer = NativeMemory.AlignedAlloc((nuint)(allocationLength * dataSize), memoryAlignment);
    }

    public void Add(T data)
    {
        if (Count == allocationLength)
            MemoryPointer = ReallocateMemory(4);
        memoryBuffer[Count] = data;
        Count++;
    }

    public void AddRange(Span<T> data)
    {
        int dataLength = data.Length;
        int countDifference = allocationLength - Count;
        if (dataLength > countDifference)
            MemoryPointer = ReallocateMemory(dataLength - countDifference);

        data.CopyTo(memoryBuffer[Count..]);
        Count += dataLength;
    }

    //TODO: Create Span<T> params in .NET 8
    public void AddRange(params T[] data)
    {
        Span<T> dataSpan = data.AsSpan();
        AddRange(dataSpan);
    }

    public void RemoveOn(int index)
    {
        memoryBuffer[(index + 1)..].CopyTo(memoryBuffer[index..]);
        ReallocateMemory(-1);
        Count--;
    }

    protected void* ReallocateMemory(int size)
    {
        allocationLength = allocationLength + size;
        return NativeMemory.AlignedRealloc(MemoryPointer, (nuint)(allocationLength * dataSize), memoryAlignment);
    }

    public void Dispose()
    {
        NativeMemory.AlignedFree(MemoryPointer);
    }
}
