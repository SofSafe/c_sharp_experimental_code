using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using Microsoft.Data.SqlClient;
using SofsafeBaseIncludes;
 
class DynamicMsSqlColumn
{
    //todo add a flag to indicate the instnace is in an un-usable state due to validation errors
    protected string IV_column_name = String.Empty;
    protected int IV_column_index = -1;
    protected Type IV_code_type = null;
    protected SqlDbType IV_db_column_type;
    protected int IV_db_column_size = -1;
    protected bool IV_is_primary_key = false;
    protected bool IV_populating_attempted = false;

    public bool P_is_primary_key
    {
        get { return this.IV_is_primary_key; }
    }

    public string P_column_name
    {
        get { return this.IV_column_name; }
    }

    public int P_column_index 
    {
        get { return this.IV_column_index;}
    }

    public int P_column_size
    {
        get { return this.IV_db_column_size; }
    }


    public Type P_code_type
    {
        get { return this.IV_code_type; }
    }

    public SqlDbType P_db_column_type
    {
        get { return this.IV_db_column_type; }
    }


    public bool PopulateSchemaInformation(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,DataTable table, DataRow schema_row, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".PopulateSchemaInformation");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

       

        //// input data validation
        if(this.IV_populating_attempted == true)
        {
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > PopulateSchemaInformation has already been called on this instance", ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_populating_attempted = true;
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".registerListOfOdcInstances");
        validator.AddEntry(new DataVR(table).AddRule(RulesCommon.R_not_null));
        validator.AddEntry(new DataVR(schema_row).AddRule(RulesCommon.R_not_null));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input data validation

        bool name_set = false;
        bool size_set = false;
        bool index_set = false;
        bool type_set = false;
        string errors = String.Empty;

        string debugging_row_data = "";
        foreach (DataColumn column in table.Columns)
        {
            if (schema_row[column].ToString() == null) { continue; }
            debugging_row_data += $"{column.ColumnName}: {schema_row[column].ToString()}\n";
            if (column.ColumnName == "TYPE_NAME")
            {
                // the last else statement sets this to false in case we didn't find a type
                type_set = true;
                string type_name = schema_row[column].ToString();
                
                if (type_name.Contains("identity") == true)
                {
                    this.IV_is_primary_key = true;
                    type_name = type_name.Replace(" identity", "");
                }
                if (type_name == "int")
                {
                    this.IV_code_type = typeof(int);
                    this.IV_db_column_type = SqlDbType.Int;

                }
                else if (type_name == "money")
                {
                    this.IV_code_type = typeof(decimal);
                    this.IV_db_column_type = SqlDbType.Money;
                }
                else if (type_name == "varchar")
                {
                    this.IV_code_type = typeof(string);
                    this.IV_db_column_type = SqlDbType.VarChar;
                }
                else if (type_name == "datetime")
                {
                    this.IV_code_type = typeof(DateTime);
                    this.IV_db_column_type = SqlDbType.DateTime;
                }
                else if (type_name == "datetime2")
                {
                    this.IV_code_type = typeof(DateTime);
                    this.IV_db_column_type = SqlDbType.DateTime2;
                }
                else if (type_name == "bit")
                {
                    this.IV_code_type = typeof(bool);
                    this.IV_db_column_type = SqlDbType.Bit;
                }
                else if (type_name == "smallint")
                {
                    this.IV_code_type = typeof(int);
                    this.IV_db_column_type = SqlDbType.SmallInt;
                }
                else if (type_name == "numeric")
                {
                    this.IV_code_type = typeof(decimal);
                    this.IV_db_column_type = SqlDbType.Decimal;
                }
                else
                {
                    errors += $"Column type {type_name} doesn't have any handler logic. ";
                    type_set = false;
                }

            } else if (column.ColumnName == "COLUMN_NAME")
            {
                this.IV_column_name = schema_row[column].ToString();
                name_set = true;
            }
            else if (column.ColumnName == "LENGTH")
            {
                this.IV_db_column_size = int.Parse(schema_row[column].ToString());
                size_set = true;
            }
            else if (column.ColumnName == "ORDINAL_POSITION")
            {
                index_set = true;
                this.IV_column_index = int.Parse(schema_row[column].ToString());
            }                
        }

        if(index_set == false) { errors += "column index not found. "; }
        if (name_set == false) { errors += "column name not found. "; }
        if (type_set == false) { errors += "column type not found. "; }
        if (size_set == false) { errors += "column size not found. "; }

        if(errors.Length > 0)
        {
            ETR.AddTag("column data", debugging_row_data);
            OTSH.AddEventThenEndActivity(ETR, $"{activity_name_prefix} > {errors}", ErrorTagsCodes.C_input_validation);
            return false;
        }


        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


}
