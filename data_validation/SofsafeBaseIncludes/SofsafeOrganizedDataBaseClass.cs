using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


interface IOdcBaseClassInheritor
{
    bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);

}

class SofsafeOrganizedDataClassBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected List<SofsafeDataObjectBaseClass> IV_Sdo_child_objects = new List<SofsafeDataObjectBaseClass>();
    protected List<SofsafeOrganizedDataClassBaseClass> IV_Odc_child_objects = new List<SofsafeOrganizedDataClassBaseClass>();
    protected List<IEnumerable<IOdcBaseClassInheritor>> IV_Odc_child_iterable_objects = new List<IEnumerable<IOdcBaseClassInheritor>>();
    protected List<IEnumerable<ISdoBaseClassInheritor>> IV_Sdo_child_iterable_objects = new List<IEnumerable<ISdoBaseClassInheritor>>();


    protected Dictionary<string, List<string>> IV_last_validation_result_details = new Dictionary<string, List<string>>();

    protected DateTime IV_last_validation_time = SofsafeOrganizedDataClassBaseClass.C_unset_time;
    protected bool IV_last_validation_result = false;

    protected string IV_organized_data_identifier;

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
        get { return IV_last_validation_result_details; }
    }



    public bool validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string issue_item_prefix="")
    {
        Activity ETR = ot_helper.startActivityWrapper("OrganizedDataClassBaseClass.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Organized Data Id", this.IV_organized_data_identifier);
       

        issue_item_prefix += this.IV_organized_data_identifier + " > ";


        //// check all individual Sdo objects 
        Dictionary<string, byte> individual_sdo_data_ids = new Dictionary<string, byte>();
        List<string> duplicate_ids = new List<string>();
        Dictionary<string, List<string>> issues_found = new Dictionary<string, List<string>>();
        bool failed_flag = false;
        foreach (SofsafeDataObjectBaseClass sdo_instance in this.IV_Sdo_child_objects)
        {
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_sdo_data_ids.ContainsKey(sdo_instance.P_data_identifier))
            {
                duplicate_ids.Add("duplicate data object instance id: " + sdo_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }
            individual_sdo_data_ids.Add(sdo_instance.P_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (!sdo_instance.runValidationMethod(ot_helper, child_activity_pointer, issue_item_prefix))
            {
                issues_found.Add(issue_item_prefix + sdo_instance.P_data_identifier,
                sdo_instance.P_last_validation_result_details);
                failed_flag = true;
            }

        }
        ////</end> check all individual Sdo objects        


        //// check all Lists of Sdc child objects
        ////</end> check all Lists of Sdc child objects

        //// check all Odc child objects
        ////</end> check all Odc child objects

        //// check all Lists of Odc child objects

        //TODO work stopping point 06/14 5:39pm 
        Dictionary<string, byte> odc_list_data_ids = new Dictionary<string, byte>();

        foreach (IEnumerable<SofsafeOrganizedDataClassBaseClass> child_class_list in this.IV_Odc_child_iterable_objects)
        {
            int loop1 = -1;
            foreach(SofsafeOrganizedDataClassBaseClass odc_instance in child_class_list)
            {
                loop1++;

                //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
                if (odc_list_data_ids.ContainsKey(odc_instance.IV_organized_data_identifier))
                {
                    duplicate_ids.Add("duplicate grouped data instance id: " + odc_instance.IV_organized_data_identifier);
                    failed_flag = true;
                    continue;
                }
                odc_list_data_ids.Add(odc_instance.IV_organized_data_identifier, 1);
                ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do


                if (!odc_instance.validateData(ot_helper, child_activity_pointer, issue_item_prefix))
                {
                    foreach (string data_id in odc_instance.P_last_validation_result_details.Keys)
                    {
                        List<string> issues_list;
                        odc_instance.P_last_validation_result_details.TryGetValue(data_id, out issues_list);
                        if(issues_list == null || issues_list.Count == 0)
                        {
                            continue;
                        }
                        issues_found.Add(odc_instance.IV_organized_data_identifier + ">" + data_id + " at index: " + loop1.ToString(), issues_list);
                    }  
                    failed_flag = true;
                }
            }
        }
        ////</end> check all Lists of Odc child objects





        if (failed_flag)
        {
            if (duplicate_ids.Count > 0)
            {
                issues_found.Add(issue_item_prefix + "duplicate Data IDs", duplicate_ids);
            }

            this.IV_last_validation_result_details = issues_found;
            return false;
        }

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
            if (field_value.GetType().GetInterfaces().Contains(typeof(ISdoBaseClassInheritor)))
            {
                this.IV_Sdo_child_objects.Add(field_value as SofsafeDataObjectBaseClass);
                continue;
            }
            ///</end> register individual Sdo class instances


            /// register individual Odc class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IOdcBaseClassInheritor)))
            {
                this.IV_Odc_child_objects.Add(field_value as SofsafeOrganizedDataClassBaseClass);
                continue;
            }
        }
    }

    public SofsafeOrganizedDataClassBaseClass(string organized_data_identifier)
    {
        this.IV_organized_data_identifier = organized_data_identifier;
        this.registerSdoOdcIndividualVariables();
    }
}