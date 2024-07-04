using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


interface ISofsafeMsSqlDbField: ISofafeDbField, ISofafeSqlDbField
{
    int P_field_size { get; }
    SqlDbType P_db_field_type { get; }
}

class SofsafeMsSqlDbField : SofsafeSqlDbField, ISofafeDbField, ISofafeSqlDbField, ISofsafeMsSqlDbField
{
    protected int IV_sql_field_size;
    protected SqlDbType IV_db_field_type;

    public int P_field_size
        {
            get { return this.IV_sql_field_size; }
        }
    public SqlDbType P_db_field_type
        {
            get { return this.IV_db_field_type; }
        }
    //todo add observability
    public SofsafeMsSqlDbField(int column_index,string code_data_identifier,
        Type code_type, string database_field_name,
        SqlDbType db_field_type, int field_size,
        ISofafeSqlDbField replicated_from_on_insert=null,
        bool is_primary_key = false, bool auto_increments=false)
        : base(code_data_identifier, code_type, database_field_name,column_index,
            replicated_from_on_insert, is_primary_key, auto_increments)
    {
        //todo add datavalidation
        this.IV_db_field_type = db_field_type;
        this.IV_sql_field_size = field_size;
    }

}



class MsSqlDbTable : SofsafeDbTable, ISofsafeDbTableInheritor
{
    protected readonly DataTable IV_table;
    protected SofsafeDbField IV_primary_key;



    public DataTable P_table
    {
        get { return IV_table; }
    }

    //todo add observability
    public bool createInsertStatement(ref SqlCommand insert_cmd)
    {
        //todo add datavalidaiton
        string fields_string = @"insert into " + this.IV_table.TableName + " (";
        string values_string = " values(";
        int loop1 = 0;
 

        foreach (SofsafeDbField field_entry_base in this.IV_field_list.Values)
        {                     
            SofsafeMsSqlDbField field_entry = field_entry_base as SofsafeMsSqlDbField;
            if(field_entry.P_auto_increments == true) { continue; }
            if(field_entry.P_sdo_instances.Count == 0 
                && field_entry.P_replicated_from_on_insert == null 
                && field_entry.P_assigned_sdo_for_all_records == null) { continue; }
            loop1++;
            if (loop1 > 1) { 
                values_string += ", ";
                fields_string += ", ";
            }
            fields_string += field_entry.P_database_field_name;
            if (field_entry.P_replicated_from_on_insert != null)
            {
                //TODO get this working as expected on msql 2012
                values_string += "@" + field_entry.P_replicated_from_on_insert.P_schema_name + "_" + field_entry.P_replicated_from_on_insert.P_database_field_name;
            }
            else
            {
                values_string += "@" + field_entry.P_database_field_name;
                insert_cmd.Parameters.Add("@" + field_entry.P_database_field_name,
                field_entry.P_db_field_type, field_entry.P_field_size,
                field_entry.P_database_field_name);
            }            
        }
        fields_string += ") ";
        values_string += "); " ;

       insert_cmd.CommandText += fields_string + values_string;
        return true;
    }

    //todo add observability
    public SqlCommand createSelectStatement(SqlConnection connection, bool include_terminator = false, List<string> include_fields=null, List<string> exclude_fields=null)
    {
        //todo add datavalidaiton
        SqlCommand cmd = connection.CreateCommand();
        string statement = @"select ";
        int loop1 = 0;
        foreach (ISofsafeMsSqlDbField field_entry in this.IV_field_list.Values)
        {
            if(include_fields != null && include_fields.Contains(field_entry.P_database_field_name) == false) { continue; }
            if(exclude_fields != null && exclude_fields.Contains(field_entry.P_database_field_name))
            { continue;}
            loop1++;
            if (loop1 > 1) { statement += " , "; }
            statement += field_entry.P_schema_name + "." + field_entry.P_database_field_name;

        }
        statement += " from " + this.IV_table.TableName;
        if(include_terminator == true) { statement += ";"; }            
        cmd.CommandText = statement;
        return cmd;
    }

    public bool populateNativeDbFieldsFromDataObjects(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateNativeDbFieldsFromSdoInstances");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        int record_position = -1;
        bool data_found = true;
        bool record_added = false;
        int existing_row_last_index = this.IV_table.Rows.Count - 1;
        ushort last_fail_code = 0;
        while (data_found)
        {
            data_found = false;
            record_added = false;
            record_position++;
            foreach (string field_entry_key in this.IV_field_list.Keys)
            {
                SofsafeDbField field_entry_base;
                if (this.IV_field_list.TryGetValue(field_entry_key, out field_entry_base) == false)
                {
                    OTSH.addEventToActivity(ETR, "failed to get field entry for column:" + field_entry_key);
                    last_fail_code = ErrorTagsCodes.C_input_validation;
                    continue;
                }

                SofsafeMsSqlDbField field_entry = field_entry_base as SofsafeMsSqlDbField;

                /// input validation
                if (field_entry.P_assigned_sdo_for_all_records == null)
                {
                    if (field_entry.P_sdo_instances.Count <= record_position)
                    {
                        continue;
                    }

                    if (field_entry.P_sdo_instances[record_position].P_last_set_time == Sdo<int>.C_unset_time)
                    {
                        continue;
                    }
                } 
                else
                {
                    if (field_entry.P_assigned_sdo_for_all_records.P_last_set_time == Sdo<int>.C_unset_time)
                    {
                        continue;
                    }
                }
                ///</end> input validation
                if (record_position > existing_row_last_index)
                {
                    this.IV_table.Rows.Add();
                    record_added = true;
                    existing_row_last_index = this.IV_table.Rows.Count - 1;
                }
                DataRow current_row = this.IV_table.Rows[record_position];

                object value_as_object;
                Type value_type;
                if (field_entry.P_assigned_sdo_for_all_records != null)
                {
                    value_as_object = field_entry.P_assigned_sdo_for_all_records.P_value_as_object;
                    value_type = field_entry.P_assigned_sdo_for_all_records.P_type;
                }
                else 
                {
                    value_as_object = field_entry.P_sdo_instances[record_position].P_value_as_object;
                    value_type = field_entry.P_sdo_instances[record_position].P_type;
                    //only keep going if we find an individual field record
                    data_found = true;
                }

                if (value_type == typeof(int))
                {
                    current_row.SetField(field_entry.P_column_index, (int)value_as_object);
                }
                else if (value_type == typeof(string))
                {
                    current_row.SetField(field_entry.P_column_index, (string)value_as_object);
                }
                else if (value_type == typeof(decimal))
                {
                    current_row.SetField(field_entry.P_column_index, (decimal)value_as_object);
                }
                else if (value_type == typeof(DateTime))
                {
                    current_row.SetField(field_entry.P_column_index, (DateTime)value_as_object);
                }
                else
                {
                    OTSH.addEventToActivity(ETR, "failed to match a type for field: " + field_entry_key);
                    last_fail_code = ErrorTagsCodes.C_input_validation;
                }
            }            
            if (data_found == false) {
                if (record_added == true)
                {
                    //the last row was added in case we had data to add, but no data was found to add to it
                    this.IV_table.Rows.RemoveAt(this.IV_table.Rows.Count - 1);
                }
                break; 
            }
        }

        if (last_fail_code != 0)
        {
            OTSH.setFailureEndActivity(ETR, last_fail_code);
        }


        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool populateEmptyDataObjectsFromDb(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, DataSet data_set , string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateDataObjectsFromDb");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //todo add data validation

        bool fail_flag = false;
        bool table_found = false;

        foreach (DataTable table in data_set.Tables)
        {
            if (table_found == true) { break; }

            if (table.TableName != this.IV_table.TableName)
            {
                continue;
            }
            table_found = true;
            int row_index = -1;
            foreach (DataColumn column in table.Columns)
            {

                SofsafeDbField field_entry;
                if (this.IV_field_list.TryGetValue(column.ColumnName, out field_entry) == false)
                {
                    OTSH.addEventToActivity(ETR, "failed to get field entry for column:" + column.ColumnName);
                    fail_flag = true;
                    continue;
                }
                if (field_entry.P_sdo_instances.Count == 0)
                {
                    //unused columns won't be pre-filled with empty instances
                    continue;
                }
                row_index = -1;
                foreach (DataRow row in table.Rows)
                {
                    if(row[column] == null) { continue; }
                    row_index++;
                    field_entry.P_sdo_instances[row_index].setValueFromObject(row[column]);
                }
            }
        }

        if (fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public MsSqlDbTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,
        string schema_name,string activity_name_prefix = "") 
        : base(ot_helper, method_activity_pointer, schema_name, typeof(SofsafeMsSqlDbField), activity_name_prefix)
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".initilizer");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ///</end> observability template




        //// add all database columns to IV_table
        bool fail_flag = false;
        this.IV_table = new DataTable(this.IV_schema_name);
        DataColumn[] primary_key_columns = new DataColumn[] { };
        int column_index = -1;
        foreach (string field_entry_key in this.IV_field_list.Keys)
        {
            SofsafeDbField field_entry_base_instance;
            if (this.IV_field_list.TryGetValue(field_entry_key, out field_entry_base_instance) == false)
            {
                OTSH.addEventToActivity(ETR, "field entry null for key: " + field_entry_key);
                fail_flag = true;
                //don't end early so all the errors get logged
                continue;
            }
            SofsafeMsSqlDbField field_entry = field_entry_base_instance as SofsafeMsSqlDbField;
            column_index++;
            this.IV_table.Columns.Add(field_entry.P_database_field_name, field_entry.P_type);

            if (field_entry.P_is_primary_key == true)
            {
                if (this.IV_primary_key != null)
                {
                    //TODO add error logging
                }
                else
                {
                    this.IV_primary_key = field_entry;
                    Array.Resize(ref primary_key_columns, 1);
                    primary_key_columns.SetValue(this.IV_table.Columns[column_index],0);
                    //#TODO fix #BUG primary_key_columns being added makes auto-increments fail
                    //IV_table.PrimaryKey = primary_key_columns;
                }
            }
        }

        
        ////</end> add all database columns to IV_table


        if (fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.setSuccessEndActivity(ETR);

    }
}
