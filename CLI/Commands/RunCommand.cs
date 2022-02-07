using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands
{
    public class RunCommand : Command<RunCommand.Settings>, ICommand
    {
        readonly ProjectsConfiguration projectsConfiguration = new ProjectsConfiguration();

        string project_argument = null;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[project]")]
            public string Project { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            project_argument = settings.Project;
            Run();
            return 0;
        }

        public string Name => "run";

        public Task Run()
        {
            var selection = project_argument ?? AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Program.Configuration.Palette.Primary}]Select a project to run[/]")
                    .AddChoices(projectsConfiguration.Data.ReleaseProjects.Select(proj => proj.Name))
                    .HighlightStyle(Style.Parse(Program.Configuration.Palette.Highlight))
                );

            Project project = projectsConfiguration.Data.ReleaseProjects.FirstOrDefault(proj => proj.Name == selection);

            Panel selectedProject = new Panel(new Markup($"[{Program.Configuration.Palette.Primary} bold]Command: [/][{Program.Configuration.Palette.Tertiary}]{project.Command}[/]\n[{Program.Configuration.Palette.Primary} bold]Directory: [/][{Program.Configuration.Palette.Tertiary}]{project.Directory}[/]"))
                .RoundedBorder()
                .BorderStyle(Style.Parse(Program.Configuration.Palette.Secondary))
                .Padding(2, 1);
            selectedProject.Header = new PanelHeader($"| [white bold]{project.Name}[/] |", Justify.Left);

            AnsiConsole.Write(selectedProject);
            AnsiConsole.WriteLine();
            bool directoryExists = Directory.Exists(project.Directory);

            AnsiConsole.Status()
                .Start($"[green bold]Locating [white bold]{project.Name}[/]...[/]", (ctx) =>
                {
                    ctx.Spinner(Spinner.Known.Default);

                    Thread.Sleep(2000);

                    if (!directoryExists)
                    {
                        AnsiConsole.MarkupLine($"[red]ERR: [/][grey82]Failed to locate [white bold]{project.Name}[/] ([underline]{project.Directory}[/])[/]");
                        return;
                    }

                    AnsiConsole.MarkupLine($"[grey50]LOG: [/][grey82]Located [white bold]{project.Name}[/] ([underline]{project.Directory}[/])[/]");
                    string rubyFile = Path.Combine(project.Directory, ".ruby-version");

                    Thread.Sleep(1000);

                    if (File.Exists(rubyFile))
                    {
                        AnsiConsole.MarkupLine($"[grey50]LOG: [/][grey82]Found [white bold]{project.Name}[/] using [red]💎 Ruby[/] [white]([underline]v{File.ReadAllText(rubyFile).Replace("\n", "")}[/])[/][/]");
                    }

                    ctx.Status($"[green bold]Starting [white bold]{project.Name}[/]...[/]");

                    Thread.Sleep(File.Exists(rubyFile) ? 1000 : 2000);

                    AnsiConsole.MarkupLine($"[grey50]LOG: [/][grey82]Started [white bold]{project.Name}[/][/]");
                    ctx.Status("[green]CLI will now exit[/]");

                    Thread.Sleep(2000);
                });

            if (!directoryExists)
                return Task.CompletedTask;

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[white bold]{project.Name}[/][grey50] ([underline]{project.Directory}[/])[/]").Centered().RuleStyle(Style.Parse(Program.Configuration.Palette.Primary)));

            var info = new ProcessStartInfo("/usr/bin/env", $"{project.Command}")
            {
                WorkingDirectory = project.Directory,
                UseShellExecute = false
            };
            var proc = Process.Start(info);
            proc.WaitForExit();
            AnsiConsole.Write(new Rule($"[white bold]{project.Name}[/] [grey50]exited with status code [bold {(proc.ExitCode == 0 ? "green" : "red")}]{proc.ExitCode}[/][/]").Centered().RuleStyle(Style.Parse(Program.Configuration.Palette.Primary)));

            return Task.CompletedTask;
        }
    }
}
