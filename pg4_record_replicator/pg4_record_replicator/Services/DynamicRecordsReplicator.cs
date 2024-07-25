using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



class DynamicRecordsReplicator
{

    //todo add a flag to indicate the instnace is in an un-usable state due to validation errors
    protected string IV_join_statement = "";
    protected string IV_primary_record_uid = "";
    protected string IV_primary_table_name = "";
    protected byte IV_replication_mode = 0;
    protected string IV_source_id = "";
    protected double IV_software_version = 0;
    protected string IV_replicated_table_name_prefix = "";
    //todo add to SBD
    private readonly bool _IV_constructor_success = false;
    protected List<DyanmicMsSqlTable> IV_tables = new List<DyanmicMsSqlTable> ();


    public DynamicRecordsReplicator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string primary_table_name, string primary_key_value, string source_id, double software_version, string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".SimpleReplicationConstructor");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template
        
        if(this.SharedConstructorLogic(ot_helper,child_activity_pointer,1,primary_table_name,primary_key_value,source_id,software_version,replication_table_prefix) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        OTSH.SetSuccessEndActivity(ETR);
        this._IV_constructor_success = true;
        return;
    }


    public DynamicRecordsReplicator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string primary_table_name, string primary_key_value, List<string> secondary_table_names, string source_id, double software_version, string join_statement, string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".ComplexReplicationConstructor");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor2");
        //TODO create a rule for a list of sql table names
        //SharedConstructorLogic validates all other arguments
        validator.AddEntry(new DataVR(secondary_table_names).AddRule(SDV.R_len_greater_0));

        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        ///</end> input data validation

        this.IV_join_statement = join_statement;

        //// create all tables associated with the primary table
        List<Activity> method_activites_list = new List<Activity>();
        foreach(string table_name in secondary_table_names)
        {
            DyanmicMsSqlTable table_entry = new DyanmicMsSqlTable(ot_helper, child_activity_pointer, table_name, source_id, replication_table_prefix + table_name, activity_name_prefix);
            method_activites_list.Add(child_activity_pointer.P_pointer);
            if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok) { continue; }
            this.IV_tables.Add(table_entry);
        }
        
        ChildActivityPointer first_failed_activity;
        if(OTSH.CheckActivityList(ETR,method_activites_list,out first_failed_activity) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return;
        }
        ////</end> create all tables associated with the primary table


        if (this.SharedConstructorLogic(ot_helper, child_activity_pointer, 2, primary_table_name, primary_key_value, source_id, software_version, replication_table_prefix) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        
        OTSH.SetSuccessEndActivity(ETR);
        this._IV_constructor_success = true;
        return;
    }

    private bool SharedConstructorLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,byte replication_type,string primary_table_name, string primary_key_value, string source_id, double software_version,string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".sharedConstructorLogic");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".sharedConstructorLogic");
        validator.AddEntry(new DataVR(replication_type, "replication_type").AddRule(SDV.R_1or2));
        validator.AddEntry(new DataVR(primary_table_name, "primary_table_name").AddRule(SDV.R_len_greater_0));
        validator.AddEntry(new DataVR(primary_key_value, "primary_key_value").AddRule(SDV.R_len_greater_0));
        validator.AddEntry(new DataVR(source_id, "source_id").AddRule(SDV.R_source_id));
        validator.AddEntry(new DataVR(software_version, "software_version").AddRule(SDV.R_greater_0));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation
      
        DyanmicMsSqlTable primary_table = new DyanmicMsSqlTable(ot_helper, child_activity_pointer, primary_table_name, source_id, replication_table_prefix + primary_table_name, activity_name_prefix);
        if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }

        this.IV_software_version = software_version;
        this.IV_source_id = source_id;
        this.IV_replication_mode = replication_type;
        this.IV_primary_table_name = primary_table_name;
        this.IV_primary_record_uid = primary_key_value;
        this.IV_tables.Add(primary_table);

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


    //TODO update return type on SBD
    public GdcReplicationBatch GetReplicationSqlStatements(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string source_db_name,SqlConnection sql_connection, string activity_name_prefix="")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".getReplicationSqlStatements");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        GdcReplicationBatch return_value = new GdcReplicationBatch(ot_helper,child_activity_pointer,this.IV_source_id,this.IV_software_version,DateTime.Now,this.IV_replication_mode, activity_name_prefix);
                

        //// input data validation
        if(this._IV_constructor_success != true)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > method not running due to the fact the class constructor failed when it ran", ErrorTagsCodes.C_input_validation);
            return return_value;
        }

        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".getReplicationSqlStatements");
        validator.AddEntry(new DataVR(sql_connection).AddRule(SDV.R_not_null));
        validator.AddEntry(new DataVR(source_db_name).AddRule(SDV.R_len_greater_0));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return return_value;
        }
        ///</end> input data validation       

        //// switch to database we are replicating records from
        ETR.AddTag("source_db_name", source_db_name);
        try
        {
            sql_connection.ChangeDatabase(source_db_name);
        }
        catch (Exception error_instance)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > switching to source database > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
            return return_value;
        }       
        ////</end> switch to database we are replicating records from



        List<Activity> method_activities_list = new List<Activity>();
        ////get the schemas for all tables and build the field list for the select query
        string records_select_statement = "SELECT ";
        string records_where_statement = " WHERE ";
        short field_count = 0;
        string secondary_tables_for_joins = "";
        foreach(DyanmicMsSqlTable dynamic_table in this.IV_tables)
        {
            bool is_primary_table = false;
            dynamic_table.GetSchemaData(ot_helper, child_activity_pointer, sql_connection, activity_name_prefix);
            method_activities_list.Add(child_activity_pointer.P_pointer);

            if(dynamic_table.P_table_name == this.IV_primary_table_name) { is_primary_table=true; } else
            {
                if(secondary_tables_for_joins.Length > 0 )
                {
                    secondary_tables_for_joins += " , ";
                }
                secondary_tables_for_joins += dynamic_table.P_table_name;
            }

            foreach(DynamicMsSqlColumn dynamic_column in dynamic_table.P_columns_meta_data.Values)
            {
                field_count++;
                if (field_count > 1) { records_select_statement += " , "; }
                records_select_statement += $"{dynamic_table.P_table_name}.{dynamic_column.P_column_name}";
                
                if(is_primary_table == true && dynamic_column.P_is_primary_key == true)
                {
                    records_where_statement += $"{dynamic_table.P_table_name}.{dynamic_column.P_column_name}";
                }
            }
        }
        ////</end>get the schemas for all tables and build the field list for the select query


        //// run query to get all records that need to be replicated
        //need to add list of table name to joins using this

        string join_statement = String.Empty;
        if(secondary_tables_for_joins.Length > 0 && this.IV_join_statement.Length >0)
        {
            join_statement = this.IV_join_statement;
        }
        records_select_statement += $" FROM {this.IV_primary_table_name} {join_statement}   {records_where_statement}={this.IV_primary_record_uid};";        
        SqlDataAdapter sqda = new SqlDataAdapter();
        SqlCommand select_command = sql_connection.CreateCommand();        
        select_command.CommandText = records_select_statement;
        sqda.SelectCommand = select_command;
        DataSet data_set = new DataSet();
        try
        {
            sqda.Fill(data_set);
        }
        catch (Exception error_instance)
        {
            ETR.AddTag("records_select_statement", records_select_statement);
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > retrieving data records from server > {error_instance.Message}", ErrorTagsCodes.C_db_read_failed);
            return return_value;
        }

        ////</end> run query to get all records that need to be replicated

        //// put records into individual table class instances and collect replication SQL statments
        foreach (DyanmicMsSqlTable dynamic_table in this.IV_tables)
        {
            dynamic_table.PutDataInColumns(ot_helper, child_activity_pointer, data_set, activity_name_prefix);
            method_activities_list.Add(child_activity_pointer.P_pointer);

            return_value.P_replication_records.AddRange(dynamic_table.GenerateSqlStatementsFromData(ot_helper, child_activity_pointer, activity_name_prefix));
            method_activities_list.Add(child_activity_pointer.P_pointer);

        }
        ////</end> put records into individual table class instances and collect replication SQL statments

        //// verify we've had no errors before doing write operations
        ChildActivityPointer first_failed_activity = null;
        if (OTSH.CheckActivityList(ETR, method_activities_list, out first_failed_activity) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return_value.P_replication_records.Clear();
            return return_value;
        }
        ////</end> verify we've had no errors before doing write operations
        
        OTSH.SetSuccessEndActivity(ETR);
        return return_value;
    }


    public int replicateSqlStatements(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string dest_db_name, SqlConnection sql_connection, GdcReplicationBatch replication_batch, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".replicateSqlStatements");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        int statements_executed = 0;
        //// input data validation
        if (this._IV_constructor_success != true)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > method not running due to the fact the class constructor failed when it ran", ErrorTagsCodes.C_input_validation);
            return statements_executed;
        }

        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".replicateSqlStatements");
        validator.AddEntry(new DataVR(sql_connection, "sql_connection").AddRule(SDV.R_not_null));
        validator.AddEntry(new DataVR(dest_db_name, "dest_db_name").AddRule(SDV.R_len_greater_0));
        //TODO make a more specific validation rule for replication lists
        validator.AddEntry(new DataVR(replication_batch, "replication_batch").AddRule(SDV.R_not_null));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return statements_executed;
        }
          
        if(replication_batch.ValidateData(ot_helper,child_activity_pointer,activity_name_prefix) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return statements_executed;
        }
        ///</end> input data validation    
        
        //// switch to database we are replicating records too
        ETR.AddTag("dest_db_name", dest_db_name);
        try
        {
            sql_connection.ChangeDatabase(dest_db_name);
        }
        catch (Exception error_instance)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > switching to destination database > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
            return statements_executed;
        }
        ////</end> switch to database we are replicating records too  

        //// replicate the records using the combined select, insert, update statements
        SqlDataAdapter adapter = new SqlDataAdapter();
        foreach (GdcReplicationRecord request in replication_batch.P_replication_records)
        {
            SqlCommand combined_command = sql_connection.CreateCommand();
            combined_command.CommandText = request.P_combined_sql_statement;
            //TODO create a method to put exception handling around SQL methods
            try
            {
                combined_command.ExecuteNonQuery();
            } catch (Exception error_instance)
            {
                ETR.AddTag("combined_sql_statement", request.P_combined_sql_statement);
                OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > executing combined SQL statement > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
                return statements_executed;
            }
            statements_executed++;
        }
        //// replicate the records using the combined select, insert, update statements



        OTSH.SetSuccessEndActivity(ETR);
        return statements_executed;
    }

    }


