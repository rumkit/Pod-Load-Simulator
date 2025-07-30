using System.Diagnostics;

namespace PodLoad.Client;

public class CpuLoadSimulator
{
    private readonly CancellationToken _externalStoppingToken;
    private CancellationTokenSource _internalCancellationTokenSource;
    private int _percentage;

    public int Percentage
    {
        get => _percentage;
        set
        {
            if(_percentage == value) 
                return;
            
            if (value is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(Percentage), "Percentage must be between 0 and 100");
            
            _percentage = value;
            _internalCancellationTokenSource.Cancel();
            _internalCancellationTokenSource = new CancellationTokenSource();
            SimulateCpuLoad();
        }
    }

    public CpuLoadSimulator(int percentage, CancellationToken externalStoppingToken)
    {
        _externalStoppingToken = externalStoppingToken;
        _internalCancellationTokenSource = new CancellationTokenSource();
        Percentage = percentage;
    }

    private void SimulateCpuLoad()
    {
        if (_percentage == 0)
            return;
        
        var internalToken = _internalCancellationTokenSource.Token;
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            var thread = new Thread(() =>
            {
                var watch = new Stopwatch();
                watch.Start();
                while (!_externalStoppingToken.IsCancellationRequested && !internalToken.IsCancellationRequested)
                {
                    // Make the loop go on for "percentage" milliseconds then sleep the 
                    // remaining percentage milliseconds. So 40% utilization means work 40ms and sleep 60ms
                    if (watch.ElapsedMilliseconds > _percentage)
                    {
                        Thread.Sleep(100 - _percentage);
                        watch.Reset();
                        watch.Start();
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }
}