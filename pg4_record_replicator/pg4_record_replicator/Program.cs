using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace pg4_record_replicator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            //SimpleReplicationExampleCrushLogic();
            ComplexReplicationExampleCrushLogic();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<ReplicationRequestEndpoint>();
            });

        static void SimpleReplicationExampleCrushLogic()
        {
            // setup open telemetry and activity top level objects
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("pg4-lan-replication", "0.0.1");
            Activity ETR = ot_helper.StartActivityWrapper("p1s1e1");

            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            //do runtime setup for data validation class
            SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);
            List<Activity> activity_list = new List<Activity>();

            GdcReplicationRequest sent_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "simple test client data");
            activity_list.Add(child_activity_pointer.P_pointer);

            sent_rep.IV_client_id.P_value = "CRUSH_1102";

            sent_rep.IV_software_version.P_value = 4.71;
            sent_rep.IV_primary_key_value.P_value = "12";
            sent_rep.IV_primary_table.P_value = "Parts";
            sent_rep.IV_replication_type.P_value = 1;
            sent_rep.IV_sql_join.SetValueNotUsed(ot_helper, child_activity_pointer, true);
            activity_list.Add(child_activity_pointer.P_pointer);

            sent_rep.ValidateData(ot_helper, child_activity_pointer);

            string json_text = sent_rep.ToJson(ot_helper, child_activity_pointer, "simple test");
            activity_list.Add(child_activity_pointer.P_pointer);

            //// client sending data to server


            GdcReplicationRequest received_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "simple test data on server");
            activity_list.Add(child_activity_pointer.P_pointer);

            received_rep.PopulateFromJson(ot_helper, child_activity_pointer, json_text, "simple test");
            activity_list.Add(child_activity_pointer.P_pointer);

            if (received_rep.IV_replication_type.P_value == 1)
            {
                received_rep.IV_sql_join.SetValueNotUsed(ot_helper, child_activity_pointer, true);
                activity_list.Add(child_activity_pointer.P_pointer);
            }


            ReplicationRequestProcessor.processRequest(ot_helper, child_activity_pointer, received_rep, "simple test");
            activity_list.Add(child_activity_pointer.P_pointer);


        }

        static void ComplexReplicationExampleCrushLogic()
        {
            // setup open telemetry and activity top level objects
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("pg4-lan-replication", "0.0.1");
            Activity ETR = ot_helper.StartActivityWrapper("p1s1e1");

            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            //do runtime setup for data validation class
            SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);
            List<Activity> activity_list = new List<Activity>();

            GdcReplicationRequest sent_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "complex test client data");
            activity_list.Add(child_activity_pointer.P_pointer);

            sent_rep.IV_client_id.P_value = "CRUSH_1102";
            sent_rep.IV_software_version.P_value = 4.71;
            sent_rep.IV_primary_key_value.P_value = "15";
            sent_rep.IV_primary_table.P_value = ReplicationTableHelper.PosSalesHdr.P_primary_table_name;
            sent_rep.IV_replication_type.P_value = 2;
            sent_rep.IV_sql_join.P_value = ReplicationTableHelper.PosSalesHdr.P_join_statement;
            foreach (string table_name in ReplicationTableHelper.PosSalesHdr.P_secondary_tables)
            {
                sent_rep.IV_secondary_tables.Add(new Dco<string>(table_name,table_name, null,ProjectValidationRuleLists.R_populated_string));
            }
            sent_rep.ValidateData(ot_helper, child_activity_pointer,"complex test");
            activity_list.Add(child_activity_pointer.P_pointer);

            string json_text = sent_rep.ToJson(ot_helper, child_activity_pointer, "complex test");
            activity_list.Add(child_activity_pointer.P_pointer);

            //// client sending data to server
            Console.WriteLine(json_text);
            GdcReplicationRequest received_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "complex test data on server");
            activity_list.Add(child_activity_pointer.P_pointer);

            received_rep.PopulateFromJson(ot_helper, child_activity_pointer, json_text, "complex test");
            activity_list.Add(child_activity_pointer.P_pointer);

            if (received_rep.IV_replication_type.P_value == 1)
            {
                received_rep.IV_sql_join.SetValueNotUsed(ot_helper, child_activity_pointer, true);
                activity_list.Add(child_activity_pointer.P_pointer);
            }
            
            received_rep.ValidateData(ot_helper, child_activity_pointer, "complex replication test");
            activity_list.Add(child_activity_pointer.P_pointer);


            ReplicationRequestProcessor.processRequest(ot_helper, child_activity_pointer, received_rep, "simple test");
            activity_list.Add(child_activity_pointer.P_pointer);


        }

    }
}
