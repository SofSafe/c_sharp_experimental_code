using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;




interface ISofafeDatabaseField
{
    string P_code_data_identifier { get; }
    string P_database_field_name { get; }

    ISdoBaseClass P_sdo_instance { get;}
    void setSdoInstance(ISdoBaseClass sdo_instance);
}

class SofsafeDatabaseField : ISofafeDatabaseField
{
    protected string IV_code_data_identifier = "";
    protected Type IV_code_type = null;
    protected string IV_database_field_name = "";
    private ISdoBaseClass IV_sdo_instance = null;


    public ISdoBaseClass P_sdo_instance
    {
        get { return this.IV_sdo_instance; }
    }

    public string P_code_data_identifier
    {
        get { return this.IV_code_data_identifier;}
    }

    public Type P_type
    {
        get { return this.IV_code_type; }
    }

    public string P_database_field_name
    {
        get { return this.IV_database_field_name; }
    }

    public void setSdoInstance(ISdoBaseClass sdo_instance)
    {
        this.IV_sdo_instance = sdo_instance;        
    }

    public SofsafeDatabaseField(string code_data_identifier, Type code_type, string database_field_name)
    {
        this.IV_database_field_name = database_field_name;
        this.IV_code_data_identifier = code_data_identifier;
        this.IV_code_type = code_type;
    }
}



class SdoToDatabaseConnector
{
    private Dictionary<string, byte> IV_single_entity_odc_id_lookup = new Dictionary<string, byte>();
    private List<IOdcBaseClass> IV_single_entity_odc_instances = new List<IOdcBaseClass>(); 
    protected List<SofsafeDatabaseTable> IV_single_entity_table_list = new List<SofsafeDatabaseTable>() { };    
    
    protected List<List<IOdcBaseClass>> IV_multi_entity_odc_lists = new List<List<IOdcBaseClass>>();
    protected List<SofsafeDatabaseTable> IV_multi_entity_table_list = new List<SofsafeDatabaseTable> { };
    protected List<string> IV_multi_entity_ids = new List<string>();

    public delegate bool D_entity_populator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);

    public bool registerOdcInstance(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,IOdcBaseClass odc_instance, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerOdcInstance");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        
        /// input validation
        if (odc_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "odc_instance is null", ErrorTagsCodes.C_input_validation);   
            return false;
        }
        if(this.IV_single_entity_odc_id_lookup.ContainsKey(odc_instance.P_organized_data_identifier))
        {
            OTSH.addEventThenEndActivity(ETR, "odc_instance with " + odc_instance.P_organized_data_identifier + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation


        ETR.AddTag("odc_instance data identifier", odc_instance.P_organized_data_identifier);
        this.IV_single_entity_odc_id_lookup.Add(odc_instance.P_organized_data_identifier, 1);
        this.IV_odc_instances.Add(odc_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerListOfOdcInstances(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, List<IOdcBaseClass> odc_instances_list, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerListOfOdcInstances");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerDatabaseTable()
    {
        return true;
    }

    public bool populateFieldsFromOdcInstances(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateTableFromOdcInstances");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// validate data that will be used during the operation
        ChildActivityPointer last_failed_validation = new ChildActivityPointer();
        bool validation_failed_flag = false;
        if (this.IV_entity_populator_method == null)
        {
            OTSH.addEventToActivity(ETR, "IV_entity_populator_method is null");
            validation_failed_flag = true;

        }
        if (this.IV_field_list.Count == 0 )
        {
            OTSH.addEventToActivity(ETR, ".IV_field_list has no values");
            validation_failed_flag = true;
        }
        if (this.IV_odc_instances.Count == 0)
        {
            OTSH.addEventToActivity(ETR, ".IV_odc_instances has no values");
            validation_failed_flag = true;
        }
        
        /////validate data in ODC instances
        foreach (IOdcBaseClass odc_instance in this.IV_odc_instances)
        {
            //we loop through even after one fails to make sure all validation errors show up in open telemtry logs
            if (odc_instance.validateData(ot_helper, child_activity_pointer) == false)
            {
                if (validation_failed_flag == false)
                {
                    last_failed_validation = child_activity_pointer;
                    validation_failed_flag = true;
                }
            }
        }
        /////</end>validate data in ODC instances


        if (validation_failed_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> validate data that will be used during the operation


        /// put references to matching data objects in all the fields
        foreach (IOdcBaseClass odc_instance in this.IV_odc_instances)
        {
            foreach (ISdoBaseClass sdo_instance in odc_instance.P_Sdo_child_objects)
            {
                foreach (ISofafeDatabaseField field_entry in this.IV_field_list)
                {
                    if (sdo_instance.P_data_identifier == field_entry.P_code_data_identifier)
                    {
                        if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
                        {
                            Console.WriteLine(sdo_instance.P_data_identifier);
                            field_entry.setSdoInstance(sdo_instance);
                            //no break becasuse an sdo_instances value can populate mutliple database fields
                        }
                    }
                }
            }
        }
        ///</end> put references to matching data objects in all field objects

        /// call inherited class method to move the data into whatever database type is being used
        if(this.IV_entity_populator_method(ot_helper, child_activity_pointer) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_child_activity_pointer);
            return false;
        }                    

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    private void registerDatabaseFields()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields())
        {
            object field_value = class_type.GetField(field_entry.Name).GetValue(this);
            if (field_value == null) { continue; }

            /// register individual ISofafeDatabaseField class instances
            if (field_value.GetType() == typeof(SofsafeDatabaseField))
            {
                this.IV_field_list.Add(field_value as SofsafeDatabaseField);
                continue;
            }
            ///</end> register individual ISofafeDatabaseField class instances

        }
    }

    public SdoToDatabaseConnector()
    {
        this.registerDatabaseFields();
    }
}

    


class SofsafeDatabaseTable
{
    protected List<ISofafeDatabaseField> IV_field_list = new List<ISofafeDatabaseField>() { };
    protected string IV_schema_name = "";
    protected string IV_entity_path = "";
    protected SdoToDatabaseConnector.D_entity_populator IV_entity_populator_method;
}


class MsSqlDatabaseTable : SofsafeDatabaseTable
{
    protected readonly DataTable IV_table;
    protected List<DataSet> IV_datasets = new List<DataSet>() { };
    public bool populateTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer) { 
        
        foreach(SofsafeDatabaseField field_entry in this.IV_field_list)
        {

        }
        return true;
    }    

    public MsSqlDatabaseTable() : base()
    {
        this.IV_entity_populator_method = this.populateTable;
        this.IV_table = new DataTable(this.IV_schema_name);        
        foreach(SofsafeDatabaseField field_entry in this.IV_field_list)
        {
            this.IV_table.Columns.Add(field_entry.P_database_field_name, field_entry.P_type);
        }
    }
}

class SqlTransactionTable : MsSqlDatabaseTable
{
    public static SofsafeDatabaseField lan_sql_uid = new SofsafeDatabaseField(PDI.transaction_LAN_uid, typeof(int), "lan_uid");
    public static SofsafeDatabaseField returned_part = new SofsafeDatabaseField(PDI.part_uid, typeof(int), "last_part_uid");
    
     
    public SqlTransactionTable() : base()
    {
        this.IV_schema_name = "transactions";


    }
}