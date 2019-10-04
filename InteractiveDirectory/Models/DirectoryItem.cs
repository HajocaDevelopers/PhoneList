using System;
using System.IO;
using System.Text;

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
        /// <summary>
        /// Properties to be filled and pushed to the client via JSON.
        /// </summary>
        #region Public Properties

        [AS400Identifier("PCREN")]
        // No AD Equivalent Filed
        public string RegionNumber { get; set; }

        [AS400Identifier("PC")]
        [ADIdentifier("Department")]
        public string PC { get; set; }

        [AS400Identifier("PCNAME")]
        [ADIdentifier("Name")] 
        public string Name { get; set; }

        [AS400Identifier("PCMGR")]
        [ADIdentifier("Description")]  
        public string ManagerDeparment { get; set; }

        [AS400Identifier("MEMAIL")]
        [ADIdentifier("Email")]
        public string EmailAddress { get; set; }

        // AS400 Pulled in via as400_Phone
        // AD    Pulled in via AD_Phone
        public string Phone { get; set; }

        // AS400 Pulled in via as400_Fax
        [ADIdentifier("facsimileTelephoneNumber")]  
        public string Fax { get; set; }

        [AS400Identifier("PCSTRE")]
        [ADIdentifier("streetAddress")]  
        public string Street { get; set; }

        [AS400Identifier("PCSTCT")]
        // todo: Specs said "City", could not find that in AD.
        public string City { get; set; }

        [AS400Identifier("PCSTST")]
        [ADIdentifier("st")] 
        public string State { get; set; }

        // AS400 Pulled in via as400_Zip
        [ADIdentifier("postalCode")]
        public string Zip { get; set; }

        public string CSZ
        {
            get
            {
                return City + "," + State + " " + Zip;
            }
        }

        // AS400 Pulled in via as400_EclipesBox
        // No AD Equivalent Filed
        public string EclipseBox { get; set; }

        // AS400 - Special field Calculations
        // No AD Equivalent Filed
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

        [AS400Identifier("PCMGR2")]
        // No AD Equivalent Filed
        public string OnSiteContact { get; set; }

        [AS400Identifier("MEMAI2")]
        // No AD Equivalent Filed
        public string OnSiteContactEmail { get; set; }

        [AS400Identifier("PCADD2")]
        // No AD Equivalent Filed
        public string POBox { get; set; }

        [AS400Identifier("PCCITY")]
        // No AD Equivalent Filed
        public string POBoxCity { get; set; }

        [AS400Identifier("PCST")]
        // No AD Equivalent Filed
        public string POBoxState { get; set; }

        // AS400 Pulled in via as400_POZip
        // No AD Equivalent Filed
        public string POBoxZip { get; set; }

        [AS400Identifier("CRDMGR")]
        // No AD Equivalent Filed
        public string CreditManager { get; set; }

        // AS400 Pulled in via as400_CreditMgrPhone
        // No AD Equivalent Filed
        public string CreditMgrPhone { get; set; }

        [AS400Identifier("CREMAI")]
        // No AD Equivalent Filed
        public string CreditMgrEmail { get; set; }

        // todo: Need to get AS400 via AD (yikes).
        [ADIdentifier("mobile")]
        public string Mobile { get; set; }

        // todo:  Need to get this.
        public string Keywords { get; set; }

        public bool IsServiceCenter { get; set; }
        public bool IsRegionStaff { get; set; }
        public bool IsProfitCenter { get; set; }
        public bool IsRegionManager { get; set; }

        #endregion

        /// <summary>
        /// Methods used on properties that do not come in as the properly formatted value
        /// and need to be converted before set as a property.  Note:  "WriteOnly" properties
        /// could have been used but are considered bad practice and methods are used instead.
        /// </summary>
        /// <param name="value">The value to be converted into a property.</param>
        #region Public Methods used for AS400 conversions to properties.

       
        // Note: Doubles cannont be converted straight to nullable varaibles so we
        // need to dance around it a little bit to get a nullable int for a PC.
        //public void as400_PC(double value)
        //{
        //    int newPC;
        //    if (int.TryParse(value.ToString(), out newPC))
        //        PC = newPC.ToString();
        //    else
        //        PC = null;
        //}

        [AS400Identifier("PCPHON")]
        public void as400_Phone(float value)
        {
            Phone = ConvertFloatPhone(value);
        }

        [AS400Identifier("FAXNO")]
        public void as400_Fax(float value)
        {
            Fax = ConvertFloatPhone(value);
        }

        [AS400Identifier("PCSTZP")]
        public void as400_Zip(float value)
        {
            Zip = ConvertDoubleZip(value);
        }

        [AS400Identifier("PCSUP3")]
        public void as400_EclipesBox(string value)
        {
            try
            {
                EclipseBox = InteractiveDirectory.Services.DirectoryItemServices.EclipseBoxLookup(value);
            }
            catch (Exception)
            {
                EclipseBox = "Err";
            }
        }

        [AS400Identifier("PCPOZP")]
        public void as400_POZip(float value)
        {
            POBoxZip = ConvertDoubleZip(value);
        }

        #endregion

        #region Public Methods used for AD conversions to properties.

        [ADIdentifier("telephonNumber")]
        public void AD_Phone(string value)
        {
            Phone = value.Replace('.', '-');
        }

        [AS400Identifier("PCCRPH")]
        public void as400_CreditMgrPhone(string value)
        {
            if (value.Length == 10)
                CreditMgrPhone = value.Substring(0, 3) + "-" + value.Substring(3, 3) + "-" + value.Substring(6, 4);
            else
                CreditMgrPhone = value;
        }
        #endregion


        /// <summary>
        /// These functions are used to convert data that is consdered too specific to use
        /// a common function.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns></returns>
        #region Private conversion methods.

        private string ConvertFloatPhone(float value)
        {
            return Convert.ToInt64(value).ToString("000-000-0000");
        }
        private string ConvertDoubleZip(double value)
        {
            string zip = value.ToString();
            if (zip == "0") zip = "";
            else if ((zip.Substring(5) == "0000") || (zip.Substring(5) == "000")) zip = zip.Substring(0, 5);
            else zip = Convert.ToInt64(zip).ToString("00000-0000");

            return zip;
        }

        #endregion

        string Escape(string s)
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

        public void SerializeAsCsv(Stream stream, bool restricted)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Escape(RegionNumber) + ",");
            sb.Append(Escape(PC) + ",");
            sb.Append(Escape(Name) + ",");
            sb.Append(Escape(ManagerDeparment) + ",");
            sb.Append(Escape(EmailAddress) + ",");
            sb.Append(Escape(Phone) + ",");
            if (!restricted) sb.Append(Escape(Mobile) + ",");
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
            sb.Append(Escape(CreditMgrEmail) + "\n");
            string value = sb.ToString();
            stream.Write(Encoding.Default.GetBytes(value), 0, value.Length);
        }

        public void SerializeCSVHeader(Stream stream, bool restricted)
        {
            string value;

            value = "Region Number," +
                "PC," +
                "Name," +
                "PC Manager / Department," +
                "Email," +
                "Phone #,";
            if (!restricted)  value += "Mobile #,";
            value += "Fax #," +
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
                "Credit Manager Email," +
                "\n";
            stream.Write(Encoding.Default.GetBytes(value), 0, value.Length);
        }
    }
} 