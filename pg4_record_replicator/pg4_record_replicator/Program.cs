using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pg4_record_replicator
{
    internal class Program
    {
        static void Main(string[] args)
        {

            JsonDecodingTesting test22 = new JsonDecodingTesting();
            //test22.testing();
            //SimpleReplicationExampleCrushLogic();
            ComplexReplicationExampleCrushLogic();
        }
    
        static void SimpleReplicationExampleCrushLogic()
        {
            string table_name = "Parts";
            string primary_key_value = "12";
            double crush_version = 1;
            string client_id = "1102";
            HttpsRequestToSychonizerProgram(table_name, primary_key_value, crush_version, client_id);
        }

        static void ComplexReplicationExampleCrushLogic()
        {
            string table_name = ReplicationTableHelper.PosSalesHdr.P_primary_table_name;
            string primary_key_value = "29";
            double crush_version = 1;
            string client_id = "1102";
            string join_statment = ReplicationTableHelper.PosSalesHdr.P_join_statement;
            List<string> secondary_tables = ReplicationTableHelper.PosSalesHdr.P_secondary_tables;

            HttpsRequestToSychonizerProgram(table_name, primary_key_value, crush_version, client_id,
                join_statment,secondary_tables);
        }

        static void HttpsRequestToSychonizerProgram(string arg_table_name,
        string arg_primary_uid,
        double arg_software_version,
        string arg_client_id,
        string arg_join_statement =null,
        List<string> arg_secondary_tables=null
        )
        {
            // setup open telemetry and activity top level objects
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

            // create objects used for monitoring activities
            List<Activity> method_activity_list = new List<Activity>();
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            //do runtime setup for data validation class
            SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);

            Activity ETR = ot_helper.StartActivityWrapper("SychonizerProgram");


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            string dest_db = "CRUSH_1507";
            builder.DataSource = "DESKTOP-3V8AOIE";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "CRUSH_" + arg_client_id;
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            DynamicRecordsReplicator replicator;
            if (arg_secondary_tables != null 
                && arg_join_statement != null 
                && arg_join_statement.Length > 0
                && arg_secondary_tables.Count > 0)
            {
                replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer, arg_table_name, arg_primary_uid,
                arg_secondary_tables, arg_client_id, arg_software_version, arg_join_statement, "Remote");
            }
            else
            {
                replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer, arg_table_name, arg_primary_uid,
                 arg_client_id, arg_software_version, "Remote");
            }
            method_activity_list.Add(child_activity_pointer.P_pointer);

            ReplicationBatch replication_batch;
            replication_batch = replicator.GetReplicationSqlStatements(ot_helper, child_activity_pointer, "CRUSH_1102", connection);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            bool test = replication_batch.ValidateData(ot_helper, child_activity_pointer);

            ChildActivityPointer first_failed_activity;
            if (OTSH.CheckActivityList(ETR, method_activity_list, out first_failed_activity) == false)
            {
                Console.WriteLine(first_failed_activity.P_pointer.DisplayName);
                OTSH.PassStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
                return;
            }

            foreach (ReplicationRecord request in replication_batch.P_replication_records)
            {
                //Console.WriteLine(request.P_select_statement);
                //Console.WriteLine(request.P_insert_statement);
                //Console.WriteLine(request.P_update_statement);
                //Console.WriteLine(request.P_combined_sql_statement);

            } 

            int replication_count = replicator.replicateSqlStatements(ot_helper, child_activity_pointer, dest_db, connection, replication_batch, "end of day batching");
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return;
            }

            Console.WriteLine($"Replicated {replication_count} records.");

            OTSH.SetSuccessEndActivity(ETR);
            return;
        }

    }
}
