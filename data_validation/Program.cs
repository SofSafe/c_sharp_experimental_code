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

            //SdoIntTest(ot_helper, child_activity_pointer);
            //SdoStringTest(ot_helper, child_activity_pointer);
            //OTSH.setSuccessEndActivity(ETR);
            //return;




            TransactionDto purchase = new TransactionDto(ot_helper,child_activity_pointer, "transaction 521251",PDCI.transaction_type_purchase_parts);

            if (populatePurchaseObject(ot_helper, child_activity_pointer, ref purchase) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return;
            }

            SqlTransactionDetail t_detail = new SqlTransactionDetail(ot_helper,child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);
            
            SqlInsertTest(ot_helper, child_activity_pointer, ref purchase);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            //SqlReadTest(ot_helper, child_activity_pointer, t_detail);
            //method_activity_list.Add(child_activity_pointer.P_pointer);


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

        private static bool populatePurchaseObject(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref TransactionDto purchase, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + "main" + ".populatePurchaseObject");
            if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template
            
            purchase.total_amount.P_value = -12;
            purchase.location_name.P_value = "S3 Solutions";
            purchase.location_uid.P_value = 1252;
            purchase.POS_name.P_value = "register 2";
            purchase.POS_uid.P_value = 22;
            purchase.transaction_cloud_uid.setInstanceDisable(ot_helper, child_activity_pointer, true);
            //purchase.transaction_lan_uid.setInstanceDisable(ot_helper, child_activity_pointer, true, "transaction.transaction_cloud_uid.setInstanceDisable");
            purchase.transaction_lan_uid.P_value = 12;
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
                new List<string> { "value1", "value2", "value3" }));

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
            purchase.last_part_scanned.transaction_uid.P_value = 55;

            //transaction.last_part_scanned.setInstanceDisable(ot_helper,child_activity_pointer,true, "transaction.last_part_scanned.setInstanceDisable");
            purchase.purchased_parts_details.Add(part_1);
            purchase.purchased_parts_details.Add(part_2);
            return true;
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
        
        private static bool SqlInsertTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,ref TransactionDto purchase, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + "main" + ".SqlInsertTest");
            if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template


            List<Activity> method_activity_list = new List<Activity>();

            OdcToDatabaseTranslator translator = new OdcToDatabaseTranslator();
            SqlTransactionTable t_main = new SqlTransactionTable(ot_helper, child_activity_pointer, "pos purchase");
            SqlTransactionDetail t_details = new SqlTransactionDetail(ot_helper, child_activity_pointer, "pos purchase");
            //translator.registerSingleEntityTable(ot_helper, child_activity_pointer, t_main);
            //method_activity_list.Add(child_activity_pointer.P_pointer);

            //translator.registerSingleEntityOdc(ot_helper, child_activity_pointer, purchase);
            //method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.registerMultiEntityAssociation(ot_helper, child_activity_pointer, "test");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.registerMultiEntitySourcePointer(ot_helper, child_activity_pointer, "test",
                purchase.purchased_parts_details, "transaction.purchased_parts_details");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.registerMultiEntityDestinationPointer(ot_helper, child_activity_pointer, "test",
                t_details, "pos");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.registerSdoWithMultiEntityAssociation(ot_helper, child_activity_pointer, "test",
                purchase.transaction_lan_uid);
            method_activity_list.Add(child_activity_pointer.P_pointer);



            translator.populateDatabaseFields(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            ChildActivityPointer activity_list_status;
            if(OTSH.checkActivityList(ETR,method_activity_list, out activity_list_status) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, activity_list_status.P_pointer);
                return false;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost\\SQLEXPRESS";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "sofsafeTesting";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            
            SqlDataAdapter da2 = new SqlDataAdapter();
            SqlCommand insert_command = t_details.createInsertStatement(connection);
            da2.InsertCommand = insert_command;
            da2.Update(t_details.P_table);
            return true;
        }


        private static bool SqlReadTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SqlTransactionDetail transaction_details_table, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + "main" + ".SqlReadTest");
            if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost\\SQLEXPRESS";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "sofsafeTesting";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            SqlDataAdapter sqda = new SqlDataAdapter();
            SqlCommand select_command = transaction_details_table.createSelectStatement(connection);
            sqda.SelectCommand = select_command;
            DataSet data_set = new DataSet();
            data_set.Tables.Add(transaction_details_table.P_table);
            sqda.Fill(data_set);

            foreach (DataTable table in data_set.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        Console.WriteLine(column.ColumnName + ": " + row[column]);
                    }
                }
            }

            TransactionDto purchase = new TransactionDto(ot_helper, child_activity_pointer, "transaction 521251", PDCI.transaction_type_purchase_parts);

            transaction_details_table.populateDataObjectsFromDb(ot_helper, child_activity_pointer, data_set);

            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
    }
}
