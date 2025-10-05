namespace MiniUrl.Services;

public interface IUrlCounter
{
    Task<long> GetIncrementalCounter();
}
