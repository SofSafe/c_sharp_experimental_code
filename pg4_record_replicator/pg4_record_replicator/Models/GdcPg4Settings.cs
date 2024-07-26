using SQLReplicationConfigurator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SofsafeBaseIncludes;


namespace SQLReplicationConfigurator.Models
{
    public class GdcPg4Settings : GroupedDataClassBase
    {
        public readonly Dco<string> IV_settings_file_location = new Dco<string>("IV_settings_file_location", ProjectValidationRuleEnums.R_file_location, null, false, false, false);
        public readonly Dco<string> IV_endpoint_interface = new Dco<string>("EndpointInterface", ProjectValidationRuleEnums.R_populated_string);
        public readonly Dco<short> IV_endpoint_port = new Dco<short>("EndpointPort", ProjectValidationRuleEnums.R_server_port_above_1200);
        public readonly NamedListOfPg4DbSettingsEntry IV_db_settings = new NamedListOfPg4DbSettingsEntry("DBSettings");

        [DisplayName("Endpoint Interface")]
        public string EndpointInterface
        {
            get => this.IV_endpoint_interface.content;
            set => this.IV_endpoint_interface.content = value;
        }

        [DisplayName("Endpoint Port")]
        public short EndpointPort
        {
            get => this.IV_endpoint_port.content;
            set => this.IV_endpoint_port.content = value;
        }

        [DisplayName("DB Settings")]
        public NamedListOfPg4DbSettingsEntry DBSettings
        {
            get => this.IV_db_settings;
        }


        public GdcPg4Settings(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier = null, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, true, true, activity_name_prefix)
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
            if (this.ValidateData(ot_helper, child_activity_pointer, activity_name_prefix) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            string json_text = this.ToJson(ot_helper, child_activity_pointer, activity_name_prefix);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            try
            {
                StreamWriter file = File.CreateText(this.IV_settings_file_location.content);
                file.Write(json_text);
                file.Close();
            }
            catch (Exception error_object)
            {
                OTSH.AddEventThenEndActivity(ETR, $"failed to write file {this.IV_settings_file_location.content} with the error: {error_object.Message}", ErrorTagsCodes.C_file_read_failed);
                return false;
            }

            OTSH.SetSuccessEndActivity(ETR);
            return true;
        }


        public bool SetLocation(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string file_location, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + "GdcPg4Settings.SetLocation");
            if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template

            ////<start> input data validation
            ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + "processRequest.PopulateFromJson");
            validator.AddEntry(new DataVR(file_location, "rep_data").AddRule(RulesCommon.R_populated_string));
            if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }
            ///</end> input data validation   

            this.IV_settings_file_location.content = file_location;
            OTSH.SetSuccessEndActivity(ETR);
            return true;
        }

        public bool ReadSettingsFile(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + "GdcPg4Settings.SetLocation");
            if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template


            string json_text = this.ToJson(ot_helper, child_activity_pointer, activity_name_prefix);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            string json_string = String.Empty;
            try
            {
                StreamReader file = new StreamReader(this.IV_settings_file_location.content);
                json_string = file.ReadToEnd();
                file.Close();
            }
            catch (Exception error_object)
            {
                OTSH.AddEventThenEndActivity(ETR, $"failed to write file {this.IV_settings_file_location.content} with the error: {error_object.Message}", ErrorTagsCodes.C_file_read_failed);
                return false;
            }

            if (this.PopulateFromJson(ot_helper, child_activity_pointer, json_string, activity_name_prefix) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            ///make sure all data in the class is good
            if (this.ValidateData(ot_helper, child_activity_pointer, activity_name_prefix) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            OTSH.SetSuccessEndActivity(ETR);
            return true;
        }


    }


    public class GdcPg4DbSettingsEntry : GroupedDataClassBase
    {
        public readonly Dco<string> IV_server_address = new Dco<string>("ServerAddress", ProjectValidationRuleEnums.R_populated_string);
        //todo add to SBD
        public readonly Dco<string> IV_description = new Dco<string>("Description", ProjectValidationRuleEnums.R_populated_string);

        public readonly Dco<string> IV_username = new Dco<string>("UserName", ProjectValidationRuleEnums.R_populated_string);

        public readonly Dco<string> IV_password = new Dco<string>("Password", ProjectValidationRuleEnums.R_populated_string);

        public readonly Dco<string> IV_database_name = new Dco<string>("DatabaseName", ProjectValidationRuleEnums.R_populated_string);

        public readonly Dco<byte> IV_authentication_type = new Dco<byte>("AuthenticationType", ProjectValidationRuleEnums.R_1or2, new List<byte> { 1, 2 });


        [DisplayName("Server Address")]
        public string ServerAddress
        {
            get { return this.IV_server_address.content; }
            set { this.IV_server_address.content = value; }
        }

        [DisplayName("Description")]
        public string Description
        {
            get => this.IV_description.content;
            set => this.IV_description.content = value;
        }

        [DisplayName("User Name")]
        public string UserName
        {
            get => this.IV_username.content;
            set => this.IV_username.content = value;
        }

        [DisplayName("Password")]
        public string Password
        {
            get => this.IV_password.content;
            set => this.IV_password.content = value;
        }

        [DisplayName("Authentication Type")]
        public enumAuthenticationType AuthenticationType
        {
            get
            {
                if (this.IV_authentication_type.content == (byte)enumAuthenticationType.Windows_Authentication)
                {
                    return enumAuthenticationType.Windows_Authentication;
                }
                else if (this.IV_authentication_type.content == (byte)enumAuthenticationType.SQL_Server_Authentication)
                {
                    return enumAuthenticationType.SQL_Server_Authentication;
                }
                return enumAuthenticationType.Default;

            }
            set { this.IV_authentication_type.content = (byte)value; }
        }

        [DisplayName("Database Name")]
        public string DatabaseName
        {
            get => this.IV_database_name.content;
            set => this.IV_database_name.content = value;
        }



        public GdcPg4DbSettingsEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier = null, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, true, true, activity_name_prefix)
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


    public class NamedListOfPg4DbSettingsEntry : NamedListOfGdcInstances<GdcPg4DbSettingsEntry>, INamedListOfGdcInstances
    {
        public NamedListOfPg4DbSettingsEntry(string group_name, bool allow_disabling_instances = false, bool allow_empty_data_identifiers = false) : base(group_name, allow_disabling_instances, allow_empty_data_identifiers)
        {
        }

        public bool CreateNewEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, out IGdcBaseClass new_instance, string activity_name_prefix = "")
        {
            /// observability template
            if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
            if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
            Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".CreateNewEntry");
            if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            ///</end> observability template


            new_instance = null;
            //// input data validation
            ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".CreateNewEntry");
            validator.AddEntry(new DataVR(organized_data_identifier).AddRule(RulesCommon.R_populated_string));
            if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }
            ///</end> input data validation

            GdcPg4DbSettingsEntry new_entry = new GdcPg4DbSettingsEntry(ot_helper, child_activity_pointer, organized_data_identifier);
            this.Add(new_entry);
            new_instance = new_entry;
            if (OTSH.CheckStatusAndEndActivity(ETR, child_activity_pointer.P_pointer) == false)
            {
                OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
                return false;
            }

            OTSH.SetSuccessEndActivity(ETR);
            return true;
        }
    }


        public enum enumAuthenticationType
        {
            Default = 0,
            Windows_Authentication = 1,
            SQL_Server_Authentication = 2,

        }

}