using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;





class JsonDecodingTesting
{

   
    public void testing(OpenTelemetryConfigHelper ot_helper = null, ChildActivityPointer method_activity_pointer = null)
    {
        ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

        string json_text = """
            {
               "EndpointInterface": "HTTPS",
               "EndpointPort": 1020,
               "DBSettings": [
                 {
                   "ServerAddress": "System1",
                   "Description": "Description1",
                   "UserName": "User1",
                   "Password": "1433",
                   "AuthenticationType": 1,
                   "DatabaseName": "DB1"
                 },
                 {
                   "ServerAddress": "System2",
                   "Description": "Description2",
                   "UserName": "User2",
                   "Password": "1433",
                   "AuthenticationType": 2,
                   "DatabaseName": "DB12"
                 }
               ]
            }
            """;


        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        SDV.runOnceAtRunTimeStart(ot_helper, child_activity_pointer);

        Pg4Settings test22 = new Pg4Settings(ot_helper, child_activity_pointer, "settings");

        OrganizedDataClassBase base_class = test22 as OrganizedDataClassBase;
        base_class.PopulateFromJson(ot_helper, child_activity_pointer, json_text);

        Console.WriteLine(base_class.ToJson(ot_helper, child_activity_pointer));

    }
}