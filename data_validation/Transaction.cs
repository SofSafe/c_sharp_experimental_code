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

class TransactionPartDetail: SofsafeOrganizedDataClassBaseClass, IOdcBaseClassInheritor
{
    public SdoInt amount = new SdoInt(PDI.part_cost);
    public SdoInt part_uid = new SdoInt(PDI.part_uid);
    public SdoString part_name = new SdoString(PDI.part_name);
    public SdoInt part_category_uid = new SdoInt(PDI.part_category_uid);
    public SdoString part_category_name = new SdoString(PDI.part_category_name);
    public List<SdoString> notes = new List<SdoString>();

    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }

    public TransactionPartDetail(string organized_data_identifier,bool allow_disabling_instance=false): base(organized_data_identifier, allow_disabling_instance)
    {

        ///register iterable objects with Odc instances
        ///</end> register iterable objects with Odc instances

        ///register iterable objects with Sdc instances
        this.IV_Sdo_child_iterable_objects.Add(notes);
        ///</end>register iterable objects with Sdc instances

        this.amount.IV_validation_rules_list.Add(SDC.R_not_zero);
        this.part_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.part_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.part_category_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.part_category_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
    }



}

class Transaction: SofsafeOrganizedDataClassBaseClass, IOdcBaseClassInheritor
{

    public SdoString transaction_type = new SdoString(PDI.transaction_type);
    public SdoString transaction_cloud_uid = new SdoString(PDI.transaction_cloud_uid,null,null,true);
    public SdoInt transaction_lan_uid = new SdoInt(PDI.transaction_LAN_uid,null,null,true);

    public SdoInt total_amount = new SdoInt(PDI.transaction_total);
    public SdoInt location_uid = new SdoInt(PDI.recycler_location_uid);
    public SdoString location_name = new SdoString(PDI.recycler_location_name);
    public SdoInt POS_uid = new SdoInt(PDI.POS_computer_uid);
    public SdoString POS_name = new SdoString(PDI.POS_computer_name);
    public SdoInt user_uid = new SdoInt(PDI.POS_user_uid);
    public SdoString user_name = new SdoString(PDI.POS_username);
    public List<TransactionPartDetail> purchased_parts_details = new List<TransactionPartDetail>();
    public List<TransactionPartDetail> returned_parts_details = new List<TransactionPartDetail>();
    public TransactionPartDetail last_part_scanned = new TransactionPartDetail("last scanned part",true);


    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }

    public Transaction(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,
        string organized_data_identifier, string transaction_type,
        bool allow_disabling_instance=false) 
        : base(organized_data_identifier,allow_disabling_instance)
    {
        Activity ETR = ot_helper.startActivityWrapper("SafeDataObjectString.addValidValue");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("transaction_type", transaction_type);

        ///register iterable objects with Odc instances
        this.IV_Odc_child_iterable_objects.Add(this.purchased_parts_details);
        this.IV_Odc_child_iterable_objects.Add(this.returned_parts_details);
        ///</end> register iterable objects with Odc instances

        ///register iterable objects with Sdc instances
        ///</end>register iterable objects with Sdc instances



        /// set any validation rules needed
        List<string> transaction_types = new List<string>
        {
            PDCI.transaction_type_purchase_parts,
            PDCI.transaction_type_purchase_gift_card,
            PDCI.transaction_type_return_parts
        };


        this.transaction_type.addValidValues(transaction_types);

        this.transaction_type.P_value = transaction_type;

        this.total_amount.IV_validation_rules_list.Add(SDC.R_not_zero);
        this.location_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.location_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.POS_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.POS_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        this.user_uid.IV_validation_rules_list.Add(SDC.R_greater_0);
        this.user_name.IV_validation_rules_list.Add(SDC.R_len_greater_2);
        ///</end> set any validation rules needed




    }

}

