using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System.Collections;
using System.Collections.Generic;




public class Program
{
    public static void Main()
    {
        OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

        Activity MAO = ot_helper.startActivityWrapper("p1s3e20CreateUser");
        MAO.SetTag("post_key1", "post key 1 value");
        MAO.SetTag("post_key2", "post key 2 value");
        List<Activity> method_activity_lists = new List<Activity>();
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        int loop1 = 0;
        while (loop1 < 10)
        {
            loop1++;
            
 

            ActivityTesting test_class = new ActivityTesting();

            if (loop1 == 3)
            {
                //this will cause the loop to stop safely
                test_class.exceptionTest(ot_helper, child_activity_pointer);
                method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);
            }


            test_class.test1(ot_helper, child_activity_pointer, "v5701", "1234");
            method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);



            test_class.test3(ot_helper, child_activity_pointer, "user@user.com", "1234");
            method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);




            foreach (var activity_entry in method_activity_lists)
            {
                if (activity_entry.Status != ActivityStatusCode.Ok)
                {

                    OTSH.addEventToActivity(MAO, "multi-sequence operation failed at:" + activity_entry.DisplayName, OTSH.C_tag_empty_tags_object);
                    
                    if (ErrorTagsCodes.shouldWeRMAOyOperation(activity_entry, ErrorTagsCodes.C_context_https_request) == false)
                    {
                        OTSH.passStatusAndEndActivity(MAO, activity_entry);
                        return;
                    }
                }
            }

            Thread.Sleep(5000);
        }


        MAO.Stop();
    }
}