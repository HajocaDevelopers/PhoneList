using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace InteractiveDirectory.Services
{
    public class Security
    {

        // xxx.Config - Used to avoid typos and let us know what Web.Config values are used.
        private const string CONFIG_ADGROUPNAMES_ALLOW_MOBILE = "ADGroupNames_AllowMobile";
        
        static public bool CurrentUserAllowMobile()
        {
            foreach (string groupName in ConfigurationManager.AppSettings[CONFIG_ADGROUPNAMES_ALLOW_MOBILE].Split('|'))
                if (HttpContext.Current.User.IsInRole("HAJOCA\\" + groupName)) return true;
            return false;
        }
    }
}
