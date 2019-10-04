using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace InteractiveDirectory.Models
{
    /// <summary>
    /// DirectoryItem object to hold a single directory item.  Please see the DirectoryItemAttribute
    /// class for an explanation of the attribute markups used by this model. 
    /// 
    /// DO NOT CHANGE PROPERTY NAMES WITHOUT EXSTENSIVE RESEARCH AND TESTING.  The properties
    /// in this object are directly linked to JS code via JSON serialization and changing them may 
    /// generate errors not caught at the compiler level.  You may add to the list of properties, but 
    /// changing or removing is not recommended.
    /// </summary>
    public class DirectoryItem
    {
        public DirectoryItem()
        {
            IsProfitCenter = false;
            IsRegionManager = false;
            IsRegionSupport = false;
            IsServiceCenter = false;
        }

        /// <summary>
        /// Properties to be filled and pushed to the client via JSON.
        /// </summary>
        #region Public Properties

        public string RegionNumber { get; set; }
        public int RegionNumberSort { get { return CovertToSafeIntForSorting(RegionNumber); } }
        public string RegionName { get; set; }
        [AS400Identifier("PCREN")]  // We want to gather the RegionName on build instead of runtime to save time.
        public void as400_RegionInfo(string value) { RegionNumber = value; RegionName = InteractiveDirectory.Services.DirectoryItemServices.RegioinNameLookup(RegionNumber); }

        [ADIdentifier("department")]
        public string PC { get; set; }
        [AS400Identifier("PC")]
        public void as400_PC(string value) { PC = ZeroFill(value,3); }
        public int PCSort { get { return CovertToSafeIntForSorting(PC); } }

        [ADIdentifier("sn")]
        public string ADLastName { get; set; }
        [ADIdentifier("givenname")]
        public string ADFirstName { get; set; }
        [AS400Identifier("PCNAME")]
        public string ASName { get; set; }
        public string Name
        {
            get
            {
                if ((ASName ?? "").Length > 0)
                {
                    return FormatTitleCaseDashException(ASName);
                }
                else
                    return ADFirstName + " " + ADLastName;
            }
        }

        [ADIdentifier("description")]
        public string ManagerDepartment { get; set; }
        [AS400Identifier("PCMGR")]
        public void as400_ManagerDepartment(string value) { ManagerDepartment = FormatTitleCaseDashException(value.Replace("-MGR", "").Replace("- MGR", "").TrimEnd()); }

        public string EmailAddress { get; set; }
        [AS400Identifier("MEMAIL")]
        public void as400_EmailAddress(string value) { EmailAddress = FormatEmail(value); }
        [ADIdentifier("mail")]
        public void AD_EmailAddress(string value) { EmailAddress = FormatEmail(value); }

        public string Phone { get; set; }
        [AS400Identifier("PCPHON")]
        public void as400_Phone(double value) { Phone = ConvertDoublePhone(value); }
        [ADIdentifier("telephonenumber")]
        public void AD_Phone(string value) { Phone = FormatPhone(value); }

        [AS400Identifier("PCDIV")]
        public string DivsionName { get; set; }

        public string Fax { get; set; }
        [AS400Identifier("FAXNO")]
        public void as400_Fax(double value) { Fax = ConvertDoublePhone(value); }
        [ADIdentifier("facsimiletelephonenumber")]
        public void AD_Fax(string value) { Fax = FormatPhone(value); }

        public string Street { get; set; }
        [AS400Identifier("PCSTRE")]
        [ADIdentifier("streetaddress")]
        public void as400_AD_Street(string value) { Street = FormatTitleCase(value); }

        [ADIdentifier("l")]
        public string City { get; set; }
        [AS400Identifier("PCSTCT")]
        public void as400_City(string value) { City = FormatTitleCase(value); }

        [ADIdentifier("st")]
        [AS400Identifier("PCSTST")]
        public string State { get; set; }

        public string Zip { get; set; }
        [ADIdentifier("postalcode")]
        public void AD_Zip(string value) { Zip = StripEmptyZipPlus4(value); }
        [AS400Identifier("PCSTZP")]
        public void as400_Zip(double value) { Zip = ConvertDoubleZip(value); }
        public string CSZ { get { return FormatCSZ(City, State, Zip); } }

        public string EclipseBox { get; set; }
        [AS400Identifier("LOSTMO")]
        public void as400_EclipesBox(string value) { EclipseBox = InteractiveDirectory.Services.DirectoryItemServices.EclipseBoxLookup(value); }

        public string EclipseNumber
        {
            get
            {
                if ((EclipseNumberB8 == null) || (EclipseNumberB8.Length == 0))
                    return EclipseNumberPC10;
                else
                    return EclipseNumberB8;
            }
        }
        [AS400Identifier("B8")]
        public string EclipseNumberB8 { get; set; }
        [AS400Identifier("PC10")]
        public string EclipseNumberPC10 { get; set; }

        public string OnSiteContact { get; set; }
        [AS400Identifier("PCMGR2")]
        public void as400_OnSiteContact(string value) { OnSiteContact = FormatTitleCase(value); }

        public string OnSiteContactEmail { get; set; }
        [AS400Identifier("MEMAI2")]
        public void as400_OnSiteContactEmail(string value) { OnSiteContactEmail = value.ToLower(); }

        public string POBox { get; set; }
        [AS400Identifier("PCADD2")]
        public void as400_POBox(string value) { POBox = FormatTitleCase(value); }

        public string POBoxCity { get; set; }
        [AS400Identifier("PCCITY")]
        public void as400_POBoxCity(string value) { POBoxCity = FormatTitleCase(value); }

        [AS400Identifier("PCST")]
        public string POBoxState { get; set; }

        public string POBoxZip { get; set; }
        [AS400Identifier("PCPOZP")]
        public void as400_POZip(double value) { POBoxZip = ConvertDoubleZip(value); }

        public string POCSZ { get { return FormatCSZ(POBoxCity, POBoxState, POBoxZip); } }

        public string CreditManager { get; set; }
        [AS400Identifier("CRDMGR")]
        public void as400_CreditManager(string value) { CreditManager = FormatTitleCase(value); }

        public string CreditMgrPhone { get; set; }
        [AS400Identifier("PCCRPH")]
        public void as400_CreditMgrPhone(string value) { CreditMgrPhone = FormatPhone(value); }

        public string CreditMgrEmail { get; set; }
        [AS400Identifier("CREMAI")]
        public void as400_CreditMgrEmail(string value) { CreditMgrEmail = value.ToLower(); }

        public string CreditMgrFax { get; set; }
        [AS400Identifier("CRFAXN")]
        public void as400_CreditMgrFax(string value) { CreditMgrFax = FormatPhone(value); }

        // Note: AS400 done manually in serivce based on ManagerID.
        public string Mobile { get; set; }
        [ADIdentifier("mobile")]
        public void AD_Mobile(string value) { Mobile = FormatPhone(value); }
        [AS400Identifier("MGRID")]
        public string ManagerID { get; set; }

        [ADIdentifier("info")]
        public string Keywords { get; set; }

        public bool IsServiceCenter { get; set; }
        public bool IsRegionSupport { get; set; }
        public bool IsRegionManager { get; set; }
        public bool IsPCManager { get; set; }
        public bool IsProfitCenter { get; set; }

        [AS400Identifier("LATITUDE")]
        public string latitude { get; set; }

        [AS400Identifier("LONGITUDE")]
        public string longitude { get; set; }

        public bool HasShowroom { get; set; }
        [AS400Identifier("PCCOD0")]
        public void checkShowroom(string value) 
        { 
            if (value.ToUpper() == "Y") { HasShowroom = true;}
            else{ HasShowroom = false;}
        }
        #endregion

        #region Private conversion methods.

        /// <summary>
        /// Returns a title case version of a string with the exception that
        /// any text after a "-" is left in uppercase.  It converts the whole string
        /// to a lower case string first since by default title casing skips all upper
        /// case words assuming it's an acronym. 
        /// Example:
        /// JOE GREEN       ->  Joe Green
        /// JOE GREEN-AC    ->  Joe Grreen-AC
        /// </summary>
        /// <param name="value">String to Title Case</param>
        /// <returns>Title Cased String</returns>
        private string FormatTitleCaseDashException(string value)
        {
            // AS400 data is usually upper case which is ignored by ToTitleCase 
            // so we convert it all to lower first.
            string name = value.ToLower();

            // Upper case anything after the dash so it's ignored by ToTitleCase and left in upper case.
            if (name.Contains("-")) name = name.Substring(0, name.IndexOf("-")) + name.Substring(name.IndexOf("-"), name.Length - name.IndexOf("-")).ToUpper();

            // Use the US culture info and make it Title Case.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

            return myTI.ToTitleCase(name);
        }

        private string ZeroFill(string value, int length)
        {
            if (value.Length<length)
                return value.PadLeft(length, '0');
            else
               return value;
        }
        
        private string FormatTitleCase(string value)
        {
            // Use the US culture info and make it Title Case.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

            // AS400 data is usually upper case which is ignored by ToTitleCase 
            // so we convert it all to lower first.
            return myTI.ToTitleCase(value.ToLower());
        }

        private string FormatEmail(string value)
        {
            return value.ToLower();
        }

        private string FormatPhone(string value)
        {
            Regex rgx = new Regex("[^0-9]");
            value = rgx.Replace(value, "");
            if (value.Length == 0) return "";
            if (value.Length > 10) value = value.Substring(0, 10);
            return Convert.ToInt64(value).ToString("000-000-0000");
        }

        private string ConvertDoublePhone(double value)
        {
            return Convert.ToInt64(value).ToString("000-000-0000");
        }

        private string ConvertDoubleZip(double value)
        {
            string zip = value.ToString("000000000");

            if (zip == "000000000") zip = "";
            else if (zip.EndsWith("0000")) zip = zip.Substring(0, 5);
            else zip = zip.Substring(0, 5) + "-" + zip.Substring(5, 4);

            return zip;
        }

        private string StripEmptyZipPlus4 (string value)
        {
            string zip = value;
            if (zip.EndsWith("-0000")) zip = zip.Substring(0, zip.Length - 5);
            return zip;
        }

        private string FormatCSZ(string city, string state, string zip)
        {
            string ret = city ?? "";
            if ((city ?? "").Length > 0 && ((state ?? "").Length > 0 || (zip ?? "").Length > 0)) ret += ", ";
            ret += state ?? "";
            if (ret.Length > 0) ret += " ";
            ret += zip ?? "";

            return ret;
        }

        private int CovertToSafeIntForSorting(string value)
        {
            int res;
            if (Int32.TryParse(value, out res))
                return res;
            else
                return 0;
        }

        #endregion

        # region Serialization Metods for Excel (CSV) Exporting

        public void SerializeAsCsv(Stream stream, bool includeMobile = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Escape(RegionNumber) + ",");
            sb.Append(Escape(RegionName) + ",");
            sb.Append(Escape(DivsionName) + ",");
            sb.Append(Escape(PC) + ",");
            sb.Append(Escape(Name) + ",");
            sb.Append(Escape(ManagerDepartment) + ",");
            sb.Append(Escape(EmailAddress) + ",");
            sb.Append(Escape(Phone) + ",");
            if (includeMobile) sb.Append(Escape(Mobile) + ",");
            sb.Append(Escape(Fax) + ",");
            sb.Append(Escape(Street) + ",");
            sb.Append(Escape(City) + ",");
            sb.Append(Escape(State) + ",");
            sb.Append(Escape(Zip) + ",");
            sb.Append(Escape(EclipseBox) + ",");
            sb.Append(Escape(EclipseNumber) + ",");
            sb.Append(Escape(OnSiteContact) + ",");
            sb.Append(Escape(OnSiteContactEmail) + ",");
            sb.Append(Escape(POBox) + ",");
            sb.Append(Escape(POBoxCity) + ",");
            sb.Append(Escape(POBoxState) + ",");
            sb.Append(Escape(POBoxZip) + ",");
            sb.Append(Escape(CreditManager) + ",");
            sb.Append(Escape(CreditMgrPhone) + ",");
            sb.Append(Escape(CreditMgrFax) + ",");
            sb.Append(Escape(CreditMgrEmail) + "\n");
            string value = sb.ToString();
            stream.Write(Encoding.Default.GetBytes(value), 0, value.Length);
        }

        public void SerializeCSVHeader(Stream stream, bool includeMobile = false)
        {
            string value;

            value =
                "Region Number," +
                "Region Name," +
                "Division," +
                "PC," +
                "Name," +
                "PC Manager / Department," +
                "Email," +
                "Phone #," +
                (includeMobile ? "Mobile #," : "") +
                "Fax #," +
                "Street," +
                "City," +
                "State," +
                "Zip," +
                "Eclipse Box," +
                "Eclipse #," +
                "On-Site Contact," +
                "On-Site Contact Email," +
                "PO Box," +
                "PO Box City," +
                "PO Box State," +
                "PO Box Zip," +
                "Credit Manager," +
                "Credit Manager Phone #," +
                "Credit Manager Fax #," +
                "Credit Manager Email," +
                "\n";
            stream.Write(Encoding.Default.GetBytes(value), 0, value.Length);
        }

        private string Escape(string s)
        {
            if (s == null) return "";
            StringBuilder sb = new StringBuilder();
            bool needQuotes = false;
            foreach (char c in s.ToCharArray())
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); needQuotes = true; break;
                    case ' ': sb.Append(" "); needQuotes = true; break;
                    case ',': sb.Append(","); needQuotes = true; break;
                    case '\t': sb.Append("\\t"); needQuotes = true; break;
                    case '\n': sb.Append("\\n"); needQuotes = true; break;
                    default: sb.Append(c); break;
                }
            }
            if (needQuotes)
                return "\"" + sb.ToString() + "\"";
            else
                return sb.ToString();
        }

        #endregion

    }
}