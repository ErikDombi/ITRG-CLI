using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CLI.Models;
using Spectre.Console;

namespace CLI.Commands
{
    public class RunCommand : ICommand
    {
        readonly ProjectsConfiguration projectsConfiguration = new ProjectsConfiguration();

        public string Name => "run";

        public Task Run()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#ffb703]Select a project to run[/]")
                    .AddChoices(projectsConfiguration.Data.ReleaseProjects.Select(proj => proj.Name))
                    .HighlightStyle(Style.Parse("#219ebc bold"))
                );

            Project project = projectsConfiguration.Data.ReleaseProjects.FirstOrDefault(proj => proj.Name == selection);

            Panel selectedProject = new Panel(new Markup($"[#FFB703 bold]Command: [/][#8ECAE6]{project.Command}[/]\n[#FFB703 bold]Directory: [/][#8ECAE6]{project.Directory}[/]"))
                .RoundedBorder()
                .BorderStyle(Style.Parse("#219EBC"))
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
            AnsiConsole.Write(new Rule($"[white bold]{project.Name}[/][grey50] ([underline]{project.Directory}[/])[/]").Centered().RuleStyle(Style.Parse("#FB8500")));

            var info = new ProcessStartInfo("/usr/bin/env", $"{project.Command}")
            {
                WorkingDirectory = project.Directory,
                UseShellExecute = false
            };
            var proc = Process.Start(info);
            proc.WaitForExit();
            AnsiConsole.Write(new Rule($"[white bold]{project.Name}[/] [grey50]exited with status code [bold {(proc.ExitCode == 0 ? "green" : "red")}]{proc.ExitCode}[/][/]").Centered().RuleStyle(Style.Parse("#FB8500")));

            return Task.CompletedTask;
        }
    }
}
