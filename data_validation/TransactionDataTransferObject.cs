using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;




//proejct common rules
static class PCR
{
    public static List<SDR> uid = new List<SDR>() { SDC.R_greater_0 };
    public static List<SDR> not_zero = new List<SDR>() { SDC.R_not_zero };
    public static List<SDR> name = new List<SDR>() { SDC.R_len_greater_2 };
}


//TODO add to SBD
// project data identifiers
static class PDI
{
    public static string part_cost = "cost of part";
    public static string part_uid = "part uid";
    public static string part_name = "part name";
    public static string part_detail_lan_uid = "part detail uid on local sql server";    
    public static string part_category_uid = "part category uid";
    public static string part_category_name = "part category name";
    public static string transaction_total = "total amount of any transaction";
    public static string transaction_type = "transaction type";
    public static string transaction_LAN_uid = "transaction uid on lan sql server";
    public static string transaction_cloud_uid = "transaction uid in azure";
    public static string transaction_time = "date and time transaction occured";
    public static string recycler_location_uid = "recylcer location uid";
    public static string recycler_location_name = "recycler location name";
    public static string POS_computer_uid = "POS computer uid";
    public static string POS_computer_name = "POS computer name";
    public static string POS_user_uid = "uid of user logged into POS";
    public static string POS_username = "username of user logged into POS";
    public static string return_uid = "return uid on lan sql server";
}
//project data constant identifiers
class PDCI {
    public static string transaction_type_purchase_parts = "Purchase parts";
    public static string transaction_type_return_parts = "Return parts";
    public static string transaction_type_purchase_gift_card = "Purchase gift card";
}

class TransactionDtoPartDetail: SofsafeOrganizedDataBaseClass, IOdcBaseClassInheritor
{
    public Sdo<decimal> amount = new Sdo<decimal>(PDI.part_cost,PCR.not_zero);
    public Sdo<int> part_uid = new Sdo<int>(PDI.part_uid, PCR.uid);
    public Sdo<string> part_name = new Sdo<string>(PDI.part_name,PCR.name);
    public Sdo<int> part_category_uid = new Sdo<int>(PDI.part_category_uid, PCR.uid);
    public Sdo<string> part_category_name = new Sdo<string>(PDI.part_category_name,PCR.name);
    public Sdo<int> transaction_uid = new Sdo<int>(PDI.transaction_LAN_uid,PCR.uid);
    public List<Sdo<string>> notes = new List<Sdo<string>>();

    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }


    public TransactionDtoPartDetail(string organized_data_identifier,       
       bool allow_disabling_instance = false) : base(organized_data_identifier, allow_disabling_instance)
    {

        ///register iterable objects with Odc instances
        ///</end> register iterable objects with Odc instances

        ///register iterable objects that contain only Sdo instances
        this.IV_named_sdo_lists.Add("notes", notes);
        ///</end>register iterable objects that contain only Sdo instances
    }



}

class TransactionDto: SofsafeOrganizedDataBaseClass, IOdcBaseClassInheritor
{

    public Sdo<string> transaction_type = new Sdo<string>(PDI.transaction_type,null,
        new List<string> {
            PDCI.transaction_type_purchase_parts,
            PDCI.transaction_type_purchase_gift_card,
            PDCI.transaction_type_return_parts 
        }); 
    public Sdo<string> transaction_cloud_uid = new Sdo<string>(PDI.transaction_cloud_uid, PCR.uid, null, true);
    public Sdo<int> transaction_lan_uid = new Sdo<int>(PDI.transaction_LAN_uid,PCR.uid,null,true);
    public Sdo<decimal> total_amount = new Sdo<decimal>(PDI.transaction_total, PCR.not_zero);
    public Sdo<int> location_uid = new Sdo<int>(PDI.recycler_location_uid, PCR.uid);
    public Sdo<string> location_name = new Sdo<string>(PDI.recycler_location_name, PCR.name);
    public Sdo<int> POS_uid = new Sdo<int>(PDI.POS_computer_uid, PCR.uid);
    public Sdo<string> POS_name = new Sdo<string>(PDI.POS_computer_name, PCR.name);
    public Sdo<int> user_uid = new Sdo<int>(PDI.POS_user_uid, PCR.uid);
    public Sdo<string> user_name = new Sdo<string>(PDI.POS_username, PCR.name);
    public List<TransactionDtoPartDetail> purchased_parts_details = new List<TransactionDtoPartDetail>();
    public List<TransactionDtoPartDetail> returned_parts_details = new List<TransactionDtoPartDetail>();
    public TransactionDtoPartDetail last_part_scanned = new TransactionDtoPartDetail("last scanned part",true);

    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        return true;
    }

    public TransactionDto(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,
        string organized_data_identifier, string transaction_type,
        bool allow_disabling_instance=false) 
        : base(organized_data_identifier,allow_disabling_instance)
    {
        Activity ETR = ot_helper.startActivityWrapper("TransactionDto.init");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("transaction_type", transaction_type);

        this.transaction_type.P_value = transaction_type;


        ///register iterable objects that contain only Odc instances
        this.IV_named_odc_lists.Add("purchased_parts_details", this.purchased_parts_details);
        this.IV_named_odc_lists.Add("returned_parts_details", this.returned_parts_details);
        ///</end> register iterable objects that contain only Odc instances

        ///register iterable objects that contain only Sdo instances
        ///</end>register iterable objects that contain only Sdo instances


    }

}