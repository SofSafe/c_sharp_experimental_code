using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;



class DyanmicMsSqlTable
{
    //todo add a flag to indicate the instnace is in an un-usable state due to validation errors
    protected string IV_client_id_number_as_string = String.Empty;
    protected string IV_replicated_data_table_name = String.Empty;
    protected DataTable IV_table = new DataTable();
    protected Dictionary<string, DynamicMsSqlColumn> IV_columns_meta_data = new Dictionary<string, DynamicMsSqlColumn>();
    protected string IV_table_name = String.Empty;

    public Dictionary<string, DynamicMsSqlColumn> P_columns_meta_data 
    {
        get { return this.IV_columns_meta_data; }
    }

    public string P_table_name
    {
        get { return this.IV_table_name; }
    }

    public DyanmicMsSqlTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string replicate_from_table_name, string client_id , string replicate_to_table_name, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor1");
        validator.AddEntry(new DataVR(replicate_from_table_name).AddRule(SDV.R_len_greater_2));
        validator.AddEntry(new DataVR(client_id).AddRule(SDV.R_client_id));
        validator.AddEntry(new DataVR(replicate_to_table_name).AddRule(SDV.R_len_greater_2));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        ///</end> input data validation

        this.IV_client_id_number_as_string = client_id.Replace("CRUSH_","");
        this.IV_table_name = replicate_from_table_name;
        this.IV_table.TableName = replicate_from_table_name;
        this.IV_replicated_data_table_name = replicate_to_table_name;
        OTSH.SetSuccessEndActivity(ETR);
        return;



    }


    public bool GetSchemaData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SqlConnection connection_instance , string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".getSchemaData");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".getSchemaData");
        validator.AddEntry(new DataVR(connection_instance).AddRule(SDV.R_not_null));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input data validation


        SqlDataAdapter adapter = new SqlDataAdapter();
        SqlCommand schema_command = connection_instance.CreateCommand();
        schema_command.CommandText = $"sp_columns '{this.IV_table.TableName}';";
        adapter.SelectCommand = schema_command;
        DataSet data_set = new DataSet();
        adapter.TableMappings.Add("Table", this.IV_table.TableName);
        try
        {
            adapter.Fill(data_set);
        } catch (Exception error_instance)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > retrieving schema record from server > {error_instance.Message}", ErrorTagsCodes.C_db_read_failed);
            return false;
        }
        List<Activity> method_activities_list = new List<Activity>();
        int primary_keys_found = 0;
        foreach (DataTable table in data_set.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                DynamicMsSqlColumn next_column = new DynamicMsSqlColumn();
                next_column.PopulateSchemaInformation(ot_helper, child_activity_pointer, table, row, this.IV_table.TableName);
                method_activities_list.Add(child_activity_pointer.P_pointer);
                if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok) { continue; }

                if(next_column.P_is_primary_key == true) { primary_keys_found++; }
                this.IV_columns_meta_data.Add(next_column.P_column_name, next_column);
                this.IV_table.Columns.Add(next_column.P_column_name, next_column.P_code_type);
            }
        }

        ChildActivityPointer first_failed_activity = null;
        if (OTSH.CheckActivityList(ETR,method_activities_list, out first_failed_activity) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return false;
        }

        if(this.IV_columns_meta_data.Count == 0)
        {
            ETR.AddTag("IV_table.TableName", this.IV_table.TableName);
            OTSH.AddEventThenEndActivity(ETR, "no data in IV_columns_meta_data", ErrorTagsCodes.C_input_validation);
            return false;
        }

        if(primary_keys_found > 1)
        {
            ETR.AddTag("IV_table.TableName", this.IV_table.TableName);
            OTSH.AddEventThenEndActivity(ETR, "more than 1 primary key found", ErrorTagsCodes.C_input_validation);
            return false;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


    public bool PutDataInColumns(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, DataSet data_set, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".PutDataInColumns");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".PutDataInColumns");
        validator.AddEntry(new DataVR(data_set).AddRule(SDV.R_not_null));
        if (data_set != null)
        {
            validator.AddEntry(new DataVR(data_set.Tables.Count).AddRule(SDV.R_greater_0));
        }
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input data validation        

        try
        {
            this.IV_table.Merge(data_set.Tables[0], true, MissingSchemaAction.Ignore);
        } catch(Exception error_object)
        {
            OTSH.AddEventThenEndActivity(ETR, $"merge failed: {error_object.Message}", ErrorTagsCodes.C_parsing_db_records_failed);
            return false;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


    //TODO change return type on SBD
    public List<GdcReplicationRecord> GenerateSqlStatementsFromData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {

        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".generateSqlStatementsFromData");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ///</end> observability template

        List<GdcReplicationRecord> return_value  = new List<GdcReplicationRecord>();

        //// build the parts of the select, insert and update statements that will use data from the replicated data
        foreach (DataRow row in this.IV_table.Rows)
        {
            string insert_columns = "";
            string insert_values = "";
            string update_part2 = "";
            string where_primary_key_statement = "";
            string select_by_all_fields = "";

            int loop1 = -1;
            foreach (DataColumn column in this.IV_table.Columns)
            {
                string equals_statement = "";

                string next_insert_value = "";
                if(row[column] == null) { continue; }
                if (row[column].GetType() == typeof(DBNull)) { continue; }

                if (this.IV_columns_meta_data[column.ColumnName].P_code_type == typeof(string))
                {
                    //don't include empty strings in sql statements
                    if(row[column].ToString().Length < 1)
                    {
                        continue;
                    }
                    next_insert_value = $"'{row[column]}'";
                    equals_statement = $"{column.ColumnName}='{row[column]}'";
                }
                else if (this.IV_columns_meta_data[column.ColumnName].P_code_type == typeof(DateTime))
                {
                    DateTime column_value = (DateTime)row[column];
                    string sql_date = column_value.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss:fff");
                    next_insert_value = $"'{sql_date}'";
                    equals_statement = $"{column.ColumnName}='{sql_date}'";
                }
                else if (this.IV_columns_meta_data[column.ColumnName].P_code_type == typeof(bool))
                {
                    if ((bool)row[column] == true)
                    {
                        next_insert_value = $"1";
                        equals_statement = $"{column.ColumnName}=1";
                    } 
                    else
                    {
                        next_insert_value = $"0";
                        equals_statement = $"{column.ColumnName}=0";
                    }
                }
                else
                {
                    next_insert_value = $"{row[column].ToString()}";
                    equals_statement = $"{column.ColumnName}={row[column].ToString()}";
                }

                if(next_insert_value.Length == 0) { continue; }

                loop1++;
                if (loop1 > 0)
                {
                    update_part2 += " , ";
                    insert_columns += " , ";
                    insert_values += " , ";
                    select_by_all_fields += " AND ";
                }

                insert_values += next_insert_value;
                insert_columns += column.ColumnName;
                update_part2 += equals_statement;
                select_by_all_fields += equals_statement;                

                

                if (this.IV_columns_meta_data[column.ColumnName].P_is_primary_key == true)
                {                   
                    where_primary_key_statement = $" clientID={this.IV_client_id_number_as_string} AND {equals_statement}";
                }
            }

            string select_statement = "";
            string where_statement = "";
            if (where_primary_key_statement.Length == 0) {
                where_statement = $" clientID={this.IV_client_id_number_as_string} AND {select_by_all_fields};";
            } else
            {
                where_statement = where_primary_key_statement ;
            }

            select_statement = $"SELECT ReplicationStartTime FROM {this.IV_replicated_data_table_name} WHERE {where_statement};";

            DateTime replication_start_time = DateTime.Now;
            string replication_start_time_string = replication_start_time.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss:fff");
            string insert_statement = $"INSERT into {this.IV_replicated_data_table_name} ({insert_columns}, ReplicationStartTime , ClientId , ReplicationFinishTime ) VALUES ({insert_values} ,'{replication_start_time_string}' , {this.IV_client_id_number_as_string}, GETUTCDATE() );";
            
            string update_statement = $"UPDATE {this.IV_replicated_data_table_name} SET {update_part2},ReplicationStartTime='{replication_start_time_string}', ClientId={this.IV_client_id_number_as_string} , ReplicationFinishTime=GETUTCDATE() WHERE {where_statement};";

            // SET the replication_start_time here
            var param_start_time = replication_start_time_string;
            string combined_statement = $@"DECLARE @pram_replication_start_time DATETIME = '{param_start_time}';

                IF NOT EXISTS (SELECT 1 FROM {this.IV_replicated_data_table_name} WHERE {where_statement})
                    BEGIN
                        -- INSERT statement
                        INSERT INTO {this.IV_replicated_data_table_name} 
                        (   {insert_columns}, 
                            ReplicationStartTime, 
                            ClientId, 
                            ReplicationFinishTime )
                        VALUES (
                        {insert_values},
                        '{replication_start_time_string}' , 
                        {this.IV_client_id_number_as_string}, 
                         GETUTCDATE() );
                    END

                ELSE

                    -- UPDATE statement
                    IF EXISTS (
                        SELECT 1
                        FROM {this.IV_replicated_data_table_name}
                        WHERE {where_statement}
                          AND ReplicationStartTime < @pram_replication_start_time
                    )
                    BEGIN
                        UPDATE {this.IV_replicated_data_table_name} 
                            SET {update_part2},ReplicationStartTime='{replication_start_time_string}', ClientId={this.IV_client_id_number_as_string} , ReplicationFinishTime=GETUTCDATE() 
                            WHERE {where_statement};
                    END
                ;";


            return_value.Add(new GdcReplicationRecord(ot_helper,child_activity_pointer,select_statement,insert_statement,update_statement, combined_statement,null, activity_name_prefix));
        }
        ////</end> build the parts of the select, insert and update statements that will use data from the replicated data


        OTSH.SetSuccessEndActivity(ETR);
        return return_value;
    }
}
