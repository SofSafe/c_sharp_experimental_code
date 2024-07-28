using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using PG4ClientLibrary;
namespace ClientCaller
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                //Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("======================");
                Console.WriteLine("Client Caller Starterd");
                Console.WriteLine("======================");

                Pg4Client client = new Pg4Client();

                Console.WriteLine("Setting SetConnectionSettings...");
                bool connectionSet = client.SetConnectionSettings("127.0.0.1", 7000);

                if (!connectionSet)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Setting Connection was unsuccessful");
                    EndProgram();                    ;
                }

                Console.WriteLine("Calling Setting DoSimpleReplication....");
                bool simpleReplicationSucceed = await client.DoSimpleReplication("CRUSH_101", 2.04, "table1", "1");

                if (!simpleReplicationSucceed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DoSimpleReplication was unsuccessful");
                    EndProgram(); ;
                }

                Console.WriteLine("Calling DoComplexReplication....");
                List<string> listSecondaryTables = new List<string>();
                listSecondaryTables.Add("table2");

                bool complexReplicationSucceed = await client.DoComplexReplication("CRUSH_101", 2.04, "table1", "1", listSecondaryTables, "table1.id on table2.id");

                if (!complexReplicationSucceed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DoComplexReplication was unsuccessful");
                    EndProgram();
                }



            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error:{ex.Message}");
                Console.WriteLine($"Error Stack:{ex.StackTrace}");
            }
            EndProgram();

            Console.WriteLine();
           
        }

        static void EndProgram()
        {
            Console.WriteLine("==============\nProgram Ended\n==============");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Environment.Exit(0);
        }

    }
}
