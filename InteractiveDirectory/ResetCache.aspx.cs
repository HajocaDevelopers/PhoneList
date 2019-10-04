using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace InteractiveDirectory
{
    public partial class ResetCache : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Services.DirectoryItemServices.ResetCurrentDirectory();
            Response.Redirect("~/");
        }
    }
}