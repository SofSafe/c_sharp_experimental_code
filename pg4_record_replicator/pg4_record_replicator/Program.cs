using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace pg4_record_replicator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // setup open telemetry and activity top level objects
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

            // create objects used for monitoring activities
            List<Activity> method_activity_list = new List<Activity>();
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            string activity_description = "";

            //do runtime setup for data validation class
            SDC.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);

            Activity ETR = ot_helper.startActivityWrapper("Endpoint Name");

            string arg_table_name = "Parts";
            string arg_primary_uid = "5";
            string arg_crush_version = "123";
            string arg_client_id = "1235";



            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "DESKTOP-3V8AOIE";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "CRUSH_1102";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();


            DynamicRecordsReplicator test = new DynamicRecordsReplicator(ot_helper,child_activity_pointer,arg_table_name,arg_primary_uid,arg_client_id,arg_crush_version,"Remote");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            test.ConnectAndReplicateLocally(ot_helper, child_activity_pointer, "CRUSH_1102", connection, "CRUSH_1102");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            ChildActivityPointer first_failed_activity;
            if (OTSH.checkActivityList(ETR, method_activity_list, out first_failed_activity) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
                return;
            }

            OTSH.setSuccessEndActivity(ETR);
            return;


            DyanmicMsSqlTable replicator = new DyanmicMsSqlTable(ot_helper, child_activity_pointer, arg_table_name, arg_client_id, arg_crush_version, $"Remote{arg_table_name}");
            method_activity_list.Add(child_activity_pointer.P_pointer);


            replicator.getSchemaData(ot_helper, child_activity_pointer, connection);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            SqlDataAdapter sqda = new SqlDataAdapter();           
            SqlCommand select_command = connection.CreateCommand();
            select_command.CommandText = "SELECT * FROM Parts WHERE PartRno<5;";
            sqda.SelectCommand = select_command;
            DataSet data_set = new DataSet();
            sqda.Fill(data_set);
 


            replicator.putDataInColumns(ot_helper,child_activity_pointer,data_set);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            List<List<string>> sql_statements = new List<List<string>>();
            sql_statements = replicator.generateSqlStatementsFromData(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);


            ChildActivityPointer first_failed_activity1 = null;
            if (OTSH.checkActivityList(ETR, method_activity_list, out first_failed_activity1) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
                return;
            }
            

            foreach(List<string> record in sql_statements)
            {
                foreach (string statement in record)
                {
                    Console.WriteLine(statement);
                }

            }

            OTSH.setSuccessEndActivity(ETR);
            Console.WriteLine("Hello, World!");
            return;
        }
    }
}
