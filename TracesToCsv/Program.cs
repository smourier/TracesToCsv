namespace TracesToCsv;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOptions<TracesOptions>()
            .Bind(builder.Configuration.GetSection(TracesOptions.Traces))
            .ValidateDataAnnotations();

        builder.Services.AddLogging(config =>
        {
            config.AddEventProvider();
        });

        builder.Services.AddControllers()
            .AddJsonOptions(config =>
            {
                config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
