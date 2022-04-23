using Amazon.Lambda.AspNetCoreServer;

namespace LegacyTransactionParser;

public class LambdaFunction : APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Program>()
            .UseLambdaServer();
    }
}