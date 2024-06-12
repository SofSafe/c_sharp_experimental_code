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
            OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

            Activity ETR = ot_helper.startActivityWrapper("p1s3e21DeleteUser");
            ETR.SetTag("post_key1", "post key 1 value");
            ETR.SetTag("post_key2", "post key 2 value");

            List<Activity> method_activity_lists = new List<Activity>();
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            SDVC.runtimeSetup(ot_helper, child_activity_pointer);


            List<SofsafeDataValidationRuleName> rules = new List<SofsafeDataValidationRuleName>();
            rules.Add(SDVC.greater_0);
            List<string> validation_results = new List<string>();

            Int128 test = -1;
            float test3 = 55;

            Type[] test22 = test.GetType().GetInterfaces();
            Type[] test33 = test3.GetType().GetInterfaces();

            validation_results = SDVC.validateData(ot_helper, child_activity_pointer, test, rules);

            foreach (var entry in validation_results)
            {
                Console.WriteLine(entry);
            }

        }

    }
}
