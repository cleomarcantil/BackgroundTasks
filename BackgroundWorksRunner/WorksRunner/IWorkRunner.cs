using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunner
{
    Task Execute();

    string Name => GetType().Name;
}
