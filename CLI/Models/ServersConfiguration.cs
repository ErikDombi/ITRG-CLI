using System;
using System.Collections.Generic;
using CLI.IO;
using Newtonsoft.Json;
using Spectre.Console;

namespace CLI.Models
{
    public class ServersConfiguration : JsonInterfacer<ServersConfiguration.Instance>
    {
        public ServersConfiguration() : base("servers.json", (location) =>
        {
            AnsiConsole.MarkupLine("[yellow]servers.json file created using default values. `cli servers` to modify...[/]");
        })
        {

        }

        public class Instance
        {
            [JsonProperty("Servers")]
            public List<Server> Servers { get; set; } = new List<Server>() { new("orange1"), new("purple1") };
        }
    }

    public class Server
    {
        public Server(string Name) { this.Name = Name; }

        public string Name { get; set; }
    }
}
