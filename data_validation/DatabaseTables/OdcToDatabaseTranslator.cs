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



class OdcToDatabaseTranslator
{
    private Dictionary<string, byte> IV_single_entity_odc_id_lookup = new Dictionary<string, byte>();
    private List<IOdcBaseClass> IV_single_entity_odc_instances = new List<IOdcBaseClass>();
    private Dictionary<string, byte> IV_single_entity_table_name_lookup = new Dictionary<string, byte>();

    protected List<SofsafeDatabaseTable> IV_single_entity_table_list = new List<SofsafeDatabaseTable>() { };    
    
    protected Dictionary<string,List<List<IOdcBaseClass>>> IV_multi_entity_odc_lists = new Dictionary<string, List<List<IOdcBaseClass>>>();
    protected Dictionary<string,List<SofsafeDatabaseTable>> IV_multi_entity_table_list = new Dictionary<string, List<SofsafeDatabaseTable>> { };
    protected List<string> IV_multi_entity_ids = new List<string>();

    public delegate bool D_entity_populator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);

    public bool registerSingleEntityTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SofsafeDatabaseTable table_instance, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerOdcInstance");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);

        /// input validation
        if (table_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance is null", ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_single_entity_table_name_lookup.ContainsKey(table_instance.P_schema_name))
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance with " + table_instance.P_schema_name + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation


        ETR.AddTag("table_instance data identifier", table_instance.P_schema_name);
        this.IV_single_entity_table_name_lookup.Add(table_instance.P_schema_name, 1);
        this.IV_single_entity_table_list.Add(table_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }
    public bool registerSingleEntityOdc(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,IOdcBaseClass odc_instance, string activity_name_prefix = "")
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
        this.IV_single_entity_odc_instances.Add(odc_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }



    public bool registerMultiEntity(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, List<List<IOdcBaseClass>> odc_instances_lists, List<SofsafeDatabaseTable> table_list, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerListOfOdcInstances");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".registerListOfOdcInstances");
        validator.addEntry(new DataVR(multi_entity_id).addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(table_list).addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(odc_instances_lists).addRule(SDC.R_populated_nested_array));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation

        this.IV_multi_entity_ids.Add(multi_entity_id);
        this.IV_multi_entity_odc_lists.Add(multi_entity_id, odc_instances_lists);
        this.IV_multi_entity_table_list.Add(multi_entity_id, table_list);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public bool populateFields(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateTableFromOdcInstances");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// validate data that will be used during the operation
        ChildActivityPointer last_failed_validation = new ChildActivityPointer();
        bool validation_failed_flag = false;     
        
        /////validate data in ODC instances
        foreach (IOdcBaseClass odc_instance in this.IV_single_entity_odc_instances)
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
        bool fail_flag = true;
        foreach (IOdcBaseClass odc_instance in this.IV_single_entity_odc_instances)
        {
            foreach (ISdoBaseClass sdo_instance in odc_instance.P_Sdo_child_objects)
            {
                foreach (ISofsafeDatabaseTable table in this.IV_single_entity_table_list)
                {
                    if(table.P_usable_code_ids_lookup.ContainsKey(sdo_instance.P_data_identifier) == false )
                    { 
                        continue;
                    }
                    if(table.setFieldsToSdo(ot_helper, child_activity_pointer, sdo_instance) == false)
                    {
                        fail_flag = true;
                    }
                }
            }
        }
        ///</end> put references to matching data objects in all field objects
                

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }



}


interface ISofsafeDatabaseTable
{
    string P_schema_name { get; }
    Dictionary<string, byte> P_usable_code_ids_lookup { get; }
    bool setFieldsToSdo(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "");
}


class SofsafeDatabaseTable: ISofsafeDatabaseTable
{
    protected List<SofsafeDatabaseField> IV_field_list = new List<SofsafeDatabaseField>() { };
    protected Dictionary<string,List<SofsafeDatabaseField>> IV_code_id_to_fields = new Dictionary<string, List<SofsafeDatabaseField>>();
    protected Dictionary<string,byte> IV_usable_code_ids_lookup = new Dictionary<string, byte>();
    protected string IV_schema_name = "";
    protected string IV_entity_path = "";
    protected OdcToDatabaseTranslator.D_entity_populator IV_entity_populator_method;
   
    public Dictionary<string,byte> P_usable_code_ids_lookup
    {
        get { return  IV_usable_code_ids_lookup;}
    }

    public bool setFieldsToSdo(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".setFieldsToSdo");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();


        ///input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".setFieldsToSdo");
        validator.addEntry(new DataVR(sdo_instance, "sdo_instance").addRule(SDC.R_not_null));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.passStatusAndEndActivity(ETR,child_activity_pointer.P_child_activity_pointer);
            return false;
        }
        ///</end>input validation

        List<SofsafeDatabaseField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null) {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true; 
            }
        foreach (SofsafeDatabaseField field_entry in field_entries)
        {
           
        if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
        {
            Console.WriteLine(sdo_instance.P_data_identifier);
            field_entry.setSdoInstance(sdo_instance);
            //no break becasuse an sdo_instances value can populate mutliple database fields
        }            
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
       
    }


    public string P_schema_name
    {
        get { return this.IV_schema_name; }
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
                SofsafeDatabaseField db_field = field_value as SofsafeDatabaseField;
                this.IV_field_list.Add(db_field);
                //this.IV_field_list.Count;
                if(this.IV_code_id_to_fields.ContainsKey(db_field.P_code_data_identifier) == true)
                {
                    List<SofsafeDatabaseField> existing_list;
                    this.IV_code_id_to_fields.TryGetValue(db_field.P_code_data_identifier, out existing_list);
                    existing_list.Add(db_field);
                } else
                {
                    this.IV_usable_code_ids_lookup.Add(db_field.P_code_data_identifier,1);
                    this.IV_code_id_to_fields.Add(db_field.P_code_data_identifier,
                        new List<SofsafeDatabaseField>(){db_field}
                    );
                }
                continue;
            }
            ///</end> register individual ISofafeDatabaseField class instances

        }
    }

    public SofsafeDatabaseTable()
    {
        this.registerDatabaseFields();
    }

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
            if (field_entry == null) 
            {

                continue;
            }
            this.IV_table.Columns.Add(field_entry.P_database_field_name, field_entry.P_type);
        }
    }
}

class SqlTransactionTable : MsSqlDatabaseTable
{
    public static SofsafeDatabaseField lan_sql_uid = new SofsafeDatabaseField(PDI.transaction_LAN_uid, typeof(int), "lan_uid");
    public static SofsafeDatabaseField total = new SofsafeDatabaseField(PDI.transaction_total, typeof(decimal), "total");
    public static SofsafeDatabaseField user_uid = new SofsafeDatabaseField(PDI.POS_user_uid, typeof(int), "user_uid");
    public static SofsafeDatabaseField pos_uid = new SofsafeDatabaseField(PDI.POS_computer_uid, typeof(int), "pos_uid");



    public SqlTransactionTable() : base()
    {
        this.IV_schema_name = "transactions";


    }
}