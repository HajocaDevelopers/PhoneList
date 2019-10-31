using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Data;
using System.Data.OleDb;
using System.Xml.Serialization;
using InteractiveDirectory.Models;
using HajClassLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InteractiveDirectory.Services
{
    public class DirectoryItemServices
    {
        #region "Local Variables and Constants"

        // xxx.Config - Used to avoid typos and let us know what Web.Config values are used.
        private const string CONFIG_CASH_RESET_IN_HOURS = "CacheResetInHours";
        private const string CONFIG_ADGROUPNAMES_SERVICE_CENTER = "ADGroupNames_ServiceCenter";
        private const string CONFIG_ADGROUPNAMES_REGION_SUPPORT = "ADGroupNames_RegionSupport";
        private const string CONFIG_ADGROUPNAMES_REGION_MANAGER = "ADGroupNames_RegionManager";
        private const string CONFIG_ADGROUPNAMES_PC_MANAGER = "ADGroupNames_PCManager";
        private const string CONFIG_ECLIPSE_BOX_LOOKUP = "EclipseBoxLookup";
        private const string CONFIG_INTERACTIVE_DIRECTORY_XML_FILE = "InteractiveDirectoryXMLFile";
        private const string CONFIG_REGION_LOOKUP_XML_FILE = "RegionLookupXMLFile";

        // Cache Constants - Used to aviod typos.
        private const string CURRENT_DIRECTORY_CACHE_NAME = "CurrentDirectory"; // Full list
        private const string CURRENT_DIRECTORY_NO_MOBILE_CACHE_NAME = "CurrentDirectoryNoMobile"; // Full list minus Mobile Phone
        private const string AD_DI_PROPERTIES_CACHE_NAME = "ADProps";
        private const string AD_DI_METHODS_CACHE_NAME = "ADMethods";
        private const string AS400_DI_PROPERTIES_CACHE_NAME = "AS400Props";
        private const string AS400_DI_METHODS_CACHE_NAME = "AS400Methods";
        private const string ECLIPSE_BOX_LOOKUP_CACHE_NAME = "EclipseBoxLookup";
        private const string REGION_NAME_LOOKUP_CACHE_NAME = "RegionNameLookup";

        // Lock Object - Used to lock the process of creating the cached directory so multiple
        // users don't wast their time getting the same data.  Once it's cached the waiting 
        // users will get the cached value.
        private static object CurrentNoMobileDirectoryLock = new object();
        private static object CurrentDirectoryLock = new object();
        private static object EclipseLookupLock = new object();
        private static object RegionLookupLock = new object();

        #endregion

        #region "Public Structures"

        /// <summary>
        /// List of different "View Types" used as a filter on the directory.
        /// </summary>
        public enum directoryViewType
        {
            All = 0,
            ProfitCenter = 1,
            RegionManager = 2,
            RegionSupport = 3,
            ServiceCenter = 4,
            ShowRoom = 5
        }

        #endregion

        #region "Public Methods"

        public static string EclipseBoxLookup(string PCSup3)
        {
            try
            {
                string ret;
                Dictionary<string, string> CurrentCache = (Dictionary<string, string>)System.Web.HttpRuntime.Cache[ECLIPSE_BOX_LOOKUP_CACHE_NAME];

                if (CurrentCache == null) CurrentCache = BuildEclipseBoxLookup();
                if (!CurrentCache.TryGetValue(PCSup3, out ret)) ret = "???";
                return ret;
            }
            catch (Exception)
            {
                return "Err";
            }
        }

        public static string RegioinNameLookup(string RegionNumber)
        {
            try
            {
                int regionNumber;
                if (!Int32.TryParse(RegionNumber, out regionNumber)) return null;

                string ret;
                Dictionary<int, string> CurrentCache = (Dictionary<int, string>)System.Web.HttpRuntime.Cache[REGION_NAME_LOOKUP_CACHE_NAME];

                if (CurrentCache == null) CurrentCache = BuildRegionLookup();
                if (!CurrentCache.TryGetValue(regionNumber, out ret)) return null;
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Currently cached directory.  If there is no cached list it will 
        /// automatically call the function to build it.
        /// </summary>
        /// <returns>All Directory Items</returns>
        public static List<DirectoryItem> GetDirectory(bool includeMobile)
        {
            List<DirectoryItem> CurrentCache;

            if (includeMobile)
            {
                CurrentCache = (List<DirectoryItem>)System.Web.HttpRuntime.Cache[CURRENT_DIRECTORY_CACHE_NAME];
                if (CurrentCache == null) CurrentCache = LoadCurrentDirectory();
            }
            else
            {
                CurrentCache = (List<DirectoryItem>)System.Web.HttpRuntime.Cache[CURRENT_DIRECTORY_NO_MOBILE_CACHE_NAME];
                if (CurrentCache == null) CurrentCache = LoadCurrentNoMobileDirectory();
            }
            List<DirectoryItem> ret = new List<DirectoryItem>(CurrentCache); // We need to disconnect from the Cache
            return ret;
        }

        /// <summary>
        /// Currently cached directory.  If there is no cached list it will 
        /// automatically call the function to build it.
        /// </summary>
        /// <returns>All Directory Items</returns>
        public static List<DirectoryItemForMobile> GetDirectoryForMobile(bool includeMobile)
        {
            List<DirectoryItem> CurrentCache;

            // First we pull the full list from cache (saves having to keep multiple caches for Mobile)
            CurrentCache = (List<DirectoryItem>)System.Web.HttpRuntime.Cache[CURRENT_DIRECTORY_CACHE_NAME];
            if (CurrentCache == null) CurrentCache = LoadCurrentDirectory();

            return CurrentCache.OrderBy(x => x.Name).Select(x => new DirectoryItemForMobile
            {
                Name = x.Name,
                PC = x.PC,
                Phone = x.Phone,
                IsServiceCenter = x.IsServiceCenter,
                IsRegionSupport = x.IsRegionSupport,
                IsRegionManager = x.IsRegionManager,
                IsPCManager = x.IsPCManager,
                IsProfitCenter = x.IsProfitCenter,
                HasShowroom = x.HasShowroom,
                Email = x.EmailAddress,
                Mobile = includeMobile ? x.Mobile : "",
                Street = x.Street,
                CSZ = x.CSZ,
                RegionNumber = x.RegionNumber,
                RegionName = x.RegionName,
                ManagerDepartment = x.ManagerDepartment,
                EclipseBox = x.EclipseBox,
                OnSiteContact = x.OnSiteContact
            }).ToList();
        }

        /// <summary>
        /// Currently cached directory.  If there is no cached list it will 
        /// automatically call the function to build it.
        /// </summary>
        /// <returns>All Directory Items</returns>
        public static List<DirectoryItem> GetDirectoryByPC(string PC, bool includeMobile)
        {
            return GetDirectory(includeMobile).Where(x => x.PC == PC).ToList();
        }

        /// <summary>
        /// List of currently cached directory items filtered by paratemter.  This
        /// function uses the base GetDirectory() to ensure the current cached items
        /// are used or created if not yet cached.  The filter will check for each items
        /// and ensure it appears someplace in the pre-defined columns.  All items in the 
        /// filter must appear in one or more of the columns in order to be returned.
        /// </summary>
        /// <param name="filter">Space delimited list of items to filter: "joe green 610".</param>
        /// <returns>List of filtered Directory Items</returns>
        public static List<DirectoryItem> GetDirectory(string filter, bool includeMobile)
        {
            List<DirectoryItem> returnList = GetDirectory(includeMobile);
            foreach (string f in filter.Split(' '))
            {
                string value = f.Trim().ToLower();
                if (value.Length > 0)
                {
                    returnList.RemoveAll(x => !((x.RegionNumber != null && x.RegionNumber.ToLower().Contains(value)) || (x.PC != "992") ||
                                                (x.PC != null && x.PC.ToLower().Contains(value)) ||
                                                (x.Name != null && x.Name.ToLower().Contains(value)) ||
                                                (x.DivsionName != null && x.DivsionName.ToLower().Contains(value)) ||
                                                (x.RegionName != null && x.RegionName.ToLower().Contains(value)) ||
                                                (x.ManagerDepartment != null && x.ManagerDepartment.ToLower().Contains(value)) ||
                                                (x.EmailAddress != null && x.EmailAddress.ToLower().Contains(value)) ||
                                                (x.Phone != null && x.Phone.ToLower().Contains(value)) ||
                                                (x.Fax != null && x.Fax.ToLower().Contains(value)) ||
                                                (x.Street != null && x.Street.ToLower().Contains(value)) ||
                                                (x.CSZ != null && x.CSZ.ToLower().Contains(value)) ||
                                                (x.EclipseBox != null && x.EclipseBox.ToLower().Contains(value)) ||
                                                (x.EclipseNumber != null && x.EclipseNumber.ToLower().Contains(value)) ||
                                                (x.OnSiteContact != null && x.OnSiteContact.ToLower().Contains(value)) ||
                                                (x.OnSiteContactEmail != null && x.OnSiteContactEmail.ToLower().Contains(value)) ||
                                                (x.POBox != null && x.POBox.ToLower().Contains(value)) ||
                                                (x.POCSZ != null && x.POCSZ.ToLower().Contains(value)) ||
                                                (x.CreditManager != null && x.CreditManager.ToLower().Contains(value)) ||
                                                (x.CreditMgrPhone != null && x.CreditMgrPhone.ToLower().Contains(value)) ||
                                                (x.CreditMgrEmail != null && x.CreditMgrEmail.ToLower().Contains(value)) ||
                                                (x.CreditMgrFax != null && x.CreditMgrFax.ToLower().Contains(value)) ||
                                                (x.Mobile != null && x.Mobile.ToLower().Contains(value)) ||
                                                (x.Keywords != null && x.Keywords.ToLower().Contains(value))
                                                ));

                }
            }
            return returnList;
        }

        /// <summary>
        /// Main function used for returning data.  All parameters are optional.  This
        /// function will use the other GetDirectory functions as needed to produce the
        /// final filter, sorted, paginated results.
        /// </summary>
        /// <param name="page">Which page of data to return, based on pageSize.  Default is page 1.</param>
        /// <param name="pageSize">How many items are considered a page.  Default is ALL items (1 page).</param>
        /// <param name="sort">Comma delimeted list of sort values that can contain ASC/DESC if needed:  "RegionName DESC,Name".  Default is LastNamePCName,FirstName.  Field names must match DirectoryItem properties!</param>
        /// <param name="filter">Space delimited list of items to filter: "joe green 610".</param>
        /// <param name="viewType">directoryViewType to indicate a filter at the ViewType leverl.  Default is "All".</param>
        /// <returns>List of filtered, sorted, paginated Directory Items in a Directory object which will also contain information about what data was returned.</returns>
        public static CompanyDirectory GetDirectory(int? page, int? pageSize, string sort = "", string filter = "", directoryViewType viewType = directoryViewType.All, bool includeMobile = false)
        {
            CompanyDirectory returnData = new CompanyDirectory();
            List<DirectoryItem> returnItems;
            if (filter.Length > 0)
                returnItems = GetDirectory(filter, includeMobile);
            else
                returnItems = GetDirectory(includeMobile);

            switch (viewType)
            {
                case directoryViewType.All:
                    break;
                case directoryViewType.ProfitCenter:
                    returnItems.RemoveAll(x => x.IsProfitCenter == false);
                    break;
                case directoryViewType.RegionManager:
                    returnItems.RemoveAll(x => x.IsRegionManager == false);
                    break;
                case directoryViewType.RegionSupport:
                    returnItems.RemoveAll(x => x.IsRegionSupport == false);
                    break;
                case directoryViewType.ServiceCenter:
                    returnItems.RemoveAll(x => x.IsServiceCenter == false);
                    break;
                case directoryViewType.ShowRoom:
                    returnItems.RemoveAll(x => x.HasShowroom == false);
                    break;
                default:
                    break;
            }

            List<Tuple<string, string>> sortTuple = new List<Tuple<string, string>>();
            if (sort == null || sort.Trim().Length == 0) sort = "RegionNumberSort,PCSort,Name";
            foreach (string sortValue in sort.Split(','))
            {
                if (sortValue.Contains(' '))
                    sortTuple.Add(new Tuple<string, string>(sortValue.Split(' ')[0].Trim(), sortValue.Split(' ')[1].Trim()));
                else
                    sortTuple.Add(new Tuple<string, string>(sortValue.Trim(), "asc"));
            }

            if (page == null) page = 1;
            if ((pageSize == null) || (pageSize == 0)) pageSize = returnItems.Count();
            int skip = (int)pageSize * ((int)page - 1);

            returnData.page = page ?? 1;  // ?? Used to for clean conversion from nullable type even though we know it can't be null because of the code above.
            returnData.pageSize = pageSize ?? returnItems.Count(); // ?? Used to for clean conversion from nullable type even though we know it can't be null because of the code above.
            if (returnData.pageSize == 0) returnData.pageSize = returnItems.Count();
            if (returnData.pageSize == 0) returnData.pageSize = 1; // Incase count was 0 too!
            returnData.totalPages = (int)Math.Ceiling(((decimal)returnItems.Count() / returnData.pageSize));
            returnData.totalRecords = returnItems.Count();
            returnData.showMobile = includeMobile;

            if (skip < returnItems.Count())
                returnData.DirectoryItems = returnItems.MultipleSort(sortTuple).Skip(skip).Take((int)pageSize).ToList();

            return returnData;
        }

        public static List<DirectoryItem> GetPcDirectory(string vt)
        {
            //CompanyDirectory returnData = new CompanyDirectory();
            List<DirectoryItem> returnItems;
            returnItems = GetDirectory(true);
            int viewType = Convert.ToInt32(vt);

            switch (viewType)
            {
                case 0:
                    break;
                case 1://All PC's
                    returnItems.RemoveAll(x => x.IsProfitCenter == false);
                    break;
                case 2://Region Managers
                    returnItems.RemoveAll(x => x.IsRegionManager == false);
                    break;
                case 3://Region Support
                    returnItems.RemoveAll(x => x.IsRegionSupport == false);
                    break;
                case 4://Service Center Employees
                    returnItems.RemoveAll(x => x.IsServiceCenter == false);
                    break;
                case 5://Alll Showrooms
                    returnItems = returnItems.Where(x => x.HasShowroom == true).ToList();
                    break;
                default:
                    break;
            }
            return returnItems;
        }

        public static string GetCSVDirectory(string sort = "", string filter = "", directoryViewType viewType = directoryViewType.All, bool includeMobile = false)
        {
            List<DirectoryItem> dis = GetDirectory(1, 0, sort, filter, viewType, includeMobile).DirectoryItems;

            using (var ms = new System.IO.MemoryStream())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Date:," + DateTime.Now.ToString("MM/dd/yyyy") + ",");
                sb.Append("Filter:,=\"" + ((filter.Length > 0) ? filter : "[None]") + "\",");
                sb.Append("Sort:," + ((sort.Length > 0) ? sort : "[Default]") + ",");
                sb.Append("Directory:," + viewType.ToString());
                sb.Append("\n");

                string value = sb.ToString();
                ms.Write(Encoding.Default.GetBytes(value), 0, value.Length);

                //  Get an empty DirectoryItem so we can get a CSV "Header"
                DirectoryItem dih = new DirectoryItem();
                dih.SerializeCSVHeader(ms, includeMobile);

                foreach (DirectoryItem di in dis)
                    di.SerializeAsCsv(ms, includeMobile);
                ms.Position = 0;
                var sr = new System.IO.StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Forces the XML to be recreated and removes the currently cached directorty from the cache. 
        /// Since the data is automatically loaded when it's accessed we don't both re-building it here.
        /// </summary>
        public static void ResetCurrentDirectory()
        {
            if (BuildCurrentDirectory())
            {
                HttpRuntime.Cache.Remove(CURRENT_DIRECTORY_CACHE_NAME);
                HttpRuntime.Cache.Remove(CURRENT_DIRECTORY_NO_MOBILE_CACHE_NAME);
            }
            //Note: We don't bother to reload since that will happen automatically when GetDirectory is called.
        }

        /// <summary>
        /// Used by both the DirecotryItemServices and ADDirectoryItemConverter this scans the DirectortItem
        /// class to find all the properties that we expect to match up with the results of an AD search.  Since
        /// this is used heavily during the build of the cache of directory items and ONLY during the build of the
        /// direcotry items we cache the results on a sliding expiration of 5 minutes after the last time it was
        /// accessed.
        /// </summary>
        /// <returns>Dictionary of AD properties with Attribute Name as the key and the matching property name as the value.</returns>
        public static Dictionary<string, string> GetDirectoryItemADProperties()
        {
            // Odds are this is only being called while we're under a CurrentDirectoryLock for a single user
            // so we don't need to be concerned with another lock here.
            Dictionary<string, string> CurrentCache = (Dictionary<string, string>)HttpRuntime.Cache[AD_DI_PROPERTIES_CACHE_NAME];
            if (CurrentCache != null) return CurrentCache;

            // Build a dictionary of the fields that we'll map based on the ADIdentifierAttribute.
            CurrentCache = typeof(DirectoryItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(ADIdentifierAttribute), false).Any())
                .ToDictionary(
                        p => p.GetCustomAttributes(typeof(ADIdentifierAttribute), false)
                        .Select(p1 => (ADIdentifierAttribute)p1)
                        .First()
                        .Value.ToString(),
                //                        .Value.ToString().ToLower(),
                        p => p.Name
                    );

            // Add it to the cache.  It shouldn't take more than a few seconds to complete a build but we'll 
            // keep this cached for 5 minutes after it's last accessed.
            HttpRuntime.Cache.Add(AD_DI_PROPERTIES_CACHE_NAME, CurrentCache, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), CacheItemPriority.High, null);

            return CurrentCache;
        }

        /// <summary>
        /// Used by both the DirecotryItemServices and ADDirectoryItemConverter this scans the DirectortItem
        /// class to find all the methods that we expect to match up with the results of an AD search.  Since
        /// this is used heavily during the build of the cache of directory items and ONLY during the build of the
        /// direcotry items we cache the results on a sliding expiration of 5 minutes after the last time it was
        /// accessed.
        /// </summary>
        /// <returns>Dictionary of AD methods with Attribute Name as the key and the matching method name as the value.</returns>
        public static Dictionary<string, string> GetDirectoryItemADMethods()
        {
            // Odds are this is only being called while we're under a CurrentDirectoryLock for a single user
            // so we don't need to be concerned with another lock here.
            Dictionary<string, string> CurrentCache = (Dictionary<string, string>)HttpRuntime.Cache[AD_DI_METHODS_CACHE_NAME];
            if (CurrentCache != null) return CurrentCache;

            // Build a dictionary of the fields that we'll map based on the ADIdentifierAttribute.
            CurrentCache = typeof(DirectoryItem).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(ADIdentifierAttribute), false).Any())
                .ToDictionary(
                        p => p.GetCustomAttributes(typeof(ADIdentifierAttribute), false)
                        .Select(p1 => (ADIdentifierAttribute)p1)
                        .First()
                            //                        .Value.ToString().ToLower(),
                        .Value.ToString(),
                        p => p.Name
                    );

            // Add it to the cache.  It shouldn't take more than a few seconds to complete a build but we'll 
            // keep this cached for 5 minutes after it's last accessed.
            HttpRuntime.Cache.Add(AD_DI_METHODS_CACHE_NAME, CurrentCache, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), CacheItemPriority.High, null);

            return CurrentCache;
        }

        /// <summary>
        /// Used by both the DirecotryItemServices and AS400DirectoryItemConverter this scans the DirectortItem
        /// class to find all the properties that we expect to match up with the results of an AS400 query.  Since
        /// this is used heavily during the build of the cache of directory items and ONLY during the build of the
        /// direcotry items we cache the results on a sliding expiration of 5 minutes after the last time it was
        /// accessed.
        /// </summary>
        /// <returns>Dictionary of AS400 properties with Attribute Name as the key and the matching property name as the value.</returns>
        public static Dictionary<string, string> GetDirectoryItemAS400Properties()
        {
            // Odds are this is only being called while we're under a CurrentDirectoryLock for a single user
            // so we don't need to be concerned with another lock here.
            Dictionary<string, string> CurrentCache = (Dictionary<string, string>)HttpRuntime.Cache[AS400_DI_PROPERTIES_CACHE_NAME];
            if (CurrentCache != null) return CurrentCache;

            // Build a dictionary of the fields that we'll map based on the AS400IdentifierAttribute.
            CurrentCache = typeof(DirectoryItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(AS400IdentifierAttribute), false).Any())
                .ToDictionary(
                        p => p.GetCustomAttributes(typeof(AS400IdentifierAttribute), false)
                        .Select(p1 => (AS400IdentifierAttribute)p1)
                        .First()
                        .Value.ToString().ToLower(),
                        p => p.Name
                    );

            // Add it to the cache.  It shouldn't take more than a few seconds to complete a build but we'll 
            // keep this cached for 5 minutes after it's last accessed.
            HttpRuntime.Cache.Add(AS400_DI_PROPERTIES_CACHE_NAME, CurrentCache, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), CacheItemPriority.High, null);

            return CurrentCache;
        }

        /// <summary>
        /// Used by both the DirecotryItemServices and AS400DirectoryItemConverter this scans the DirectortItem
        /// class to find all the methods that we expect to match up with the results of an AS400 query.  Since
        /// this is used heavily during the build of the cache of directory items and ONLY during the build of the
        /// direcotry items we cache the results on a sliding expiration of 5 minutes after the last time it was
        /// accessed.
        /// </summary>
        /// <returns>Dictionary of AS400 properties with Attribute Name as the key and the matching method name as the value.</returns>
        public static Dictionary<string, string> GetDirectoryItemAS400Methods()
        {
            // Odds are this is only being called while we're under a CurrentDirectoryLock for a single user
            // so we don't need to be concerned with another lock here.
            Dictionary<string, string> CurrentCache = (Dictionary<string, string>)HttpRuntime.Cache[AS400_DI_METHODS_CACHE_NAME];
            if (CurrentCache != null) return CurrentCache;

            // Build a dictionary of the fields that we'll map based on the AS400IdentifierAttribute.
            CurrentCache = typeof(DirectoryItem).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(AS400IdentifierAttribute), false).Any())
                .ToDictionary(
                        p => p.GetCustomAttributes(typeof(AS400IdentifierAttribute), false)
                        .Select(p1 => (AS400IdentifierAttribute)p1)
                        .First()
                        .Value.ToString().ToLower(),
                        p => p.Name
                    );

            // Add it to the cache.  It shouldn't take more than a few seconds to complete a build but we'll 
            // keep this cached for 5 minutes after it's last accessed.
            HttpRuntime.Cache.Add(AS400_DI_METHODS_CACHE_NAME, CurrentCache, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), CacheItemPriority.High, null);

            return CurrentCache;
        }

        /// <summary>
        /// This is the main building of the directory and storage to an XML file.
        /// </summary>
        /// <returns>True if successful.</returns>
        public static bool BuildCurrentDirectory()
        {
            try
            {
                List<DirectoryItem> CurrentDirectory = new List<DirectoryItem>();
                CurrentDirectory.AddRange(LoadPcInformation());
                CurrentDirectory.AddRange(LoadADInformation());
                string fileName = ConfigurationManager.AppSettings[CONFIG_INTERACTIVE_DIRECTORY_XML_FILE];
                File.Delete(fileName);
                Stream stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
                XmlSerializer xs = new XmlSerializer(CurrentDirectory.GetType());
                xs.Serialize(stream, CurrentDirectory);
                stream.Close();
            }
            catch (Exception ex)
            {
                HajClassLib.ErrorReporting.ReportError(ErrorReporting.ErrorReportType.HAJNET_Error, "InteractiveDirectory.Library", MethodBase.GetCurrentMethod(), ex);
                return false;
            }
            return true;
        }

        #endregion

        #region "Private Methods"

        private static List<DirectoryItem> LoadCurrentNoMobileDirectory()
        {
            // Lock up the process to make sure another process doesn't try to do the same thing.
            lock (CurrentNoMobileDirectoryLock)
            {
                //Now that we're locked, check that it wasn't already created.
                List<DirectoryItem> CurrentNoMobileCache = (List<DirectoryItem>)HttpRuntime.Cache[CURRENT_DIRECTORY_NO_MOBILE_CACHE_NAME];

                // If it was then another process already created it while we were waiting.
                if (CurrentNoMobileCache != null) return CurrentNoMobileCache;

                // It still isn't there so let's grab the version WITH mobile numbers and clone it.
                // We'll do a simple clone method of Serializing and Deserializing using JSON.  It 
                // may not be the fastest way, but it is easier than cloning by hand and it's getting 
                // Cached anyway so a small hit on speed here is acceptable.
                CurrentNoMobileCache = JsonConvert.DeserializeObject<List<DirectoryItem>>(JsonConvert.SerializeObject(LoadCurrentDirectory()));

                // Now lets remove the mobile number in the clone.
                foreach (DirectoryItem di in CurrentNoMobileCache) di.Mobile = "";

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string CacheResetInHours = ConfigurationManager.AppSettings[CONFIG_CASH_RESET_IN_HOURS];
                int cacheHours;
                if (!int.TryParse(CacheResetInHours, out cacheHours)) cacheHours = 8;
                HttpRuntime.Cache.Add(CURRENT_DIRECTORY_NO_MOBILE_CACHE_NAME, CurrentNoMobileCache, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                // Return it.
                return CurrentNoMobileCache;
            }
        }

        private static List<DirectoryItem> LoadCurrentDirectory()
        {
            // Lock up the process to make sure another process doesn't try to do the same thing.
            lock (CurrentDirectoryLock)
            {
                //Now that we're locked, check that it wasn't already created.
                List<DirectoryItem> CurrentCache = (List<DirectoryItem>)HttpRuntime.Cache[CURRENT_DIRECTORY_CACHE_NAME];

                // If it was then another process already created it while we were waiting.
                if (CurrentCache != null) return CurrentCache;

                // It still isn't there so lets load the file.
                CurrentCache = new List<DirectoryItem>();
                string fileName = ConfigurationManager.AppSettings[CONFIG_INTERACTIVE_DIRECTORY_XML_FILE];

                // We'll try to access the file 3 times with a 10 second pause between failures just in case
                // the XML file is locked up by the build process which could be running from another process
                // like the InteractiveDirectoryBuilder which is likely on a scheduled task.
                int Retries = 3;
                while (Retries > 0)
                {
                    try
                    {
                        Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
                        XmlSerializer xs = new XmlSerializer(CurrentCache.GetType());
                        CurrentCache = (List<DirectoryItem>)(xs.Deserialize(stream));
                        stream.Close();
                        Retries = 0;  // Drop out of the loop and continue.
                    }
                    catch (Exception ex)
                    {
                        Retries -= 1;
                        if (Retries <= 0) HajClassLib.ErrorReporting.ReportError(HajClassLib.ErrorReporting.ErrorReportType.HAJNET_Alert, "Interactive Directory", System.Reflection.MethodBase.GetCurrentMethod(), ex);
                        else System.Threading.Thread.Sleep(10000); // Wait 10 seconds and try again.  This is in case the file is being rebuilt.
                    }
                }

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string CacheResetInHours = ConfigurationManager.AppSettings[CONFIG_CASH_RESET_IN_HOURS];
                int cacheHours;
                if (!int.TryParse(CacheResetInHours, out cacheHours)) cacheHours = 8;
                HttpRuntime.Cache.Add(CURRENT_DIRECTORY_CACHE_NAME, CurrentCache, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                // Return it.
                return CurrentCache;
            }
        }


        /// <summary>
        /// Pulls the information from AD to be included in the direcotry.  It looks for all users that are
        /// currently in the groups that are named in the web.config (WEB_CONFIG_SERVICE_CENTER_EMPLOYEES and
        /// WEB_CONFIG_REGION_STAFF).
        /// </summary>
        /// <returns>List of directory items from Active Directory.</returns>
        private static List<DirectoryItem> LoadADInformation()
        {
            List<DirectoryItem> directoryItems = new List<DirectoryItem>();

            // Pull in the Service Center
            foreach (string adGroup in ConfigurationManager.AppSettings[CONFIG_ADGROUPNAMES_SERVICE_CENTER].Split('|'))
            {
                foreach (DirectoryItem di in LoadADInformation(adGroup.Split('/')[0], adGroup.Split('/')[1]))
                {
                    if (directoryItems.Any(x => x.Name == di.Name))
                        directoryItems.First(x => x.Name == di.Name).IsServiceCenter = true;
                    else
                    {
                        di.IsServiceCenter = true;
                        directoryItems.Add(di);
                    }
                }
            }

            // Pull in the Region Support
            foreach (string adGroup in ConfigurationManager.AppSettings[CONFIG_ADGROUPNAMES_REGION_SUPPORT].Split('|'))
            {
                foreach (DirectoryItem di in LoadADInformation(adGroup.Split('/')[0], adGroup.Split('/')[1]))
                {
                    if (directoryItems.Any(x => x.Name == di.Name))
                        directoryItems.First(x => x.Name == di.Name).IsRegionSupport = true;
                    else
                    {
                        di.IsRegionSupport = true;
                        directoryItems.Add(di);
                    }
                }
            }

            // Pull in the Region Managers
            foreach (string adGroup in ConfigurationManager.AppSettings[CONFIG_ADGROUPNAMES_REGION_MANAGER].Split('|'))
            {
                foreach (DirectoryItem di in LoadADInformation(adGroup.Split('/')[0], adGroup.Split('/')[1]))
                {
                    if (directoryItems.Any(x => x.Name == di.Name))
                        directoryItems.First(x => x.Name == di.Name).IsRegionManager = true;
                    else
                    {
                        di.IsRegionManager = true;
                        directoryItems.Add(di);
                    }
                }
            }

            // Pull in the PC Managers
            foreach (string adGroup in ConfigurationManager.AppSettings[CONFIG_ADGROUPNAMES_PC_MANAGER].Split('|'))
            {
                foreach (DirectoryItem di in LoadADInformation(adGroup.Split('/')[0], adGroup.Split('/')[1]))
                {
                    if (directoryItems.Any(x => x.Name == di.Name))
                        directoryItems.First(x => x.Name == di.Name).IsPCManager = true;
                    else
                    {
                        di.IsPCManager = true;
                        directoryItems.Add(di);
                    }
                }
            }

            return directoryItems;
        }


        private static List<DirectoryItem> LoadADInformation(string ParentGroupName, string GroupName)
        {
            List<DirectoryItem> directoryItems = new List<DirectoryItem>();

            // We need both the properties AND method names to look for.
            List<string> fields = DirectoryItemServices.GetDirectoryItemADProperties().Select(p => p.Key).ToList();
            fields.AddRange(DirectoryItemServices.GetDirectoryItemADMethods().Select(p => p.Key).ToList());


            try
            {
                foreach (SearchResult searchResult in
                            HajClassLib.ADInfo.SearchADInformation(
                                new TupleList<string, string> { { "ou", "people" } },
                                new List<string>() { "objectCategory=user", "memberOf=CN=" + GroupName + ",OU=" + ParentGroupName + ",OU=Groups,DC=Hajoca,DC=com" },
                                fields))
                {
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(searchResult.Properties);
                    DirectoryItem di = JsonConvert.DeserializeObject<DirectoryItem>(output, new ADDirectoryItemConverter());
                    directoryItems.Add(di);
                }
            }
            catch (Exception ex)
            {
                ErrorReporting.ReportError(ErrorReporting.ErrorReportType.HAJNET_Error, "InteractiveDirectory.Library", MethodBase.GetCurrentMethod(), ex);
            }

            return directoryItems;
        }

        /// <summary>
        /// Pulls the information from AS400 to be included in the directory. We grab all the attritubte names
        /// to map and build the list of fields for the PcTable function.
        /// </summary>
        /// <returns></returns>
        private static List<DirectoryItem> LoadPcInformation()
        {
            List<DirectoryItem> directoryItems = new List<DirectoryItem>();

            // Build a comman delimeted list of field names from both the properties and methosd marked in the class
            // with the AS400 attribute.
            string fieldsFromProperties = string.Join(",", DirectoryItemServices.GetDirectoryItemAS400Properties().Select(p => p.Key).ToArray());
            string fieldsFromMethods = string.Join(",", DirectoryItemServices.GetDirectoryItemAS400Methods().Select(p => p.Key).ToArray());

            //  Get the data table from AS400.
            //HajProfitCenter hajProfitCenter = new HajProfitCenter();
            /*DataTable dt = hajProfitCenter.PcTable(
                fieldsFromProperties + ((fieldsFromProperties.Length > 0) & (fieldsFromMethods.Length > 0) ? "," : "") + fieldsFromMethods,
                HajProfitCenter.TableOrder.OrderByPc, HajProfitCenter.PcType.TypeDirectory, false, true, true
             );*/
            DataTable dt = GetPcInfoWithCoords();
           

            // Serialize the object to a string and then send it into our custom deserialization/mapping process.
            string output = JsonConvert.SerializeObject(dt, new DataSetConverter());
            directoryItems = JsonConvert.DeserializeObject<List<DirectoryItem>>(output, new AS400DirectoryItemConverter());

            // Add some specific property info for our newly created list.
            directoryItems.ForEach(di =>
            {
                if ((di.ManagerID ?? "").Length > 0)
                    foreach (SearchResult searchResult in
                            HajClassLib.ADInfo.SearchADInformation(new TupleList<string, string> { { "ou", "people" } }, new List<string>() { "sAMAccountName=" + di.ManagerID }, new List<string>() { "mobile" }))
                        if (searchResult.Properties.Contains("mobile"))
                            di.Mobile = (string)searchResult.Properties["mobile"][0];
                            di.IsProfitCenter = true;
                            di.IsPCManager = false;
                            di.IsRegionManager = false;
                            di.IsServiceCenter = false;
                            di.IsRegionSupport = false;
            });

            return directoryItems;
        }

        private static DataTable GetPcInfoWithCoords()
        {
            OleDbDataAdapter _da = new OleDbDataAdapter();
            DataSet _ds = new DataSet();
            DataTable _dt = new DataTable();
            CommonFunctions oCommonFunctions = new CommonFunctions();
            //private OleDbConnection _cn = new OleDbConnection();
            FileSet _fs = new FileSet();
            using (OleDbConnection _cn = new OleDbConnection(new CommonFunctions().ConnString))
            {
                _da.SelectCommand = new OleDbCommand("Select pcren,a.pc,pcname,pcdiv,pcmgr,memail,pcstre,pcstct,pcstst,b8,pc10,pcmgr2,memai2,pcadd2,pccity,pcst,crdmgr,cremai,mgrid,pccod0,LATITUDE,LONGITUDE from HAJ.PRFCTRN a, HAJ.PCSYSTEM b, weblib.pccoords c where a.PC=b.PCACTU and a.PC=c.PC and b.PCACTU=c.PC and pcclos=0 and substring(digits(ifnull(nullif(pcstrt,0),202001)),5,2) ||'/'||'01'||'/'||substring(digits(ifnull(nullif(pcstrt,0),202001)),1,4)<=curdate() and (PCTYPE=91 OR PCTYPE=93 OR PCTYPE=98) and pcdelt='' order by a.pc for read only", _cn);
                try
                {               
                    _da.Fill(_ds);
                    _dt = _ds.Tables[0];
                }
                catch (Exception ex)
                {

                }
            }
            return _dt;
        }

        private static Dictionary<string, string> BuildEclipseBoxLookup()
        {
            // Lock up the process to make sure another process doesn't try to do the same thing.
            lock (EclipseLookupLock)
            {
                //Now that we're locked, check that it wasn't already created.
                Dictionary<string, string> CurrentCache = (Dictionary<string, string>)HttpRuntime.Cache[ECLIPSE_BOX_LOOKUP_CACHE_NAME];

                // If it was then another process already created it while we were waiting.
                if (CurrentCache != null) return CurrentCache;

                // It still isn't there.
                CurrentCache = new Dictionary<string, string>();

                // Load our data from web config.
                string lookups = ConfigurationManager.AppSettings[CONFIG_ECLIPSE_BOX_LOOKUP];
                if (lookups != null)
                    foreach (string lookup in lookups.Split('|'))
                        CurrentCache.Add(lookup.Split(':')[0], lookup.Split(':')[1]);

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string CacheResetInHours = ConfigurationManager.AppSettings[ECLIPSE_BOX_LOOKUP_CACHE_NAME];
                int cacheHours;
                if (!int.TryParse(CacheResetInHours, out cacheHours)) cacheHours = 8;
                HttpRuntime.Cache.Add(ECLIPSE_BOX_LOOKUP_CACHE_NAME, CurrentCache, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                // Return it.
                return CurrentCache;
            }
        }

        private static Dictionary<int, string> BuildRegionLookup()
        {
            // Lock up the process to make sure another process doesn't try to do the same thing.
            lock (RegionLookupLock)
            {
                //Now that we're locked, check that it wasn't already created.
                Dictionary<int, string> CurrentCache = (Dictionary<int, string>)HttpRuntime.Cache[REGION_NAME_LOOKUP_CACHE_NAME];

                // If it was then another process already created it while we were waiting.
                if (CurrentCache != null) return CurrentCache;

                // It still isn't there.
                CurrentCache = new Dictionary<int, string>();

                // Load our data from xml file.
                Stream stream = File.Open(ConfigurationManager.AppSettings[CONFIG_REGION_LOOKUP_XML_FILE], FileMode.Open, FileAccess.Read);
                DataContractSerializer xsIN = new DataContractSerializer(CurrentCache.GetType());
                CurrentCache = (Dictionary<int, string>)(xsIN.ReadObject(stream));
                stream.Close();

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string CacheResetInHours = ConfigurationManager.AppSettings[REGION_NAME_LOOKUP_CACHE_NAME];
                int cacheHours;
                if (!int.TryParse(CacheResetInHours, out cacheHours)) cacheHours = 8;
                HttpRuntime.Cache.Add(REGION_NAME_LOOKUP_CACHE_NAME, CurrentCache, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                // Return it.
                return CurrentCache;
            }
        }
        #endregion
    }
}