using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace GuestBook_WebRole
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            CloudStorageAccount.SetConfigurationSettingPublisher
                (
                    (configName, configSetter) =>
                        configSetter(RoleEnvironment.GetConfigurationSettingValue(configName))
                );
            
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterOpenAuth();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        void Application_End(object sender, EventArgs e)
        {
        }

        void Application_Error(object sender, EventArgs e)
        {
        }
    }
}
