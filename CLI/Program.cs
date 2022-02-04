using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLI.Commands;
using Spectre.Console;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            List<ICommand> commands = new()
            {
                new RunCommand(),
                new DebugCommand(),
                new DeployCommand()
            };

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold white]ITRG CLI[/]").LeftAligned().RuleStyle("#fb8500"));
            AnsiConsole.WriteLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#ffb703]Select Subcommand[/]")
                    .AddChoices(commands.Select(cmd => cmd.Name))
                    .HighlightStyle(Style.Parse("#219ebc bold"))
                );

            ICommand command = commands.FirstOrDefault(cmd => cmd.Name.Equals(selection, StringComparison.CurrentCultureIgnoreCase));
            command.Run();
        }
    }
}
