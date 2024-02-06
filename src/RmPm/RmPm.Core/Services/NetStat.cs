using System.Text.RegularExpressions;
using RmPm.Core.Contracts;
using RmPm.Core.Models;

namespace RmPm.Core.Services;

public partial class NetStat
{
    private readonly IProcessManager _pm;

    public NetStat(IProcessManager pm)
    {
        _pm = pm;
    }

    public async Task<IEnumerable<NetListener>> GetListenersAsync()
    {
        const string state = "LISTEN";
        
        var input = await _pm.RunAsync(new BashNetListeners(state));

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidOperationException("[netstat] Failed to receive net listeners list");
        }
        
        var results = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Select(line => new NetListener(
                string.Empty,
                IpRegex().Match(line).Value,
                state,
                PidRegex().Match(line).Value,
                ProgramNameRegex().Match(line).Value
            )).ToList();

        return results;
    }

    [GeneratedRegex(@"([0-9.]*)(\/|:)([0-9]*)")]
    private static partial Regex IpRegex();
    
    [GeneratedRegex(@"\d+(?=/)")]
    private static partial Regex PidRegex();
    
    [GeneratedRegex(@"([^\/]+$)")]
    private static partial Regex ProgramNameRegex(); // TODO: FIX IT (bad: "13019/sshd: /usr/sb" -> result: "sb")
}

public record NetListener(string Proto, string LocalAddress, string State, string Pid, string ProgramName);