namespace TracesToCsv.Logging;

public static class EventProviderLoggerExtensions
{
    [RequiresUnreferencedCode("")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static ILoggingBuilder AddEventProvider(this ILoggingBuilder builder, Action<EventProviderLoggerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // fail gracefully
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return builder;

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, EventProviderLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<EventProviderLoggerConfiguration, EventProviderLoggerProvider>(builder.Services);

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }
        return builder;
    }
}
