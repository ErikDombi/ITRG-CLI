using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CLI.IO;
using Newtonsoft.Json;

namespace CLI.Models
{
    public class ProjectsConfiguration : JsonInterfacer<ProjectsConfiguration.Instance>
    {
        public ProjectsConfiguration() : base("projects.json", (location) =>
        {
            throw new Exception("projects.json file created. Please populate configuration and restart...");
        }) {
            
        }

        public class Instance
        {
            [JsonProperty("ReleaseProjects")]
            public List<Project> ReleaseProjects { get; set; }

            [JsonProperty("DebugProjects")]
            public List<Project> DebugProjects { get; set; }
        }
    }

    public class Project
    {
        public string Name { get; set; }

        [JsonProperty("Directory")]
        private string _directory { get; set; }

        [JsonIgnore]
        public string Directory {
            get {
                return _directory.StartsWith("~") ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _directory.Substring((_directory.StartsWith("~/") || _directory.StartsWith("~\\")) ? 2 : 1)) :
                    _directory;
            }

            set { _directory = value; }
        }

        public string Command { get; set; }

        public string DebugCommand { get; set; }
    }
}
