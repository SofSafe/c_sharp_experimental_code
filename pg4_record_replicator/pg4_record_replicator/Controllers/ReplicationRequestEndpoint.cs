using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using SQLReplicationConfigurator.Models;
using SQLReplicationConfigurator;
using SofsafeBaseIncludes;



public class ReplicationRequestEndpoint
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

public class JsonInput
{
    public string json_input { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class P1s1e1CreateReplicationRequest : ControllerBase
{
    [HttpPost]
    public ActionResult<string> Post([FromBody] JsonInput post_keys)
    {
        // setup open telemetry and activity top level objects
        OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("pg4-lan-replication", "0.0.1");
        Activity ETR = ot_helper.StartActivityWrapper("p1s1e1");

        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        // create objects used for monitoring activities
        List<Activity> activity_list = new List<Activity>();

        /// populate and validate data in request
        GdcReplicationRequest request_data = new GdcReplicationRequest(ot_helper, child_activity_pointer, "p1s1e1");
        activity_list.Add(child_activity_pointer.P_pointer);

        request_data.PopulateFromJson(ot_helper, child_activity_pointer, post_keys.json_input);
        activity_list.Add(child_activity_pointer.P_pointer);
        
        request_data.ValidateData(ot_helper,child_activity_pointer);
        activity_list.Add(child_activity_pointer.P_pointer);

        ChildActivityPointer first_failed_activity1;
        if(OTSH.CheckActivityList(ETR,activity_list,out first_failed_activity1) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, first_failed_activity1.P_pointer);
            return StatusCode(ErrorTagsCodes.ConvertErrorToHttpsCode(ETR),ETR.TraceId);
        }
        activity_list.Clear();
        ///</end> populate and validate data in request


        //ReplicationRequestProcessor.processRequest(ot_helper, child_activity_pointer, request_data, frmGlobalSettings.Pg4Setting.DBSettings, "p1s1e1");
        if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {            
            return StatusCode(ErrorTagsCodes.ConvertErrorToHttpsCode(child_activity_pointer.P_pointer),OTSH.GenerateUIErrorString(child_activity_pointer.P_pointer));
        }

        OTSH.SetSuccessEndActivity(ETR);
        return Ok();
    }
}