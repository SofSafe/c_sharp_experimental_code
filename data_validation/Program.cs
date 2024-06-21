using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace data_validation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // setup open telemetry and activity top level objects
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

            // create objects used for monitoring activities
            List<Activity> method_activity_list = new List<Activity>();
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();


            //do runtime setup
            SDC.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);


            Activity ETR = ot_helper.startActivityWrapper("Endpoint Name");
            ETR.SetTag("post_key1", "post key 1 value");
            ETR.SetTag("post_key2", "post key 2 value");

            SqlServerTest(ot_helper, child_activity_pointer);           
            //SdoIntTest(ot_helper, child_activity_pointer);
            //SdoStringTest(ot_helper, child_activity_pointer);
            OTSH.setSuccessEndActivity(ETR);
            return;




            TransactionDto purchase = new TransactionDto(ot_helper,child_activity_pointer, "transaction 521251",PDCI.transaction_type_purchase_parts);

            purchase.total_amount.P_value = -12;
            purchase.location_name.P_value = "S3 Solutions";
            purchase.location_uid.P_value = 1252;
            purchase.POS_name.P_value = "register 2";
            purchase.POS_uid.P_value = 22;
            purchase.transaction_cloud_uid.setInstanceDisable(ot_helper, child_activity_pointer, true);
            purchase.transaction_lan_uid.setInstanceDisable(ot_helper, child_activity_pointer, true, "transaction.transaction_cloud_uid.setInstanceDisable");
            purchase.user_uid.P_value = 221;
            purchase.user_name.P_value = "kevin@sofsafe.cloud";
            purchase.last_part_scanned.amount.P_value = 2;

            TransactionDtoPartDetail part_1 = new TransactionDtoPartDetail("part 1");
            { };
            part_1.part_category_name.P_value = "Engine Part";
            part_1.part_category_uid.P_value = 1;
            part_1.amount.P_value = 3;
            part_1.part_name.P_value = "Oxygen Sensor";
            part_1.part_uid.P_value = 2;
            
            
            part_1.notes.Add(new Sdo<string>("value2", "note 1", 
                new List<SDR>() { SDC.R_len_greater_2 },
                new List<string> { "value1","value2","value3"}));

            TransactionDtoPartDetail part_2 = new TransactionDtoPartDetail("part 2");
            { };
            part_2.part_category_name.P_value = "Transmission Part";
            part_2.part_category_uid.P_value = 4;
            part_2.amount.P_value = 6;
            part_2.part_name.P_value = "test";
            part_2.part_uid.P_value = 5;


            purchase.last_part_scanned.part_category_name.P_value = "Engine Part";
            purchase.last_part_scanned.part_category_uid.P_value = 2212;
            purchase.last_part_scanned.amount.P_value = 22;
            purchase.last_part_scanned.part_name.P_value = "tst";
            purchase.last_part_scanned.part_uid.P_value = 221;

            //transaction.last_part_scanned.setInstanceDisable(ot_helper,child_activity_pointer,true, "transaction.last_part_scanned.setInstanceDisable");
            purchase.purchased_parts_details.Add(part_1);
            purchase.purchased_parts_details.Add(part_2);



            OdcToDatabaseTranslator translator = new OdcToDatabaseTranslator();


            //SqlTransactionTable t_main = new SqlTransactionTable(ot_helper,child_activity_pointer,"pos purchase");
            SqlTransactionDetail t_details = new SqlTransactionDetail(ot_helper, child_activity_pointer, "pos purchase");
            //translator.registerSingleEntityTable(ot_helper, child_activity_pointer, t_main);
            //method_activity_list.Add(child_activity_pointer.P_pointer);
            
            //translator.registerSingleEntityOdc(ot_helper, child_activity_pointer, purchase);
            //method_activity_list.Add(child_activity_pointer.P_pointer);


            
            translator.registerMultiEntityAssociation(ot_helper, child_activity_pointer, "test");            
            method_activity_list.Add(child_activity_pointer.P_pointer);

            
            translator.registerMultiEntitySourcePointer(ot_helper, child_activity_pointer,"test" ,
                purchase.purchased_parts_details, "transaction.purchased_parts_details");
            method_activity_list.Add(child_activity_pointer.P_pointer);
            
            translator.registerMultiEntityDestinationPointer(ot_helper, child_activity_pointer, "test",
                t_details, "pos");
            method_activity_list.Add(child_activity_pointer.P_pointer);
            

            translator.populateDatabaseFields(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);


            bool fail_flag = false;
            ChildActivityPointer first_failed_activity = new ChildActivityPointer();
            foreach (Activity entry in method_activity_list)
            {
                if (entry == null) {
                    OTSH.addEventThenEndActivity(ETR, "null method_activity_list entry", ErrorTagsCodes.C_input_validation);
                    continue; 
                }
                if(entry.Status != ActivityStatusCode.Ok)
                {
                    if(fail_flag == false)
                    {
                        first_failed_activity.setPointerValue(entry,entry.TraceId);
                        fail_flag = true;
                    }

                }
            }

            if(fail_flag == true)
            {
                OTSH.passStatusAndEndActivity(ETR, first_failed_activity.P_pointer);
                return;
            }


            Console.WriteLine(purchase.P_last_validation_result);
            foreach (string data_id_key in purchase.P_last_validation_result_details.Keys)
            {
                List<string> issues_found;
                purchase.P_last_validation_result_details.TryGetValue(data_id_key, out issues_found);
                if (issues_found == null) { continue; }
                foreach (string issue in issues_found)
                {
                    Console.WriteLine(issue);
                }

            }

            OTSH.passStatusAndEndActivity(ETR,child_activity_pointer.P_pointer);
            return;

            



            List<SDR> validation_rules = new List<SDR>();
            validation_rules.Add(SDC.R_greater_0);
            validation_rules.Add(SDC.R_us_zip_code);

            
            List<string> validation_results = new List<string>();

            double zip_code_number = -22;
            string zip_code_string = "0000";



            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_string, validation_rules, "zip_code_string"));
            method_activity_list.Add(child_activity_pointer.P_pointer);

            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_number, validation_rules, "zip_code_number"));
            method_activity_list.Add(child_activity_pointer.P_pointer);


            foreach (var entry in validation_results)
            {

                Console.WriteLine(entry);
            }

        }

        private static void SdoIntTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
        {

            Activity ETR = ot_helper.startActivityWrapper("Sdo<int>Test");
            method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            List<Activity> method_activity_list = new List<Activity>();

            Sdo<int> test_string = new Sdo<int>("User Type UID");
            test_string.P_value = 7;
            List<int> valid_values = new List<int>();
            valid_values.Add(1);
            valid_values.Add(5);
            valid_values.Add(12);

            test_string.IV_valid_values_list.AddRange(valid_values);
            test_string.runValidationMethod(ot_helper, child_activity_pointer);

            OTSH.setSuccessEndActivity(ETR);
            return;
        }

        private static void SdoStringTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
        {

            Activity ETR = ot_helper.startActivityWrapper("Sdo<string>Test");
            method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            List<Activity> method_activity_list = new List<Activity>();

            Sdo<string> test_string = new Sdo<string>("zip code");
            test_string.P_value = "4212";
            List<string> valid_values = new List<string>();
            valid_values.Add("22122");
            valid_values.Add("32122");
            valid_values.Add("42122");

            test_string.IV_valid_values_list.AddRange(valid_values);

            test_string.IV_validation_rules_list.Add(SDC.R_us_zip_code);


            test_string.runValidationMethod(ot_helper, child_activity_pointer);

            OTSH.setSuccessEndActivity(ETR);
            return;
        }
        
        private static void SqlServerTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost\\SQLEXPRESS";
            builder.UserID = "sa";
            builder.Password = "123";            
            //builder.InitialCatalog = nul;
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name, collation_name FROM sys.databases";
            da.SelectCommand = cmd;
            DataSet ds = new DataSet();

            ///conn.Open();
            da.Fill(ds);
            ///conn.Close();

            foreach(DataTable table in  ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {                   
                    foreach (DataColumn column in table.Columns)                 
                    {
                        Console.WriteLine(column.ColumnName + ": " + row[column]);
                    }
                }
            }
        }

    }
}
