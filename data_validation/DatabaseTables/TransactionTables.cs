using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;



class DboStoreUsers: MsSqlDbTable
{
    public static SofsafeMsSqlDbField UserRno = new SofsafeMsSqlDbField(0, "UserRno",
        typeof(int), "UserRno", SqlDbType.Int, 4, null, true);
    
    public static SofsafeMsSqlDbField CmsnEligibleFlg = new SofsafeMsSqlDbField(0, "CmsnEligibleFlg", typeof(bool), "CmsnEligibleFlg", SqlDbType.Bit, 1);

    public static SofsafeMsSqlDbField AfterHrsLoginFlg = new SofsafeMsSqlDbField(0, "AfterHrsLoginFlg", typeof(int), "AfterHrsLoginFlg", SqlDbType.Int, 4);

    public static SofsafeMsSqlDbField MaxSimultLogins = new SofsafeMsSqlDbField(0, "MaxSimultLogins", typeof(int), "MaxSimultLogins", SqlDbType.SmallInt, 2);

    //this shouldn't be replicated, our automated tool class generation tool needs to handle fields that shouldn't be included
    //public static SofsafeMsSqlDbField PosPassword = new SofsafeMsSqlDbField(0, "PosPassword", typeof(int), "PosPassword", SqlDbType.BigInt, 8, null, true, true);

    public static SofsafeMsSqlDbField IntNote = new SofsafeMsSqlDbField(0, "IntNote", typeof(string), "IntNote", SqlDbType.VarChar, 2000);

    public static SofsafeMsSqlDbField CreDtTm = new SofsafeMsSqlDbField(0, "CreDtTm", typeof(DateTime), "CreDtTm", SqlDbType.DateTime, 8);

    public static SofsafeMsSqlDbField CreUsrRno = new SofsafeMsSqlDbField(0, "CreUsrRno", typeof(int), "CreUsrRno", SqlDbType.Int, 4);

    public static SofsafeMsSqlDbField CreStnRno = new SofsafeMsSqlDbField(0, "CreStnRno", typeof(int), "CreStnRno", SqlDbType.Int, 4);

    public static SofsafeMsSqlDbField ModDtTm = new SofsafeMsSqlDbField(0, "ModDtTm", typeof(DateTime), "ModDtTm", SqlDbType.DateTime, 8);

    public static SofsafeMsSqlDbField ModUsrRno = new SofsafeMsSqlDbField(0, "ModUsrRno", typeof(int), "ModUsrRno", SqlDbType.Int, 4);

    public static SofsafeMsSqlDbField ModStnRno = new SofsafeMsSqlDbField(0, "ModStnRno", typeof(int), "ModStnRno", SqlDbType.Int, 4);

    public DboStoreUsers(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "dbo.StoreUsers", activity_name_prefix)
    {
    }
}

class s3TransactionTable : MsSqlDbTable
{
    public static SofsafeMsSqlDbField lan_sql_uid = new SofsafeMsSqlDbField(0,PDI.transaction_LAN_uid,
        typeof(int), "uid", SqlDbType.BigInt, 8,null,true,true);
    public static List<SDR> rule_lan_sql_uid = new List<SDR>() { SDC.R_greater_0 };

    public static List<string> valid_values_transaction_types = new List<string> {
            PDCI.transaction_type_purchase_parts,
            PDCI.transaction_type_purchase_gift_card,
            PDCI.transaction_type_return_parts
            };

public static SofsafeMsSqlDbField total = new SofsafeMsSqlDbField(1,PDI.transaction_total, typeof(decimal),
        "total", SqlDbType.Money, 8);
    public static SofsafeMsSqlDbField pos_uid = new SofsafeMsSqlDbField(2,PDI.POS_computer_uid, typeof(int),
        "pos_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField user_uid = new SofsafeMsSqlDbField(3,PDI.POS_user_uid, typeof(int),
        "user_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField time = new SofsafeMsSqlDbField(4,PDI.transaction_time, typeof(DateTime),
        "timestamp", SqlDbType.DateTime, 8);

    public s3TransactionTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "Transactions", activity_name_prefix)
    {
    }
}

class s3TransactionDetailTable : MsSqlDbTable
{
    public static SofsafeMsSqlDbField category_uid = new SofsafeMsSqlDbField(0,PDI.part_category_uid, typeof(int), "category_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField item_id = new SofsafeMsSqlDbField(1, PDI.part_uid, typeof(int), "part_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField cost = new SofsafeMsSqlDbField(2, PDI.part_cost, typeof(decimal), "value", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField transaction_uid = new SofsafeMsSqlDbField(3, PDI.transaction_LAN_uid, typeof(int), "transaction_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField return_id = new SofsafeMsSqlDbField(4, PDI.return_uid, typeof(int), "return_uid", SqlDbType.BigInt, 8);    


    public s3TransactionDetailTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "TransactionDetail", activity_name_prefix)
    {
    }
}



class s3CategoriesTable : MsSqlDbTable
{
    public static SofsafeMsSqlDbField uid = new SofsafeMsSqlDbField(0, PDI.part_category_uid,
        typeof(int), "uid", SqlDbType.BigInt, 8, null, true, true);
    public static SofsafeMsSqlDbField name = new SofsafeMsSqlDbField(1, PDI.part_category_name, typeof(string),
        "name", SqlDbType.VarChar, 100);    

    public s3CategoriesTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "Categories", activity_name_prefix)
    {
    }
}
