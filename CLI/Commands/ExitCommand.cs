using System;
using System.Threading.Tasks;

namespace CLI.Commands
{
    public class ExitCommand : ICommand
    {
        public string Name => "exit";

        public Task Run()
        {
            return Task.CompletedTask;
        }
    }
}
