using System;
using System.Threading.Tasks;

namespace CLI
{
    public interface ICommand
    {
        public string Name { get; }
        public Task Run();
    }
}
