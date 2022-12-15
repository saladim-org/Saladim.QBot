using System.Reflection;
using SaladimQBot.Core;

namespace SaladimQBot.SimCommand;

public class SimCommandServiceConfig
{
    public string RootPrefix { get; set; }
    public Assembly ModulesAssembly { get; set; }

    public SimCommandServiceConfig(string rootPrefix, Assembly modulesAssembly)
    {
        this.RootPrefix = rootPrefix;
        this.ModulesAssembly = modulesAssembly;
    }
}
