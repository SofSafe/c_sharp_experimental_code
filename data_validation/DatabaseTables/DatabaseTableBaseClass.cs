using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



class DatabaseFieldMap
{
    protected string IV_code_data_identifier = "";
    protected Type IV_code_type = null;
    protected string IV_database_field_name = "";
    private ISdoBaseClass IV_sdo_instance = null;

    public string P_code_data_identifier
    {
        get { return IV_code_data_identifier;}
    }


    public void setSdoInstance(ISdoBaseClass sdo_instance)
    {
        this.IV_sdo_instance = sdo_instance;        
    }

    public DatabaseFieldMap(string code_data_identifier, Type code_type, string database_field_name)
    {
        this.IV_database_field_name = database_field_name;
        this.IV_code_data_identifier = code_data_identifier;
        this.IV_code_type = code_type;
    }

}

class DatabaseTableBaseClass
{
    protected List<DatabaseFieldMap> IV_field_list = new List<DatabaseFieldMap>() { };

    public bool addOdcList(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, List<IOdcBaseClass> odc_instances_list, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".addOdcList");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        foreach (IOdcBaseClass odc_instance in odc_instances_list)
        {
            if(this.addOdcInstance(ot_helper, child_activity_pointer, odc_instance, activity_name_prefix)== false)
            {
                OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_child_activity_pointer);
                return false;
            }
        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public bool addOdcInstance(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, IOdcBaseClass data_class, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".addOdcInstance");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();


        if (data_class.validateData(ot_helper, child_activity_pointer) == false) {
            OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_child_activity_pointer);
            return false; 
        }

        foreach(DatabaseFieldMap field_entry in this.IV_field_list)
        {
            bool field_found = false;

            foreach(ISdoBaseClass sdo_instance in data_class.P_Sdo_child_objects )
            {
                if(sdo_instance.P_data_identifier == field_entry.P_code_data_identifier)
                {
                    Console.WriteLine(sdo_instance.P_data_identifier);
                    field_entry.setSdoInstance(sdo_instance);
                    field_found = true;
                    break;
                }
            }            
            if (field_found == true) { continue; }

            foreach (IOdcBaseClass odc_instance in data_class.P_odc_child_objects)
            {
                foreach (ISdoBaseClass sdo_instance in odc_instance.P_Sdo_child_objects)
                {
                    if (sdo_instance.P_data_identifier == field_entry.P_code_data_identifier)
                    {
                        Console.WriteLine(sdo_instance.P_data_identifier);      
                        field_entry.setSdoInstance(sdo_instance);
                        field_found = true;
                        break;
                    }
                }
                if (field_found == true) {break; }
            }            

        }
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

}


class TransactionTable : DatabaseTableBaseClass
{
    public static DatabaseFieldMap lan_sql_uid = new DatabaseFieldMap(PDI.transaction_LAN_uid, typeof(int), "lan_uid");
    public static DatabaseFieldMap returned_part = new DatabaseFieldMap(PDI.part_uid, typeof(int), "last_part_uid");
    
    public TransactionTable()
    {
        this.IV_field_list.Add(TransactionTable.lan_sql_uid);
        this.IV_field_list.Add(TransactionTable.returned_part);
    }
}