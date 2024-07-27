using Microsoft.Data.SqlClient;
using SQLReplicationConfigurator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SofsafeBaseIncludes;


class ReplicationRequestProcessor
{
    //todo add db_settings arugment to SBD
    public static bool processRequest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, GdcReplicationRequest rep_data, NamedListOfPg4DbSettingsEntry db_settings, string activity_name_prefix = "")  
    {
         /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix  + "ReplicationRequestProcessor.processRequest");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template
        List<Activity> activity_list = new List<Activity>();


        ////<start> input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + "processRequest.PopulateFromJson");
        validator.AddEntry(new DataVR(rep_data, "rep_data").AddRule(RulesCommon.R_not_null));
        validator.AddEntry(new DataVR(db_settings, "rep_data").AddRule(RulesCommon.R_enumerable_no_nulls));
        
        validator.ValidateData(ot_helper, child_activity_pointer);
        activity_list.Add(child_activity_pointer.P_pointer);

        rep_data.ValidateData(ot_helper, child_activity_pointer, activity_name_prefix);
        activity_list.Add(child_activity_pointer.P_pointer);

        db_settings.ValidateAllListInstances(ot_helper, child_activity_pointer, activity_name_prefix);
        activity_list.Add(child_activity_pointer.P_pointer);

        ChildActivityPointer validation_failure;
      if (OTSH.CheckActivityList(ETR, activity_list, out validation_failure) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, validation_failure.P_pointer);
            return false;
        }
        ///</end> input data validation

        ////<start> get the data we need to replicate from the source server 

        ///////<start> connect to the source server
        bool source_server_found = false;
        GdcPg4DbSettingsEntry source_server = null;
        foreach (GdcPg4DbSettingsEntry server_entry in db_settings)
        {
            if(server_entry.IV_database_name.content == rep_data.IV_source_id.content)
            {
                source_server = server_entry;
                source_server_found = true;
                break;
            }
        }

        if(source_server_found == false)
        {
            OTSH.AddEventThenEndActivity(ETR, $"source server for database: {rep_data.IV_source_id.content} not found", ErrorTagsCodes.C_input_validation);
            return false;
        }

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.DataSource = source_server.IV_server_address.content;
        builder.UserID = source_server.IV_username.content;
        builder.Password = source_server.IV_password.content;
        builder.InitialCatalog = source_server.IV_database_name.content;
        builder.TrustServerCertificate = true;        

        SqlConnection source_connection = null;
        try
        {
            source_connection = new SqlConnection(builder.ConnectionString);
            source_connection.Open();
        } catch (Exception error_object)
        {
            OTSH.AddEventThenEndActivity(ETR, $"failed to connect to {source_server.IV_server_address.content}:{source_server.IV_database_name.content} with error {error_object.Message}",ErrorTagsCodes.C_server_db_connection);
            return false;
        }
        ///////</end> connect to the source server

        ///////<start> read all the records to replicate and generate replication records
        DynamicRecordsReplicator replicator;

        if (rep_data.IV_secondary_tables.Count > 0
            && rep_data.IV_sql_join.P_is_set_and_valid == true
            )
        {
            List<string> secondary_tables = new List<string>();
            foreach(Dco<string> table_name in rep_data.IV_secondary_tables)
            {
                secondary_tables.Add(table_name.content);                    
            }

            replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer,
                rep_data.IV_primary_table.content,
                rep_data.IV_primary_key_value.content,
                secondary_tables,
                rep_data.IV_source_id.content,
                rep_data.IV_software_version.content,
                rep_data.IV_sql_join.content,
                "Remote");
        }
        else
        {
            replicator = new DynamicRecordsReplicator(ot_helper, child_activity_pointer,
                rep_data.IV_primary_table.content,
                rep_data.IV_primary_key_value.content,
                rep_data.IV_source_id.content,
                rep_data.IV_software_version.content,
                "Remote");
                
        }
        activity_list.Add(child_activity_pointer.P_pointer);

        GdcReplicationBatch replication_batch;
        replication_batch = replicator.GetReplicationSqlStatements(ot_helper, child_activity_pointer, source_server.IV_database_name.content, source_connection);
        activity_list.Add(child_activity_pointer.P_pointer);
        
        //Console.WriteLine(replication_batch.ToJson(ot_helper,child_activity_pointer));
        //activity_list.Add(child_activity_pointer.P_pointer);

        /////// read all the records to replicate and generate replication records


        ChildActivityPointer first_failed_activity;
        if (OTSH.CheckActivityList(ETR, activity_list, out first_failed_activity) == false)
        {
            Console.WriteLine(first_failed_activity.P_pointer.DisplayName);
            OTSH.PassStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return false;
        }
        ////</end> get the data we need to replicate from the source server 


        //// replicate data to every other destination SQL server
        bool fail_flag = false;
        foreach (GdcPg4DbSettingsEntry dest_server in db_settings)
        {

            if (dest_server.IV_database_name.content == source_server.IV_database_name.content)
            {
                continue;
            }

            SqlConnectionStringBuilder dest_builder = new SqlConnectionStringBuilder();
            dest_builder.DataSource = dest_server.IV_server_address.content;
            dest_builder.UserID = dest_server.IV_username.content;
            dest_builder.Password = dest_server.IV_password.content;
            dest_builder.InitialCatalog = dest_server.IV_database_name.content;
            dest_builder.TrustServerCertificate = true;

            SqlConnection dest_connection = null;
            try
            {
                dest_connection = new SqlConnection(dest_builder.ConnectionString);
                dest_connection.Open();
            }
            catch (Exception error_object)
            {
                OTSH.AddEventThenEndActivity(ETR, $"failed to connect to {dest_server.IV_server_address.content}:{dest_server.IV_database_name.content} with error {error_object.Message}", ErrorTagsCodes.C_server_db_connection);
                return false;
            }
            /////</end> connect to the source server

            int replication_count = replicator.replicateSqlStatements(ot_helper, child_activity_pointer, dest_server.IV_database_name.content, dest_connection, replication_batch, dest_server.IV_database_name.content);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                fail_flag = true;
                continue;
            }
            Console.WriteLine($"Replicated {replication_count} records.");
        }
        ////</end> replicate data to every other destination SQL server

        

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }

}

