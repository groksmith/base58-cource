using Amazon.Lambda.AspNetCoreServer;

namespace BtcTransactionParser;

public class LambdaFunction : APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .UseLambdaServer();
    }
}