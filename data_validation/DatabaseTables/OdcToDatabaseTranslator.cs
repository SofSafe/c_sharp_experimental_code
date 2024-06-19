using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Reflection;




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
    private Dictionary<string, byte> IV_registered_single_entity_odc_ids = new Dictionary<string, byte>();
    private List<IOdcBaseClass> IV_single_entity_odc_instances = new List<IOdcBaseClass>();
    private Dictionary<string, byte> IV_registered_single_entity_table_names = new Dictionary<string, byte>();
    List<List<int>> IV_single_entity_mapping_index;
    private object IV_build_entity_map_thread_lock = new object();
    private List<SofsafeDatabaseTable> IV_single_entity_table_list = new List<SofsafeDatabaseTable>() { };

    
    protected Dictionary<string, byte> IV_multi_entity_ids = new Dictionary<string, byte>();
    private Dictionary<string, byte> IV_registered_multi_entity_tables = new Dictionary<string, byte> { };
    protected Dictionary<string, List<IEnumerable<IOdcBaseClassInheritor>>> IV_multi_entity_odc_lists = new Dictionary<string, List<IEnumerable<IOdcBaseClassInheritor>>>();
    private Dictionary<string, List<ISofsafeDatabaseTable>> IV_multi_entity_table_list = new Dictionary<string, List<ISofsafeDatabaseTable>> { };
    List<List<int>> IV_multi_entity_mapping_index;

    public delegate bool D_entity_populator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);

    private DateTime IV_last_mapped_fields = DateTime.MinValue;
    private DateTime IV_last_schema_change = DateTime.MinValue;
    private DateTime IV_last_db_to_data_object_transfer = DateTime.MinValue;
    private DateTime IV_last_data_object_to_db_transfer = DateTime.MinValue;


    public bool registerSingleEntityTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SofsafeDatabaseTable table_instance, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerSingleEntityTable");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }

        /// input validation
        if (table_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance is null", ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_registered_single_entity_table_names.ContainsKey(table_instance.P_schema_name))
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance with " + table_instance.P_schema_name + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation


        ETR.AddTag("table_instance data identifier", table_instance.P_schema_name);
        this.IV_registered_single_entity_table_names.Add(table_instance.P_schema_name, 1);
        this.IV_single_entity_table_list.Add(table_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }
    public bool registerSingleEntityOdc(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,IOdcBaseClass odc_instance, string activity_name_prefix = "")
    {
        if(ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default;}
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerOdcInstance");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        
        /// input validation
        if (odc_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "odc_instance is null", ErrorTagsCodes.C_input_validation);   
            return false;
        }
        if(this.IV_registered_single_entity_odc_ids.ContainsKey(odc_instance.P_organized_data_identifier))
        {
            OTSH.addEventThenEndActivity(ETR, "odc_instance with " + odc_instance.P_organized_data_identifier + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation


        ETR.AddTag("odc_instance data identifier", odc_instance.P_organized_data_identifier);
        this.IV_registered_single_entity_odc_ids.Add(odc_instance.P_organized_data_identifier, 1);
        this.IV_single_entity_odc_instances.Add(odc_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public bool registerMultiEntityJoin(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerMultiEntityJoin");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".registerListOfOdcInstances");
        validator.addEntry(new DataVR(multi_entity_id).addRule(SDC.R_len_greater_0));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_multi_entity_ids.ContainsKey(multi_entity_id))
        {
            OTSH.addEventThenEndActivity(ETR, "multi_entity_id: " + multi_entity_id + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation

        this.IV_multi_entity_ids.Add(multi_entity_id,1);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerMultiEntityDestinationPointer(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, ISofsafeDatabaseTable table_instance, string activity_name_prefix = "")
    {
    if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " > "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerMultiEntitySourcePointer");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".registerListOfOdcInstances");
        validator.addEntry(new DataVR(multi_entity_id, activity_name_prefix + "multi_entity_id").
            addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(table_instance, activity_name_prefix + "table_list").
            addRule(SDC.R_not_null));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_multi_entity_ids.ContainsKey(multi_entity_id) == false)
        {
            OTSH.addEventThenEndActivity(ETR, "multi entity join: " + multi_entity_id + " has not been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }

        /////make sure we aren't adding a duplicate reference
        if (this.IV_multi_entity_table_list.ContainsKey(multi_entity_id) == true)
        {
            foreach (ISofsafeDatabaseTable table_entity in this.IV_multi_entity_table_list[multi_entity_id])
            {
                if (table_instance.P_schema_name == table_entity.P_schema_name)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "an instance with the schema name: " + table_entity.P_schema_name + " has already been registered with multi entity id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }

                if (Object.ReferenceEquals(table_entity, table_instance) == true)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + table_entity.P_schema_name + " instance has already been registered with multi entity id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }

            }
            
        }
        /////</end>make sure we aren't adding a duplicate reference

        ///</end> input validation


        if (this.IV_multi_entity_table_list.ContainsKey(multi_entity_id) == false)
        {
            this.IV_multi_entity_table_list.Add(multi_entity_id, new List<ISofsafeDatabaseTable>() { });
        }
            this.IV_multi_entity_table_list[multi_entity_id].Add(table_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerMultiEntitySourcePointer(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, IEnumerable<IOdcBaseClassInheritor> odc_instances_lists, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " > "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerMultiEntitySourcePointer");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        /// input validation
        ArgumentValidator validator = new ArgumentValidator(this.GetType().Name + ".registerListOfOdcInstances");
        validator.addEntry(new DataVR(multi_entity_id, activity_name_prefix + "multi_entity_id").
            addRule(SDC.R_len_greater_0));
        validator.addEntry(new DataVR(odc_instances_lists, activity_name_prefix + "odc_instances_lists").
            addRule(SDC.R_len_greater_0).
            addRule(SDC.R_odc_instance_list));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_multi_entity_ids.ContainsKey(multi_entity_id) == false)
        {
            OTSH.addEventThenEndActivity(ETR, "multi entity join: " + multi_entity_id + " has not been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation

        if (this.IV_multi_entity_odc_lists.ContainsKey(multi_entity_id) == false)
        {
            this.IV_multi_entity_odc_lists.Add(multi_entity_id,new List<IEnumerable<IOdcBaseClassInheritor>>() { });
        }
        else
        {
            //make sure we aren't adding a duplicate reference
            foreach (object list_to_check in this.IV_multi_entity_odc_lists[multi_entity_id])
            {                 
                if(Object.ReferenceEquals(list_to_check, odc_instances_lists) == true)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "instance specified by odc_instances_lists has already been registered with multi entity id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }
            }
        }


        this.IV_multi_entity_odc_lists[multi_entity_id].Add(odc_instances_lists);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    private bool rebuildMap()
    {

        //// build map for single entity table to data associations
        List<List<int>> single_entity_map = new List<List<int>>();
        int odc_instance_index;
        int sdo_instance_index;
        int table_instance_index;
        odc_instance_index = -1;
        foreach (IOdcBaseClass odc_instance in this.IV_single_entity_odc_instances)
        {
            odc_instance_index++;
            sdo_instance_index = -1;
            foreach (ISdoBaseClass sdo_instance in odc_instance.P_Sdo_child_objects)
            {
                sdo_instance_index++;
                table_instance_index = -1;
                foreach (ISofsafeDatabaseTable table in this.IV_single_entity_table_list)
                {
                    table_instance_index++;
                    if (table.P_usable_code_ids_lookup.ContainsKey(sdo_instance.P_data_identifier) == true)
                    {
                        single_entity_map.Add(new List<int>() {
                            odc_instance_index, sdo_instance_index, table_instance_index 
                        });
                    }                    
                }
            }
        }

        lock (this.IV_build_entity_map_thread_lock)
        {
            if (this.IV_single_entity_mapping_index == null)
            {
                this.IV_single_entity_mapping_index = single_entity_map;
            }
            else
            {
                this.IV_single_entity_mapping_index.Clear();
                this.IV_single_entity_mapping_index.AddRange(single_entity_map);
            }
        }
        ////</end> build map for single entity table to data associations




        //// build map for multi entity table to data associations
        List<List<int>> multi_entity_map = new List<List<int>>();
        int odc_instance_list_index;
        foreach (string multi_id in this.IV_multi_entity_ids.Keys)
        {
            odc_instance_list_index = -1;
            foreach (IEnumerable<IOdcBaseClass> odc_instance_list in this.IV_multi_entity_odc_lists[multi_id])
            {
                odc_instance_list_index++;
                foreach (IOdcBaseClass odc_instance in odc_instance_list)
                {
                    odc_instance_index++;
                    sdo_instance_index = -1;
                    foreach (ISdoBaseClass sdo_instance in odc_instance.P_Sdo_child_objects)
                    {
                        sdo_instance_index++;
                        table_instance_index = -1;
                        foreach (ISofsafeDatabaseTable table_instance in this.IV_multi_entity_table_list[multi_id])
                        {
                            table_instance_index++;
                            if (table_instance.P_usable_code_ids_lookup.ContainsKey(sdo_instance.P_data_identifier) == true)
                            {
                                multi_entity_map.Add(new List<int>() {
                            odc_instance_list_index,odc_instance_index, sdo_instance_index, table_instance_index
                        });
                            }
                        }
                    }
                }

            }
        }
        
        lock (this.IV_build_entity_map_thread_lock)
        {
            if (this.IV_multi_entity_mapping_index == null)
            {
                this.IV_multi_entity_mapping_index = single_entity_map;
            }
            else
            {
                this.IV_multi_entity_mapping_index.Clear();
                this.IV_multi_entity_mapping_index.AddRange(multi_entity_map);
            }
        }
        ////</end> build map for multi entity table to data associations


        this.IV_last_mapped_fields = DateTime.Now;
        return true;
    }


    public bool populateDatabaseFields(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateDatabaseFields");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
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


        this.rebuildMap();
        /// put references to matching data objects in all the fields
        bool fail_flag = false;
        ChildActivityPointer first_failed_activity = new ChildActivityPointer();
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
                        if (fail_flag == true)
                        {
                            first_failed_activity = child_activity_pointer;
                            fail_flag = true;
                        }
                    }
                }
            }
        }
        ///</end> put references to matching data objects in all field objects
                
        if(fail_flag == true)
        {
            OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return false;
        }

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
            OTSH.passStatusAndEndActivity(ETR,child_activity_pointer.P_pointer);
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


            /// TODO add data validation for duplicate database field names
            /// 

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

    public SofsafeDatabaseTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string schema_name, string activity_name_prefix = "")
    {
        this.IV_schema_name = schema_name;
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

    public MsSqlDatabaseTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string schema_name, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, schema_name, activity_name_prefix)
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

    public SqlTransactionTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "Transactions", activity_name_prefix)
    {
    }
}

class SqlTransactionDetail : MsSqlDatabaseTable
{
    public static SofsafeDatabaseField category_uid = new SofsafeDatabaseField(PDI.part_category_uid, typeof(int), "cat_id");
    public static SofsafeDatabaseField cost = new SofsafeDatabaseField(PDI.part_cost, typeof(decimal), "cost");
    public static SofsafeDatabaseField item_id = new SofsafeDatabaseField(PDI.part_uid, typeof(int), "id");

    public SqlTransactionDetail(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "TransactionDetail", activity_name_prefix)
    {
    }
}
