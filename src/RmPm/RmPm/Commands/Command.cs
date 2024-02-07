namespace RmPm.Commands;

public abstract class Command
{
    public abstract Task ExecuteAsync();
}