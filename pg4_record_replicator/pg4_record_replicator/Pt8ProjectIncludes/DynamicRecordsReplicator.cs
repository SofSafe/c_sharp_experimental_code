using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


//TODO add to SBD and finish implementing

class ReplicationBatch
{
    protected byte IV_replication_type;
    List<ReplicationRecord> IV_replication_records = new List<ReplicationRecord>();
    protected decimal IV_crush_verison = 0;
    protected short IV_client_id = 0;
    protected DateTime IV_replication_start_time;

    public DateTime P_replication_start_time
    {
        get { return this.IV_replication_start_time; }
    }

    public byte P_replication_type
    {
        get { return this.IV_replication_type; }
    }
    public void SetReplicationTypeSimple()
    {
        this.IV_replication_type = 1;
    }
    public void SetReplicationTypeComplex()
    {
        this.IV_replication_type = 2;
    }

    public string GetBatchAsJson()
    {
        string json_data = "";
        return json_data;

    }

    public bool PopulateBatchFromJson()
    {
        return true;
    }


}


//TODO add to SBD and finish implementing
class ReplicationRecord
{
    private string IV_select_statement;
    private string IV_insert_statement;
    private string IV_update_statement;
    private string IV_combined_statement;

    public string P_select_statement {
        get {return this.IV_select_statement;}
    }
    public string P_insert_statement
    {
        get { return this.IV_insert_statement; }
    }
    public string P_update_statement
    {
        get { return this.IV_update_statement; }
    }

    public string P_combined_sql_statement
    {
        get { return this.IV_combined_statement; }
    }

    public ReplicationRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string select_statment, string insert_statement, string update_statement, string combined_statement, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".Constructor1");
        //TODO create a rules to detect that a string has the right keywords for a select, insert, or update statement     
        validator.addEntry(new DataVR(select_statment).addRule(SDC.R_len_greater_2));
        validator.addEntry(new DataVR(insert_statement).addRule(SDC.R_len_greater_2));
        validator.addEntry(new DataVR(update_statement).addRule(SDC.R_len_greater_2));
        validator.addEntry(new DataVR(combined_statement).addRule(SDC.R_len_greater_2));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        ///</end> input data validation

        this.IV_select_statement = select_statment;
        this.IV_update_statement = update_statement;
        this.IV_insert_statement = insert_statement;
        this.IV_combined_statement = combined_statement;
    }

}

class DynamicRecordsReplicator
{

    //todo add a flag to indicate the instnace is in an un-usable state due to validation errors
    protected string IV_join_statement = "";
    protected string IV_primary_record_uid = "";
    protected string IV_primary_table_name = "";
    protected byte IV_replication_mode = 0;
    protected string IV_client_id = "";
    protected string IV_crush_version = "";
    protected string IV_replicated_table_name_prefix = "";
    protected List<DyanmicMsSqlTable> IV_tables = new List<DyanmicMsSqlTable> ();


    public DynamicRecordsReplicator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string primary_table_name, string primary_key_value, string client_id, string crush_version, string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".SimpleReplicationConstructor");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template
        
        if(this.SharedConstructorLogic(ot_helper,child_activity_pointer,1,primary_table_name,primary_key_value,client_id,crush_version,replication_table_prefix) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        OTSH.setSuccessEndActivity(ETR);
        return;
    }


    public DynamicRecordsReplicator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string primary_table_name, string primary_key_value, List<string> secondary_table_names, string client_id, string crush_version, string join_statement, string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".ComplexReplicationConstructor");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".Constructor2");
        //TODO create a rule for a list of sql table names
        validator.addEntry(new DataVR(secondary_table_names).addRule(SDC.R_len_greater_0));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        ///</end> input data validation

        this.IV_join_statement = join_statement;

        //// create all tables associated with the primary table
        List<Activity> method_activites_list = new List<Activity>();
        foreach(string table_name in secondary_table_names)
        {
            DyanmicMsSqlTable table_entry = new DyanmicMsSqlTable(ot_helper, child_activity_pointer, table_name, client_id, replication_table_prefix + table_name, activity_name_prefix);
            method_activites_list.Add(child_activity_pointer.P_pointer);
            if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok) { continue; }
            this.IV_tables.Add(table_entry);
        }
        
        ChildActivityPointer first_failed_activity;
        if(OTSH.checkActivityList(ETR,method_activites_list,out first_failed_activity) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return;
        }
        ////</end> create all tables associated with the primary table


        if (this.SharedConstructorLogic(ot_helper, child_activity_pointer, 2, primary_table_name, primary_key_value, client_id, crush_version, replication_table_prefix) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        
        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    private bool SharedConstructorLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,byte replication_type,string primary_table_name, string primary_key_value, string client_id, string crush_version,string replication_table_prefix, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".sharedConstructorLogic");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".sharedConstructorLogic");
        validator.addEntry(new DataVR(replication_type).addRule(SDC.R_1or2));
        validator.addEntry(new DataVR(primary_table_name).addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(primary_key_value).addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(client_id).addRule(SDC.R_client_id));
        validator.addEntry(new DataVR(crush_version).addRule(SDC.R_len_greater_2));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input data validation
      
        DyanmicMsSqlTable primary_table = new DyanmicMsSqlTable(ot_helper, child_activity_pointer, primary_table_name, client_id, replication_table_prefix + primary_table_name, activity_name_prefix);
        if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }

        this.IV_crush_version = crush_version;
        this.IV_client_id = client_id;
        this.IV_replication_mode = replication_type;
        this.IV_primary_table_name = primary_table_name;
        this.IV_primary_record_uid = primary_key_value;
        this.IV_tables.Add(primary_table);

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    //TODO update return type on SBD
    public List<ReplicationRecord> GetReplicationSqlStatements(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string source_db_name,SqlConnection sql_connection, string activity_name_prefix="")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".getReplicationSqlStatements");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        List<ReplicationRecord> return_data = new List<ReplicationRecord>();
        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".getReplicationSqlStatements");
        validator.addEntry(new DataVR(sql_connection).addRule(SDC.R_not_null));
        validator.addEntry(new DataVR(source_db_name).addRule(SDC.R_len_greater_0));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return return_data;
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
            OTSH.addEventThenEndActivity(ETR, $"{activity_name_prefix} > switching to source database > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
            return return_data;
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
            OTSH.addEventThenEndActivity(ETR, $"{activity_name_prefix} > retrieving data records from server > {error_instance.Message}", ErrorTagsCodes.C_db_read_failed);
            return return_data;
        }

        ////</end> run query to get all records that need to be replicated

        //// put records into individual table class instances and collect replication SQL statments
        foreach (DyanmicMsSqlTable dynamic_table in this.IV_tables)
        {
            dynamic_table.PutDataInColumns(ot_helper, child_activity_pointer, data_set, activity_name_prefix);
            method_activities_list.Add(child_activity_pointer.P_pointer);

            return_data.AddRange(dynamic_table.GenerateSqlStatementsFromData(ot_helper, child_activity_pointer, activity_name_prefix));
            method_activities_list.Add(child_activity_pointer.P_pointer);

        }
        ////</end> put records into individual table class instances and collect replication SQL statments

        //// verify we've had no errors before doing write operations
        ChildActivityPointer first_failed_activity = null;
        if (OTSH.checkActivityList(ETR, method_activities_list, out first_failed_activity) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return_data.Clear();
            return return_data;
        }
        ////</end> verify we've had no errors before doing write operations
        
        OTSH.setSuccessEndActivity(ETR);
        return return_data;
    }


    public int replicateSqlStatements(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string dest_db_name, SqlConnection sql_connection, List<ReplicationRecord> replication_requests, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".replicateSqlStatements");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        int statements_executed = 0;
        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".replicateSqlStatements");
        validator.addEntry(new DataVR(sql_connection).addRule(SDC.R_not_null));
        validator.addEntry(new DataVR(dest_db_name).addRule(SDC.R_len_greater_0));
        //TODO make a more specific validation rule for replication lists
        validator.addEntry(new DataVR(replication_requests).addRule(SDC.R_len_greater_0));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
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
            OTSH.addEventThenEndActivity(ETR, $"{activity_name_prefix} > switching to destination database > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
            return statements_executed;
        }
        ////</end> switch to database we are replicating records too  

        //// replicate the records using the combined select, insert, update statements
        SqlDataAdapter adapter = new SqlDataAdapter();
        foreach (ReplicationRecord request in replication_requests)
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
                OTSH.addEventThenEndActivity(ETR, $"{activity_name_prefix} > executing combined SQL statement > {error_instance.Message}", ErrorTagsCodes.C_server_db_connection);
                return statements_executed;
            }
            statements_executed++;
        }
        //// replicate the records using the combined select, insert, update statements



        OTSH.setSuccessEndActivity(ETR);
        return statements_executed;
    }

    }


