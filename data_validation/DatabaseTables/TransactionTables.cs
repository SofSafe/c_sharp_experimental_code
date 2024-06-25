using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


class SqlTransactionTable : MsSql2012DbTable
{
    public static SofsafeMsSqlDbField lan_sql_uid = new SofsafeMsSqlDbField(0,PDI.transaction_LAN_uid,
        typeof(int), "lan_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField total = new SofsafeMsSqlDbField(1,PDI.transaction_total, typeof(decimal),
        "total", SqlDbType.Money, 8);
    public static SofsafeMsSqlDbField pos_uid = new SofsafeMsSqlDbField(2,PDI.POS_computer_uid, typeof(int),
        "pos_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField user_uid = new SofsafeMsSqlDbField(3,PDI.POS_user_uid, typeof(int),
        "user_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField time = new SofsafeMsSqlDbField(4,PDI.transaction_time, typeof(DateTime),
        "timestamp", SqlDbType.DateTime, 8);

    public SqlTransactionTable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "Transactions", activity_name_prefix)
    {
    }
}

class SqlTransactionDetail : MsSql2012DbTable
{
    public static SofsafeMsSqlDbField category_uid = new SofsafeMsSqlDbField(0,PDI.part_category_uid, typeof(int), "category_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField item_id = new SofsafeMsSqlDbField(1, PDI.part_uid, typeof(int), "part_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField cost = new SofsafeMsSqlDbField(2, PDI.part_cost, typeof(decimal), "value", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField transaction_uid = new SofsafeMsSqlDbField(3, PDI.transaction_LAN_uid, typeof(int), "transaction_uid", SqlDbType.BigInt, 8);
    public static SofsafeMsSqlDbField return_id = new SofsafeMsSqlDbField(4, PDI.return_uid, typeof(int), "return_uid", SqlDbType.BigInt, 8);    


    public SqlTransactionDetail(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "TransactionDetail", activity_name_prefix)
    {
    }
}
