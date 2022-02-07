using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.CLICommand
{
    public class ResetCommand : Command<ResetCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<CONFIGURATION>")]
            public string Configuration { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            string location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", settings.Configuration + ".json");
            if (!File.Exists(location))
            {
                AnsiConsole.MarkupLine($"[red]{settings.Configuration}.json does not exist![/]");
                return 1;
            }

            File.Delete(location);
            new Configuration();
            new ProjectsConfiguration();
            new ServersConfiguration();

            return 0;
        }
    }
}
