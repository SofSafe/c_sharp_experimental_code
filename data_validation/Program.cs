using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //sdoStringTest(ot_helper, child_activity_pointer);
            sdoIntTest(ot_helper, child_activity_pointer);
            OTSH.setSuccessEndActivity(ETR);
            return;



            List<SDR> validation_rules = new List<SDR>();
            validation_rules.Add(SDC.R_greater_0);
            validation_rules.Add(SDC.R_us_zip_code);

            
            List<string> validation_results = new List<string>();

            double zip_code_number = -22;
            string zip_code_string = "0000";



            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_string, validation_rules, "zip_code_string"));
            method_activity_list.Add(child_activity_pointer.PIV_child_activity_pointer);

            validation_results.AddRange(SDC.validateData(ot_helper, child_activity_pointer, zip_code_number, validation_rules, "zip_code_number"));
            method_activity_list.Add(child_activity_pointer.PIV_child_activity_pointer);


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
            test_string.PIV_value =5;
            List<object> valid_values = new List<object>();
            valid_values.Add(1);
            valid_values.Add(7);
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
            test_string.PIV_value = "4212";
            List<object> valid_values = new List<object>();
            valid_values.Add("22122");
            valid_values.Add("32122");
            valid_values.Add("42122");

            test_string.addValidValues(ot_helper, child_activity_pointer, valid_values);

            test_string.addValidatorRule(ot_helper, child_activity_pointer, SDC.R_us_zip_code);


            test_string.runValidationMethod(ot_helper, child_activity_pointer);

            OTSH.setSuccessEndActivity(ETR);
            return;
        }

    }
}
