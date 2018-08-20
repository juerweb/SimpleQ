using SimpleQ.Extensions;
using SimpleQ.UWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Xamarin.Forms;

[assembly: Dependency(typeof(VersionAndBuild_UWP))]
namespace SimpleQ.UWP
{
    public class VersionAndBuild_UWP : IAppVersionAndBuild
    {
        public string GetBuildNumber()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public string GetVersionNumber()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
