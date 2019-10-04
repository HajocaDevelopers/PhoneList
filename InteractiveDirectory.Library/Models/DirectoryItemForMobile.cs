using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace InteractiveDirectory.Models
{
    // This object will hold a subset of data from DirectoryItem so there is no need for 
    // extra markup.  We keep in clean with only the information really needed by the 
    // mobile platform.
    // We're using the DataMember Name to cut down of the json object that it sent to the user.
    [DataContract]
    public class DirectoryItemForMobile
    {
        [DataMember(Name = "N")]
        public string Name { get; set; }
        [DataMember(Name = "PC")]
        public string PC { get; set; }
        [DataMember(Name = "P")]
        public string Phone { get; set; }

        [DataMember(Name = "iSC")]
        public bool IsServiceCenter { get; set; }
        [DataMember(Name = "iRS")]
        public bool IsRegionSupport { get; set; }
        [DataMember(Name = "iPM")]
        public bool IsPCManager { get; set; }
        [DataMember(Name = "iRM")]
        public bool IsRegionManager { get; set; }
        [DataMember(Name = "iPC")]
        public bool IsProfitCenter { get; set; }
        [DataMember(Name = "iSR")]
        public bool HasShowroom { get; set; }

        [DataMember(Name = "EM")]
        public string Email { get; set; }
        [DataMember(Name = "M")]
        public string Mobile { get; set; }
        [DataMember(Name = "S")]
        public string Street { get; set; }
        [DataMember(Name = "C")]
        public string CSZ { get; set; }

        [DataMember(Name = "RN")]
        public string RegionName { get; set; }
        [DataMember(Name = "R")]
        public string RegionNumber { get; set; }
        [DataMember(Name = "MD")]
        public string ManagerDepartment { get; set; }
        [DataMember(Name = "E")]
        public string EclipseBox { get; set; }
        [DataMember(Name = "OC")]
        public string OnSiteContact { get; set; }


    }
}
