using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



interface IOdcInstanceList
{
    string P_instnace_name { get; }
    IOdcBaseClass CreateNewEntry(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null, string organized_data_identifier = "", string activity_name_prefix = "");
    bool SetInstanceName(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null, string odc_list_name = "", string activity_name_prefix = "");
}

class OdcInstanceList<type_of_generic>: List<type_of_generic>
{
    public bool SetInstanceName(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null, string odc_list_name = "", string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor1");
        validator.AddEntry(new DataVR(odc_list_name, "odc_list_name").AddRule(SDV.R_len_greater_0));      
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation  

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }
    public string P_instnace_name {
        get {
            return "";        
        }
    }
}

class ReplicationRecordList : OdcInstanceList<ReplicationRecord>, IOdcInstanceList
{
    public IOdcBaseClass CreateNewEntry(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null, string organized_data_identifier="",string activity_name_prefix = "")
    {
        ReplicationRecord new_entry = new ReplicationRecord(ot_helper, method_activity_pointer, organized_data_identifier, activity_name_prefix);
        this.Add(new_entry);
        return new_entry as IOdcBaseClass;
    }


}

class JsonDecodingTesting
{
    Dictionary<string, IOdcInstanceList> IV_odc_lists = new Dictionary<string, IOdcInstanceList>();
    ReplicationRecordList IV_replication_records = new ReplicationRecordList();
    public void testing(OpenTelemetryConfigHelper ot_helper=null, ChildActivityPointer method_activity_pointer=null)
    {
        ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");      

        string json_text = """
            {"ReplicationBatch":{"IV_replication_type":1,"IV_software_verison":1.0,"IV_client_id":"1102","IV_replication_start_time":"2024-07-21T19:56:44.3815345-06:00","IV_replication_records":[{"Unset Odc Identifier":{"t1IV_select_statement":"SELECT ReplicationStartTime FROM RemoteParts WHERE  clientID=1102 AND PartRno=12;","IV_insert_statement":"INSERT into RemoteParts (PartRno , StoreRno , Status , PartId , ShortName , LongName , AbbrName , PriceBasis , Price , CoreReturnAmt , GuarFlg , WarrFlg , WarrPriceAmt , AllowDiscFlg , TaxableFlg , ShowCheckoutFlg , ShowGateFlg , ShowCustSvcFlg , PopularFlg , EssentialFlg , TaxIncludedFlg , EnviroChrgFlg , SortSeq , EodDtlFlg , ShowAlertFlg , CreDtTm , CreUsrRno , CreStnRno , ModDtTm , ModUsrRno , ModStnRno, ReplicationStartTime , ClientId , ReplicationFinishTime ) VALUES (12 , 1 , 'Active' , 'A70831178' , 'ENGINE BLOCK' , 'ENGINE BLOCK UPDATED' , 'ENGINE BLOCK' , 'Each' , 49.9900 , 24.9900 , 0 , 0 , 0.0000 , 1 , 1 , 1 , 0 , 0 , 1 , 0 , 0 , 1 , 0 , 0 , 0 , '09/01/2009 06:00:00:000' , 1 , 6 , '01/23/2024 22:24:19:070' , 7 , 6 ,'07/22/2024 01:56:44:526' , 1102, GETUTCDATE() );","IV_update_statement":"UPDATE RemoteParts SET PartRno=12 , StoreRno=1 , Status='Active' , PartId='A70831178' , ShortName='ENGINE BLOCK' , LongName='ENGINE BLOCK UPDATED' , AbbrName='ENGINE BLOCK' , PriceBasis='Each' , Price=49.9900 , CoreReturnAmt=24.9900 , GuarFlg=0 , WarrFlg=0 , WarrPriceAmt=0.0000 , AllowDiscFlg=1 , TaxableFlg=1 , ShowCheckoutFlg=1 , ShowGateFlg=0 , ShowCustSvcFlg=0 , PopularFlg=1 , EssentialFlg=0 , TaxIncludedFlg=0 , EnviroChrgFlg=1 , SortSeq=0 , EodDtlFlg=0 , ShowAlertFlg=0 , CreDtTm='09/01/2009 06:00:00:000' , CreUsrRno=1 , CreStnRno=6 , ModDtTm='01/23/2024 22:24:19:070' , ModUsrRno=7 , ModStnRno=6,ReplicationStartTime='07/22/2024 01:56:44:526', ClientId=1102 , ReplicationFinishTime=GETUTCDATE() WHERE  clientID=1102 AND PartRno=12;","IV_combined_statement":"DECLARE @pram_replication_start_time DATETIME = '07/22/2024 01:56:44:526';\r\n\r\n                IF NOT EXISTS (SELECT 1 FROM RemoteParts WHERE  clientID=1102 AND PartRno=12)\r\n                    BEGIN\r\n                        -- INSERT statement\r\n                        INSERT INTO RemoteParts \r\n                        (   PartRno , StoreRno , Status , PartId , ShortName , LongName , AbbrName , PriceBasis , Price , CoreReturnAmt , GuarFlg , WarrFlg , WarrPriceAmt , AllowDiscFlg , TaxableFlg , ShowCheckoutFlg , ShowGateFlg , ShowCustSvcFlg , PopularFlg , EssentialFlg , TaxIncludedFlg , EnviroChrgFlg , SortSeq , EodDtlFlg , ShowAlertFlg , CreDtTm , CreUsrRno , CreStnRno , ModDtTm , ModUsrRno , ModStnRno, \r\n                            ReplicationStartTime, \r\n                            ClientId, \r\n                            ReplicationFinishTime )\r\n                        VALUES (\r\n                        12 , 1 , 'Active' , 'A70831178' , 'ENGINE BLOCK' , 'ENGINE BLOCK UPDATED' , 'ENGINE BLOCK' , 'Each' , 49.9900 , 24.9900 , 0 , 0 , 0.0000 , 1 , 1 , 1 , 0 , 0 , 1 , 0 , 0 , 1 , 0 , 0 , 0 , '09/01/2009 06:00:00:000' , 1 , 6 , '01/23/2024 22:24:19:070' , 7 , 6,\r\n                        '07/22/2024 01:56:44:526' , \r\n                        1102, \r\n                         GETUTCDATE() );\r\n                    END\r\n\r\n                ELSE\r\n\r\n                    -- UPDATE statement\r\n                    IF EXISTS (\r\n                        SELECT 1\r\n                        FROM RemoteParts\r\n                        WHERE  clientID=1102 AND PartRno=12\r\n                          AND ReplicationStartTime < @pram_replication_start_time\r\n                    )\r\n                    BEGIN\r\n                        UPDATE RemoteParts \r\n                            SET PartRno=12 , StoreRno=1 , Status='Active' , PartId='A70831178' , ShortName='ENGINE BLOCK' , LongName='ENGINE BLOCK UPDATED' , AbbrName='ENGINE BLOCK' , PriceBasis='Each' , Price=49.9900 , CoreReturnAmt=24.9900 , GuarFlg=0 , WarrFlg=0 , WarrPriceAmt=0.0000 , AllowDiscFlg=1 , TaxableFlg=1 , ShowCheckoutFlg=1 , ShowGateFlg=0 , ShowCustSvcFlg=0 , PopularFlg=1 , EssentialFlg=0 , TaxIncludedFlg=0 , EnviroChrgFlg=1 , SortSeq=0 , EodDtlFlg=0 , ShowAlertFlg=0 , CreDtTm='09/01/2009 06:00:00:000' , CreUsrRno=1 , CreStnRno=6 , ModDtTm='01/23/2024 22:24:19:070' , ModUsrRno=7 , ModStnRno=6,ReplicationStartTime='07/22/2024 01:56:44:526', ClientId=1102 , ReplicationFinishTime=GETUTCDATE() \r\n                            WHERE  clientID=1102 AND PartRno=12;\r\n                    END\r\n                ;"}}]}}
            """;

        this.IV_odc_lists.Add("IV_replication_records", IV_replication_records);

        StringReader string_reader = new StringReader(json_text);
        JsonTextReader reader = new JsonTextReader(string_reader);

        int start_object_count = 0;
        int loop1 = -1;
        while(reader.Read())
        {
            if(loop1 > 10)
            {
                //add error logic here
                return;
            }
            loop1++;
            if (reader.TokenType == JsonToken.StartObject)
            {
                start_object_count++;
                if (start_object_count == 2) { break; }
            }                
        }

        this.ProcessJsonToken(ot_helper,method_activity_pointer, ref reader);
       
        
    }

    public void ProcessJsonToken(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonTextReader reader)
    {
        Console.WriteLine("starting");

        string last_property_name = "";
        string current_property_name = "";
        bool skip_end_object_logic_once = false;
        bool last_entry_is_array_start = false;
        bool within_array = false;
        int arrays_found = 0;
        int dictionaries_found = 0;
        string list_name = "";
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                last_property_name = current_property_name;
                current_property_name = reader.Value.ToString();
                continue;
            }

            if (last_property_name == current_property_name)
            {
                current_property_name = "";
                last_property_name = "";               
            }

            //Console.WriteLine($"{current_property_name}: Type: {reader.TokenType}");


            if (reader.TokenType == JsonToken.StartArray)
            {
                list_name = current_property_name;
                arrays_found++;
                last_entry_is_array_start = true;
                within_array = true;
                continue;
            }


            //handle the start of the list of Odc or Dco instances
            if (reader.TokenType == JsonToken.StartObject && last_entry_is_array_start == true)
            {
                IOdcInstanceList odc_instance_list;
                this.IV_odc_lists.TryGetValue(current_property_name, out odc_instance_list);               
                IOdcBaseClass new_entry = odc_instance_list.CreateNewEntry(ot_helper, method_activity_pointer, current_property_name);

                ProcessJsonToken(ot_helper, method_activity_pointer, ref reader);


                last_entry_is_array_start = false;
                continue;

            }

            if(reader.TokenType == JsonToken.StartObject && within_array == true)
            {

                last_entry_is_array_start = false;
                continue;

            }

            if (reader.TokenType == JsonToken.EndArray)
            {
                list_name = "";
                within_array = false;
                continue;
            }



            if (reader.TokenType == JsonToken.StartObject && current_property_name.IndexOf("t3") == 0)
            {
                ProcessJsonToken(ot_helper,method_activity_pointer, ref reader);
                skip_end_object_logic_once = true;
                continue;
            }

            if(reader.TokenType == JsonToken.EndObject && skip_end_object_logic_once == false)
            {
                Console.WriteLine("returing");
                return;
            }

            if(skip_end_object_logic_once == true)
            {
                skip_end_object_logic_once = false;
            }
            
            if (current_property_name.Length == 0)
            {
                continue;
            }
            
            last_property_name = current_property_name;            
            Console.WriteLine($"{current_property_name}: Type: {reader.TokenType}");
        }

        Console.WriteLine("returing2");


    }
}

