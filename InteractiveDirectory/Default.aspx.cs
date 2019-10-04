using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace InteractiveDirectory
{
    public partial class Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            HttpCookie itemsPerPage = Request.Cookies["ItemsPerPage"];
            if (itemsPerPage == null)
            {
                itemsPerPage = new HttpCookie("ItemsPerPage");
                itemsPerPage.Value = "20";
                Response.Cookies.Add(itemsPerPage);
            }
        }
    }
}