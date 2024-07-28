using Newtonsoft.Json;
using Pg4DataTransferObjects;
using SofsafeSharedComponents.CCL22;
using SofsafeSharedComponents.CCL23;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PG4ClientLibrary
{
    public class Pg4Client
    {
        
        protected string IV_server_address;
        protected short IV_server_port;

        /// <summary>
        /// Sets the connection settings for the server.
        /// </summary>
        /// <param name="host">The address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <returns>Returns true if the settings were successfully applied; otherwise, false.</returns>
        public bool SetConnectionSettings(string host, short port)
        {
            // Input validation
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host is required.", nameof(host));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException("Port must be a valid number between 1 and 65535.", nameof(port));
            }

            try
            {
                // Set the server address and port
                IV_server_address = host;
                IV_server_port = port;

                // Simulate a configuration application (e.g., saving to a configuration file)
                Console.WriteLine($"Server address set to: {IV_server_address}");
                Console.WriteLine($"Server port set to: {IV_server_port}");

                // Indicate success
                return true;
            }
            catch (Exception ex)
            {
                // Log exception and return false
                Console.WriteLine($"Failed to set connection settings: {ex.Message}");
                return false;
            }
        }

        #region Complex Replicaiton

        /// <summary>
        /// Performs a complex replication operation.
        /// </summary>
        /// <param name="client_id">Crush client ID of the client performing the replication.</param>
        /// <param name="software_version">The version of the Crush.</param>
        /// <param name="primary_table">The name of the primary table involved in the replication.</param>
        /// <param name="primary_field_value">The value of the primary field for the primary table.</param>
        /// <param name="secondary_tables">A list of secondary tables involved in the replication.</param>
        /// <param name="join_statement">The SQL join statement used to link the primary and secondary tables.</param>
        /// <returns>Returns true if the replication is successful; otherwise, false.</returns>
        public async Task<bool> DoComplexReplication(
            string client_id,
            double software_version,
            string primary_table,
            string primary_field_value,
            List<string> secondary_tables,
            string join_statement)
        {

            try
            {
 

                string activity_prefix = "complex_replication";
                OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("pg4-lan-replication-client", "0.0.1");
                //ChildActivityPointer method_activity_pointer = new ChildActivityPointer();
                ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

                Activity ETR = ot_helper.StartActivityWrapper("Pg4Client.DoComplexReplication");
                List<Activity> activity_list = new List<Activity>();
                // Create a GdcReplicationRequest object
                GdcReplicationRequest replicationRequest = new GdcReplicationRequest(ot_helper, child_activity_pointer, activity_prefix);

                // Set the properties
                replicationRequest.IV_source_id.content = client_id;
                replicationRequest.IV_primary_table.content = primary_table;
                replicationRequest.IV_primary_key_value.content = primary_field_value;
                replicationRequest.IV_software_version.content = software_version;
                replicationRequest.IV_sql_join.content = join_statement;

                // Populate secondary tables
                foreach (var table in secondary_tables)
                {
                    replicationRequest.IV_secondary_tables.Add(new Dco<string>(table));
                }

                //Setting complex replication type
                replicationRequest.IV_replication_type.content = 2;

                replicationRequest.ValidateData(ot_helper, child_activity_pointer, activity_prefix);
                activity_list.Add(child_activity_pointer.P_pointer);


                ChildActivityPointer validation_failure;
                if (OTSH.CheckActivityList(ETR, activity_list, out validation_failure) == false)
                {
                    OTSH.PassStatusAndEndActivity(ETR, validation_failure.P_pointer);
                    return false;
                }

                string json_data = replicationRequest.ToJson(ot_helper, child_activity_pointer, activity_prefix);

                return await SendRequest(json_data);
            }
            catch (Exception ex)
            {
                // Log exception and return false
                Console.WriteLine($"Replication failed: {ex.Message}");
                return false;
            }
        }


        #endregion



        #region Simple Replicaiton

        /// <summary>
        /// Performs a simple replication operation.
        /// </summary>
        /// <param name="client_id">Crush client ID of the client performing the replication.</param>
        /// <param name="software_version">The version of the Crush software.</param>
        /// <param name="primary_table">The main table, which will be used in the SQL where statement.</param>
        /// <param name="primary_field_value">The value to look for in the main table's primary key field.</param>
        /// <returns>Returns true if the replication is successful; otherwise, false.</returns>
        public async Task<bool> DoSimpleReplication(
            string client_id,
            double software_version,
            string primary_table,
            string primary_field_value)
        {
            

            try
            {
                string activity_prefix = "simple_replication";
                OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("pg4-lan-replication-client", "0.0.1");
                //ChildActivityPointer method_activity_pointer = new ChildActivityPointer();
                ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

                Activity ETR = ot_helper.StartActivityWrapper("Pg4Client.DoSimpleReplication");
                List<Activity> activity_list = new List<Activity>();
                // Create a GdcReplicationRequest object
                GdcReplicationRequest replicationRequest = new GdcReplicationRequest(ot_helper, child_activity_pointer, activity_prefix);

                // Set the properties
                replicationRequest.IV_source_id.content = client_id;
                replicationRequest.IV_primary_table.content = primary_table;
                replicationRequest.IV_primary_key_value.content = primary_field_value;
                replicationRequest.IV_software_version.content = software_version;
                replicationRequest.IV_sql_join.SetValueNotUsed(ot_helper, child_activity_pointer ,true);


                //Setting simple replication type
                replicationRequest.IV_replication_type.content = 1;

                replicationRequest.ValidateData(ot_helper, child_activity_pointer, activity_prefix);
                activity_list.Add(child_activity_pointer.P_pointer);


                ChildActivityPointer validation_failure;
                if (OTSH.CheckActivityList(ETR, activity_list, out validation_failure) == false)
                {
                    OTSH.PassStatusAndEndActivity(ETR, validation_failure.P_pointer);
                    return false;
                }

                string json_data = replicationRequest.ToJson(ot_helper, child_activity_pointer, activity_prefix);

                return await SendRequest(json_data);

                
            }
            catch (Exception ex)
            {
                // Log exception and return false
                Console.WriteLine($"Simple replication failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendRequest(string jsonPayload)
        {
            // Use HttpClient to send the request
            using (var client = new HttpClient())
            {
                var url = $"http://{IV_server_address}:{IV_server_port}/api/P1s1e1CreateReplicationRequest";
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseString}");

                return response.IsSuccessStatusCode;
            }
        }


        #endregion





    }
}
