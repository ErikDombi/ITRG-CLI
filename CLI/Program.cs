using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLI.CLICommand;
using CLI.Commands;
using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI
{
    class Program
    {
        public static Configuration.Instance Configuration = new Configuration();

        static int Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.AddCommand<ResetCommand>("reset");
                config.AddCommand<DebugCommand>("debug");
                config.AddCommand<RunCommand>("run");
                config.AddCommand<DeployCommand>("deploy");
                config.SetApplicationName("cli");
            });

            if (args.Length > 0)
                return app.Run(args);

            List<ICommand> commands = new()
            {
                new RunCommand(),
                new DebugCommand(),
                new DeployCommand(),
                new ExitCommand()
            };

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold white]ITRG CLI[/]").LeftAligned().RuleStyle(Configuration.Palette.Primary));
            AnsiConsole.WriteLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Configuration.Palette.Secondary}]Select Subcommand[/]")
                    .AddChoices(commands.Select(cmd => cmd.Name))
                    .HighlightStyle(Style.Parse(Configuration.Palette.Highlight))
                );

            ICommand command = commands.FirstOrDefault(cmd => cmd.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase));
            try
            {
                command.Run();
            } catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return 1;
            }

            return 0;
        }
    }
}
