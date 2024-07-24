using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;



interface IOdcBaseClassInheritor: IOdcBaseClass
{
    List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    
}

//TODO update on SBD
interface IOdcBaseClass
{    
    string ToJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    void NotPublicFromJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonTextReader reader, string activity_name_prefix = "");

    bool PopulateFromJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string json_string, string activity_name_prefix = "");
    void NotPublicToJsonInternal(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonWriter json_writer, string activity_name_prefix = "");
    
    bool SetInstanceNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, bool new_disable_setting, string activity_name_prefix = "");
    bool ValidateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");
    bool P_data_not_used { get; }
    string P_organized_data_identifier { get; }
    DateTime P_last_validation_attempt { get; }
    bool P_last_validation_result { get; }

    Dictionary<string,IDcoBaseClass> P_Dco_child_objects { get; }

    List<IOdcBaseClass> P_odc_child_objects { get; }

    Dictionary<string, List<string>> P_last_validation_result_details { get; }
}

class OrganizedDataClassBase : IOdcBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    private bool IV_allow_disabling_instance = false;
    private bool IV_instance_not_used = false;
    private readonly Dictionary<string,IDcoBaseClass> IV_Dco_child_objects = new Dictionary<string,IDcoBaseClass>();
    private readonly List<IOdcBaseClass> IV_Odc_child_objects = new List<IOdcBaseClass>();
    //TODO add to SBD
    protected Dictionary<string, INamedListOfOdcInstances> IV_named_odc_lists = new Dictionary<string, INamedListOfOdcInstances>();
    //TODO add to SBD
    protected Dictionary<string, IEnumerable<IDcoBaseClass>> IV_named_dco_lists = new Dictionary<string, IEnumerable<IDcoBaseClass>>();

    private Dictionary<string, List<string>> IV_last_validation_result_details = new Dictionary<string, List<string>>();

    private DateTime IV_last_validation_time = OrganizedDataClassBase.C_unset_time;
    private bool IV_last_validation_result = false;

    private string IV_organized_data_identifier = "Unset Odc Identifier";

    public delegate List<string> CVD_validation_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
    protected CVD_validation_method_delegate IV_special_validation_method = null;



    public List<IOdcBaseClass> P_odc_child_objects
    {
        get { return this.IV_Odc_child_objects; }
    }
    //TODO add to SBD
    public Dictionary<string,IDcoBaseClass> P_Dco_child_objects
    {
        get { return this.IV_Dco_child_objects; }
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


    public string ToJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        //because this is a wrapper method all observability and data vaidation is handled in NotPublicJsonInternal
        StringBuilder string_builder = new StringBuilder();
        StringWriter string_writer = new StringWriter(string_builder);
        JsonWriter json_writer = new JsonTextWriter(string_writer);
        
        this.NotPublicToJsonInternal(ot_helper,method_activity_pointer,ref json_writer, activity_name_prefix);
        
        json_writer.Close();
        return string_writer.ToString();
    }

    public void NotPublicToJsonInternal(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonWriter json_writer, string activity_name_prefix = "")
    {

        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".NotPublicJsonInternal");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        if (this.IV_instance_not_used == true)
        {
            OTSH.SetSuccessEndActivity(ETR);
            return;
        }

        activity_name_prefix += this.IV_organized_data_identifier + "(" + this.GetType().Name + ") > ";
        List<string> duplicate_id_errors = new List<string>();
        List<string> null_value_errors = new List<string>();
        List<string> failed_json_errors = new List<string>();

        Dictionary<string, List<string>> issues_found = new Dictionary<string, List<string>>();
        bool failed_flag = false;
        string error_msg;

        json_writer.WriteStartObject();
        json_writer.WritePropertyName("P_organized_data_identifier");
        json_writer.WriteValue(this.P_organized_data_identifier);

        //// get JSON values from all individual Dco objects 
        Dictionary<string, byte> individual_dco_data_ids = new Dictionary<string, byte>();
        int instance_index = -1;
        foreach (IDcoBaseClass dco_instance in this.IV_Dco_child_objects.Values)
        {
            instance_index++;
            if (dco_instance == null)
            {
                null_value_errors.Add(activity_name_prefix + "> IV_Dco_child_objects index:" + instance_index.ToString() + " is null.");
                failed_flag = true;
                continue;
            }
            if (dco_instance.P_value_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_dco_data_ids.ContainsKey(dco_instance.P_data_identifier))
            {
                duplicate_id_errors.Add("duplicate data object instance id: " + dco_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }
            individual_dco_data_ids.Add(dco_instance.P_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do            

            if(dco_instance.P_export_to_json == false) { continue; }
            try
            {
                json_writer.WritePropertyName(dco_instance.P_data_identifier);
                if (dco_instance.P_is_set)
                {
                    json_writer.WriteValue(dco_instance.P_value_as_object);
                }
                else
                {
                    json_writer.WriteNull();
                }                
            } catch (Exception error_object)
            {
                failed_json_errors.Add($"failed to convert {dco_instance.P_data_identifier} with exception: {error_object.Message}");
                failed_flag = true;
            }
                       
        }
        ////</end> get JSON values from all individual Dco objects        


        //// get JSON values from all named Lists of Dco child objects
        int loop1 = -1;
        bool json_array_active = false;
        foreach (string dco_list_key in this.IV_named_dco_lists.Keys)
        {
            IEnumerable<IDcoBaseClass> child_class_list;
            if (this.IV_named_dco_lists.TryGetValue(dco_list_key, out child_class_list) == false)
            {
                null_value_errors.Add(activity_name_prefix + "> " + dco_list_key + "> child_class_list is null.");
                failed_flag = true;
                continue;
            }
            loop1++;

            int loop2 = -1;
            foreach (IDcoBaseClass dco_instance in child_class_list)
            {
                loop2++;
                string loop_index = dco_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() + "> ";
                if (dco_instance == null)
                {
                    null_value_errors.Add(activity_name_prefix + "> " + loop_index + "dco_instance is null.");
                    failed_flag = true;
                    continue;
                }

                if (dco_instance.P_export_to_json == false) { continue; }

                if (loop2 == 0)
                {
                    json_writer.WritePropertyName(dco_list_key);
                    json_writer.WriteStartArray();
                    json_array_active = true;
                }


                json_writer.WriteStartArray();
                json_writer.WriteValue(dco_instance.P_data_identifier);
                try
                {

                    if (dco_instance.P_is_set == true && dco_instance.P_value_not_used == false)
                    {
                        json_writer.WriteValue(dco_instance.P_value_as_object);
                    }
                    else
                    {
                        json_writer.WriteNull();                        
                    }
                }                
                catch (Exception error_object)
                {
                    failed_json_errors.Add($"failed to convert dco list {loop_index} with exception: {error_object.Message}");
                    failed_flag = true;
                }
                json_writer.WriteEndArray();

            }
            if (json_array_active == true)
            {
                json_writer.WriteEndArray();
                json_array_active = false;
            }

        }
        ////</end> Get JSON values from all named Lists of Dco child objects

        //// Get JSON values from all Odc child objects
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
                error_msg = "duplicate organized data instance id: " + odc_instance.P_organized_data_identifier;
                duplicate_id_errors.Add(error_msg);
                OTSH.AddEventToActivity(ETR, error_msg);
                failed_flag = true;
                continue;
            }
            individual_odc_data_ids.Add(odc_instance.P_organized_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            odc_instance.NotPublicToJsonInternal(ot_helper, child_activity_pointer, ref json_writer, activity_name_prefix);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                failed_flag = true;
            }           
        }
        ////</end> Get JSON values from all Odc child objects

        //// Get JSON values from all named Lists of Odc child objects
        loop1 = -1;
        json_array_active = false;
        foreach (string odc_list_key in this.IV_named_odc_lists.Keys)
        {
            INamedListOfOdcInstances child_class_list;
            
            if (this.IV_named_odc_lists.TryGetValue(odc_list_key, out child_class_list) == false)
            {
                null_value_errors.Add(activity_name_prefix + "> " + odc_list_key + "> child_class_list is null.");
                failed_flag = true;
                continue;
            }
            loop1++;
            int loop2 = -1;            
            foreach (IOdcBaseClass odc_instance in child_class_list)
            {
                loop2++;
                string loop_index = odc_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() + "> ";

                if (odc_instance == null)
                {
                    null_value_errors.Add(activity_name_prefix + "> " + loop_index + "odc_instance is null.");
                    failed_flag = true;
                    continue;
                }

                if(loop2 == 0)
                {
                    json_writer.WritePropertyName(odc_list_key);

                    json_writer.WriteStartArray();
                    json_array_active = true;
                }


                odc_instance.NotPublicToJsonInternal(ot_helper, child_activity_pointer, ref json_writer,activity_name_prefix);
                if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
                {
                    failed_flag = true;
                }
            }

            if (json_array_active == true)
            {
                json_writer.WriteEndArray();
                json_array_active = false;
            }

        }
        ////</end> Get JSON values from all named Lists of Odc child objects

        //// failed state logic
        if (failed_flag == true)
        {

            if (null_value_errors.Count > 0)
            {
                issues_found.Add(activity_name_prefix + " null instances", null_value_errors);
                foreach (string error in null_value_errors)
                {
                    OTSH.AddEventToActivity(ETR, error);
                }
            }

            if (failed_json_errors.Count > 0)
            {
                issues_found.Add(activity_name_prefix + " json errors", failed_json_errors);
                foreach (string error in failed_json_errors)
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

            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            json_writer.WriteEndObject();
            return;
        }
        ////</end> failed state logic

        OTSH.SetSuccessEndActivity(ETR);
        json_writer.WriteEndObject();
        return;
    }


    public bool PopulateFromJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string json_text, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".PopulateFromJson");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".PopulateFromJson");
        validator.AddEntry(new DataVR(json_text, "json_text").AddRule(SDV.R_len_greater_0));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation

        StringReader string_reader = new StringReader(json_text);
        JsonTextReader reader = new JsonTextReader(string_reader);


        this.NotPublicFromJson(ot_helper, method_activity_pointer, ref reader, "top-level");

        return true;
    }
    public void NotPublicFromJson(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref JsonTextReader reader, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".NotPublicFromJson");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template
        
        string last_property_name = "";
        string current_property_name = "";


        //// create structured pointers to things need to reference alot
        Dictionary<string, IDcoBaseClass> child_dco_instances_by_id = new Dictionary<string, IDcoBaseClass>();

        foreach(IDcoBaseClass child_odc in this.IV_Dco_child_objects.Values)
        {
            child_dco_instances_by_id[child_odc.P_data_identifier] = child_odc;
        }
        ////</end> create structured pointers to things need to reference alot


        JsonToken last_token_type = JsonToken.None;
        JsonToken current_token_type = JsonToken.None;

        bool within_array = false;
        int arrays_found = 0;
        string json_array_name = "";
        bool fail_flag = false;
        List<JsonToken> ignored_token_types = new List<JsonToken>() {
        JsonToken.Comment, JsonToken.None, JsonToken.Undefined
        };
        while (reader.Read())
        {
            last_token_type = current_token_type;
            current_token_type = reader.TokenType;
            if (reader.TokenType == JsonToken.PropertyName)
            {
                last_property_name = current_property_name;
                current_property_name = reader.Value.ToString();
                continue;
            }
            //last_property_name = current_property_name;


            if (last_property_name == current_property_name)
            {
                current_property_name = "";
                last_property_name = "";
            }

            //Console.WriteLine($"{current_property_name}: Type: {reader.TokenType}");


            //handle a dco instance in an array
            if (reader.TokenType == JsonToken.StartArray && within_array == true)
            {
                //Console.WriteLine($"handling  dco instance for array: {json_array_name}");
                //todo add dco list support
                continue;
            }

            //handle an Odc instance in an array
            if (reader.TokenType == JsonToken.StartObject && within_array == true)
            {
                //Console.WriteLine($"calling ProcessJsonToken for array: {json_array_name},  property: {current_property_name}");
                foreach(string odc_list_key in this.IV_named_odc_lists.Keys)
                {
                    if (odc_list_key != json_array_name
                        && this.IV_named_odc_lists[odc_list_key].P_group_name != json_array_name) { continue; }
                    IOdcBaseClass odc_instance;
                    this.IV_named_odc_lists[odc_list_key].CreateNewEntry(ot_helper,child_activity_pointer,"not set by json",out odc_instance,activity_name_prefix);

                    odc_instance.NotPublicFromJson(ot_helper, method_activity_pointer, ref reader, activity_name_prefix);
                    break;
                }
                continue;
            }


            if (reader.TokenType == JsonToken.StartArray)
            {
                json_array_name = current_property_name;
                arrays_found++;
                within_array = true;
                continue;
            }

            if (reader.TokenType == JsonToken.EndArray)
            {
                json_array_name = "";
                within_array = false;
                continue;
            }


            //handle child ODC instance
            if (reader.TokenType == JsonToken.StartObject)
            {                
                continue;
            }

            if (reader.TokenType == JsonToken.EndObject)
            {
                return;
            }

            if (current_property_name.Length == 0)
            {
                continue;
            }

            if(reader.Value == null || ignored_token_types.Contains(reader.TokenType) == true)
            {
                continue;
            }

            /// handle the data identifier for the current Odc instance
            if(current_property_name == "P_organized_data_identifier")
            {
                string data_id = System.String.Empty;
                try
                {
                    this.IV_organized_data_identifier = reader.Value.ToString();
                }
                catch
                {
                    //todo add error handling
                    this.IV_organized_data_identifier = "json failed to set";
                    fail_flag = true;                    
                }
                continue;
            }
            ///</end> handle the data identifier for the current Odc instance


            //// this can only be a value, so put it in Dco
            IDcoBaseClass dco_instance = null;
            //find by instance variable name
            if(this.IV_Dco_child_objects.ContainsKey(current_property_name) == true)
            {
                dco_instance = this.IV_Dco_child_objects[current_property_name];
            } 
            else if(child_dco_instances_by_id.ContainsKey(current_property_name) == true)
            {
                dco_instance = child_dco_instances_by_id[current_property_name];
            } 
            else
            {
                //todo add error handling for failing to find a value
                continue; 
            }
            //Console.WriteLine(dco_instance.P_data_identifier);
            object json_value = null;            
            
            try
            {
                if (dco_instance.P_type == typeof(System.String))
                {
                    json_value = reader.Value.ToString();
                }
                else if (dco_instance.P_type == typeof(DateTime)) {
                    json_value = DateTime.Parse(reader.Value.ToString());
                } 
                else
                { 
                    json_value = JsonConvert.DeserializeObject(reader.Value.ToString(),dco_instance.P_type);                    
                }

                
            } catch (Exception error_object)
            {
                //todo add error handling
                fail_flag = true;
                continue;
            }
            dco_instance.SetValueFromObject(json_value);
            //Console.WriteLine($"{current_property_name}");

            //Console.WriteLine($"{current_property_name}: Data: {dco_instance.P_value_as_object}");            
            ////</end> this can only be a value, so put it in Dco/

        }

        //Console.WriteLine("returing2");
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

        //// check all individual Dco objects 
        Dictionary<string, byte> individual_dco_data_ids = new Dictionary<string, byte>();
        int instance_index = -1;
        foreach (IDcoBaseClass dco_instance in this.IV_Dco_child_objects.Values)
        {
            instance_index++;
            if (dco_instance == null)
            {
                null_value_errors.Add(activity_name_prefix + "> IV_Dco_child_objects index:" + instance_index.ToString() +  " is null.");
                failed_flag = true;
                continue;
            }
            if(dco_instance.P_value_not_used == true) { continue; }
            //// logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do
            if (individual_dco_data_ids.ContainsKey(dco_instance.P_data_identifier))
            {
                duplicate_id_errors.Add("duplicate data object instance id: " + dco_instance.P_data_identifier);
                failed_flag = true;
                continue;
            }
            individual_dco_data_ids.Add(dco_instance.P_data_identifier, 1);
            ////</end> logic to detect when someone duplicated a data identifier in the same class, which they shouldn't do

            if (!dco_instance.RunValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix))
            {
                issues_found.Add(activity_name_prefix + dco_instance.P_data_identifier,
                dco_instance.P_last_validation_result_details);
                failed_flag = true;
            }

        }
        ////</end> check all individual Dco objects        


        //// check all named Lists of Dco child objects
        int loop1 = -1;
        foreach (string dco_list_key in this.IV_named_dco_lists.Keys)
        {
            IEnumerable<IDcoBaseClass> child_class_list;
            if (this.IV_named_dco_lists.TryGetValue(dco_list_key, out child_class_list) == false)
            {
                null_value_errors.Add(activity_name_prefix + "> " + dco_list_key + "> child_class_list is null.");
                failed_flag = true;
                continue;
            }
            loop1++;
            
            int loop2 = -1;
            string loop_index = dco_list_key + " > loop index:" + loop1.ToString() + "-" + loop2.ToString() + "> ";



            foreach (IDcoBaseClass dco_instance in child_class_list)
            {
                if (dco_instance == null)
                {
                    null_value_errors.Add(activity_name_prefix + "> " + loop_index + "dco_instance is null.");
                    failed_flag = true;
                    continue;
                }

                if (dco_instance.P_value_not_used == true) { continue; }
                if (!dco_instance.RunValidationMethod(ot_helper, child_activity_pointer, activity_name_prefix + loop_index))
                {
                    issues_found.Add(activity_name_prefix + dco_instance.P_data_identifier + ">" + loop_index,
                    dco_instance.P_last_validation_result_details);
                    failed_flag = true;
                }
            }
        }
        ////</end> check all named Lists of Dco child objects

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
                error_msg = "duplicate organized data instance id: " + odc_instance.P_organized_data_identifier;
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
            INamedListOfOdcInstances child_class_list;
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


    private void RegisterDcoAndOdcInstances()
    {
        Type class_type = this.GetType();
        foreach (var field_entry in class_type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            FieldInfo field_info = class_type.GetField(field_entry.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object field_value = field_info.GetValue(this);
            if (field_value == null) { continue; }

            /// register individual Dco class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IDcoBaseClass)))
            {
                this.IV_Dco_child_objects.Add(field_info.Name, field_value as IDcoBaseClass);
                continue;
            }
            ///</end> register individual Dco class instances


            /// register individual Odc class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(IOdcBaseClassInheritor)))
            {                
                this.IV_Odc_child_objects.Add(field_value as IOdcBaseClass);
                continue;
            }
            ///</end> register individual Odc class instances
            
            /// register lists of Odc class instances
            if (field_value.GetType().GetInterfaces().Contains(typeof(INamedListOfOdcInstances)))
            {
                INamedListOfOdcInstances named_list = field_value as INamedListOfOdcInstances;
                string list_name = string.Empty;
                if(System.String.IsNullOrEmpty(named_list.P_group_name) == false)
                {
                    list_name = named_list.P_group_name;
                } else
                {
                    list_name = field_info.Name;
                }               
                this.IV_named_odc_lists.Add(list_name,field_value as INamedListOfOdcInstances);
                continue;
            }
            ///</end> register lists of Odc class instances



        }
    }



    public OrganizedDataClassBase(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string organized_data_identifier,
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
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
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
        
        this.RegisterDcoAndOdcInstances();
        OTSH.SetSuccessEndActivity(ETR);
    }
}


interface INamedListOfOdcInstances: INamedListOfInstances
{
    bool CreateNewEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, out IOdcBaseClass new_instance, string activity_name_prefix = "");
}
class NamedListOfOdcInstances<organized_data_class>: NamedListOfInstances<organized_data_class>
{
    protected bool IV_allow_disabling_instances = false;
    protected bool IV_allow_empty_data_identifiers = false;    
    public NamedListOfOdcInstances(string group_name, bool allow_disabling_instances,bool allow_empty_data_identifiers) : base(group_name)
    {
        this.IV_allow_disabling_instances = allow_disabling_instances;
        this.IV_allow_empty_data_identifiers = allow_empty_data_identifiers;
    }
}