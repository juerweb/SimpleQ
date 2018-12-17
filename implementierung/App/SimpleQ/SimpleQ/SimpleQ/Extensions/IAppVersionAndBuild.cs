using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.Extensions
{
    public interface IAppVersionAndBuild
    {
        string GetVersionNumber();
        string GetBuildNumber();
    }
}
