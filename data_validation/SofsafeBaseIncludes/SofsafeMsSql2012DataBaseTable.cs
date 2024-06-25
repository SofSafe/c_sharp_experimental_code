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
    public SofsafeMsSqlDbField(int column_index,string code_data_identifier, Type code_type, string database_field_name, SqlDbType db_field_type, int field_size, bool is_primary_key = false)
        : base(code_data_identifier, code_type, database_field_name,column_index,is_primary_key)
    {
        this.IV_db_field_type = db_field_type;
        this.IV_sql_field_size = field_size;
    }

}



class MsSql2012DbTable : SofsafeDbTable, ISofsafeDatabaseTableInheritor
{
    protected readonly DataTable IV_table;
    protected SofsafeDatabaseField IV_primary_key;



    public DataTable P_table
    {
        get { return IV_table; }
    }

    //todo add observability
    public SqlCommand createInsertStatement(SqlConnection connection)
    {
        //todo add datavalidaiton
        string statement = @"insert into " + this.IV_table.TableName + " values(";
        int loop1 = 0;
        SqlCommand cmd = connection.CreateCommand();
        foreach (SofsafeDatabaseField field_entry_base in this.IV_field_list.Values)
        {
            loop1++;
            SofsafeMsSqlDbField field_entry = field_entry_base as SofsafeMsSqlDbField;
            if (loop1 > 1) { statement += ", "; }
            statement += "@" + field_entry.P_database_field_name;
            cmd.Parameters.Add("@" + field_entry.P_database_field_name,
                field_entry.P_db_field_type, field_entry.P_field_size,
                field_entry.P_database_field_name);
        }
        statement += ")";
        cmd.CommandText = statement;
        return cmd;
    }

    //todo add observability
    public SqlCommand createSelectStatement(SqlConnection connection, List<string> include_fields=null, List<string> exclude_fields=null)
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
            if (loop1 > 1) { statement += ", "; }
            statement += field_entry.P_database_field_name;
            //cmd.Parameters.Add("@" + field_entry.P_database_field_name,
              //  field_entry.P_db_field_type, field_entry.P_field_size,
                //field_entry.P_database_field_name);

        }
        statement += " from " + this.IV_table.TableName + ";";
        cmd.CommandText = statement;
        return cmd;
    }

    public bool populateNativeDbFieldsFromSdoInstances(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
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
        int existing_row_last_index = this.IV_table.Rows.Count - 1;
        ushort last_fail_code = 0;
        while (data_found)
        {
            data_found = false;
            record_position++;
            foreach (string field_entry_key in this.IV_field_list.Keys)
            {
                SofsafeDatabaseField field_entry_base;
                if (this.IV_field_list.TryGetValue(field_entry_key, out field_entry_base) == false)
                {
                    OTSH.addEventToActivity(ETR, "failed to get field entry for column:" + field_entry_key);
                    last_fail_code = ErrorTagsCodes.C_input_validation;
                }

                SofsafeMsSqlDbField field_entry = field_entry_base as SofsafeMsSqlDbField;

                if (field_entry.P_sdo_instances.Count <= record_position)
                {
                    continue;
                }

                if (field_entry.P_sdo_instances[record_position].P_last_set_time == Sdo<int>.C_unset_time)
                {
                    continue;
                }
                if (record_position > existing_row_last_index)
                {
                    this.IV_table.Rows.Add();
                    existing_row_last_index = this.IV_table.Rows.Count - 1;
                }
                DataRow current_row = this.IV_table.Rows[record_position];


                object value_as_object = field_entry.P_sdo_instances[record_position].P_value_as_object;
                Type value_type = field_entry.P_sdo_instances[record_position].P_type;
                if (value_type == typeof(int))
                {
                    current_row.SetField(field_entry.P_column_index, (int)field_entry.P_sdo_instances[record_position].P_value_as_object);
                }
                else if (value_type == typeof(string))
                {
                    current_row.SetField(field_entry.P_column_index, (string)field_entry.P_sdo_instances[record_position].P_value_as_object);
                }
                else if (value_type == typeof(decimal))
                {
                    current_row.SetField(field_entry.P_column_index, (decimal)field_entry.P_sdo_instances[record_position].P_value_as_object);
                }
                data_found = true;
            }
            if (data_found == false) { break; }

        }

        foreach (DataRow row in this.IV_table.Rows)
        {
            foreach (DataColumn column in this.IV_table.Columns)
            {
                Console.WriteLine(column.ColumnName + ": " + row[column]);
            }
        }

        if (last_fail_code != 0)
        {
            OTSH.setFailureEndActivity(ETR, last_fail_code);
        }


        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool populateDataObjectsFromDb(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, DataSet data_set, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateDataObjectsFromDb");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

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
            foreach (DataRow row in table.Rows)
            {
                row_index++;
                foreach (DataColumn column in table.Columns)
                {

                    SofsafeDatabaseField field_entry;
                    if (this.IV_field_list.TryGetValue(column.ColumnName, out field_entry) == false)
                    {
                        OTSH.addEventToActivity(ETR, "failed to get field entry for column:" + column.ColumnName);
                        fail_flag = true;
                    }

                    if (field_entry.P_sdo_instances.Count < row_index - 1)
                    {
                        //TODO create a method that allows setting SofsafeDatabaseField default parameters for new SDO instances so rules, etc can be passed on. this have those params passed here
                        if (row[column].GetType() == typeof(int))
                        {
                            field_entry.P_sdo_instances.Add(new Sdo<int>(Convert.ToInt32(row[column]),
                                field_entry.P_code_data_identifier));
                        }
                        else if (row[column].GetType() == typeof(string))
                        {
                            field_entry.P_sdo_instances.Add(new Sdo<string>(Convert.ToString(row[column]),
                                field_entry.P_code_data_identifier));
                        }
                        else if (row[column].GetType() == typeof(decimal))
                        {
                            field_entry.P_sdo_instances.Add(new Sdo<decimal>(Convert.ToDecimal(row[column]),
                                field_entry.P_code_data_identifier));
                        }

                    }
                    else
                    {
                        field_entry.P_sdo_instances[row_index].setValueFromObject(row[column]);
                    }



                }

            }
        }

        if (fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        return true;
    }

    public MsSql2012DbTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,
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
            SofsafeDatabaseField field_entry_base_instance;
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
                    //#TODO fix #BUG primary_key_columns array isn't being populated 
                    IV_table.PrimaryKey = primary_key_columns;
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
