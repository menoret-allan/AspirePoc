namespace TestAspire.AlgorithmDummy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.AddServiceDefaults();
        builder.AddRabbitMQClient("messaging");
        builder.Services.AddHostedService<AlgorithmWorker>();
        builder.Services.AddTransient<ChannelFactory>();

        var host = builder.Build();
        host.Run();
    }
}