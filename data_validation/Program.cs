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
using System.Security.Cryptography;

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

           
            
            //SqlInsertTest(ot_helper, child_activity_pointer, ref purchase);
            //method_activity_list.Add(child_activity_pointer.P_pointer);

            SqlReadTest(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);


            OTSH.passStatusAndEndActivity(ETR,child_activity_pointer.P_pointer);
            return;           

            List<SDR> validation_rules = new List<SDR>();
            validation_rules.Add(SDC.R_greater_0);
            validation_rules.Add(SDC.R_us_zip_code);

            
            List<string> validation_results = new List<string>();

            double zip_code_number = -22;
            string zip_code_string = "0000";



            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer,  zip_code_string, validation_rules, "zip_code_string"));
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
            purchase.time.P_value = DateTime.Now;
            purchase.transaction_cloud_uid.setValueDisabled(ot_helper, child_activity_pointer, true);
            purchase.transaction_lan_uid.setValueDisabled(ot_helper, child_activity_pointer, true, "transaction.transaction_cloud_uid.setInstanceDisable");
            //purchase.transaction_lan_uid.P_value = 12;
            purchase.user_uid.P_value = 221;
            purchase.user_name.P_value = "kevin@sofsafe.cloud";

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

            Sdo<string> test_string = new Sdo<string>("zip code",PCR.us_zip_code);
            test_string.P_value = "4212";
            List<string> valid_values = new List<string>();
            valid_values.Add("22122");
            valid_values.Add("32122");
            valid_values.Add("42122");

            test_string.IV_valid_values_list.AddRange(valid_values);



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
            s3TransactionTable t_main = new s3TransactionTable(ot_helper, child_activity_pointer, "pos purchase");
            translator.registerSingleEntityTable(ot_helper, child_activity_pointer, t_main);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.registerSingleEntityOdc(ot_helper, child_activity_pointer, purchase);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            translator.populateDatabaseFields(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            ChildActivityPointer activity_list_status;
            if(OTSH.checkActivityList(ETR,method_activity_list, out activity_list_status) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, activity_list_status.P_pointer);
                return false;
            }
            method_activity_list.Clear();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost\\SQLEXPRESS";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "sofsafeTesting";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            
            SqlDataAdapter da2 = new SqlDataAdapter();
            SqlCommand insert_command = connection.CreateCommand();
            t_main.createInsertStatement(ref insert_command);
            insert_command.CommandText += "SELECT uid FROM Transactions WHERE uid = SCOPE_IDENTITY();";
            da2.InsertCommand = insert_command;           
            Console.WriteLine(insert_command.CommandText);
            DataSet ds1 = new DataSet();
            ds1.Namespace = "sofsafeTesting";
            da2.TableMappings.Add("Table", t_main.P_schema_name);
            ds1.Tables.Add(t_main.P_table);
            foreach (DataTable table in ds1.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        Console.WriteLine(column.ColumnName + ": " + row[column]);
                    }
                }
            }
            da2.Update(t_main.P_table);


            purchase.transaction_lan_uid.setValueDisabled(ot_helper, child_activity_pointer, false);
            purchase.transaction_lan_uid.P_value = 55;

            s3TransactionDetailTable t_details = new s3TransactionDetailTable(ot_helper, child_activity_pointer, "pos purchase");

            OdcToDatabaseTranslator details_translator = new OdcToDatabaseTranslator();
            details_translator.createMultiEntityAssociation(ot_helper, child_activity_pointer, "test");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            details_translator.registerMultiEntityOdcInstanceList(ot_helper, child_activity_pointer, "test",
                purchase.purchased_parts_details, "transaction.purchased_parts_details");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            details_translator.registerMultiEntityTable(ot_helper, child_activity_pointer, "test",
                t_details, "pos");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            if (purchase.transaction_lan_uid.P_last_set_time != Sdo<int>.C_unset_time)
            {
                details_translator.registerSdoWithMultiEntityAssociation(ot_helper, child_activity_pointer, "test",
                   purchase.transaction_lan_uid);
                method_activity_list.Add(child_activity_pointer.P_pointer);
            }

            details_translator.populateDatabaseFields(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            if (OTSH.checkActivityList(ETR, method_activity_list, out activity_list_status) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, activity_list_status.P_pointer);
                return false;
            }
            method_activity_list.Clear();

            da2.TableMappings.Add("Table1", t_details.P_schema_name);
            ds1.Tables.Add(t_details.P_table);


            SqlCommand insert_command2 = connection.CreateCommand();
            t_details.createInsertStatement(ref insert_command2);
            da2.InsertCommand = insert_command2;
            Console.WriteLine(insert_command2.CommandText);


            foreach (DataRow row in t_details.P_table.Rows)
            {
                foreach (DataColumn column in t_details.P_table.Columns)
                {
                    Console.WriteLine(column.ColumnName + ": " + row[column]);
                }
            }

            da2.Update(t_details.P_table);

            connection.Close();
            return true;
        }


        private static bool SqlReadTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + "main" + ".SqlReadTest");
            if (method_activity_pointer != null) { method_activity_pointer.setPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template

            List<Activity> method_activity_list = new List<Activity>();


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost\\SQLEXPRESS";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "sofsafeTesting";
            builder.TrustServerCertificate = true;

            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            s3TransactionDetailTable transaction_details_table = new s3TransactionDetailTable(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);
            s3CategoriesTable part_categories_table = new s3CategoriesTable(ot_helper, child_activity_pointer);


            SqlDataAdapter sqda = new SqlDataAdapter();
            //SqlCommand select_command = transaction_details_table.createSelectStatement(connection);
            SqlCommand select_command = connection.CreateCommand();
            select_command.CommandText = "SELECT TransactionDetail.category_uid, TransactionDetail.part_uid, TransactionDetail.value,  Categories.name  FROM TransactionDetail Inner JOIN Categories on  Categories.uid=TransactionDetail.category_uid  WHERE TransactionDetail.transaction_uid=5;";
            sqda.SelectCommand = select_command;
            DataSet data_set = new DataSet();
            data_set.Tables.Add(transaction_details_table.P_table);
            data_set.Tables.Add(part_categories_table.P_table);
            sqda.Fill(data_set);
            
            int table_index = 0;
            int loop1 = -1;
            foreach (DataTable table in data_set.Tables)
            {
                loop1++;
                if(table.TableName == "Table")
                {
                    table_index = loop1;
                }               
            }

            //this separates the data into the tables the data came from
            part_categories_table.P_table.Merge(data_set.Tables[table_index],true, MissingSchemaAction.Ignore);
            transaction_details_table.P_table.Merge(data_set.Tables[table_index], true,MissingSchemaAction.Ignore);

            TransactionDto purchase = new TransactionDto(ot_helper, child_activity_pointer, "transaction 521251", PDCI.transaction_type_purchase_parts);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            for (loop1=0; loop1 < transaction_details_table.P_table.Rows.Count; loop1++)
            {
                purchase.purchased_parts_details.Add(new TransactionDtoPartDetail("read-test >" + loop1.ToString()));
            }           


            DatabaseToOdcTranslator details_translator = new DatabaseToOdcTranslator();
            details_translator.createMultiEntityAssociation(ot_helper, child_activity_pointer, "test");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            details_translator.registerMultiEntityOdcInstanceList(ot_helper, child_activity_pointer, "test",
                purchase.purchased_parts_details, "transaction.purchased_parts_details");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            details_translator.registerMultiEntityTable(ot_helper, child_activity_pointer, "test",
                transaction_details_table, "pos");
            method_activity_list.Add(child_activity_pointer.P_pointer);

            details_translator.registerMultiEntityTable(ot_helper, child_activity_pointer, "test",
    part_categories_table, "pos");
            method_activity_list.Add(child_activity_pointer.P_pointer);


            details_translator.populateEmptySdoInstances(ot_helper, child_activity_pointer);
            method_activity_list.Add(child_activity_pointer.P_pointer);

            transaction_details_table.populateEmptyDataObjectsFromDb(ot_helper, child_activity_pointer, data_set);

            part_categories_table.populateEmptyDataObjectsFromDb(ot_helper, child_activity_pointer, data_set);



            ChildActivityPointer activity_list_status;
            if (OTSH.checkActivityList(ETR, method_activity_list, out activity_list_status) == false)
            {
                OTSH.passStatusAndEndActivity(ETR, activity_list_status.P_pointer);
                return false;
            }
            activity_list_status = null;
            method_activity_list.Clear();     

            foreach (TransactionDtoPartDetail entry in purchase.purchased_parts_details)
            {
                Console.WriteLine("entry.category_name:" + entry.part_category_name.P_value);
                Console.WriteLine("entry.category_uid:" + entry.part_category_uid.P_value);
                Console.WriteLine("entry.part_uid:" + entry.part_uid.P_value);
            }


            OTSH.setSuccessEndActivity(ETR);
            return true;
        }
    }
}
