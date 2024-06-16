using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class EndpointExample
{
    public static void processRequest(string transaction_type, OpenTelemetryConfigHelper ot_helper)
    {
        Activity ETR = ot_helper.startActivityWrapper("EndpointExample.processRequest");
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        Transaction data = new Transaction(ot_helper,child_activity_pointer,"p1s3e20", transaction_type);        
    }

}

