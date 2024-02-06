using RmPm.Core.Services;

namespace RmPm.Core.Models;

public record BashRun(string Arguments) : RunArgs("/bin/bash", $"-c \"{Arguments}\"", true);