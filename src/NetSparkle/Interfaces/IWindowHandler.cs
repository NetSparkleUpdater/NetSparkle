using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Interfaces
{
    // TODO: comments, refactoring, possibly more events for completion's sake since this will be a public interface, cleanup
    public delegate void WindowHandlerShown(object sender);
    public interface IWindowHandler
    {
        event WindowHandlerShown WindowHasBeenShown;
    }
}
