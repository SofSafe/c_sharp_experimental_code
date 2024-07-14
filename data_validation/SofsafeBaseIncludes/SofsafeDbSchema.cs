using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface ISofafeDbField
{
    string P_bus_data_identifier { get; }
    string P_database_field_name { get; }

    string P_schema_name { get; }

    Type P_type { get; }

    List<ISdoBaseClass> P_sdo_instances { get; }
    
    bool assignSdoInstanceToRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,ISdoBaseClass sdo_instance, int record_position);
    bool removeSdoInstance(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, int sdo_instance_index, string activity_name_prefix = "");
    //todo add observability
    bool setSchemaName(string schema_name);
    bool assignSdoInstanceToAllRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance);
}

class SofsafeDbField : ISofafeDbField
{
    //todo implment an instance validation method
    protected bool instance_valid = false;
    protected string IV_bus_data_identifier = "Class Default Value";
    protected Type IV_code_type = null;
    protected string IV_database_field_name = "Default Field Name";
    private List<ISdoBaseClass> IV_sdo_instances = new List<ISdoBaseClass>();
    protected ISdoBaseClass IV_assigned_sdo_for_all_records = null;
    protected string IV_schema_name = "Default Schema Name";


    public string P_schema_name
    {
        get { return this.IV_schema_name; }
    }
    public List<ISdoBaseClass> P_sdo_instances
    {
        get { return this.IV_sdo_instances; }
    }

    public ISdoBaseClass P_assigned_sdo_for_all_records
    {
        get { return this.IV_assigned_sdo_for_all_records; }
    }

    public string P_bus_data_identifier
    {
        get { return this.IV_bus_data_identifier; }
    }

    public Type P_type
    {
        get { return this.IV_code_type; }
    }

    public string P_database_field_name
    {
        get { return this.IV_database_field_name; }
    }


    public bool setSchemaName(string schema_name)
    {
        //todo add input validation
        this.IV_schema_name= schema_name;
        return true;
    }
    public bool assignSdoInstanceToRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1)
    {
        //todo add datavalidation since sdo_instance can be null or not be the type specified by IV_code_type
        
        if(record_position == -1) { record_position = this.IV_sdo_instances.Count; }

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
        //todo add datavalidation since sdo_instance can be null or not be the type specified by IV_code_type, or have invalid data
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

    //todo add observability
    public SofsafeDbField(string bus_data_identifier, Type code_type, string database_field_name)
    {
        //todo add data validation
        this.IV_database_field_name = database_field_name;
        this.IV_bus_data_identifier = bus_data_identifier;
        this.IV_code_type = code_type;
    }


}

interface ISofafeSqlDbField : ISofafeDbField
{    
    int P_column_index { get; }
    bool P_is_primary_key { get; }
    bool P_auto_increments { get; }
    //todo get this working
}

class SofsafeSqlDbField : SofsafeDbField, ISofafeDbField, ISofafeSqlDbField
{
    protected int IV_column_index = -1;
    protected bool IV_is_primary_key = false;
    protected bool IV_auto_increments = false;


    public bool P_auto_increments
    {
        get { return IV_auto_increments; }
    }
    public bool P_is_primary_key
    {
        get { return IV_is_primary_key; }
    }

    public int P_column_index
    {
        get { return IV_column_index; }
    }

    //todo add observability
    public SofsafeSqlDbField(string bus_data_identifier, Type code_type,
        string database_field_name, int column_index,       
        bool is_primary_key = false,bool auto_increments=false)
        : base(bus_data_identifier, code_type, database_field_name)
    {
        //todo add input validation
        this.IV_column_index = column_index;
        this.IV_is_primary_key = is_primary_key;
        this.IV_auto_increments = auto_increments;
    }

}


interface ISofsafeDbSchemaInheritor : ISofsafeDbSchema
{
    bool populateNativeDbFieldsFromDataObjects(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");
    //bool updateRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool createRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool deleteRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix = "");
    //bool populateRecordsFromQuery(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string query,string activity_name_prefix = "");
}


interface ISofsafeDbSchema
{
    string P_schema_name { get; }
    Dictionary<string, byte> P_usable_code_ids_lookup { get; }
    bool assignSdoToRecordField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1, string activity_name_prefix = "");
    bool assignSdoToAllFieldRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "");

    bool assignEmptySdoToRecordField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1, string activity_name_prefix = "");
}


class SofsafeDbSchema : ISofsafeDbSchema
{
    //this dictionary can store SofsafeDatabaseField instances or any class derived from the SofsafeDatabaseField
    protected readonly Dictionary<string, SofsafeDbField> IV_fields_dict = new Dictionary<string, SofsafeDbField>() { };
    //this should be overwritten with the derivative of SofsafeDatabaseField a class is using.
    protected readonly Type IV_fields_dict_type;

    protected readonly Dictionary<string, List<SofsafeDbField>> IV_code_id_to_fields = new Dictionary<string, List<SofsafeDbField>>();
    protected readonly Dictionary<string, byte> IV_usable_code_ids_lookup = new Dictionary<string, byte>();
    protected readonly string IV_schema_name = "Default Name";
    protected string IV_entity_path = "Default Path";

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
        
        //FIXME add check to make sure data is valid here
        if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
        {
            OTSH.setSuccessEndActivity(ETR);
            //FIXME make this return an error
            return true;
        }

        List<SofsafeDbField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null)
        {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
        foreach (SofsafeDbField field_entry in field_entries)
        {        
            //no break becasuse an sdo_instances value can populate mutliple database fields
            field_entry.assignSdoInstanceToRecord(ot_helper,child_activity_pointer, sdo_instance, record_position);
            //FIXME add error handling           
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;

    }

    public bool assignSdoToAllFieldRecords(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, string activity_name_prefix = "")
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

        //FIXME add check to make sure data is valid here
        if (sdo_instance.P_last_set_time != Sdo<int>.C_unset_time)
        {
            OTSH.setSuccessEndActivity(ETR);
            //FIXME make this return an error
            return true;
        }

        bool fail_flag = false;
        List<SofsafeDbField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null)
        {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
        foreach (SofsafeDbField field_entry in field_entries)
        {
            //no break becasuse an sdo_instances value can populate mutliple database fields
            if(field_entry.assignSdoInstanceToAllRecords(ot_helper, child_activity_pointer, sdo_instance) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;                
            }
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public bool assignEmptySdoToRecordField(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ISdoBaseClass sdo_instance, int record_position = -1, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".assignEmptySdoToRecordField");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ///input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".assignEmptySdoToRecordField");
        validator.addEntry(new DataVR(sdo_instance, "sdo_instance").addRule(SDC.R_not_null));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        //FIXME add logic to make sure the Sdo instance is empty
        ///</end>input validation
       

        List<SofsafeDbField> field_entries;
        this.IV_code_id_to_fields.TryGetValue(sdo_instance.P_data_identifier, out field_entries);
        if (field_entries == null)
        {
            //this is isn't a failure because fields don't have to exist for every sdo
            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
        foreach (SofsafeDbField field_entry in field_entries)
        {
            //no break becasuse an sdo_instances value can populate mutliple database fields
            field_entry.assignSdoInstanceToRecord(ot_helper, child_activity_pointer, sdo_instance, record_position);
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;

    }


    public string P_schema_name
    {
        get { return this.IV_schema_name; }
    }

    //todo add observability
    private bool registerDatabaseFields()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields())
        {
            object field_value = class_type.GetField(field_entry.Name).GetValue(this);
            if (field_value == null) { continue; }

            /// TODO add data validation for duplicate database field names
            /// 

            /// register individual ISofafeDatabaseField class instances
            if (field_value.GetType() != this.IV_fields_dict_type)
            {
                continue;
            }

            
            SofsafeDbField db_field = field_value as SofsafeDbField;
            db_field.setSchemaName(this.IV_schema_name);
            this.IV_fields_dict.Add(db_field.P_database_field_name, db_field);

                
            if (this.IV_code_id_to_fields.ContainsKey(db_field.P_bus_data_identifier) == true)
            {
                List<SofsafeDbField> existing_list;
                this.IV_code_id_to_fields.TryGetValue(db_field.P_bus_data_identifier, out existing_list);
                existing_list.Add(db_field);
            }
            else
            {
                this.IV_usable_code_ids_lookup.Add(db_field.P_bus_data_identifier, 1);
                this.IV_code_id_to_fields.Add(db_field.P_bus_data_identifier,
                    new List<SofsafeDbField>() { db_field }
                );
            }
            continue;
            
            ///</end> register individual ISofafeDatabaseField class instances

        }
        return true;
    }

    public SofsafeDbSchema(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string schema_name, Type field_type, string activity_name_prefix = "")
    {
        this.IV_schema_name = schema_name;
        this.IV_fields_dict_type = field_type;
        this.registerDatabaseFields();

    }

}


