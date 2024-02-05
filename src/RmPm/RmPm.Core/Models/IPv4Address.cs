namespace RmPm.Core.Models;

// ReSharper disable once InconsistentNaming
public readonly record struct IPv4Address(string Ip, int Port)
{
    public static IPv4Address? Parse(string row)
    {
        var split = row.Split(":");
        if (split.Length != 2) 
            return null;

        if (int.TryParse(split[1], out var port))
        {
            return new IPv4Address(split[0], port);
        }

        return null;
    }
}