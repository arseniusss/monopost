using Monopost.BLL.SocialMediaManagement.Posting;
using Monopost.Logging;
using WTelegram;
using Serilog;

LoggerConfig.ConfigureLogging();
TelegramPoster poster = new TelegramPoster("10439443", "2e780917ebaa32ded4269e5a4f14cf3a", "+380662706287", "-1002255912507", "&37ZuY9TYqngH^o7PEoH9SEYMQm2UazWN#awQN9mfnEtwvNnG6Mx5DzNXg^2FF&EK4&cyNr$6ibfB8sHM2YDY%^DpX9&2aVLS#bdt&Lhq4WtNJ^Wb%S2P#ju$a!qeWsT");
// TelegramPoster poster = new TelegramPoster("25172074", "df2b1683a35445f026d8802d456535a7", "+380957379130", "-1002255912507");
await poster.LoginAsync();
public class TelegramPoster
{
    private ILogger logger = LoggerConfig.GetLogger();

    private readonly Client _client;
    private readonly string _channelId;
    public TelegramPoster(string apiId, string apiHash, string phoneNumber, string channelId, string? password = null)
    {
        string sessionFilePath = "telegram_session_.dat";

        logger.Information($"api_id={apiId}, apiHash={apiHash}, phone={phoneNumber}, pass={password}");
        _client = new Client(Config);
        logger.Information("hi");

        _channelId = channelId;
        string Config(string what)
        {
            return what switch
            {
                "api_id" => apiId,
                "api_hash" => apiHash,
                "phone_number" => phoneNumber,
                "password" => password ?? string.Empty,
                "verification_code" => GetVerificationCode(),
                "session_pathname" => sessionFilePath,
                _ => null,
            };
        }

        string GetVerificationCode()
        {
            Console.Write("Code: ");
            return Console.ReadLine() ?? string.Empty;
        }
        // _client.LoginUserIfNeeded().Wait();
    }

    public async Task LoginAsync()
    {
        var user = await _client.LoginUserIfNeeded();
        logger.Information($"logging into telegram");
    }
}