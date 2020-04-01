using NetSparkle.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkle.UI.Avalonia.Interfaces
{
    public interface IUserRespondedToUpdateCheck
    {
        void UserRespondedToUpdateCheck(UpdateAvailableResult response);
    }
}
