using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;





class JsonDecodingTesting
{

   
    public void testing(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null)
    {
        ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

        string json_text = """
            {"replication_type":1,"client_id":"1102","table":"PosSalesHdr","primary_key_value":"29","crush_version":1.0,"join_statement":" INNER JOIN PosSalesDtl ON PosSalesDtl.PosSaleHdrRno=PosSalesHdr.PosSaleHdrRno INNER JOIN PosPaymentsDtl ON PosPaymentsDtl.RefTableRno=PosSalesHdr.PosSaleHdrRno","secondary_tables":["PosSalesDtl","PosPaymentsDtl"]}
            """;


        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);


        GdcReplicationRequest get_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "replicate", "receive replication request");

        get_rep.PopulateFromJson(ot_helper, child_activity_pointer, json_text, "receive replication request");
        Console.WriteLine(get_rep.ToJson(ot_helper, child_activity_pointer));
        return;

        GdcReplicationRequest send_rep = new GdcReplicationRequest(ot_helper, child_activity_pointer, "replicate", "create replication request");

        send_rep.IV_replication_type.P_value = 1;
        send_rep.IV_primary_table.P_value = ReplicationTableHelper.PosSalesHdr.P_primary_table_name;
        send_rep.IV_primary_key_value.P_value = "29";
        send_rep.IV_client_id.P_value = "1102";
        send_rep.IV_software_version.P_value = 1;
        send_rep.IV_sql_join.P_value = ReplicationTableHelper.PosSalesHdr.P_join_statement;

        foreach (string table_name in ReplicationTableHelper.PosSalesHdr.P_secondary_tables)
        {
            IDcoBaseClass _Dco_instance = null;
            if (send_rep.IV_secondary_tables.CreateNewEntry(ot_helper, child_activity_pointer, table_name, out _Dco_instance) == false )
            {                
                return;
            }
            _Dco_instance.SetValueFromObject((object)table_name);
        }

            
        json_text = send_rep.ToJson(ot_helper, child_activity_pointer);
        Console.WriteLine(json_text);

       

        return;




        GdcPg4Settings test22 = new GdcPg4Settings(ot_helper, child_activity_pointer, "settings");

        GroupedDataClassBase base_class = test22 as GroupedDataClassBase;
        base_class.PopulateFromJson(ot_helper, child_activity_pointer, json_text);

        Console.WriteLine(base_class.ToJson(ot_helper, child_activity_pointer));

    }
}