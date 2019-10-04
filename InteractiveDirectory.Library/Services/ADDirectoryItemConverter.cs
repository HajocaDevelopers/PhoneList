using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using InteractiveDirectory.Models;

namespace InteractiveDirectory.Services
{
    /// <summary>
    /// This is our custom mapping conversion utility that takes a serialized string of a SINGLE directory
    /// item pulled from AD and maps it into a DiretoryItem.  It will map all of the properties and methods
    /// that have been decorated with the ADIdentifier attribute with any fields in the serialized data
    /// that have the same name as the attribute.  All other fields are ignored.
    /// 
    /// This converter is meant to be run repeated times (in a loop) since it only works on one AD SearchResult 
    /// object at a time and not the entire list of SearchResults.
    /// 
    /// Note: LDAP SeaerchRestuls returns values as arrays whether they are or not.  This mapper will only ever
    /// grab the first item in the array.  If a property is supposed to be an array or the method expects to
    /// be hit muliple times you'll need to rework this converter to deal with arrays.  At the time of the creation
    /// of this utility we had no need for arrays so only the first value is brought it.
    /// </summary>
    public class ADDirectoryItemConverter : CustomCreationConverter<DirectoryItem>
    {
        public override DirectoryItem Create(Type objectType) {return new DirectoryItem();}

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var directoryItem = new DirectoryItem();

            // Get the list of properties and methods that have been marked with the ADIdentifier attribute.
            Dictionary<string, string> objProps = DirectoryItemServices.GetDirectoryItemADProperties();
            Dictionary<string, string> objMethods = DirectoryItemServices.GetDirectoryItemADMethods();
            string propertyName;
            string methodName;

            // Move through the SearchResult object.
            while (reader.Read())
            {
                // Loop through until we're at an AD field name.
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    // Read in the AD field name.
                    string adIdentifier = reader.Value.ToString().ToLower();  

                    //read in the property value.
                    if (reader.Read())  
                    {
                        // LDAP results serialize as an arrary, so we need to read the first value.
                        if (reader.TokenType == JsonToken.StartArray) reader.Read();

                        // Double check this is a field we actually care about and get the name of the field it maps to.
                        if (objProps.TryGetValue(adIdentifier, out propertyName))  // Property Mapping
                        {
                            // Get the field's type and convert and store the value.
                            PropertyInfo pi = directoryItem.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            var convertedValue = Convert.ChangeType(reader.Value, pi.PropertyType);
                            pi.SetValue(directoryItem, convertedValue, null);
                        }
                        else if (objMethods.TryGetValue(adIdentifier, out methodName))  // Method Mapping
                        {
                            // Get the method and execute it.
                            MethodInfo mi = directoryItem.GetType().GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            var convertedValue = Convert.ChangeType(reader.Value, mi.GetParameters()[0].ParameterType);
                            mi.Invoke(directoryItem, new object[] { convertedValue });
                        }

                    }
                }
            }
            return directoryItem;
        }
    }
}