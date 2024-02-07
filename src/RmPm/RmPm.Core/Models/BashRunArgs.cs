using RmPm.Core.Services;

namespace RmPm.Core.Models;

// ReSharper disable file NotAccessedPositionalProperty.Global

public record BashRun(string Arguments) : RunArgs("/bin/bash", $"-c \"{Arguments}\"", true);
public record BashRunDetached(string Arguments) : BashRun(Arguments + " & ");

public record BashNetListeners(string State) : BashRun("sudo netstat -tulpn | grep " + State);
public record BashKill(string Pid) : BashRun("kill " + Pid);
public record BashRunSocks(string ConfigPath) : BashRunDetached($"ss-server -c {ConfigPath}");
