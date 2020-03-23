using NetSparkle.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkle.UI.WPF.Interfaces
{
    public interface IUserRespondedToUpdateCheck
    {
        void UserRespondedToUpdateCheck(UpdateAvailableResult response);
    }
}
