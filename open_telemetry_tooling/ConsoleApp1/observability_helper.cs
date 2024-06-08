using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
class ObsHelper
{
    public static string CV_exception_message = "exception";
    public static string CV_error_code = "error_code";
    public static string CV_uid = "entity UID";
    public static string CV_input_value = "Input Value";

    public TracerProvider IV_tracer_provider;
    public ActivitySource IV_activity_source;

    public ObsHelper(TracerProvider trace_provider, ActivitySource acivity_source)
    {
        this.IV_tracer_provider = trace_provider;
        this.IV_activity_source = acivity_source;

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

    public static void passStatusAndEndSpan(Activity parent_activity, Activity child_activity_pointer)
    {
        parent_activity.SetStatus(child_activity_pointer.Status);
        parent_activity.SetTag(ObsHelper.CV_error_code, child_activity_pointer.GetTagItem(ObsHelper.CV_error_code));
        parent_activity.Stop();
        return;

    }

    public static bool checkStatusAndEndSpan(Activity parent_activity, Activity child_activity_pointer)
    {
        if (child_activity_pointer.Status == ActivityStatusCode.Ok) { return true; }

        parent_activity.SetStatus(child_activity_pointer.Status);
        if (child_activity_pointer.GetTagItem(ObsHelper.CV_error_code) != null) {
            parent_activity.SetTag(ObsHelper.CV_error_code, child_activity_pointer.GetTagItem(ObsHelper.CV_error_code));          
        }        
        parent_activity.Stop();
        return false; 

    }


    public static void addEventThenEndSpan(Activity activity, string event_summary, int error_code, Dictionary<string,Object>? tag_items)
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
        activity.SetTag(ObsHelper.CV_error_code, error_code);
        activity.SetStatus(ActivityStatusCode.Error);
        activity.Stop();
    }

    public static void addEventToSpan(Activity activity, string event_summary, Dictionary<string, Object>? tag_items)
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


class ChildActivityPointer
{
    private Activity? IV_child_activity_pointer;
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