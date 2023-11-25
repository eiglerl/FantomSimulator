namespace FantomSimulatorLibrary;

public class ConsoleLogger : ILogger
{
    private int _verbosity;
    public ConsoleLogger(int verbosity=0)
    {
        _verbosity = verbosity;
    }
    public void LogMessage(LogType type, string message)
    {
        int verbosityLever = logTypeToVerbosityLevel(type);
        if (verbosityLever <= _verbosity)
            Console.WriteLine(message);
    }

    private int logTypeToVerbosityLevel(LogType type)
    {
        return type switch
        {
            LogType.Error => 0,
            LogType.Move => 1,
            LogType.Info => 2,
            _ => 0
        };
    }
}
