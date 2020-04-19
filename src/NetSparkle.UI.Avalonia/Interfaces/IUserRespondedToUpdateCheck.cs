using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.UI.Avalonia.Interfaces
{
    public interface IUserRespondedToUpdateCheck
    {
        void UserRespondedToUpdateCheck(UpdateAvailableResult response);
    }
}
