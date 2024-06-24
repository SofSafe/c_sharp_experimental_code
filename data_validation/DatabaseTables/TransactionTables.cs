using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


class SqlTransactionTable : MsSql2012DbTable
{
    public static SofsafeMsSqlDbField lan_sql_uid = new SofsafeMsSqlDbField(PDI.transaction_LAN_uid,
        typeof(int), "lan_uid",0, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField total = new SofsafeMsSqlDbField(PDI.transaction_total, typeof(decimal),
        "total", 1, SqlDbType.Money, 8);
    public static SofsafeMsSqlDbField pos_uid = new SofsafeMsSqlDbField(PDI.POS_computer_uid, typeof(int),
        "pos_uid", 2, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField user_uid = new SofsafeMsSqlDbField(PDI.POS_user_uid, typeof(int),
        "user_uid", 3, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField time = new SofsafeMsSqlDbField(PDI.transaction_time, typeof(DateTime),
        "timestamp", 4, SqlDbType.DateTime, 8);

    public SqlTransactionTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "Transactions", activity_name_prefix)
    {
    }
}

class SqlTransactionDetail : MsSql2012DbTable
{
    public static SofsafeMsSqlDbField category_uid = new SofsafeMsSqlDbField(PDI.part_category_uid, typeof(int), "category_uid",0, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField item_id = new SofsafeMsSqlDbField(PDI.part_uid, typeof(int), "part_uid",1, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField cost = new SofsafeMsSqlDbField(PDI.part_cost, typeof(decimal), "value", 2, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField transaction_uid = new SofsafeMsSqlDbField(PDI.transaction_LAN_uid, typeof(int), "transcation_uid",3, SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField return_id = new SofsafeMsSqlDbField(PDI.return_uid, typeof(int), "return_uid", 4, SqlDbType.BigInt, 8);
    


    public SqlTransactionDetail(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "TransactionDetail", activity_name_prefix)
    {
    }
}
