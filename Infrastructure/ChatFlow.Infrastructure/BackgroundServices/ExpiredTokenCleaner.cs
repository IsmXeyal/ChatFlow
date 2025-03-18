//using ChatFlow.Infrastructure.Services;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;

//namespace ChatFlow.Infrastructure.BackgroundServices;

//public class ExpiredTokenCleaner : BackgroundService
//{
//    private readonly TokenService tokenService;
//    private readonly ILogger<ExpiredTokenCleaner> logger;
//    private Timer timer;

//    public ExpiredTokenCleaner(TokenService tokenService, ILogger<ExpiredTokenCleaner> logger, Timer timer)
//    {
//        this.tokenService = tokenService;
//        this.logger = logger;
//        this.timer = timer;
//    }

//    public override async Task StartAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Start BackgroundService ExpiredTokenCleaner");

//        await base.StartAsync(cancellationToken);
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        var startMessage = "Start executing RemoveExpiredTokensAsync";
//        var executeMessage = "Executing RemoveExpiredTokensAsync";

//        this.logger.LogInformation(startMessage);
//        await Console.Out.WriteLineAsync(startMessage);

//        // Set the timer to run the task repeatedly
//        this.timer = new Timer(async _ =>
//        {
//            this.logger.LogInformation(executeMessage);
//            await Console.Out.WriteLineAsync(executeMessage);
//            await tokenService.RemoveExpiredTokenAsync();
//        },
//        null,
//        TimeSpan.Zero, // Start immediately
//        TimeSpan.FromMinutes(3)); // Execute every 3 minutes
//    }

//    public override async Task StopAsync(CancellationToken cancellationToken)
//    {
//        this.logger.LogInformation("Stopping BackgroundService ExpiredTokenCleaner");

//        // Dispose the timer to release resources
//        if (this.timer != null)
//        {
//            await this.timer.DisposeAsync();
//        }

//        await base.StopAsync(cancellationToken);
//    }
//}
