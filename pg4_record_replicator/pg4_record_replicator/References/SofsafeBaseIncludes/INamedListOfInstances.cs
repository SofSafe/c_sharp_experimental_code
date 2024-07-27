using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SofsafeBaseIncludes;


public interface INamedListOfInstances : IList
{
    bool setGroupName(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string group_name, string activity_name_prefix = "");
    string P_group_name { get; }
}

public class NamedListOfInstances<type_of_list> : List<type_of_list>, INamedListOfInstances
{
    protected string IV_group_name = String.Empty;
    public NamedListOfInstances(string group_name)
    {
        if(group_name != null && group_name.Length > 0)
        {
            this.IV_group_name = group_name;
        }
    }
    public string P_group_name => IV_group_name;

    public bool setGroupName(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string group_name, string activity_name_prefix = "")
    {

        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".setGroupName");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".setGroupName");
        validator.AddEntry(new DataVR(group_name).AddRule(RulesCommon.R_populated_string));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation

        this.IV_group_name = group_name;
        return true;
    }


}

