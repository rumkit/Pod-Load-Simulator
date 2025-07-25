using System.Runtime.InteropServices;

namespace PodLoad.Client;

public unsafe class MemoryKeeper : IDisposable
{
    private uint _previousChunkSize;
    private void* _memoryChunk; 
    private bool _disposed;
    
    public void Allocate(uint bytesCount)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(MemoryKeeper));
        
        if (bytesCount == _previousChunkSize)
            return;
        
        _previousChunkSize = bytesCount;
        NativeMemory.Free(_memoryChunk);
        _memoryChunk = NativeMemory.Alloc(new UIntPtr(_previousChunkSize));
    }
    public void Dispose()
    {
        _disposed = true;
        NativeMemory.Free(_memoryChunk);
        GC.SuppressFinalize(this);
    }

    ~MemoryKeeper()
    {
        NativeMemory.Free(_memoryChunk);
    }
}