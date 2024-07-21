using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Pg4Settings: SofsafeOrganizedDataClassBase, IOdcBaseClassInheritor
{
    public readonly Dco<string> IV_settings_file_location = new Dco<string>("IV_settings_file_location", ProjectValidationRuleLists.R_file_location);
    public readonly Dco<string> IV_endpoint_interface = new Dco<string>("IV_endpoint_interface", ProjectValidationRuleLists.R_populated_string);
    public readonly Dco<short> IV_endpoint_port = new Dco<short>("IV_endpoint_port", ProjectValidationRuleLists.R_port_above_1200);

    public readonly List<Pg4DbSettingsEntry> IV_db_settings = new List<Pg4DbSettingsEntry>();

    public List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        List<string> return_value = new List<string>();
        return return_value;
    }




    public Pg4Settings(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier = null, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, true, true, activity_name_prefix)
    {
        /// observability template for constructor in an ODC inheritor
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");

        // check if the base class constructor created an error
        if (method_activity_pointer != null && method_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.PassStatusAndEndActivity(ETR, method_activity_pointer.P_pointer);
            return;
        }

        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        /// observability template for constructor in an ODC inheritor


        OTSH.SetSuccessEndActivity(ETR);
        return;
    }

    public bool WriteSettingsFile(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".WriteSettingsFile");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        ///make sure all data in the class is good
        if(this.ValidateData(ot_helper,child_activity_pointer,activity_name_prefix) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }





        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


}


class Pg4DbSettingsEntry: SofsafeOrganizedDataClassBase, IOdcBaseClassInheritor
{

    public readonly Dco<string> IV_server_address = new Dco<string>("IV_server_address", ProjectValidationRuleLists.R_populated_string);

    public readonly Dco<string> IV_username = new Dco<string>("IV_username", ProjectValidationRuleLists.R_populated_string);

    public readonly Dco<string> IV_password = new Dco<string>("IV_password", ProjectValidationRuleLists.R_populated_string);

    public readonly Dco<string> IV_database_name = new Dco<string>("IV_database_name", ProjectValidationRuleLists.R_populated_string);

    public readonly Dco<byte> IV_authentication_type = new Dco<byte>("IV_password", ProjectValidationRuleLists.R_1or2,new List<byte> {1,2});
 

    public List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        List<string> return_value = new List<string>();
        return return_value;
    }

    public Pg4DbSettingsEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string organized_data_identifier=null, string activity_name_prefix = "") : base(ot_helper,method_activity_pointer,organized_data_identifier,true,true, activity_name_prefix)
    {
        /// observability template for constructor in an ODC inheritor
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");

        // check if the base class constructor created an error
        if (method_activity_pointer != null && method_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.PassStatusAndEndActivity(ETR, method_activity_pointer.P_pointer);
            return;
        }

        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        /// observability template for constructor in an ODC inheritor
         

        OTSH.SetSuccessEndActivity(ETR);
        return;
    }

}
     
