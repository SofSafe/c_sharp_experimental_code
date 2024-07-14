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
using System.Runtime.CompilerServices;



class OdcToDatabaseTranslatorBase
{
    protected Dictionary<string, byte> IV_registered_single_entity_odc_ids = new Dictionary<string, byte>();
    protected List<IOdcBaseClass> IV_single_entity_odc_instances = new List<IOdcBaseClass>();
    protected Dictionary<string, byte> IV_registered_single_entity_schema_names = new Dictionary<string, byte>();
    protected List<List<int>> IV_single_entity_mapping_index;
    protected object IV_build_entity_map_thread_lock = new object();
    protected List<ISofsafeDbSchema> IV_single_entity_schema_list = new List<ISofsafeDbSchema>() { };


    protected Dictionary<string, byte> IV_multi_entity_ids = new Dictionary<string, byte>();
    protected Dictionary<string, List<ISofsafeDbSchema>> IV_multi_entity_schema_list = new Dictionary<string, List<ISofsafeDbSchema>> { };

    protected Dictionary<string, List<IEnumerable<IOdcBaseClassInheritor>>> IV_multi_entity_odc_lists = new Dictionary<string, List<IEnumerable<IOdcBaseClassInheritor>>>();
    protected Dictionary<string, List<List<int>>> IV_multi_entity_odcs_to_schemas_map = new Dictionary<string, List<List<int>>>() { };
    

    protected Dictionary<string, List<ISdoBaseClass>> IV_multi_entity_sdo_instances = new Dictionary<string, List<ISdoBaseClass>>();

    protected Dictionary<string, List<List<int>>> IV_multi_entity_sdos_to_schemas_map = new Dictionary<string, List<List<int>>>() { };


    public delegate bool D_entity_populator(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    protected DateTime IV_last_mapped_fields_time = DateTime.MinValue;
    protected DateTime IV_last_schema_change_time = DateTime.MinValue;
    protected DateTime IV_last_db_to_data_object_transfer = DateTime.MinValue;
    protected DateTime IV_last_data_object_to_db_transfer = DateTime.MinValue;


    public bool registerSingleEntitySchema(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SofsafeDbSchema schema_instance, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerSingleEntityTable");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }

        /// input validation
        if (schema_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance is null", ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_registered_single_entity_schema_names.ContainsKey(schema_instance.P_schema_name))
        {
            OTSH.addEventThenEndActivity(ETR, "table_instance with " + schema_instance.P_schema_name + " has already been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation


        ETR.AddTag("table_instance data identifier", schema_instance.P_schema_name);
        this.IV_registered_single_entity_schema_names.Add(schema_instance.P_schema_name, 1);
        this.IV_single_entity_schema_list.Add(schema_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }
    public bool registerSingleEntityOdc(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, IOdcBaseClass odc_instance, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerSingleEntityOdc");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }

        /// input validation
        if (odc_instance == null)
        {
            OTSH.addEventThenEndActivity(ETR, "odc_instance is null", ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_registered_single_entity_odc_ids.ContainsKey(odc_instance.P_organized_data_identifier))
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


    public bool registerSdoWithMultiEntityAssociation(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, ISdoBaseClass sdo_instance_to_add, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerSdoWithMultiEntityAssociation");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        /// input validation
        if (sdo_instance_to_add == null)
        {
            OTSH.addEventThenEndActivity(ETR, "sdo_instance is null", ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_multi_entity_ids.ContainsKey(multi_entity_id) == false)
        {
            OTSH.addEventThenEndActivity(ETR, "multi entity association: " + multi_entity_id + " has not been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }

        /////make sure we aren't adding a duplicate reference
        if (this.IV_multi_entity_sdo_instances.ContainsKey(multi_entity_id) == true)
        {
            foreach (ISdoBaseClass existing_sdo in this.IV_multi_entity_sdo_instances[multi_entity_id])
            {
                if (existing_sdo.P_data_identifier == sdo_instance_to_add.P_data_identifier)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "an instance with the schema name: " + existing_sdo.P_data_identifier + " has already been registered with multi entity id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }

                if (Object.ReferenceEquals(existing_sdo, sdo_instance_to_add) == true)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + existing_sdo.P_data_identifier + " instance has already been registered with multi entity id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }

            }

        }
        /////</end>make sure we aren't adding a duplicate reference

        ///</end> input validation

        if (this.IV_multi_entity_sdo_instances.ContainsKey(multi_entity_id) == false)
        {
            this.IV_multi_entity_sdo_instances.Add(multi_entity_id, new List<ISdoBaseClass>() { });
        }
        this.IV_multi_entity_sdo_instances[multi_entity_id].Add(sdo_instance_to_add);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool createMultiEntityAssociation(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".registerMultiEntityAssociation");
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

        this.IV_multi_entity_ids.Add(multi_entity_id, 1);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerMultiEntityTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, ISofsafeDbSchema table_instance, string activity_name_prefix = "")
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
        validator.addEntry(new DataVR(table_instance, activity_name_prefix + "table_entity").
            addRule(SDC.R_not_null));
        if (validator.validateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }
        if (this.IV_multi_entity_ids.ContainsKey(multi_entity_id) == false)
        {
            OTSH.addEventThenEndActivity(ETR, "multi entity association: " + multi_entity_id + " has not been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }

        /////make sure we aren't adding a duplicate reference
        if (this.IV_multi_entity_schema_list.ContainsKey(multi_entity_id) == true)
        {
            foreach (ISofsafeDbSchema table_entity in this.IV_multi_entity_schema_list[multi_entity_id])
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


        if (this.IV_multi_entity_schema_list.ContainsKey(multi_entity_id) == false)
        {
            this.IV_multi_entity_schema_list.Add(multi_entity_id, new List<ISofsafeDbSchema>() { });
        }
        this.IV_multi_entity_schema_list[multi_entity_id].Add(table_instance);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool registerMultiEntityOdcInstanceList(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string multi_entity_id, IEnumerable<IOdcBaseClassInheritor> odc_instances_lists, string activity_name_prefix = "")
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
            OTSH.addEventThenEndActivity(ETR, "multi entity association: " + multi_entity_id + " has not been registered", ErrorTagsCodes.C_input_validation);
            return false;
        }
        ///</end> input validation

        if (this.IV_multi_entity_odc_lists.ContainsKey(multi_entity_id) == false)
        {
            this.IV_multi_entity_odc_lists.Add(multi_entity_id, new List<IEnumerable<IOdcBaseClassInheritor>>() { });
        }
        else
        {
            //make sure we aren't adding a duplicate reference
            foreach (object list_to_check in this.IV_multi_entity_odc_lists[multi_entity_id])
            {
                if (Object.ReferenceEquals(list_to_check, odc_instances_lists) == true)
                {
                    OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "instance specified by odc_instances_lists has already been registered with multi entity association id: " + multi_entity_id + " before", ErrorTagsCodes.C_input_validation);
                    return false;
                }
            }
        }


        this.IV_multi_entity_odc_lists[multi_entity_id].Add(odc_instances_lists);
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    protected bool rebuildMap()
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
                foreach (ISofsafeDbSchema table in this.IV_single_entity_schema_list)
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





        //// build map for multi entity table to sdo data associations
        int sdo_instance_list_index;
        List<List<int>> multi_entity_sdo_list_map = new List<List<int>>();

        foreach (string multi_id in this.IV_multi_entity_ids.Keys)
        {
            if(this.IV_multi_entity_sdo_instances.ContainsKey(multi_id) == false )
            {
                continue;
            }
            sdo_instance_list_index = -1;
            foreach (ISdoBaseClass sdo_instance in this.IV_multi_entity_sdo_instances[multi_id])
            {
                sdo_instance_list_index++;
                sdo_instance_index = -1;

                sdo_instance_index++;
                table_instance_index = -1;
                foreach (ISofsafeDbSchema table_instance in this.IV_multi_entity_schema_list[multi_id])
                {
                    table_instance_index++;
                    if (table_instance.P_usable_code_ids_lookup.ContainsKey(sdo_instance.P_data_identifier) == true)
                    {
                        multi_entity_sdo_list_map.Add(new List<int>() {
                        sdo_instance_list_index,sdo_instance_index, table_instance_index
                    });
                    }
                }                
            }
            lock (this.IV_build_entity_map_thread_lock)
            {
                this.IV_multi_entity_sdos_to_schemas_map[multi_id] = multi_entity_sdo_list_map;
            }
        }       
        //// build map for multi entity table to sdo data associations


        //// build map for multi entity table to odc list data associations
        List<List<int>> multi_entity_odc_list_map = new List<List<int>>();
        int odc_instance_list_index;
        foreach (string multi_id in this.IV_multi_entity_ids.Keys)
        {
            if (this.IV_multi_entity_odc_lists.ContainsKey(multi_id) == false)
            {
                continue;
            }
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
                        foreach (ISofsafeDbSchema table_instance in this.IV_multi_entity_schema_list[multi_id])
                        {
                            table_instance_index++;
                            if (table_instance.P_usable_code_ids_lookup.ContainsKey(sdo_instance.P_data_identifier) == true)
                            {
                                multi_entity_odc_list_map.Add(new List<int>() {
                            odc_instance_list_index,odc_instance_index, sdo_instance_index, table_instance_index
                        });
                            }
                        }
                    }
                }

            }
            lock (this.IV_build_entity_map_thread_lock)
            {
                this.IV_multi_entity_odcs_to_schemas_map[multi_id] = multi_entity_odc_list_map;
            }
        }

        ////</end> build map for multi entity table to odc list data associations


        this.IV_last_mapped_fields_time = DateTime.Now;
        return true;
    }
   


   
}


class OdcToDatabaseTranslator : OdcToDatabaseTranslatorBase
{
    public bool populateDatabaseFields(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".populateDatabaseFields");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        List<Activity> method_activity_list = new List<Activity>();


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


        if (this.IV_last_mapped_fields_time == DateTime.MinValue ||
            this.IV_last_mapped_fields_time < this.IV_last_schema_change_time)
        {
            //todo figure out a way to detect changes in odc_instance_lists that would require a rebuild
            //todo add observability to rebuildMap
            this.rebuildMap();
        }

        /// for single entity data associations put references to data objects in the database fields they match
        bool fail_flag = false;
        ChildActivityPointer first_failed_activity = new ChildActivityPointer();


        foreach (List<int> index_list in this.IV_single_entity_mapping_index)
        {
            if (this.IV_single_entity_odc_instances[index_list[0]].P_Sdo_child_objects[index_list[1]].P_value_not_used == true) { continue; }

            if (this.IV_single_entity_schema_list[index_list[2]].assignSdoToRecordField(ot_helper, child_activity_pointer,
            this.IV_single_entity_odc_instances[index_list[0]].P_Sdo_child_objects[index_list[1]]) == false)
            {
                if (fail_flag == true)
                {
                    first_failed_activity = child_activity_pointer;
                    fail_flag = true;
                }
            }
        }
        ///</end> for single entities put references to data objects in the database fields they match



        /// for multi entity data associations put the sdo instance many to one associations in first, this way any specific odc_instnaces can overwrite them as if they match too
        int record_position;
        foreach (string multi_id in this.IV_multi_entity_sdo_instances.Keys)
        {
            foreach (List<int> idx in this.IV_multi_entity_sdos_to_schemas_map[multi_id])
            {
                ISofsafeDbSchema table = this.IV_multi_entity_schema_list[multi_id][idx[2]];
                record_position = -1;
                ISdoBaseClass sdo_instance = this.IV_multi_entity_sdo_instances[multi_id][idx[0]];
                if (sdo_instance.P_value_not_used == true) { continue; }


                record_position++;
                //Console.WriteLine(String.Join(",", idx));
                //Console.WriteLine("sdo_instance:" + sdo_instance.P_data_identifier);
                //bug I'm pretty sure its calling this twice, but the record_position is preventing it from duplicating data.
                if (table.assignSdoToAllFieldRecords(ot_helper, child_activity_pointer,
                    sdo_instance) == false)
                {
                    if (fail_flag == true)
                    {
                        first_failed_activity = child_activity_pointer;
                        fail_flag = true;
                    }
                }
            }
        }
        ///</end> for multi entity data associations put the sdo instance many to one associations in first, this way any specific odc_instnaces can overwrite them as if they match too



        /// for multiple entity data associations put references to lists of data objects in the database fields they match
        foreach (string multi_id in this.IV_multi_entity_odcs_to_schemas_map.Keys)
        {
            foreach (List<int> idx in this.IV_multi_entity_odcs_to_schemas_map[multi_id])
            {
                ISofsafeDbSchema table = this.IV_multi_entity_schema_list[multi_id][idx[3]];
                record_position = -1;
                foreach (IOdcBaseClass odc_instance in this.IV_multi_entity_odc_lists[multi_id][idx[0]])
                {
                    if (odc_instance.P_Sdo_child_objects[idx[2]].P_value_not_used == true) { continue; }
                    record_position++;
                    //bug I'm pretty sure its calling this twice, but the record_position is preventing it from duplicating data.
                    if (table.assignSdoToRecordField(ot_helper, child_activity_pointer,
                        odc_instance.P_Sdo_child_objects[idx[2]], record_position) == false)
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
        /// for multiple entity data associations put references to lists of data objects in the database fields they match

        //// tell table instances to move data from sdo instances to native database instance types

        foreach (string multi_id in this.IV_multi_entity_odcs_to_schemas_map.Keys)
        {
            foreach (ISofsafeDbSchemaInheritor table in this.IV_multi_entity_schema_list[multi_id])
            {
                table.populateNativeDbFieldsFromDataObjects(ot_helper, child_activity_pointer);
                method_activity_list.Add(child_activity_pointer.P_pointer);

            }
        }
        foreach (ISofsafeDbSchemaInheritor table in this.IV_single_entity_schema_list)
        {
            table.populateNativeDbFieldsFromDataObjects(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);
        }

        ChildActivityPointer activity_list_status;
        if (OTSH.checkActivityList(ETR, method_activity_list, out activity_list_status) == false)
        {
            OTSH.passStatusAndEndActivity(ETR, activity_list_status.P_pointer);
            return false;
        }
        method_activity_list.Clear();
        ////</end> tell table instances to move data from sdo instances to native database instance types


        if (fail_flag == true)
        {
            OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return false;
        }

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }
}

class DatabaseToOdcTranslator: OdcToDatabaseTranslatorBase {

    public bool populateEmptySdoInstances(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + "main" + ".populatePurchaseObject");
        if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template


        //todo add observability to rebuildMap
        this.rebuildMap();        

        /// for single entity data associations put references to data objects in the database fields they match
        bool fail_flag = false;
        ChildActivityPointer first_failed_activity = new ChildActivityPointer();


        foreach (List<int> index_list in this.IV_single_entity_mapping_index)
        {
            if (this.IV_single_entity_odc_instances[index_list[0]].P_Sdo_child_objects[index_list[1]].P_value_not_used == true) { continue; }

            if (this.IV_single_entity_schema_list[index_list[2]].assignSdoToRecordField(ot_helper, child_activity_pointer,
            this.IV_single_entity_odc_instances[index_list[0]].P_Sdo_child_objects[index_list[1]]) == false)
            {
                if (fail_flag == true)
                {
                    first_failed_activity = child_activity_pointer;
                    fail_flag = true;
                }
            }
        }
        ///</end> for single entities put references to data objects in the database fields they match       



        /// for multiple entity data associations put references to lists of data objects in the database fields they match
        int record_position;
        foreach (string multi_id in this.IV_multi_entity_odcs_to_schemas_map.Keys)
        {
            foreach (List<int> idx in this.IV_multi_entity_odcs_to_schemas_map[multi_id])
            {
                ISofsafeDbSchema table = this.IV_multi_entity_schema_list[multi_id][idx[3]];
                record_position = -1;
                foreach (IOdcBaseClass odc_instance in this.IV_multi_entity_odc_lists[multi_id][idx[0]])
                {
                    if (odc_instance.P_Sdo_child_objects[idx[2]].P_value_not_used == true) { continue; }
                    record_position++;
                    //bug I'm pretty sure its calling this twice, but the record_position is preventing it from duplicating data.
                    //Console.WriteLine(String.Join(",", idx));
                    //Console.WriteLine("sdo_instance:" + odc_instance.P_Sdo_child_objects[idx[2]].P_data_identifier);
                    odc_instance.P_Sdo_child_objects[idx[2]].clearValue();
                    if (table.assignEmptySdoToRecordField(ot_helper, child_activity_pointer,
                        odc_instance.P_Sdo_child_objects[idx[2]], record_position) == false)
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
        /// for multiple entity data associations put references to lists of data objects in the database fields they match


        if (fail_flag == true)
        {
            OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
            return false;
        }

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

}