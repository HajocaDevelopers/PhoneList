using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Configuration;
using System.Data;
using InteractiveDirectory.Models;
using HajClassLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InteractiveDirectory.Services
{
    public class DirectoryItemServices
    {
        #region "Local Variables and Constants"

        // Web.Config - Used to avoid typos and let us know what Web.Config values are used.
        private const string WEB_CONFIG_CASH_RESET_IN_HOURS = "CashResetInHours";
        private const string WEB_CONFIG_SERVICE_CENTER_EMPLOYEES = "ServiceCenterEmployeesADGroupName";
        private const string WEB_CONFIG_REGION_STAFF = "RegionStaffADGroupName";
        private const string WEB_CONFIG_ECLIPSE_BOX_LOOKUP = "EclipseBoxLookup";

        // Cache Constants - Used to aviod typos.
        private const string CURRENT_DIRECTORY_CACHE_NAME = "CurrentDirectory";
        private const string AD_DI_PROPERTIES_CACHE_NAME = "ADProps";
        private const string AD_DI_METHODS_CACHE_NAME = "ADMethods";
        private const string AS400_DI_PROPERTIES_CACHE_NAME = "AS400Props";
        private const string AS400_DI_METHODS_CACHE_NAME = "AS400Methods";
        private const string ECLIPSE_BOX_LOOKUP_CACHE_NAME = "EclipseBoxLookup";

        // Lock Object - Used to lock the process of creating the cached directory so multiple
        // users don't wast their time getting the same data.  Once it's cached the waiting 
        // users will get the cached value.
        private static object CurrentDirectoryLock = new object();
        private static object EclipseLookupLock = new object();

        #endregion

        #region "Public Structures"

        /// <summary>
        /// List of different "View Types" used as a filter on the directory.
        /// </summary>
        public enum directoryViewType
        {
            All = 0,
            ServiceCenter = 1,
            RegionStaff = 2,
            PCOnly = 3,
            RegionManager = 4
        }

        #endregion

        #region "Public Methods"

        public static string EclipseBoxLookup(string PCSup3)
        {
            string ret;
            Dictionary<string, string> CurrentCache = (Dictionary<string, string>)System.Web.HttpRuntime.Cache[ECLIPSE_BOX_LOOKUP_CACHE_NAME];

            if (CurrentCache == null) CurrentCache = BuildEclipseBoxLookup();
            if (!CurrentCache.TryGetValue(PCSup3, out ret)) ret = "???";
            return ret;
        }

        /// <summary>
        /// Currently cached directory.  If there is no cached list it will 
        /// automatically call the function to build it.
        /// </summary>
        /// <returns>All Directory Items</returns>
        public static List<DirectoryItem> GetDirectory()
        {
            List<DirectoryItem> CurrentCache = (List<DirectoryItem>)System.Web.HttpRuntime.Cache[CURRENT_DIRECTORY_CACHE_NAME];
            if (CurrentCache == null) CurrentCache = BuildCurrentDirectory();
            return new List<DirectoryItem>(CurrentCache);   // Note: "new List" is used to clone the list at this point.
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
        public static List<DirectoryItem> GetDirectory(string filter)
        {
            List<DirectoryItem> returnList = GetDirectory();
            foreach (string f in filter.Split(' '))
            {
                string value = f.Trim().ToLower();
                if (value.Length > 0)
                {
                    returnList.RemoveAll(x => !((x.RegionNumber != null && x.RegionNumber.ToLower().Contains(value)) ||
                                                (x.PC != null && x.PC.ToLower().Contains(value)) ||
                                                (x.Name != null && x.Name.ToLower().Contains(value)) ||
                                                (x.ManagerDeparment != null && x.ManagerDeparment.ToLower().Contains(value)) ||
                                                (x.EmailAddress != null && x.EmailAddress.ToLower().Contains(value)) ||
                                                (x.Phone != null && x.Phone.ToLower().Contains(value)) ||
                                                (x.Fax != null && x.Fax.ToLower().Contains(value)) ||
                                                (x.Street != null && x.Street.ToLower().Contains(value)) ||
                                                (x.City != null && x.City.ToLower().Contains(value)) ||
                                                (x.State != null && x.State.ToLower().Contains(value)) ||
                                                (x.Zip != null && x.Zip.ToLower().Contains(value)) ||
                                                (x.EclipseBox != null && x.EclipseBox.ToLower().Contains(value)) ||
                                                (x.EclipseNumber != null && x.EclipseNumber.ToLower().Contains(value)) ||
                                                (x.OnSiteContact != null && x.OnSiteContact.ToLower().Contains(value)) ||
                                                (x.OnSiteContactEmail != null && x.OnSiteContactEmail.ToLower().Contains(value)) ||
                                                (x.POBox != null && x.POBox.ToLower().Contains(value)) ||
                                                (x.POBoxCity != null && x.POBoxCity.ToLower().Contains(value)) ||
                                                (x.POBoxState != null && x.POBoxState.ToLower().Contains(value)) ||
                                                (x.POBoxZip != null && x.POBoxZip.ToLower().Contains(value)) ||
                                                (x.CreditManager != null && x.CreditManager.ToLower().Contains(value)) ||
                                                (x.CreditMgrPhone != null && x.CreditMgrPhone.ToLower().Contains(value)) ||
                                                (x.CreditMgrEmail != null && x.CreditMgrEmail.ToLower().Contains(value)) ||
                                                (x.Mobile != null && x.Mobile.ToLower().Contains(value)) ||
                                                (x.Keywords != null && x.Keywords.ToLower().Contains(value))
                                                ));

                }
            }
            return returnList;
        }

        /// <summary>
        /// Main function used for returning data.  All pameters are optional.  This
        /// function will use the other GetDirectory functions as needed to produce the
        /// final filter, sorted, paginated results.
        /// </summary>
        /// <param name="page">Which page of data to return, based on pageSize.  Default is page 1.</param>
        /// <param name="pageSize">How many items are considered a page.  Default is ALL items (1 page).</param>
        /// <param name="sort">Comma delimeted list of sort values that can contain ASC/DESC if needed:  "RegionName DESC,Name".  Default is LastNamePCName,FirstName.  Field names must match DirectoryItem properties!</param>
        /// <param name="filter">Space delimited list of items to filter: "joe green 610".</param>
        /// <param name="viewType">directoryViewType to indicate a filter at the ViewType leverl.  Default is "All".</param>
        /// <returns>List of filtered, sorted, paginated Directory Items in a Directory object which will also contain information about what data was returned.</returns>
        public static Directory GetDirectory(int? page, int? pageSize, string sort = "", string filter = "", directoryViewType viewType = directoryViewType.All)
        {
            Directory returnData = new Directory();
            List<DirectoryItem> returnItems;
            if (filter.Length > 0)
                returnItems = GetDirectory(filter);
            else
                returnItems = GetDirectory();

            switch (viewType)
            {
                case directoryViewType.All:
                    break;
                case directoryViewType.ServiceCenter:
                    returnItems.RemoveAll(x => x.IsServiceCenter == false);
                    break;
                case directoryViewType.RegionStaff:
                    returnItems.RemoveAll(x => x.IsRegionStaff == false);
                    break;
                case directoryViewType.PCOnly:
                    returnItems.RemoveAll(x => x.IsProfitCenter == false);
                    break;
                case directoryViewType.RegionManager:
                    returnItems.RemoveAll(x => x.IsRegionManager == false);
                    break;
                default:
                    break;
            }

            List<Tuple<string, string>> sortTuple = new List<Tuple<string, string>>();
            if (sort == null || sort.Trim().Length == 0) sort = "Name";
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
            returnData.totalPages = (int)Math.Ceiling(((decimal)returnItems.Count() / returnData.pageSize));
            returnData.totalRecords = returnItems.Count();

            if (skip < returnItems.Count())
                returnData.DirectoryItems = returnItems.MultipleSort(sortTuple).Skip(skip).Take((int)pageSize).ToList();

            return returnData;
        }

        public static string GetCSVDirectory(string sort = "", string filter = "", directoryViewType viewType = directoryViewType.All, bool restricted = true)
        {
            List<DirectoryItem> dis = GetDirectory(1, 0, sort, filter, viewType).DirectoryItems;

            using (var ms = new System.IO.MemoryStream())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Date:," + DateTime.Now.ToString("MM/dd/yyyy") + ",");
                sb.Append("Filter:,=\"" + ((filter.Length>0)?filter:"[None]") + "\",");
                sb.Append("Sort:," + ((sort.Length > 0) ? sort : "[Default]") + ",");
                sb.Append("Directory:," + viewType.ToString());
                sb.Append("\n");

                string value = sb.ToString();
                ms.Write(Encoding.Default.GetBytes(value), 0, value.Length);

                //  Get an empty DirectoryItem so we can get a CSV "Header"
                DirectoryItem dih = new DirectoryItem();
                dih.SerializeCSVHeader(ms, restricted);

                foreach (DirectoryItem di in dis)
                    di.SerializeAsCsv(ms, restricted);
                ms.Position = 0;
                var sr = new System.IO.StreamReader(ms);
                return sr.ReadToEnd();
            }
        }


        /// <summary>
        /// Removes the currently cached directorty from the cache.  Since the data is automatically
        /// built when it's accessed we don't both re-building it here.
        /// </summary>
        public static void ResetCurrentDirectory()
        {
            HttpRuntime.Cache.Remove(CURRENT_DIRECTORY_CACHE_NAME);
            //Note: We don't bother to reload since that will happen automatically when GetCurrentDirectory is called.
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
                        .Value.ToString().ToLower(),
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
                        .Value.ToString().ToLower(),
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

        #endregion

        #region "Private Methods"

        /// <summary>
        /// This is the main building of the directory.  It will build, cache and return the list of direcotry items.
        /// It will first lock the process so if that two processes try to access this portion of code only the 
        /// first will proceed to build the data.  Once the first person has released it the next person will lock
        /// it but immedietally find it was created by the first person and get the cached data back and unlock it
        /// for the next person that might have come along.  Only the first person in should fire off this process
        /// since it does take several seconds to get the data from all the different sources.
        /// 
        /// Currently data is pulled from both an AS400 call as well as multiple AD seraches.
        /// </summary>
        /// <returns>List of currently (possibly freshly) cached direcotry items.</returns>
        private static List<DirectoryItem> BuildCurrentDirectory()
        {
            // Lock up the process to make sure another process doesn't try to do the same thing.
            lock (CurrentDirectoryLock)
            {
                //Now that we're locked, check that it wasn't already created.
                List<DirectoryItem> CurrentCache = (List<DirectoryItem>)HttpRuntime.Cache[CURRENT_DIRECTORY_CACHE_NAME];

                // If it was then another process already created it while we were waiting.
                if (CurrentCache != null) return CurrentCache;

                // It still isn't there.
                CurrentCache = new List<DirectoryItem>();

                // Load our data from private methods.
                CurrentCache.AddRange(LoadPcInformation());
                CurrentCache.AddRange(LoadADInformation());

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string cashResetInHours = ConfigurationManager.AppSettings[WEB_CONFIG_CASH_RESET_IN_HOURS];
                int cacheHours;
                if (!int.TryParse(cashResetInHours, out cacheHours)) cacheHours = 8;
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

            // We need both the properties AND method names to look for.
            List<string> fields = DirectoryItemServices.GetDirectoryItemADProperties().Select(p => p.Key).ToList();
            fields.AddRange(DirectoryItemServices.GetDirectoryItemADMethods().Select(p => p.Key).ToList());

            // Pull in the Service Center Employees
            string GroupName = ConfigurationManager.AppSettings[WEB_CONFIG_SERVICE_CENTER_EMPLOYEES];
            try
            {
                /// Because AD does not return a clean data table like the AS400 we need to manually go through 
                /// the results and add the directory items one at a time.
                foreach (SearchResult searchResult in
                            HajClassLib.ADInfo.SearchADInformation(
                                new TupleList<string, string> { { "ou", "people" } },
                                new List<string>() { "objectCategory=user", "memberOf=CN=" + GroupName + ",OU=Distribution Groups,OU=Groups,DC=Hajoca,DC=com" },
                                fields))
                {
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(searchResult.Properties);
                    DirectoryItem di = JsonConvert.DeserializeObject<DirectoryItem>(output, new ADDirectoryItemConverter());
                    di.IsServiceCenter = true;
                    directoryItems.Add(di);
                }
            }
            catch (Exception)
            {
                //todo: Report the problem.
                throw;
            }

            // Pull in the Region Staff Employees
            GroupName = ConfigurationManager.AppSettings[WEB_CONFIG_REGION_STAFF];
            try
            {
                /// Because AD does not return a clean data table like the AS400 we need to manually go through 
                /// the results and add the directory items one at a time.
                foreach (SearchResult searchResult in
                            HajClassLib.ADInfo.SearchADInformation(
                                new TupleList<string, string> { { "ou", "people" } },
                                new List<string>() { "objectCategory=user", "memberOf=CN=" + GroupName + " Staff,OU=Distribution Groups,OU=Groups,DC=Hajoca,DC=com" },
                                fields))
                {
                    // Serialize the object to a string and then send it into our custom deserialization/mapping process.
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(searchResult.Properties);
                    DirectoryItem di = JsonConvert.DeserializeObject<DirectoryItem>(output, new ADDirectoryItemConverter());

                    // Add some specific property info and add it to our list.
                    di.IsRegionStaff = true;
                    directoryItems.Add(di);
                }
            }
            catch (Exception)
            {
                //todo: Report the problem.
                throw;
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
            HajProfitCenter hajProfitCenter = new HajProfitCenter();
            DataTable dt = hajProfitCenter.PcTable(
                fieldsFromProperties + ((fieldsFromProperties.Length > 0) & (fieldsFromMethods.Length > 0) ? "," : "") + fieldsFromMethods,
                HajProfitCenter.TableOrder.OrderByPc, HajProfitCenter.PcType.TypeDirectory, false, true, true
             );

            // Serialize the object to a string and then send it into our custom deserialization/mapping process.
            string output = JsonConvert.SerializeObject(dt, new DataSetConverter());
            directoryItems = JsonConvert.DeserializeObject<List<DirectoryItem>>(output, new AS400DirectoryItemConverter());

            // Add some specific property info for our newly created list.
            // todo: this needs to be redone to set Service Center on certain PCs and Region Staff on other PCs.  We are still waiting on the magic number that determines which is which.
            directoryItems.ForEach(di =>
            {
                di.IsProfitCenter = true;
                di.IsRegionManager = true;
                di.IsServiceCenter = false;
                di.IsRegionStaff = false;
            });

            return directoryItems;
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
                string lookups = ConfigurationManager.AppSettings[WEB_CONFIG_ECLIPSE_BOX_LOOKUP];
                if (lookups != null)
                    foreach (string lookup in lookups.Split('|'))
                        CurrentCache.Add(lookup.Split(':')[0], lookup.Split(':')[1]);

                // Store it in the cache for the number of hours requested in the web.config (or 8 if not set).
                string cashResetInHours = ConfigurationManager.AppSettings[ECLIPSE_BOX_LOOKUP_CACHE_NAME];
                int cacheHours;
                if (!int.TryParse(cashResetInHours, out cacheHours)) cacheHours = 8;
                HttpRuntime.Cache.Add(ECLIPSE_BOX_LOOKUP_CACHE_NAME, CurrentCache, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.High, null);

                // Return it.
                return CurrentCache;
            }
        }
        #endregion
    }
}