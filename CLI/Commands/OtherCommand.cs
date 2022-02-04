using System;
using System.Threading.Tasks;

namespace CLI.Commands
{
    public class OtherCommand : ICommand
    {
        public string Name => "other";

        public Task Run()
        {
            throw new NotImplementedException();
        }
    }
}
