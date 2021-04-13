using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using InteractiveDirectory.Services;

namespace InteractiveDirectory.Controllers
{
    /// <summary>
    /// API Calls for ther for the Directory.
    /// </summary>
    public class DirectoryController : ApiController
    {
        [HttpGet]
        public dynamic GetPC([FromUri] string PC)
        {
            return Services.DirectoryItemServices.GetDirectory(false).Where(x => x.PC == PC && x.IsProfitCenter).ToList();
        }

        [HttpGet]
        public dynamic GetCurrentDirectory()
        {
            HajClassLib.DevelopmentConfiguration.DeveloperUserImperosnate();

            return Services.DirectoryItemServices.GetDirectory(Services.Security.CurrentUserAllowMobile());
        }

        [HttpGet]
        public dynamic GetCurrentDirectoryForMobile()
        {
            HajClassLib.DevelopmentConfiguration.DeveloperUserImperosnate();

            return Services.DirectoryItemServices.GetDirectoryForMobile(Services.Security.CurrentUserAllowMobile());
        }

        [HttpGet]
        public dynamic GetAllowMobile()
        { 
            HajClassLib.DevelopmentConfiguration.DeveloperUserImperosnate();
            return Services.Security.CurrentUserAllowMobile();
        }

        [HttpGet]
        public dynamic GetCurrentDataCache()
        {
            return (List<InteractiveDirectory.Models.DirectoryItem>)System.Web.HttpRuntime.Cache["CurrentDirectory"];
        }

        [HttpPost]
        public dynamic GetCurrentDirectory([FromBody] dynamic data)
        {
            HajClassLib.DevelopmentConfiguration.DeveloperUserImperosnate();

            string filter = data.filter;
            int? page = data.page;
            int? pageSize = data.pageSize;
            string sort = data.sort;

            DirectoryItemServices.directoryViewType view;
            if (data.view==null) view = DirectoryItemServices.directoryViewType.All;
            else view = data.view;

            return Services.DirectoryItemServices.GetDirectory(page, pageSize, sort, filter, view, Services.Security.CurrentUserAllowMobile());
        }

        [HttpGet]
        public dynamic GetExcelExport([FromUri] string filter, [FromUri] string sort,  [FromUri] int view)
        {
            HajClassLib.DevelopmentConfiguration.DeveloperUserImperosnate();

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(DirectoryItemServices.GetCSVDirectory(sort ?? string.Empty, filter ?? string.Empty, (DirectoryItemServices.directoryViewType)view, Services.Security.CurrentUserAllowMobile()));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "directory.csv";
            return result;
        }

    }
}