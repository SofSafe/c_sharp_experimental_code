using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;


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
    public string Message { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class JsonEndpointController : ControllerBase
{
    [HttpPost]
    public ActionResult<string> Post([FromBody] JsonInput input)
    {

        if (input == null || string.IsNullOrEmpty(input.Message))
        {
            return BadRequest("Invalid input");
        }

        // Process the input JSON here
        string response = $"Received message: {input.Message}";

        return Ok(response);
    }
}