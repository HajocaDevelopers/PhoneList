using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using InteractiveDirectory.Services;

namespace InteractiveDirectory
{
    public partial class Export : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string attachment = "attachment; filename=Directory.csv";
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
            HttpContext.Current.Response.ContentType = "text/csv";
            HttpContext.Current.Response.AddHeader("Pragma", "public");

            if (Page.IsPostBack)
            {
            }
            else
                HttpContext.Current.Response.Write( DirectoryItemServices.GetCSVDirectory("","",DirectoryItemServices.directoryViewType.All));

            HttpContext.Current.Response.End();
        }
    }
}