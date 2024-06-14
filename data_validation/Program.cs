using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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


            Transaction transaction = new Transaction(ot_helper,child_activity_pointer,PDCI.transaction_type_purchase_parts);

            transaction.total_amount.P_value = -5;
            transaction.location_name.P_value = "S3 Solutions";
            transaction.location_uid.P_value = 1252;
            transaction.POS_name.P_value = "register 2";
            transaction.POS_uid.P_value = 5522;
            transaction.transaction_cloud_uid.P_value_disabled = true;

            TransactionPartDetail part_detail = new TransactionPartDetail();
            { };
            part_detail.part_category_name.P_value = "engine part";
            part_detail.part_category_uid.P_value = 252;
            part_detail.amount.P_value = 55;
            part_detail.part_name.P_value = "Oxygen Sensor";
            part_detail.part_uid.P_value = 122;       

            
            transaction.part_details.Add(part_detail);
            transaction.validateData(ot_helper, child_activity_pointer);
            
            
            foreach(string data_id_key in transaction.P_last_validation_result_details.Keys)
            {
                List<string> issues_found;
                transaction.P_last_validation_result_details.TryGetValue(data_id_key, out issues_found);
                if(issues_found == null) { continue; }

                foreach (string issue in issues_found)
                {
                    Console.WriteLine(issue);
                }

            }
            return;

            sdoStringTest(ot_helper, child_activity_pointer);
            sdoIntTest(ot_helper, child_activity_pointer);
            OTSH.setSuccessEndActivity(ETR);



            List<SDR> validation_rules = new List<SDR>();
            validation_rules.Add(SDC.R_greater_0);
            validation_rules.Add(SDC.R_us_zip_code);

            
            List<string> validation_results = new List<string>();

            double zip_code_number = -22;
            string zip_code_string = "0000";



            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_string, validation_rules, "zip_code_string"));
            method_activity_list.Add(child_activity_pointer.P_child_activity_pointer);

            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_number, validation_rules, "zip_code_number"));
            method_activity_list.Add(child_activity_pointer.P_child_activity_pointer);


            foreach (var entry in validation_results)
            {

                Console.WriteLine(entry);
            }

        }

        private static void sdoIntTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
        {

            Activity ETR = ot_helper.startActivityWrapper("sdoIntTest");
            method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            List<Activity> method_activity_list = new List<Activity>();

            SdoInt test_string = new SdoInt("User Type UID");
            test_string.P_value = 7;
            List<object> valid_values = new List<object>();
            valid_values.Add(1);
            valid_values.Add(5);
            valid_values.Add(12);

            test_string.addValidValues(ot_helper, child_activity_pointer, valid_values);
            test_string.runValidationMethod(ot_helper, child_activity_pointer);

            OTSH.setSuccessEndActivity(ETR);
            return;
        }

        private static void sdoStringTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
        {

            Activity ETR = ot_helper.startActivityWrapper("sdoStringTest");
            method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            List<Activity> method_activity_list = new List<Activity>();

            SdoString test_string = new SdoString("zip code");
            test_string.P_value = "4212";
            List<object> valid_values = new List<object>();
            valid_values.Add("22122");
            valid_values.Add("32122");
            valid_values.Add("42122");

            test_string.addValidValues(ot_helper, child_activity_pointer, valid_values);

            test_string.IV_validation_rules_list.Add(SDC.R_us_zip_code);


            test_string.runValidationMethod(ot_helper, child_activity_pointer);

            OTSH.setSuccessEndActivity(ETR);
            return;
        }

    }
}
