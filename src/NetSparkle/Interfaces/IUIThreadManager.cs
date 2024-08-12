using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    public interface IUIThreadManager
    {
        void PerformUIAction(Action action);
        Task PerformAsyncUIAction(Func<Task> action);
    }
}
