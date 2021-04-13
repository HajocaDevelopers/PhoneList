using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.DirectoryServices;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using InteractiveDirectory.Models;
using InteractiveDirectory.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using HajClassLib;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Serialization;


namespace InteractiveDirectory.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BuildRegionName()
        {
            //Region names are stored as an XML for the application.  You can build/edit it manually
            // or use this tool to ensure the correct structure.
            Dictionary<int, string> RegionNames = new Dictionary<int, string>();
            RegionNames.Add(1,"NORTHEAST REGION");
            RegionNames.Add(2,"MOUNTAIN REGION");
            RegionNames.Add(3,"EAST CENTRAL REGION");
            RegionNames.Add(4,"GREAT LAKES REGION");
            RegionNames.Add(6,"MID-SOUTH REGION");
            RegionNames.Add(7,"NORTH CENTRAL REGION");
            RegionNames.Add(8,"SOUTHWEST REGION");
            RegionNames.Add(11,"NORTHWEST REGION");
            RegionNames.Add(12,"SOUTH FLORIDA REGION");
            RegionNames.Add(13,"CHESAPEAKE REGION");
            RegionNames.Add(140, "NEW VENTURES PRIVATE LABEL REGION");
            RegionNames.Add(200, "NORTH FLORIDA REGION");
            RegionNames.Add(300,"CENTRAL REGION");
            RegionNames.Add(301,"TEXAS REGION RECRUITING");
            RegionNames.Add(302, "FLORIDA HS REGION");
            RegionNames.Add(303, "US WATERWORKS REGION");
            RegionNames.Add(304, "ALL-TEX REGION");
            RegionNames.Add(305,"CAROLINAS REGION");
            RegionNames.Add(306,"SOUTHEAST REGION");
            RegionNames.Add(307, "MOUNTAIN WEST REGION");
            RegionNames.Add(308,"SOUTHERN CAL REG");
            RegionNames.Add(310, "SANDALE REGION");
            RegionNames.Add(315, "HJC REGION");
            RegionNames.Add(320, "SOUTHERN REGION");
            RegionNames.Add(400, "BLODGETT REGION");
            RegionNames.Add(421, "KSS _ NORTHEAST REGION");
            RegionNames.Add(449, "NEW VENTURES PEABODY REGION");
            RegionNames.Add(450, "NEW ENGLAND REGION");
            RegionNames.Add(470, "KEYSTONE REGION");
            RegionNames.Add(755, "KSS SOUTH CENTRAL REGION");
            RegionNames.Add(978, "NEW VENTURES HOSPITALITY REGION");
            File.Delete("D:\\source\\repos\\PhoneListFromGH\\PhoneList\\InteractiveDirectory\\RegionNames.xml");
            Stream stream = File.Open("D:\\source\\repos\\PhoneListFromGH\\PhoneList\\InteractiveDirectory\\RegionNames.xml", FileMode.Create, FileAccess.ReadWrite);
            DataContractSerializer xs = new DataContractSerializer(RegionNames.GetType());
            xs.WriteObject(stream, RegionNames);
            stream.Close();

            // Test the load
            Dictionary<int, string> RegionNamesIN = new Dictionary<int, string>();
            Stream streamIN = File.Open("D:\\source\\repos\\PhoneListFromGH\\PhoneList\\InteractiveDirectory\\RegionNames.xml", FileMode.Open, FileAccess.Read);
            DataContractSerializer xsIN = new DataContractSerializer(RegionNamesIN.GetType());
            RegionNamesIN = (Dictionary<int, string>)(xsIN.ReadObject(streamIN));
            streamIN.Close();

            Assert.IsTrue(RegionNamesIN.Count  == RegionNames.Count);
        }

        [TestMethod]
        public void PlayMethod()
        {
            // Do what you want with this test.
           
            //HajClassLib.DeveloperTools.InitMockHttpContext();
            //Console.Write(HajClassLib.DeveloperTools.SerializeObjectAsXml(InteractiveDirectory.Services.DirectoryItemServices.GetDirectory(1,999,null,"")));
            string s1 = "ani-mal";
            string s2 = "animal";

            // Find the index of the soft hyphen.
            Console.WriteLine(s1.IndexOf("-"));
            Console.WriteLine(s2.IndexOf("\u00AD"));

            // Find the index of the soft hyphen followed by "n".
            Console.WriteLine(s1.IndexOf("\u00ADn"));
            Console.WriteLine(s2.IndexOf("\u00ADn"));

            // Find the index of the soft hyphen followed by "m".
            Console.WriteLine(s1.IndexOf("\u00ADm"));
            Console.WriteLine(s2.IndexOf("\u00ADm"));
        }


        [TestMethod]
        public void TestAS400Data()
        {
            HajProfitCenter hajProfitCenter = new HajClassLib.HajProfitCenter();
            DataTable dt = hajProfitCenter.PcTable(
                "pcren,pc,pcname,pcmgr,memail,pcstre,pcstct,pcstst,b8,pc10,pcmgr2,memai2,pcadd2,pccity,pcst,crdmgr,cremai,mgrid",
                HajProfitCenter.TableOrder.OrderByPc, HajProfitCenter.PcType.TypeDirectory, false, true, true
             );
           
            foreach (DataRow row in dt.Rows)
            {         
                Console.WriteLine(row["pc"]);
            }

        }
        
           
        [TestMethod]
        public void getPcInfoWithCoords(){
            OleDbDataAdapter _da = new OleDbDataAdapter();
            DataSet _ds = new DataSet();
            DataTable _dt = new DataTable();
            CommonFunctions oCommonFunctions = new CommonFunctions();
            //private OleDbConnection _cn = new OleDbConnection();
            FileSet _fs = new FileSet();
            using (OleDbConnection _cn = new OleDbConnection(new CommonFunctions().ConnString))

            {
                //_da.SelectCommand = new OleDbCommand("Select * from weblib.pccoords for read only", _cn);
                //_da.SelectCommand = new OleDbCommand("Select a.pcren,a.pc,a.pcname,a.pcmgr,a.memail,a.pcstre,a.pcstct,a.pcstst,a.b8,b.pc10,a.pcmgr2,a.memai2,a.pcadd2,a.pccity,a.pcst,a.crdmgr,a.cremai,a.mgrid,c.LATITUDE,c.LONGITUDE from HAJ.PRFCTRN a LEFT JOIN HAJ.PCSYSTEM b on b.PCACTU = a.PC INNER JOIN weblib.pccoords c on c.PC=a.PC and pcclos=0 and substring(digits(ifnull(nullif(pcstrt,0),202001)),5,2) ||'/'||'01'||'/'||substring(digits(ifnull(nullif(pcstrt,0),202001)),1,4)<=curdate() and (PCTYPE=91 OR PCTYPE=93) and pcdelt='' order by pc for read only", _cn);
                //_da.SelectCommand = new OleDbCommand("Select * from HAJ.PCSYSTEM", _cn);
                _da.SelectCommand = new OleDbCommand("Select pcren,a.pc,pcname,pcdiv,pcmgr,memail,pcstre,pcstct,pcstst,b8,pc10,pcmgr2,memai2,pcadd2,pccity,pcst,crdmgr,cremai,mgrid,pccod0,LATITUDE,LONGITUDE FROM HAJ.PRFCTRN a JOIN HAJ.PCSYSTEM b on b.PCACTU = a.PC INNER JOIN weblib.pccoords c on c.PC=a.PC WHERE a.pcclos=0 and substring(digits(ifnull(nullif(pcstrt,0),202001)),5,2) ||'/'||'01'||'/'||substring(digits(ifnull(nullif(pcstrt,0),202001)),1,4)<=curdate() and (PCTYPE=91 OR PCTYPE=93) and pcdelt='' ORDER BY pc for read only", _cn);
                try
                {
                    
                        _da.Fill(_ds);
                        _dt = _ds.Tables[0];

                        foreach (DataColumn col in _dt.Columns)
                        {
                            Console.Write(col + "\t");
                            
                        }
                        Console.WriteLine(_dt.Rows.Count.ToString());
                        //for(int i = 0; i < 2; i++)
                        foreach( DataRow row in _dt.Rows)
                        {
                          //  foreach (DataColumn col in _dt.Columns)
                           // {
                            Console.WriteLine(row["pc"] + " " + row["pccod0"] + " " + row["pcdiv"]);  
                            //  Console.Write(_dt.Rows[i][col.ColumnName.ToString()] + "\t");
                            //}
                        }


                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoadCoords: " + ex.Message.ToString());
                }
            }
        }

        [TestMethod]
        public void BuildXML()
        {
            HajClassLib.DeveloperTools.InitMockHttpContext();

            Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Started...");
            DevelopmentConfiguration.DeveloperUserImperosnate();
            if (DirectoryItemServices.BuildCurrentDirectory())
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Successful...");
            else
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Failed...");

        }

        [TestMethod]
        public void TestSearchADInformation()
        {
            // Set this to part of a name of somebody that has a valid ad account like the current 
            // developer's last name.
            string sPartOfANameOfSomebodyThatBetterExist = "fizz";

            // Get ADInformation from "People"
            SearchResultCollection res = HajClassLib.ADInfo.SearchADInformation(
                new TupleList<string, string> { { "ou", "people" } },
                new List<string>() { "cn=*" + sPartOfANameOfSomebodyThatBetterExist + "*" },
                new List<string>() { "SAMAccountName", "givenName", "sn", "displayName", "distinguishedName" }
                );

            //Make sure it worked.
            Assert.IsTrue(res.Count > 0);

            // Dump it out.
            foreach (SearchResult sr in res)
            {
                foreach (string propName in sr.Properties.PropertyNames)
                {
                    ResultPropertyValueCollection valueCollection = sr.Properties[propName];
                    foreach (Object propertyValue in valueCollection)
                    {
                        Console.WriteLine(propName + ": " + propertyValue.ToString());
                    }
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void TestSearchADInformationByGroup()
        {
            // Set this to part of a name of a distribution group you know exists.
            string sNameOfGroupThatBetterExist = "Service Center Employees";

            // Get ADInformation from group.
            SearchResultCollection res = HajClassLib.ADInfo.SearchADInformation(
                new TupleList<string, string> { { "ou", "people" } },
                new List<string>() { "objectCategory=user", "memberOf=CN=" + sNameOfGroupThatBetterExist + ",OU=Distribution Groups,OU=Groups,DC=Hajoca,DC=com" },
                new List<string>(){"SAMAccountName","givenName","sn","displayName","distinguishedName"}
                );

            //Make sure it worked.
            Assert.IsTrue(res.Count > 0);

            // Dump it out.
            foreach (SearchResult sr in res)  
            {
                foreach (string propName in sr.Properties.PropertyNames)
                {
                    ResultPropertyValueCollection valueCollection = sr.Properties[propName];
                    foreach (Object propertyValue in valueCollection)
                    {
                        Console.WriteLine(propName + ": " + propertyValue.ToString());
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
