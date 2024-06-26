using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface ISofafeDbField
{
    string P_code_data_identifier { get; }
    string P_database_field_name { get; }

    Type P_type { get; }

    List<ISdoBaseClass> P_sdo_instances { get; }
    bool assignSdoInstanceToRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,ISdoBaseClass sdo_instance, int record_position);
    bool removeSdoInstance(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, int sdo_instance_index, string activity_name_prefix = "");
}

class SofsafeDatabaseField : ISofafeDbField
{
    //todo implment an instance validation method
    protected bool instance_valid = false;
    protected string IV_code_data_identifier = "";
    protected Type IV_code_type = null;
    protected string IV_database_field_name = "";
    private List<ISdoBaseClass> IV_sdo_instances = new List<ISdoBaseClass>();
    private ISdoBaseClass IV_assigned_sdo_for_all_records = null;

    public List<ISdoBaseClass> P_sdo_instances
    {
        get { return this.IV_sdo_instances; }
    }

    public ISdoBaseClass P_assigned_sdo_for_all_records
    {
        get { return this.IV_assigned_sdo_for_all_records; }
    }

    public string P_code_data_identifier
    {
        get { return this.IV_code_data_identifier; }
    }

    public Type P_type
    {
        get { return this.IV_code_type; }
    }

    public string P_database_field_name
    {
        get { return this.IV_database_field_name; }
    }

    public bool assignSdoInstanceToRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position)
    {
        //todo add datavalidation since sdo_instance can be null

        if (this.IV_sdo_instances.Count < record_position + 1)
        {
            this.IV_sdo_instances.Add(sdo_instance);
        }
        else
        {
            this.IV_sdo_instances[record_position] = sdo_instance;
        }
        return true;
    }

    public bool assignSdoInstanceToAllRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance)
    {
        //todo add datavalidation since sdo_instance can be null
        IV_assigned_sdo_for_all_records = sdo_instance;
        return true;
    }

    public bool removeSdoInstance(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, int sdo_instance_index, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".removeSdoInstance");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ETR.AddTag("sdo_instance_index", sdo_instance_index);

        //// input validation
        if (this.IV_sdo_instances.Count < sdo_instance_index + 1)
        {
            OTSH.addEventThenEndActivity(ETR, "sdo_instance_index value is out of range", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ////</end> input validation
        
        ////TODO finish this method

        return true;
    }

    public SofsafeDatabaseField(string code_data_identifier, Type code_type, string database_field_name)
    {
        this.IV_database_field_name = database_field_name;
        this.IV_code_data_identifier = code_data_identifier;
        this.IV_code_type = code_type;
    }


}

interface ISofafeSqlDbField : ISofafeDbField
{    
    int P_column_index { get; }
    bool P_is_primary_key { get; }
}

class SofsafeSqlDbField : SofsafeDatabaseField, ISofafeDbField, ISofafeSqlDbField
{
    protected int IV_column_index = 0;
    protected bool IV_is_primary_key = false;


    public bool P_is_primary_key
    {
        get { return IV_is_primary_key; }
    }

    public int P_column_index
    {
        get { return IV_column_index; }
    }

    public SofsafeSqlDbField(string code_data_identifier, Type code_type, string database_field_name, int column_index, bool is_primary_key = false)
        : base(code_data_identifier, code_type, database_field_name)
    {
        this.IV_column_index = column_index;
        this.IV_is_primary_key = is_primary_key;

    }

}


interface ISofsafeDatabaseTableInheritor : ISofsafeDbTable
{
    bool populateNativeDbFieldsFromSdoInstances(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");
    //bool updateRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool createRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool deleteRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool populateRecordsFromQuery(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string query,string activity_name_prefix = "");
}


interface ISofsafeDbTable
{
    string P_schema_name { get; }
    Dictionary<string, byte> P_usable_code_ids_lookup { get; }
    bool assignSdoToRecordField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1, string activity_name_prefix = "");
    bool assignSdoToAllRecordsField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "");
}


class SofsafeDbTable : ISofsafeDbTable
{
    //this dictionary can store SofsafeDatabaseField instances or any class derived from the SofsafeDatabaseField
    protected Dictionary<string, SofsafeDatabaseField> IV_field_list = new Dictionary<string, SofsafeDatabaseField>() { };
    //this should be overwritten with the derivative of SofsafeDatabaseField a class is using.
    protected Type IV_field_list_type;
    public readonly Dictionary<string, byte> IV_field_names = new Dictionary<string, byte>() { };

    protected Dictionary<string, List<SofsafeDatabaseField>> IV_code_id_to_fields = new Dictionary<string, List<SofsafeDatabaseField>>();
    protected Dictionary<string, byte> IV_usable_code_ids_lookup = new Dictionary<string, byte>();
    protected string IV_schema_name = "";
    protected string IV_entity_path = "";
    protected OdcToDatabaseTranslator.D_entity_populator IV_entity_populator_method;

    public Dictionary<string, byte> P_usable_code_ids_lookup
    {
        get { return IV_usable_code_ids_lookup; }
    }

    public bool assignSdoToRecordField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1, string activity_name_prefix = "")
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
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end>input validation

        List<SofsafeDatabaseField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null)
        {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
        foreach (SofsafeDatabaseField field_entry in field_entries)
        {
            if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
            {
                //no break becasuse an sdo_instances value can populate mutliple database fields
                field_entry.assignSdoInstanceToRecord(ot_helper,child_activity_pointer, sdo_instance, record_position);
            }
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;

    }

    public bool assignSdoToAllRecordsField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".assignSdoToAllRecordsField");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template
        

        ///input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".setFieldsToSdo");
        validator.addEntry(new DataVR(sdo_instance, "sdo_instance").addRule(SDC.R_not_null));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end>input validation

        bool fail_flag = false;
        List<SofsafeDatabaseField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null)
        {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
        foreach (SofsafeDatabaseField field_entry in field_entries)
        {
            if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
            {
                //no break becasuse an sdo_instances value can populate mutliple database fields
                if(field_entry.assignSdoInstanceToAllRecords(ot_helper, child_activity_pointer, sdo_instance) == false)
                {
                    OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                    return false;

                }
            }
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public bool populateDbFields()
    {

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

            /// TODO add data validation for duplicate database field names
            /// 

            /// register individual ISofafeDatabaseField class instances
            if (field_value.GetType() != this.IV_field_list_type)
            {
                continue;
            }
            
            SofsafeDatabaseField db_field = field_value as SofsafeDatabaseField;
            this.IV_field_list.Add(db_field.P_database_field_name, db_field);

            this.IV_field_names.Add(db_field.P_database_field_name, 1);                
                
            if (this.IV_code_id_to_fields.ContainsKey(db_field.P_code_data_identifier) == true)
            {
                List<SofsafeDatabaseField> existing_list;
                this.IV_code_id_to_fields.TryGetValue(db_field.P_code_data_identifier, out existing_list);
                existing_list.Add(db_field);
            }
            else
            {
                this.IV_usable_code_ids_lookup.Add(db_field.P_code_data_identifier, 1);
                this.IV_code_id_to_fields.Add(db_field.P_code_data_identifier,
                    new List<SofsafeDatabaseField>() { db_field }
                );
            }
            continue;
            
            ///</end> register individual ISofafeDatabaseField class instances

        }
    }

    public SofsafeDbTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string schema_name, Type field_type, string activity_name_prefix = "")
    {
        this.IV_schema_name = schema_name;
        this.IV_field_list_type = field_type;
        this.registerDatabaseFields();

    }

}


