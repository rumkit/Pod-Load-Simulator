using System.Runtime.InteropServices;

namespace PodLoad.Client;

public class MemoryKeeper : IDisposable
{
    private IntPtr _memoryChunk;
    private int _previousChunkSize;
    
    public void Allocate(int bytesCount)
    {
        if (bytesCount == _previousChunkSize)
            return;
        
        if (_memoryChunk != IntPtr.Zero)
            Marshal.FreeHGlobal(_memoryChunk);
        _previousChunkSize = bytesCount;
        _memoryChunk = Marshal.AllocHGlobal(bytesCount);
    }
    public void Dispose()
    {
        if (_memoryChunk != IntPtr.Zero)
            Marshal.FreeHGlobal(_memoryChunk);
    }
}