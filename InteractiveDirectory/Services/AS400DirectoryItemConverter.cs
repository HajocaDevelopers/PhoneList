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
    /// This is our custom mapping conversion utility that takes a serialized string of an entire table
    /// of AS400 results it into a list of DiretoryItems.  It will map all of the properties and methods
    /// that have been decorated with the AS400Identifier attribute with any fields in the serialized data
    /// that have the same name as the attribute.  All other fields are ignored.
    /// 
    /// This converter is meant to be run on an entire table (unlike the AD version which is one "row" at time).
    /// </summary>
    public class AS400DirectoryItemConverter : CustomCreationConverter<List<DirectoryItem>>
    {
        public override List<DirectoryItem> Create(Type objectType) { return new List<DirectoryItem>(); }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<DirectoryItem> directoryItems = new List<DirectoryItem>();
            DirectoryItem directoryItem = null;

            // Get the list of properties and methods that have been marked with the ADIdentifier attribute.
            Dictionary<string, string> objProps = DirectoryItemServices.GetDirectoryItemAS400Properties();
            Dictionary<string, string> objMethods = DirectoryItemServices.GetDirectoryItemAS400Methods();
            string propertyName;
            string methodName;

            /* Conversions aren't done like "rows" and "fields" in a data table.  What we actually get are markers 
             * (TokenTypes) telling us what this particular piece of information is.  So we need to loop through it 
             * all and watch the TokenTypes to know if this is the equivalent of a new "row" or a "field" in the 
             * current row.*/
            while (reader.Read())
            {
                // Loop through until we're at new "row" (StartObject) or an AD field name (PropertyName) or
                // we know we've gotten all the properties of the current "row" (EndObject).
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject: // This is a new "row".
                        directoryItem = new DirectoryItem();
                        break;
                    case JsonToken.PropertyName: // This is a "field".
                        // Read in the field name.
                        string fieldIdentifier = reader.Value.ToString().ToLower();

                        // Read in the property value.  It will be next in the reader.
                        if (reader.Read())
                        {
                            // Double check this is a field we actually care about and get the name of the field it maps to.
                            if (objProps.TryGetValue(fieldIdentifier, out propertyName))  // Look for propery.
                            {
                                // Get the field's type and convert and store the value.
                                PropertyInfo pi = directoryItem.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                                var convertedValue = Convert.ChangeType(reader.Value, pi.PropertyType);
                                pi.SetValue(directoryItem, convertedValue, null);
                            }
                            else if (objMethods.TryGetValue(fieldIdentifier, out methodName))  // Look for method.
                            {
                                // Get the method and execute it.
                                MethodInfo mi = directoryItem.GetType().GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                                var convertedValue = Convert.ChangeType(reader.Value, mi.GetParameters()[0].ParameterType);
                                mi.Invoke(directoryItem, new object[] { convertedValue });
                            }
                        }
                        break;
                    case JsonToken.EndObject: // We've reached the end of the "row".
                        directoryItems.Add(directoryItem);
                        break;
                    default:
                        break;
                }
            }
            return directoryItems;
        }
    }
}