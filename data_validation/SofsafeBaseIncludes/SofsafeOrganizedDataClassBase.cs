using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


interface IOdcBaseClassInheritor: IOdcBaseClass
{
    List<string> specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
}

//TODO update on SBD
interface IOdcBaseClass
{
    bool setInstanceNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, bool new_disable_setting, string activity_name_prefix = "");
    bool validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");
    bool P_data_not_used { get; }
    string P_organized_data_identifier { get; }
    DateTime P_last_validation_attempt { get; }
    bool P_last_validation_result { get; }

    List<ISdoBaseClass> P_Sdo_child_objects { get; }

    List<IOdcBaseClass> P_odc_child_objects { get; }

    Dictionary<string, List<string>> P_last_validation_result_details { get; }
}

class SofsafeOrganizedDataClassBase : IOdcBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected bool IV_allow_disabling_instance = false;
    protected bool IV_instance_not_used = false;
    protected readonly List<ISdoBaseClass> IV_Sdo_child_objects = new List<ISdoBaseClass>();
    protected readonly List<IOdcBaseClass> IV_Odc_child_objects = new List<IOdcBaseClass>();
    //TODO add to SBD
    protected Dictionary<string, IEnumerable<IOdcBaseClass>> IV_named_odc_lists = new Dictionary<string, IEnumerable<IOdcBaseClass>>();
    //TODO add to SBD
    protected Dictionary<string, IEnumerable<ISdoBaseClass>> IV_named_sdo_lists = new Dictionary<string, IEnumerable<ISdoBaseClass>>();

    protected Dictionary<string, List<string>> IV_last_validation_result_details = new Dictionary<string, List<string>>();

    protected DateTime IV_last_validation_time = SofsafeOrganizedDataClassBase.C_unset_time;
    protected bool IV_last_validation_result = false;

    protected string IV_organized_data_identifier = null;

    public delegate List<string> CVD_validation_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
    protected CVD_validation_method_delegate IV_special_validation_method = null;



    public List<IOdcBaseClass> P_odc_child_objects
    {
        get { return this.IV_Odc_child_objects; }
    }
    //TODO add to SBD
    public List<ISdoBaseClass> P_Sdo_child_objects
    {
        get { return this.IV_Sdo_child_objects; }
    }
    //TODO add to SBD


    //TODO add P_data_not_used and all supporting logic to SBD
    public bool P_data_not_used
    {
        get { return this.IV_instance_not_used; }
    }
    public string P_organized_data_identifier
    {
        get { return this.IV_organized_data_identifier; }
    }

    public DateTime P_last_validation_attempt
    {
        get { return this.IV_last_validation_time; }
    }


    public bool P_last_validation_result
    {
        get { return this.IV_last_validation_result; }
    }
    public Dictionary<string, List<string>> P_last_validation_result_details
    {
        get { return this.IV_last_validation_result_details; }
    }
    

    public bool setInstanceNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, bool new_disable_setting, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".setInstanceDisable");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("new_disable_setting", new_disable_setting);

        if (new_disable_setting == true && this.IV_allow_disabling_instance == false)
        {
            OTSH.addEventThenEndActivity(ETR, "can't set instance to disabled when IV_allow_disabling_instance is false", ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_instance_not_used = new_disable_setting;
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public bool validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix="")
    {
        Activity ETR = ot_helper.startActivityWrapper("OrganizedDataClassBaseClass.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Organized Data Id", this.IV_organized_data_identifier);


        //set to default safe values
        this.IV_last_validation_result = false;
        this.IV_last_validation_result_details.Clear();
        
        this.IV_last_validation_time = DateTime.Now;

        activity_name_prefix += this.IV_organized_data_identifier + "(" + this.GetType().Name + ") > ";

        List<string> duplicate_ids = new List<string>();
        Dictionary<string, List<string>> issues_found = new Dictionary<string, List<string>>();
        bool failed_flag = false;
        string error_msg;

        if (this.IV_instance_not_used == true)
        {
            OTSH.addEventThenEndActivity(ETR, activity_name_prefix + " this instance is set to disabled", ErrorTagsCodes.C_input_validation);
            return false;
        }

        //// check all individual Sdo objects 
        Dictionary<string, byte> individual_sdo_data_ids = new Dictionary<string, byte>();
        foreach (ISdoBaseClass sdo_instance in this.IV_Sdo_child_objects)
        {
            if(sdo_instance.P_value_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_sdo_data_ids.ContainsKey(sdo_instance.P_data_identifier))
            {
                duplicate_ids.Add("duplicate data object instance id: " + sdo_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }
            individual_sdo_data_ids.Add(sdo_instance.P_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (!sdo_instance.runValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix))
            {
                issues_found.Add(activity_name_prefix + sdo_instance.P_data_identifier,
                sdo_instance.P_last_validation_result_details);
                failed_flag = true;
            }

        }
        ////</end> check all individual Sdo objects        


        //// check all named Lists of Sdo child objects
        int loop1 = -1;
        foreach (string sdo_list_key in this.IV_named_sdo_lists.Keys)
        {
            IEnumerable<ISdoBaseClass> child_class_list;
            this.IV_named_sdo_lists.TryGetValue(sdo_list_key, out child_class_list);
            loop1++;
            int loop2 = -1;
            string loop_index = sdo_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() + "> ";

            foreach (ISdoBaseClass sdo_instance in child_class_list)
            {
                if (sdo_instance.P_value_not_used == true) { continue; }
                if (!sdo_instance.runValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix + loop_index))
                {
                    issues_found.Add(activity_name_prefix + sdo_instance.P_data_identifier + ">" + loop_index,
                    sdo_instance.P_last_validation_result_details);
                    failed_flag = true;
                }
            }
        }
        ////</end> check all named Lists of Sdo child objects

        //// check all Odc child objects
        Dictionary<string, byte> individual_odc_data_ids = new Dictionary<string, byte>();
        foreach (IOdcBaseClass odc_instance in this.IV_Odc_child_objects)
        {
            if (odc_instance.P_data_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_odc_data_ids.ContainsKey(odc_instance.P_organized_data_identifier))
            {
                error_msg = "duplicate grouped data instance id: " + odc_instance.P_organized_data_identifier;
                duplicate_ids.Add(error_msg);
                OTSH.addEventToActivity(ETR, error_msg);
                failed_flag = true; 
                continue;
            }
            individual_odc_data_ids.Add(odc_instance.P_organized_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (odc_instance.validateData(ot_helper, child_activity_pointer, activity_name_prefix) == false)
            {
                foreach (string data_id in odc_instance.P_last_validation_result_details.Keys)
                {
                    List<string> issues_list;
                    odc_instance.P_last_validation_result_details.TryGetValue(data_id, out issues_list);
                    if (issues_list == null || issues_list.Count == 0)
                    {
                        continue;
                    }
                    issues_found.Add(odc_instance.P_organized_data_identifier + ">" + data_id, issues_list);
                }
                failed_flag = true;
            }
        }
        ////</end> check all Odc child objects

        //// check all named Lists of Odc child objects
        loop1 = -1;
        foreach (string odc_list_key in this.IV_named_odc_lists.Keys)
        {
            IEnumerable<IOdcBaseClass> child_class_list;
            this.IV_named_odc_lists.TryGetValue(odc_list_key, out child_class_list);
            loop1++;
            int loop2 = -1;
            foreach(IOdcBaseClass odc_instance in child_class_list)
            {
                loop2++;
                string loop_index = odc_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() +"> ";
                if (!odc_instance.validateData(ot_helper, child_activity_pointer, activity_name_prefix + loop_index))
                {
                    foreach (string data_id in odc_instance.P_last_validation_result_details.Keys)
                    {
                        List<string> issues_list;
                        odc_instance.P_last_validation_result_details.TryGetValue(data_id, out issues_list);
                        if(issues_list == null || issues_list.Count == 0)
                        {
                            continue;
                        }
                        issues_found.Add(odc_instance.P_organized_data_identifier + ">" + data_id + loop_index, issues_list);
                    }  
                    failed_flag = true;
                }
            }
        }
        ////</end> check all named Lists of Odc child objects


        //TODO add validation of specialValidationLogic method from inheriting class, which is set via a delegate

        if (failed_flag == true)
        {
            if (duplicate_ids.Count > 0)
            {
                issues_found.Add(activity_name_prefix + "duplicate Data IDs", duplicate_ids);
            }            
            this.IV_last_validation_result_details = issues_found;
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    private void registerSdoOdcIndividualVariables()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields())
        {
            object field_value = class_type.GetField(field_entry.Name).GetValue(this);
            if (field_value == null) { continue; }

            /// register individual Sdo class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(ISdoBaseClass)))
            {
                this.IV_Sdo_child_objects.Add(field_value as ISdoBaseClass);
                continue;
            }
            ///</end> register individual Sdo class instances


            /// register individual Odc class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IOdcBaseClassInheritor)))
            {
                this.IV_Odc_child_objects.Add(field_value as IOdcBaseClass);
                continue;
            }
            ///</end> register individual Odc class instances
            

        }
    }

    //todo add observability
    public SofsafeOrganizedDataClassBase(string organized_data_identifier,
        bool allow_disabling_instance = false)
    {
        //todo add data valdation
        this.IV_allow_disabling_instance = allow_disabling_instance;
        if (organized_data_identifier == null || organized_data_identifier.Length == 0)
        {
            organized_data_identifier = "organized_data_identifier not set";
        }
        this.IV_organized_data_identifier = organized_data_identifier;
        this.registerSdoOdcIndividualVariables();
    }
}