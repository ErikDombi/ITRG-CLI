using System;
using System.Collections.Generic;
using CLI.IO;
using Newtonsoft.Json;

namespace CLI.Models
{
    public class ServersConfiguration : JsonInterfacer<ServersConfiguration.Instance>
    {
        public ServersConfiguration() : base("servers.json", (location) =>
        {
            throw new Exception("servers.json file created. Please populate configuration and restart...");
        })
        {

        }

        public class Instance
        {
            [JsonProperty("Servers")]
            public List<Server> Servers { get; set; }
        }
    }

    public class Server
    {
        public string Name { get; set; }
    }
}
