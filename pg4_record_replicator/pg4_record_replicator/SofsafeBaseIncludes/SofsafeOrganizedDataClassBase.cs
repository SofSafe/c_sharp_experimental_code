using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


interface IOdcBaseClassInheritor: IOdcBaseClass
{
    List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
}

//TODO update on SBD
interface IOdcBaseClass
{
    bool SetInstanceNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, bool new_disable_setting, string activity_name_prefix = "");
    bool ValidateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");
    bool P_data_not_used { get; }
    string P_organized_data_identifier { get; }
    DateTime P_last_validation_attempt { get; }
    bool P_last_validation_result { get; }

    List<IDcoBaseClass> P_Sdo_child_objects { get; }

    List<IOdcBaseClass> P_odc_child_objects { get; }

    Dictionary<string, List<string>> P_last_validation_result_details { get; }
}

class SofsafeOrganizedDataClassBase : IOdcBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    private bool IV_allow_disabling_instance = false;
    private bool IV_instance_not_used = false;
    private readonly List<IDcoBaseClass> IV_Sdo_child_objects = new List<IDcoBaseClass>();
    private readonly List<IOdcBaseClass> IV_Odc_child_objects = new List<IOdcBaseClass>();
    //TODO add to SBD
    protected Dictionary<string, IEnumerable<IOdcBaseClass>> IV_named_odc_lists = new Dictionary<string, IEnumerable<IOdcBaseClass>>();
    //TODO add to SBD
    protected Dictionary<string, IEnumerable<IDcoBaseClass>> IV_named_sdo_lists = new Dictionary<string, IEnumerable<IDcoBaseClass>>();

    private Dictionary<string, List<string>> IV_last_validation_result_details = new Dictionary<string, List<string>>();

    private DateTime IV_last_validation_time = SofsafeOrganizedDataClassBase.C_unset_time;
    private bool IV_last_validation_result = false;

    private string IV_organized_data_identifier = "Unset Odc Identifier";

    public delegate List<string> CVD_validation_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
    protected CVD_validation_method_delegate IV_special_validation_method = null;



    public List<IOdcBaseClass> P_odc_child_objects
    {
        get { return this.IV_Odc_child_objects; }
    }
    //TODO add to SBD
    public List<IDcoBaseClass> P_Sdo_child_objects
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
    

    public bool SetInstanceNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, bool new_disable_setting, string activity_name_prefix = "")
    {
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".setInstanceDisable");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("new_disable_setting", new_disable_setting);

        if (new_disable_setting == true && this.IV_allow_disabling_instance == false)
        {
            OTSH.AddEventThenEndActivity(ETR, "can't set instance to disabled when IV_allow_disabling_instance is false", ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_instance_not_used = new_disable_setting;
        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


    public bool ValidateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string activity_name_prefix="")
    {
        Activity ETR = ot_helper.StartActivityWrapper("OrganizedDataClassBaseClass.validateData");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Organized Data Id", this.IV_organized_data_identifier);


        //set to default safe values
        this.IV_last_validation_result = false;
        this.IV_last_validation_result_details.Clear();
        
        this.IV_last_validation_time = DateTime.Now;

        activity_name_prefix += this.IV_organized_data_identifier + "(" + this.GetType().Name + ") > ";

        List<string> duplicate_id_errors = new List<string>();
        List<string> null_value_errors = new List<string>();

        Dictionary<string, List<string>> issues_found = new Dictionary<string, List<string>>();
        bool failed_flag = false;
        string error_msg;

        if (this.IV_instance_not_used == true)
        {
            OTSH.AddEventThenEndActivity(ETR, activity_name_prefix + " this instance is set to disabled", ErrorTagsCodes.C_input_validation);
            return false;
        }

        //// check all individual Sdo objects 
        Dictionary<string, byte> individual_sdo_data_ids = new Dictionary<string, byte>();
        int instance_index = -1;
        foreach (IDcoBaseClass sdo_instance in this.IV_Sdo_child_objects)
        {
            instance_index++;
            if (sdo_instance == null)
            {
                null_value_errors.Add(activity_name_prefix + "> IV_Sdo_child_objects index:" + instance_index.ToString() +  " is null.");
                failed_flag = true;
                continue;
            }
            if(sdo_instance.P_value_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_sdo_data_ids.ContainsKey(sdo_instance.P_data_identifier))
            {
                duplicate_id_errors.Add("duplicate data object instance id: " + sdo_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }
            individual_sdo_data_ids.Add(sdo_instance.P_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (!sdo_instance.RunValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix))
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
            IEnumerable<IDcoBaseClass> child_class_list;
            if (this.IV_named_sdo_lists.TryGetValue(sdo_list_key, out child_class_list) == false)
            {
                null_value_errors.Add(activity_name_prefix + "> " + sdo_list_key + "> child_class_list is null.");
                failed_flag = true;
                continue;
            }
            loop1++;
            
            int loop2 = -1;
            string loop_index = sdo_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() + "> ";



            foreach (IDcoBaseClass sdo_instance in child_class_list)
            {
                if (sdo_instance == null)
                {
                    null_value_errors.Add(activity_name_prefix + "> " + loop_index + "sdo_instance is null.");
                    failed_flag = true;
                    continue;
                }

                if (sdo_instance.P_value_not_used == true) { continue; }
                if (!sdo_instance.RunValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix + loop_index))
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
        instance_index = -1;
        foreach (IOdcBaseClass odc_instance in this.IV_Odc_child_objects)
        {
            if (odc_instance == null)
            {
                null_value_errors.Add(activity_name_prefix + "> IV_Odc_child_objects index:" + instance_index.ToString() + " is null.");
                failed_flag = true;
                continue;
            }
            if (odc_instance.P_data_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_odc_data_ids.ContainsKey(odc_instance.P_organized_data_identifier))
            {
                error_msg = "duplicate grouped data instance id: " + odc_instance.P_organized_data_identifier;
                duplicate_id_errors.Add(error_msg);
                OTSH.AddEventToActivity(ETR, error_msg);
                failed_flag = true; 
                continue;
            }
            individual_odc_data_ids.Add(odc_instance.P_organized_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (odc_instance.ValidateData(ot_helper, child_activity_pointer, activity_name_prefix) == false)
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
            if(this.IV_named_odc_lists.TryGetValue(odc_list_key, out child_class_list) == false)
            {
                null_value_errors.Add(activity_name_prefix + "> " + odc_list_key + "> child_class_list is null.");
                failed_flag = true;
                continue;
            }
            loop1++;
            int loop2 = -1;
            foreach(IOdcBaseClass odc_instance in child_class_list)
            {               
                loop2++;
                string loop_index = odc_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() +"> ";

                if (odc_instance == null)
                {
                    null_value_errors.Add(activity_name_prefix + "> " + loop_index + "odc_instance is null.");
                    failed_flag = true;
                    continue;
                }

                if (!odc_instance.ValidateData(ot_helper, child_activity_pointer, activity_name_prefix + loop_index))
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

            if(null_value_errors.Count > 0)
            {
                issues_found.Add(activity_name_prefix + " null instances", null_value_errors);
                foreach(string error in null_value_errors)
                {
                    OTSH.AddEventToActivity(ETR, error);
                }
            }

            if (duplicate_id_errors.Count > 0)
            {
                issues_found.Add(activity_name_prefix + "duplicate Data IDs", duplicate_id_errors);
                foreach (string error in duplicate_id_errors)
                {
                    OTSH.AddEventToActivity(ETR, error);
                }
            }            
            this.IV_last_validation_result_details = issues_found;
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }


    private void RegisterSdoOdcIndividualVariables()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields(BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            FieldInfo field_info = class_type.GetField(field_entry.Name,
                BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance);
            object field_value = field_info.GetValue(this);
            if (field_value == null) { continue; }

            /// register individual Sdo class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IDcoBaseClass)))
            {
                this.IV_Sdo_child_objects.Add(field_value as IDcoBaseClass);
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

    //todo add class_type argument to SBD
    public SofsafeOrganizedDataClassBase(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string organized_data_identifier,
        bool allow_disabling_instance = false,
        //todo add argument to SBD
        bool allow_empty_data_identifier = false,
        string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new();
        ///</end> observability template



        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor1");
        
        if (allow_empty_data_identifier == false)
        {
            validator.AddEntry(new DataVR(organized_data_identifier, "organized_data_identifier").AddRule(SDV.R_not_null).AddRule(SDV.R_len_greater_0));
        }

        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        ///</end> input data validation       

        this.IV_allow_disabling_instance = allow_disabling_instance;
        if (organized_data_identifier != null)
        {
            this.IV_organized_data_identifier = organized_data_identifier;
        }
        
        this.RegisterSdoOdcIndividualVariables();
        OTSH.SetSuccessEndActivity(ETR);
    }
}