using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InteractiveDirectory.Models
{
    /// <summary>
    /// Directory object to hold a filtered, sorted, paginated block of DirectoryItems.
    /// 
    /// DO NOT CHANGE PROPERTY NAMES WITHOUT EXSTENSIVE RESEARCH AND TESTING.  The properties
    /// in this object are directly linked to JS code via JSON serialization and changing them may 
    /// generate errors not caught at the compiler level.  You may add to the list of properties, but 
    /// changing or removing is not recommended.
    /// </summary>
    public class CompanyDirectory
    {
        public List<DirectoryItem> DirectoryItems { get; set; } // List of filtered, sorted, paginated directory items.
        public int page { get; set; }  // Page of the data actually returned.
        public int pageSize { get; set; }  // Number of items considered as a "Page"
        public int totalRecords { get; set; } // Total number of records proir to pagination.
        public int totalPages { get; set; } // Total number of pages based on pagination.
        public bool showMobile { get; set; } // Whether or not to show the Mobil column.

        /// <summary>
        /// Default constructor to ensure DirectoryItems is never "Nothing" and will by default
        /// always contain an empty list of DirectortItems.
        /// </summary>
        public CompanyDirectory()
        {
            DirectoryItems = new List<DirectoryItem>();
        }
    }
}