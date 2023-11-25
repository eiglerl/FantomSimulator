namespace FantomSimulatorLibrary;

public enum LogType { Error, Move, Info }

public interface ILogger
{
    public void LogMessage(LogType type, string message);
}
