using System;

namespace InteractiveDirectory.Models
{
    /// <summary>
    /// The ADIdentifierAttribute is used by the custom ADDirectoryItemConverter to map results
    /// of an AD search into a list of Direcotry Items.  By marking a property or method of
    /// the DirectoryItem class it will tell the mapper to look for a field named in the attribute
    /// in the list of AD fields returned and populate it.
    /// 
    /// Note: You must make sure that the data type of the returning value is directly convertable
    /// to the field in the DirectoryItem class that holds the attribute or the "value" of the method.  
    /// 
    /// Methods should be used if the desired property does not match the data type of what is returned
    /// or if there is special formatting required.
    /// 
    /// To use the attribute simply give it a value that matches the AD Field Name:
    /// Property Example:
    ///     [ADIdentifier("givenname")]
    ///     public string FirstName { get; set; }
    /// Method Example:
    ///     public string Fax { get; set; }
    ///     
    ///     [ADIdentifier("fax")]
    ///     public void AD_Fax(float value)
    ///     {
    ///         Fax = value.ToString().Substring(0,10);
    ///     }
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class ADIdentifierAttribute : Attribute
    {
        public string Value { get; set; }
        public ADIdentifierAttribute(string value)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// The AS400IdentifierAttribute is used by the custom AS400DirectoryItemConverter to map results
    /// of an AS400 call into a list of Direcotry Items.  By marking a property or method of
    /// the DirectoryItem class it will tell the mapper to look for a field named in the attribute
    /// in the list of AS400 table fields returned and populate it.
    /// 
    /// Note: You must make sure that the data type of the returning value is directly convertable
    /// to the field in the DirectoryItem class that holds the attribute or the "value" of the method.  
    /// 
    /// Methods should be used if the desired property does not match the data type of what is returned
    /// or if there is special formatting required.
    /// 
    /// To use the attribute simply give it a value that matches the AD Field Name:
    /// Property Example:
    ///     [AS400IdentifierAttribute("fname")]
    ///     public string FirstName { get; set; }
    /// Method Example:
    ///     public string Fax { get; set; }
    ///     
    ///     [AS400IdentifierAttribute("fax")]
    ///     public void AD_Fax(float value)
    ///     {
    ///         Fax = value.ToString().Substring(0,10);
    ///     }
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class AS400IdentifierAttribute : Attribute
    {
        public string Value { get; set; }
        public AS400IdentifierAttribute(string value)
        {
            this.Value = value;
        }
    }
}