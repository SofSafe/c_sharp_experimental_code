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

            List<SofsafeDataValidationRuleName> rules = new List<SofsafeDataValidationRuleName>();
            rules.Add(SDVRN.len_greater_0);
            List<string> validation_results = new List<string>();

            SofsafeDataValidators.runtimeSetup(ot_helper,child_activity_pointer);

            return;
            validation_results = SofsafeDataValidators.validateData(ot_helper, child_activity_pointer, "", rules);

            foreach (var entry in validation_results)
            {
                Console.WriteLine(entry);
            }

        }

    }
}
