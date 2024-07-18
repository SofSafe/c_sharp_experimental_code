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
            //do runtime setup for data validation class
            SDC.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);

            Activity ETR = ot_helper.startActivityWrapper("Local replication");

            ////arguments that will be passed from the crush POS client via https request
            string arg_table_name = "Parts";
            string arg_primary_uid = "5";
            string arg_crush_version = "1232";
            string arg_client_id = "1235";
           
            //we can store the join statements and secondary tables in the Crush POS as a definition that can updated with crush
            string arg_join_statement = "PartPrices.PartRno=Parts.PartRno";
            List<string> arg_secondary_tables = new List<string>() { "PartPrices" };
            
            //uncomment for only replicating a single record
            arg_join_statement = "";
            arg_secondary_tables.Clear();
            ////</end> arguments that will be passed from the crush POS client via https request




            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "DESKTOP-3V8AOIE";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "CRUSH_1102";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            DynamicRecordsReplicator replicator;
            if (arg_secondary_tables.Count > 0)
            {
                replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer, arg_table_name, arg_primary_uid,
                arg_secondary_tables, arg_client_id, arg_crush_version, arg_join_statement, "Remote");
            } else
            {
                replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer, arg_table_name, arg_primary_uid,
                 arg_client_id, arg_crush_version, "Remote");
            }
            method_activity_list.Add(child_activity_pointer.P_pointer);

            List<ReplicationRequest> replication_statements = new List<ReplicationRequest>();
            replication_statements = replicator.getReplicationSqlStatements(ot_helper, child_activity_pointer, "CRUSH_1102", connection, "CRUSH_1102");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            ChildActivityPointer first_failed_activity;
            if (OTSH.checkActivityList(ETR, method_activity_list, out first_failed_activity) == false)
            {
                Console.WriteLine(first_failed_activity.P_pointer.DisplayName);
                OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
                return;
            }
            
            foreach (ReplicationRequest request in replication_statements)
            {
                Console.WriteLine(request.P_select_statement);
                Console.WriteLine(request.P_insert_statement);
                Console.WriteLine(request.P_update_statement);
            }

            replicator.replicateSqlStatements(ot_helper, child_activity_pointer, "CRUSH_1102", connection, replication_statements);
            if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.passStatusAndEndActivity(ETR,child_activity_pointer.P_pointer);
                return;
            }

            

            OTSH.setSuccessEndActivity(ETR);
            return;
        }        
    }
}
