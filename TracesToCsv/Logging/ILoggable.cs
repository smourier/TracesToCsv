namespace TracesToCsv.Logging;

public interface ILoggable<out T>
{
    ILogger<T>? Logger { get; }
}

public interface ILoggable
{
    ILogger? Logger { get; }
}
