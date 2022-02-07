using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CLI.IO;
using Newtonsoft.Json;
using Spectre.Console;

namespace CLI.Models
{
    public class ProjectsConfiguration : JsonInterfacer<ProjectsConfiguration.Instance>
    {
        public ProjectsConfiguration() : base("projects.json", (location) =>
        {
            AnsiConsole.MarkupLine("[yellow]projects.json file created using default values. `cli projects` to modify...[/]");
        }) {
            
        }

        public class Instance
        {
            [JsonProperty("ReleaseProjects")]
            public List<ReleaseProject> ReleaseProjects { get; set; } = new List<ReleaseProject> {
                new("www", "~/InfoTech/www.infotech.com", "rails s", true),
                new("mongoose", "~/InfoTech/mongoose", "invoker start", true),
                new("Invoker", "~/InfoTech/", "invoker start", false)
            };

            [JsonProperty("DebugProjects")]
            public List<DebugProject> DebugProjects { get; set; } = new List<DebugProject>
            {
                new("www", "~/InfoTech/www.infotech.com", "BYEBUGPORT=9191 RBENV_VERSION=$(cat '.ruby-version') rbenv exec bundle exec rails s -p 3000"),
                new("mongoose", "~/InfoTech/mongoose", "invoker start")
            };
        }
    }

    public class Project
    {
        public Project(string Name, string Directory = "", string Command = "")
        {
            this.Name = Name;
            this.Directory = Directory;
            this.Command = Command;
        }

        public string Name { get; set; }

        [JsonProperty("Directory")]
        private string _directory { get; set; }

        [JsonIgnore]
        public string Directory {
            get {
                var dir = _directory.StartsWith("~") ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _directory.Substring((_directory.StartsWith("~/") || _directory.StartsWith("~\\")) ? 2 : 1)) :
                    _directory;
                if (System.IO.Directory.Exists(dir))
                    return dir;
                throw new DirectoryNotFoundException($"Directory for {Name} not found! Ensure you're not using environment variables!");
            }

            set { _directory = value; }
        }

        public string Command { get; set; }
    }

    public class ReleaseProject : Project
    {
        public ReleaseProject(string Name, string Directory = "", string Command = "", bool Deployable = false) : base(Name, Directory, Command)
        {
            this.Deployable = Deployable;
        }

        public bool Deployable { get; set; } = false;
    }

    public class DebugProject : Project
    {
        public DebugProject(string Name, string Directory = "", string Command = "") : base(Name, Directory, Command)
        {

        }
    }
}
