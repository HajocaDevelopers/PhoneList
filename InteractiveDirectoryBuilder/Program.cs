using System;
using HajClassLib;
using InteractiveDirectory.Services;

namespace InteractiveDirectoryBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Started...");
            DevelopmentConfiguration.DeveloperUserImperosnate();
            if (DirectoryItemServices.BuildCurrentDirectory())
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Successful...");
            else
                Console.WriteLine("[" + DateTime.Now.ToString() + "] Interactive Directory Builder Failed...");
        }
    }
}
