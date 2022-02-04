using System;
using Spectre.Console;
using SimpleExec;
using System.Threading.Tasks;
using CLI.Models;
using System.Linq;
using System.Diagnostics;
using LibGit2Sharp;
using System.Collections.Generic;
using Spectre.Console.Rendering;

namespace CLI.Commands
{
    public class DeployCommand : ICommand
    {
        readonly ProjectsConfiguration projectsConfiguration = new ProjectsConfiguration();
        readonly ServersConfiguration serversConfiguration = new ServersConfiguration();

        public string Name => "deploy";

        public Task Run()
        {
            var projectSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#ffb703]Select a project to deploy[/]")
                    .AddChoices(projectsConfiguration.Data.ReleaseProjects.Select(proj => proj.Name))
                    .HighlightStyle(Style.Parse("#219ebc bold"))
                );

            Project project = projectsConfiguration.Data.ReleaseProjects.FirstOrDefault(proj => proj.Name == projectSelection);

            Panel selectedProject = new Panel(new Markup($"[#FFB703 bold]Directory: [/][#8ECAE6]{project.Directory}[/]"))
                .RoundedBorder()
                .BorderStyle(Style.Parse("#219EBC"))
                .Expand();
            selectedProject.Header = new PanelHeader($"| [white bold]{project.Name}[/] |", Justify.Left);

            AnsiConsole.Write(selectedProject);
            AnsiConsole.WriteLine();

            var repo = new Repository(project.Directory);
            List<Branch> branches = repo.Branches.Where(t => t.Reference.IsLocalBranch).OrderBy(t => t.TrackingDetails.AheadBy).Reverse().ToList();
            List<string> selectBranches = new List<string> { "[[Manually Enter]]", "" };
            selectBranches.AddRange(branches.Select(t => t.FriendlyName + (t.TrackedBranch != null ? " [green][[R]][/]" : " [red][[L]][/]")));

            var branchSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#ffb703]Select a branch to deploy[/]")
                    .AddChoices(selectBranches)
                    .HighlightStyle(Style.Parse("#219ebc bold"))
                ).Split(" ")[0].Trim();
            if (branchSelection == "[[Manually") {
                branchSelection = AnsiConsole.Ask<string>("[yellow]Enter branch name[/] [green bold]❯[/]");
            }

            var branch = repo.Branches.FirstOrDefault(t => t.FriendlyName == branchSelection);
            if(branch == null)
            {
                AnsiConsole.MarkupLine($"[red]ERR: [/][grey50]Branch [bold]{branchSelection}[/] not found![/]");
                return Task.CompletedTask;
            }

            if (branch.TrackedBranch == null)
            {
                AnsiConsole.MarkupLine($"[red]ERR: [/][grey50]Local branch [bold]{branchSelection}[/] does not have a remote branch![/]");
                return Task.CompletedTask;
            }

            Panel selectedBranch = new Panel(new Markup($"[#FFB703 bold]Remote Name: [/][#8ECAE6]{branch.RemoteName}[/]"))
                .RoundedBorder()
                .BorderStyle(Style.Parse("#219EBC"))
                .Expand();
            selectedBranch.Header = new PanelHeader($"| [white bold]{branchSelection}[/] |", Justify.Left);

            AnsiConsole.Write(selectedBranch);
            AnsiConsole.WriteLine();

            var servers = new List<string> { "[[Manually Enter]]", "" };
            servers.AddRange(serversConfiguration.Data.Servers.Select(t => t.Name));
            var serverSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[#ffb703]Select a server to deploy to[/]")
                    .AddChoices(servers)
                    .HighlightStyle(Style.Parse("#219ebc bold"))
                );

            if (serverSelection == "[[Manually Enter]]")
                serverSelection = AnsiConsole.Ask<string>("[yellow]Enter server name[/] [green bold]❯[/]");

            if (string.IsNullOrWhiteSpace(serverSelection))
                return Task.CompletedTask;

            // Keep this as string#Equals(). Don't want accidental assignment of prod to serverSelection
            if(serverSelection.Equals("production", StringComparison.CurrentCultureIgnoreCase))
            {
                // Fail verification true by default, just in case
                bool verified = false;

                AnsiConsole.AlternateScreen(() => {
                    Panel childPanel = new Panel(
                            new Markup("\n\n[red]You have selected [bold yellow]PRODUCTION[/] as your deployment destination[/]\n[red]To confirm this choice, please enter [bold yellow]\"deploy production\"[/][/]\n\n").Centered()
                        ).RoundedBorder().Padding(2, 1);
                    childPanel.Header("|[bold white on red blink] WARNING [/]|").HeaderAlignment(Justify.Center);

                    Table table = new Table().NoBorder().Expand();
                    table.AddColumns(new TableColumn("").Width(Console.WindowWidth / 4), new TableColumn(""), new TableColumn("").Width(Console.WindowWidth / 4));
                    table.AddRow(new IRenderable[] { new Text(""), childPanel.Expand(), new Text("") });

                    AnsiConsole.Write(table);
                    AnsiConsole.Markup("[yellow]Confirmation [/][red bold]❯[/] ");
                    string confirmSelectionProduction = Console.ReadLine().Trim();
                    verified = confirmSelectionProduction.Equals("deploy production", StringComparison.CurrentCultureIgnoreCase);
                });

                // Return to regular terminal screen before exiting
                if (!verified) {
                    AnsiConsole.MarkupLine("[bold white on red] Verification failed! [/]");
                    Environment.Exit(1);
                }

                AnsiConsole.WriteLine();
            }

            if (AnsiConsole.Confirm($"[grey]Deploy [green bold]{project.Name}[/] (Branch: [yellow underline]{branch.FriendlyName}[/]) to [red]{serverSelection}[/]?[/]", false))
            {
                AnsiConsole.MarkupLine("[green]Starting Deployment![/]\n");
#if DEBUG
                AnsiConsole.WriteLine($"/usr/local/bin/zsh -c \"cd {project.Directory}; export BRANCH={branch.FriendlyName}; /usr/local/bin/bundle exec cap {serverSelection} deploy\"");
#else
                var info = new ProcessStartInfo("/usr/local/bin/zsh", $"-c \"cd {project.Directory}; export BRANCH={branch.FriendlyName}; /usr/local/bin/bundle exec cap {serverSelection} deploy\"")
                {
                    WorkingDirectory = project.Directory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                var proc = new Process() { StartInfo = info };
                proc.OutputDataReceived += (s, e) =>
                {
                    AnsiConsole.WriteLine(e.Data);
                };
                proc.ErrorDataReceived += (s, e) =>
                {
                    AnsiConsole.WriteLine(e.Data);
                };

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
#endif
                AnsiConsole.MarkupLine("\n[green]Deployment Complete![/]\n");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Cancelling Deployment![/]");
            }
            return Task.CompletedTask;
        }
    }
}
