using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;


//Open Telemetry Sofsafe Helper
class OTSH
{
    //TODO add to c# programing standards that C_ prefix means constant
    public static string C_exception_message = "exception";
    public static string C_error_code = "error_code";
    public static string C_uid = "entity UID";
    public static string C_input_value = "Input Value";
    public static Dictionary<string, Object> C_empty_tags_object = [];
    public static void passStatusAndEndSpan(Activity parent_activity, Activity child_activity)
    {
        parent_activity.SetStatus(child_activity.Status);
        if (child_activity.GetTagItem(OTSH.C_error_code) != null)
        {
            parent_activity.SetTag(OTSH.C_error_code, child_activity.GetTagItem(OTSH.C_error_code));
        }
        parent_activity.Stop();
        return;

    }

    public static bool checkStatusAndEndSpan(Activity parent_activity, Activity child_activity)
    {
        if (child_activity.Status == ActivityStatusCode.Ok) { return true; }

        OTSH.passStatusAndEndSpan(parent_activity, child_activity);
        return false;

    }


    public static void addEventThenEndSpan(Activity activity, string event_summary, int error_code, Dictionary<string, Object> tag_items)
    {
        OTSH.addEventToSpan(activity, event_summary, tag_items);
        activity.SetTag(OTSH.C_error_code, error_code);
        activity.SetStatus(ActivityStatusCode.Error);
        activity.Stop();
    }

    public static void addEventToSpan(Activity activity, string event_summary, Dictionary<string, Object> tag_items)
    {
        if (tag_items != null && tag_items.Count > 0)
        {
            ActivityTagsCollection obs_tags = new ActivityTagsCollection();
            foreach (var dict_key in tag_items.Keys)
            {
                obs_tags.Add(dict_key, tag_items[dict_key]);
            }
            activity.AddEvent(new ActivityEvent(event_summary, DateTime.Now, obs_tags));

        }
        activity.AddEvent(new ActivityEvent(event_summary, DateTime.Now));
    }

}
class OpenTelemetryConfigHelper
{
    private TracerProvider IV_tracer_provider;
    private ActivitySource IV_activity_source;

    public OpenTelemetryConfigHelper(string source_name, string version)
    {
        this.IV_tracer_provider = Sdk.CreateTracerProviderBuilder()
       .AddSource(source_name)
       .ConfigureResource(res => res.AddService("console-messages"))
       .AddConsoleExporter()
       .Build();

         this.IV_activity_source = new ActivitySource(source_name, version);

    }


    //TODO add comments to SBD this has to exist because the StartActivity method returns an Activity instance or null, this means every call to StartActivity will create a CS8600 warning in the compiler and a 8603;
    public Activity startActivityWrapper(string activity_name)
    {

        //these 2 disables have to be done because the StartActivity method returns an Activity instance or null. in this case we know the activity source is good.
#pragma warning disable 8600
        Activity new_activity = this.IV_activity_source.StartActivity(activity_name);
        // "as Activity is used here to make sure the compiler knows this is not null
        #pragma warning disable 8603
        return new_activity as Activity;
    }
}



class ChildActivityPointer
{
    private Activity IV_child_activity_pointer;
    private static Activity CV_default_safe_value = new Activity("ChildActivityPointer default safe value");


    public Activity PIV_child_activity_pointer
    {
        get
        {
            if (this.IV_child_activity_pointer == null)
            {
                return ChildActivityPointer.CV_default_safe_value;
            }
            else
            {
                return this.IV_child_activity_pointer;
            }
        }

        set
        {
            //if an existing Activity instance is stored in IV_child_activity_pointer this only overwrites the pointer stored in this class.
            if (value != null) { this.IV_child_activity_pointer = value; }
        }
    }
}