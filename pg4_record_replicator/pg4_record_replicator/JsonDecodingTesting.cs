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

    NamedListOfReplicationRecord IV_records = new NamedListOfReplicationRecord("replication_records",false,false);
    public void testing(OpenTelemetryConfigHelper ot_helper=null, ChildActivityPointer method_activity_pointer=null)
    {
        ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");      

        string json_text = """
            {"P_organized_data_identifier":"ReplicationBatch","replication_type":1,"source_software_verison":1.0,"source_client_id":"1102","replication_read_time":"2024-07-24T07:19:37.6431277-06:00","replication_records":[{"P_organized_data_identifier":"Unset Odc Identifier","IV_select_statement":"SELECT ReplicationStartTime FROM RemoteParts WHERE  clientID=1102 AND PartRno=12;","IV_insert_statement":"INSERT into RemoteParts (PartRno , StoreRno , Status , PartId , ShortName , LongName , AbbrName , PriceBasis , Price , CoreReturnAmt , GuarFlg , WarrFlg , WarrPriceAmt , AllowDiscFlg , TaxableFlg , ShowCheckoutFlg , ShowGateFlg , ShowCustSvcFlg , PopularFlg , EssentialFlg , TaxIncludedFlg , EnviroChrgFlg , SortSeq , EodDtlFlg , ShowAlertFlg , CreDtTm , CreUsrRno , CreStnRno , ModDtTm , ModUsrRno , ModStnRno, ReplicationStartTime , ClientId , ReplicationFinishTime ) VALUES (12 , 1 , 'Active' , 'A70831178' , 'ENGINE BLOCK' , 'ENGINE BLOCK UPDATED' , 'ENGINE BLOCK' , 'Each' , 49.9900 , 24.9900 , 0 , 0 , 0.0000 , 1 , 1 , 1 , 0 , 0 , 1 , 0 , 0 , 1 , 0 , 0 , 0 , '09/01/2009 06:00:00:000' , 1 , 6 , '01/23/2024 22:24:19:070' , 7 , 6 ,'07/24/2024 13:19:37:790' , 1102, GETUTCDATE() );","IV_update_statement":"UPDATE RemoteParts SET PartRno=12 , StoreRno=1 , Status='Active' , PartId='A70831178' , ShortName='ENGINE BLOCK' , LongName='ENGINE BLOCK UPDATED' , AbbrName='ENGINE BLOCK' , PriceBasis='Each' , Price=49.9900 , CoreReturnAmt=24.9900 , GuarFlg=0 , WarrFlg=0 , WarrPriceAmt=0.0000 , AllowDiscFlg=1 , TaxableFlg=1 , ShowCheckoutFlg=1 , ShowGateFlg=0 , ShowCustSvcFlg=0 , PopularFlg=1 , EssentialFlg=0 , TaxIncludedFlg=0 , EnviroChrgFlg=1 , SortSeq=0 , EodDtlFlg=0 , ShowAlertFlg=0 , CreDtTm='09/01/2009 06:00:00:000' , CreUsrRno=1 , CreStnRno=6 , ModDtTm='01/23/2024 22:24:19:070' , ModUsrRno=7 , ModStnRno=6,ReplicationStartTime='07/24/2024 13:19:37:790', ClientId=1102 , ReplicationFinishTime=GETUTCDATE() WHERE  clientID=1102 AND PartRno=12;","IV_combined_statement":"DECLARE @pram_replication_start_time DATETIME = '07/24/2024 13:19:37:790';\r\n\r\n                IF NOT EXISTS (SELECT 1 FROM RemoteParts WHERE  clientID=1102 AND PartRno=12)\r\n                    BEGIN\r\n                        -- INSERT statement\r\n                        INSERT INTO RemoteParts \r\n                        (   PartRno , StoreRno , Status , PartId , ShortName , LongName , AbbrName , PriceBasis , Price , CoreReturnAmt , GuarFlg , WarrFlg , WarrPriceAmt , AllowDiscFlg , TaxableFlg , ShowCheckoutFlg , ShowGateFlg , ShowCustSvcFlg , PopularFlg , EssentialFlg , TaxIncludedFlg , EnviroChrgFlg , SortSeq , EodDtlFlg , ShowAlertFlg , CreDtTm , CreUsrRno , CreStnRno , ModDtTm , ModUsrRno , ModStnRno, \r\n                            ReplicationStartTime, \r\n                            ClientId, \r\n                            ReplicationFinishTime )\r\n                        VALUES (\r\n                        12 , 1 , 'Active' , 'A70831178' , 'ENGINE BLOCK' , 'ENGINE BLOCK UPDATED' , 'ENGINE BLOCK' , 'Each' , 49.9900 , 24.9900 , 0 , 0 , 0.0000 , 1 , 1 , 1 , 0 , 0 , 1 , 0 , 0 , 1 , 0 , 0 , 0 , '09/01/2009 06:00:00:000' , 1 , 6 , '01/23/2024 22:24:19:070' , 7 , 6,\r\n                        '07/24/2024 13:19:37:790' , \r\n                        1102, \r\n                         GETUTCDATE() );\r\n                    END\r\n\r\n                ELSE\r\n\r\n                    -- UPDATE statement\r\n                    IF EXISTS (\r\n                        SELECT 1\r\n                        FROM RemoteParts\r\n                        WHERE  clientID=1102 AND PartRno=12\r\n                          AND ReplicationStartTime < @pram_replication_start_time\r\n                    )\r\n                    BEGIN\r\n                        UPDATE RemoteParts \r\n                            SET PartRno=12 , StoreRno=1 , Status='Active' , PartId='A70831178' , ShortName='ENGINE BLOCK' , LongName='ENGINE BLOCK UPDATED' , AbbrName='ENGINE BLOCK' , PriceBasis='Each' , Price=49.9900 , CoreReturnAmt=24.9900 , GuarFlg=0 , WarrFlg=0 , WarrPriceAmt=0.0000 , AllowDiscFlg=1 , TaxableFlg=1 , ShowCheckoutFlg=1 , ShowGateFlg=0 , ShowCustSvcFlg=0 , PopularFlg=1 , EssentialFlg=0 , TaxIncludedFlg=0 , EnviroChrgFlg=1 , SortSeq=0 , EodDtlFlg=0 , ShowAlertFlg=0 , CreDtTm='09/01/2009 06:00:00:000' , CreUsrRno=1 , CreStnRno=6 , ModDtTm='01/23/2024 22:24:19:070' , ModUsrRno=7 , ModStnRno=6,ReplicationStartTime='07/24/2024 13:19:37:790', ClientId=1102 , ReplicationFinishTime=GETUTCDATE() \r\n                            WHERE  clientID=1102 AND PartRno=12;\r\n                    END\r\n                ;"}]}
            """;


        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);

        StringReader string_reader = new StringReader(json_text);
        JsonTextReader reader = new JsonTextReader(string_reader);

        ReplicationBatch test22 = new ReplicationBatch(ot_helper, child_activity_pointer,"json test");

        OrganizedDataClassBase base_class = test22 as OrganizedDataClassBase;
        base_class.PopulateFromJson(ot_helper, child_activity_pointer, json_text);

    }

    public void ProcessJsonToken(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonTextReader reader, string parent_property_name)
    {
        Console.WriteLine("starting");

        string last_property_name = "";
        string current_property_name = "";        
        JsonToken last_token_type = JsonToken.None;
        JsonToken current_token_type = JsonToken.None;

        bool within_array = false;
        int arrays_found = 0;
        string json_array_name = "";        
        while (reader.Read())
        {
            last_token_type = current_token_type;
            current_token_type = reader.TokenType;
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


            //handle a dco instance in an array
            if (reader.TokenType == JsonToken.StartArray && within_array == true)
            {
                Console.WriteLine($"handling  dco instance for array: {json_array_name}");
                //todo add dco list support
                continue;
            }

            //handle an Odc instance in an array
            if (reader.TokenType == JsonToken.StartObject && within_array == true)
            {
                Console.WriteLine($"calling ProcessJsonToken for array: {json_array_name},  property: {current_property_name}");
                ProcessJsonToken(ot_helper, method_activity_pointer, ref reader, json_array_name);
                continue;
            }


            if (reader.TokenType == JsonToken.StartArray)
            {
                json_array_name = current_property_name;
                arrays_found++;
                within_array = true;
                continue;
            }

            if (reader.TokenType == JsonToken.EndArray)
            {
                json_array_name = "";
                within_array = false;
                continue;
            }


            //handle child ODC instance
            if (reader.TokenType == JsonToken.StartObject)
            {
                ProcessJsonToken(ot_helper,method_activity_pointer, ref reader,current_property_name);
                continue;
            }
            //only return when we find a }}
            if (reader.TokenType == JsonToken.EndObject && last_token_type == JsonToken.EndObject)
            {
                Console.WriteLine("returing");
                return;
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

