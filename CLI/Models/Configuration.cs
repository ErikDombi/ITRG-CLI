using System;
using System.Collections.Generic;
using System.ComponentModel;
using CLI.IO;
using Newtonsoft.Json;
using Spectre.Console;

namespace CLI.Models
{
    public class Configuration : JsonInterfacer<Configuration.Instance>
    {
        public Configuration() : base("config.json", (location) =>
        {
            AnsiConsole.MarkupLine("[yellow]config.json file created using default values. `cli config` to modify...[/]");
        })
        {

        }

        public class Instance
        {
            public PaletteConfig Palette { get; set; } = new PaletteConfig();
        }

        public class PaletteConfig
        {
            public string Primary { get; set; } = "#56B6C2";

            public string Secondary { get; set; } = "#C678DD";

            public string Tertiary { get; set; } = "#98C379";

            public string Highlight { get; set; } = "#98C379 bold";
        }
    }
}
