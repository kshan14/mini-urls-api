using MiniUrl.Extensions;
using MiniUrl.Models.Requests.MiniUrl;
using MiniUrl.Models.Responses.MiniUrl;

namespace MiniUrl.Services;

public class MiniUrlGenerator : IMiniUrlGenerator
{
    private readonly ILogger<MiniUrlGenerator> _logger;
    private readonly IBase62Encoder _base62Encoder;
    private readonly IUrlCounter _urlCounter;

    public MiniUrlGenerator(ILogger<MiniUrlGenerator> logger, IBase62Encoder base62Encoder, IUrlCounter urlCounter)
    {
        _logger = logger;
        _base62Encoder = base62Encoder;
        _urlCounter = urlCounter;
    }

    public async Task<CreateMiniUrlResponse> GenerateUrl(CreateMiniUrlRequest req)
    {
        // 1. Get counter
        var counter = await _urlCounter.GetIncrementalCounter();
        // 2. Generate Tiny Url on top of counter
        var miniUrl = _base62Encoder.Encode(counter);
        _logger.LogInformation("GenerateUrl with counter {Counter} and mini url {MiniUrl}", counter, miniUrl);
        // 3. Save into db
        return new CreateMiniUrlResponse
        {
            Id = Guid.NewGuid(),
            Url = req.Url,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ShortenedUrl = miniUrl
        };
    }
}
