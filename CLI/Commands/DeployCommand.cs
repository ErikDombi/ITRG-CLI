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
using System.IO;
using Spectre.Console.Cli;

namespace CLI.Commands
{
    public class DeployCommand : Command<DeployCommand.Settings>, ICommand
    {
        readonly ProjectsConfiguration projectsConfiguration = new ProjectsConfiguration();
        readonly ServersConfiguration serversConfiguration = new ServersConfiguration();

        string project_argument = null;
        string branch_argument = null;
        string server_argument = null;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[project]")]
            public string Project { get; set; }

            [CommandOption("-b|--branch")]
            public string Branch { get; set; }

            [CommandOption("-s|--server")]
            public string Server { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            project_argument = settings.Project;
            branch_argument = settings.Branch;
            server_argument = settings.Server;
            Run();
            return 0;
        }

        public string Name => "deploy";

        public Task Run()
        {
            var projectSelection = project_argument ?? AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Program.Configuration.Palette.Secondary}]Select a project to deploy[/]")
                    .AddChoices(projectsConfiguration.Data.ReleaseProjects.Where(t => t.Deployable).Select(proj => proj.Name))
                    .HighlightStyle(Style.Parse(Program.Configuration.Palette.Highlight))
                );

            Project project = projectsConfiguration.Data.ReleaseProjects.FirstOrDefault(proj => proj.Name == projectSelection);

            var repo = new Repository(project.Directory);

            string rubyFile = Path.Combine(project.Directory, ".ruby-version");
            string rubyVersion = File.Exists(rubyFile) ? File.ReadAllText(rubyFile).Replace("\n", "") : null;
            string rubyMarkup = rubyVersion != null ? $"\n[{Program.Configuration.Palette.Primary} bold]Ruby Version: [/][{Program.Configuration.Palette.Tertiary}]v{rubyVersion}[/]" : "";

            Panel selectedProject = new Panel(new Markup($"[{Program.Configuration.Palette.Primary} bold]Directory: [/][{Program.Configuration.Palette.Tertiary}]{project.Directory}[/]\n[{Program.Configuration.Palette.Primary} bold]Active Branch: [/][{Program.Configuration.Palette.Tertiary}]{repo.Head.FriendlyName}[/]{rubyMarkup}"))
                .RoundedBorder()
                .BorderStyle(Style.Parse(Program.Configuration.Palette.Secondary))
                .Expand();
            selectedProject.Header = new PanelHeader($"| [white bold]{project.Name}[/] |", Justify.Left);

            AnsiConsole.Write(selectedProject);
            AnsiConsole.WriteLine();

            
            List<Branch> branches = repo.Branches.Where(t => t.Reference.IsLocalBranch).OrderBy(t => t.TrackingDetails.AheadBy).Reverse().ToList();
            List<string> selectBranches = new List<string> { "[[Manually Enter]]", "" };
            selectBranches.AddRange(branches.Select(t => t.FriendlyName + (t.TrackedBranch != null ? " [green][[R]][/]" : " [red][[L]][/]")));

            var branchSelection = branch_argument ?? AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Program.Configuration.Palette.Secondary}]Select a branch to deploy[/]")
                    .AddChoices(selectBranches)
                    .HighlightStyle(Style.Parse(Program.Configuration.Palette.Highlight))
                ).Split(" ")[0].Trim();
            if (branchSelection == "[[Manually") {
                branchSelection = AnsiConsole.Ask<string>($"[{Program.Configuration.Palette.Secondary}]Enter branch name[/] [{Program.Configuration.Palette.Primary} bold]❯[/]");
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

            Panel selectedBranch = new Panel(new Markup($"[{Program.Configuration.Palette.Secondary} bold]Remote Name: [/][{Program.Configuration.Palette.Tertiary}]{branch.RemoteName}[/]"))
                .RoundedBorder()
                .BorderStyle(Style.Parse(Program.Configuration.Palette.Secondary))
                .Expand();
            selectedBranch.Header = new PanelHeader($"| [white bold]{branchSelection}[/] |", Justify.Left);

            AnsiConsole.Write(selectedBranch);
            AnsiConsole.WriteLine();

            var servers = new List<string> { "[[Manually Enter]]", "" };
            servers.AddRange(serversConfiguration.Data.Servers.Select(t => t.Name));
            var serverSelection = server_argument ?? AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Program.Configuration.Palette.Secondary}]Select a server to deploy to[/]")
                    .AddChoices(servers)
                    .HighlightStyle(Style.Parse(Program.Configuration.Palette.Highlight))
                );

            if (serverSelection == "[[Manually Enter]]")
            {
                serverSelection = AnsiConsole.Ask<string>($"[{Program.Configuration.Palette.Secondary}]Enter server name[/] [{Program.Configuration.Palette.Primary} bold]❯[/]");
                AnsiConsole.WriteLine();
            }

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
