using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;





//TODO add to SBD
// project data identifiers
static class PDI
{
    public static string part_cost = "cost of part";
    public static string part_uid = "part uid";
    public static string part_name = "part name";
    public static string part_category_uid = "part category uid";
    public static string part_category_name = "part category name";
    public static string transaction_total = "total amount of any transaction";
    public static string transaction_type = "transaction type";
    public static string transaction_LAN_uid = "transaction uid on lan sql server";
    public static string transaction_cloud_uid = "transaction uid in azure";
    public static string recycler_location_uid = "recylcer location uid";
    public static string recycler_location_name = "recycler location name";
    public static string POS_computer_uid = "POS computer uid";
    public static string POS_computer_name = "POS computer name";
    public static string POS_user_uid = "uid of user logged into POS";
    public static string POS_username = "username of user logged into POS";
}
//project data constant identifiers
class PDCI {
    public static string transaction_type_purchase_parts = "Purchase parts";
    public static string transaction_type_return_parts = "Return parts";
    public static string transaction_type_purchase_gift_card = "Purchase gift card";
}


interface IOdcBaseClassInheritor
{
    bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);

}

class OrganizedDataClassBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected List<SofsafeDataObjectBaseClass> IV_Sdo_child_objects = new List<SofsafeDataObjectBaseClass>();
    protected List<OrganizedDataClassBaseClass> IV_Odc_child_objects = new List<OrganizedDataClassBaseClass>();
    protected List<IEnumerable<IOdcBaseClassInheritor>> IV_Odc_child_iterable_objects = new List<IEnumerable<IOdcBaseClassInheritor>>();
    protected List<IEnumerable<ISdoBaseClassInheritor>> IV_Sdo_child_iterable_objects = new List<IEnumerable<ISdoBaseClassInheritor>>();


    protected Dictionary<string, List<string>> IV_last_validation_result_details = new Dictionary<string, List<string>>();

    protected DateTime IV_last_validation_time = OrganizedDataClassBaseClass.C_unset_time;
    protected bool IV_last_validation_result = false;

    protected string organized_data_identifier;

    public string P_organized_data_identifier
    {
        get { return this.organized_data_identifier; }
    }

    public DateTime P_last_validation_attempt {
        get {return this.IV_last_validation_time; }
    }


    public bool P_last_validation_result
    {
        get {  return this.IV_last_validation_result;}
    }
    public Dictionary<string, List<string>> P_last_validation_result_details
    {
        get { return IV_last_validation_result_details; }
    }



    public bool validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("OrganizedDataClassBaseClass.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Organized Data Id", this.organized_data_identifier);
        
        
        //// check all individual Sdo objects 
        Dictionary<string,byte> duplicate_id_check = new Dictionary<string,byte>();
        List<string> duplicate_ids = new List<string>();
        Dictionary<string, List<string>> issues_found = new Dictionary<string, List<string>>();
        bool failed_flag = false;

        foreach (SofsafeDataObjectBaseClass sdo_instance in this.IV_Sdo_child_objects)
        {

            //this only happens when someone duplicated a data identifier in the same class, which they shouldn't do
            if (duplicate_id_check.ContainsKey(sdo_instance.P_data_identifier))
            {
                duplicate_ids.Add("duplicate data id: " + sdo_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }

            duplicate_id_check.Add(sdo_instance.P_data_identifier, 1);

            if (!sdo_instance.runValidationMethod(ot_helper,child_activity_pointer))
            {
                issues_found.Add(sdo_instance.P_data_identifier,
                sdo_instance.P_last_validation_result_details);
                failed_flag = true;
            }

        }
        ////</end> check all individual Sdo objects        


        //// check all Lists of Sdc child objects
        ////</end> check all Lists of Sdc child objects

        //// check all Odc child objects
        ////</end> check all Odc child objects

        //// check all Lists of Odc child objects
        
        //TODO work stopping point 06/14 5:39pm 
        foreach(OrganizedDataClassBaseClass child_class_list in this.IV_Odc_child_iterable_objects) { 
        
        }
        ////</end> check all Lists of Odc child objects





        if (failed_flag)
        {   
            if(duplicate_ids.Count > 0)
            {
                issues_found.Add("duplicate Data IDs",duplicate_ids);
            }

            this.IV_last_validation_result_details = issues_found;
            return false;
        }

        return true;
    }

    private void registerSdoOdcIndividualVariables()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields())
        {
            object field_value = class_type.GetField(field_entry.Name).GetValue(this);
            if (field_value == null) { continue; }

            /// register individual Sdo class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(ISdoBaseClassInheritor)))
            {
                this.IV_Sdo_child_objects.Add(field_value as SofsafeDataObjectBaseClass);
                continue;
            }
            ///</end> register individual Sdo class instances


            /// register individual Odc class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IOdcBaseClassInheritor)))
            {
                this.IV_Odc_child_objects.Add(field_value as OrganizedDataClassBaseClass);
                continue;
            }
        }
    }

    public OrganizedDataClassBaseClass(string organized_data_identifier)
    {
        this.organized_data_identifier = organized_data_identifier;
        this.registerSdoOdcIndividualVariables();
    }
}

class TransactionPartDetail: OrganizedDataClassBaseClass, IOdcBaseClassInheritor
{
    public SdoInt amount = new SdoInt(PDI.part_cost);
    public SdoInt part_uid = new SdoInt(PDI.part_uid);
    public SdoString part_name = new SdoString(PDI.part_name);
    public SdoInt part_category_uid = new SdoInt(PDI.part_category_uid);
    public SdoString part_category_name = new SdoString(PDI.part_category_name);

    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }

    public TransactionPartDetail(string organized_data_identifier): base(organized_data_identifier)
    {
        this.part_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.part_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.part_category_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.part_category_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
    }



}

class Transaction: OrganizedDataClassBaseClass, IOdcBaseClassInheritor
{

    public SdoString transaction_type = new SdoString(PDI.transaction_type);
    public SdoString transaction_cloud_uid = new SdoString(PDI.transaction_cloud_uid,true);
    public SdoInt transaction_lan_uid = new SdoInt(PDI.transaction_LAN_uid);

    public SdoInt total_amount = new SdoInt(PDI.transaction_total);
    public SdoInt location_uid = new SdoInt(PDI.recycler_location_uid);
    public SdoString location_name = new SdoString(PDI.recycler_location_name);
    public SdoInt POS_uid = new SdoInt(PDI.POS_computer_uid);
    public SdoString POS_name = new SdoString(PDI.POS_computer_name);
    public SdoInt user_uid = new SdoInt(PDI.POS_user_uid);
    public SdoString user_name = new SdoString(PDI.POS_username);
    public List<TransactionPartDetail> part_details = new List<TransactionPartDetail>();

    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }

    public Transaction(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, string transaction_type): base(organized_data_identifier)
    {
        Activity ETR = ot_helper.startActivityWrapper("SafeDataObjectString.addValidValue");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("transaction_type", transaction_type);

        List<object> transaction_types = new List<object>
        {
            PDCI.transaction_type_purchase_parts,
            PDCI.transaction_type_purchase_gift_card,
            PDCI.transaction_type_return_parts
        };

        this.transaction_type.addValidValues(ot_helper,child_activity_pointer, transaction_types);
        this.location_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.location_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.POS_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.POS_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.user_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.user_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);

        this.IV_Odc_child_iterable_objects.Add(this.part_details);
    }

}

