namespace MiniUrl.Services;

public interface IUrlCacheService
{
    Task<string?> GetRedirectedUrl(string shortenedPath);
    Task SaveRedirectedUrl(string shortenedPath, string redirectedUrl);
    Task RemoveRedirectedUrl(string shortenedPath);
}
