using System;
using System.Data;
using Microsoft.Data.SqlClient;

class LocalDboStoreUsers : MsSqlDbTable
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

    public LocalDboStoreUsers(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "dbo.StoreUsers", activity_name_prefix)
    {
    }
}

class RemoteDboStoreUsers : MsSqlDbTable
{
    public static SofsafeMsSqlDbField replication_uid = new SofsafeMsSqlDbField(0, "replication_uid",
    typeof(int), "replication_uid", SqlDbType.Int, 4, null, true,true);

    public static SofsafeMsSqlDbField ClientId = new SofsafeMsSqlDbField(0, "ClientId",
        typeof(int), "ClientId", SqlDbType.SmallInt, 2, null, true);

    public static SofsafeMsSqlDbField UserRno = new SofsafeMsSqlDbField(0, "UserRno",
        typeof(int), "UserRno", SqlDbType.Int, 4);

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

    public RemoteDboStoreUsers(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "dbo.StoreUsers", activity_name_prefix)
    {
    }
}

