using GasContract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

namespace Servers;

/// <summary>
/// Controller for managing gas pressure operations via REST, backed by RPC communication.
/// </summary>
[Route("/GasPressure")]
[ApiController]
public class GasPressureController : ControllerBase
{
    /// <summary>
    /// Adds mass to the gas container.
    /// </summary>
    /// <param name="mass">The amount of mass to add.</param>
    /// <returns>True if successful, otherwise false.</returns>
    [HttpPost("/increaseMass")]
    public ActionResult<bool> IncreaseMass([FromBody] double mass)
    {
        var sc = new ServiceCollection();
        sc
            .AddSimpleRpcClient(
                "GasPressureService",
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/gasrpc",
                    Serializer = "HyperionMessageSerializer"
                }
            )
            .AddSimpleRpcHyperionSerializer();

        sc.AddSimpleRpcProxy<IGasContainerService>("GasPressureService");

        var sp = sc.BuildServiceProvider();

        Console.WriteLine("Routing REST AddMass Call");

        try
        {
            var gasService = sp.GetService<IGasContainerService>();
            gasService.IncreaseMass(mass);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddMass: {ex.Message}");
            return BadRequest(false);
        }
    }

    /// <summary>
    /// Removes mass from the gas container.
    /// </summary>
    /// <param name="mass">The amount of mass to remove.</param>
    /// <returns>True if successful, otherwise false.</returns>
    [HttpPost("/decreaseMass")]
    public ActionResult<bool> DecreaseMass([FromBody] double mass)
    {
        var sc = new ServiceCollection();
        sc
            .AddSimpleRpcClient(
                "GasPressureService",
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/gasrpc",
                    Serializer = "HyperionMessageSerializer"
                }
            )
            .AddSimpleRpcHyperionSerializer();

        sc.AddSimpleRpcProxy<IGasContainerService>("GasPressureService");

        var sp = sc.BuildServiceProvider();

        Console.WriteLine("Routing REST RemoveMass Call");

        try
        {
            var gasService = sp.GetService<IGasContainerService>();
            gasService.DecreaseMass(mass);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveMass: {ex.Message}");
            return BadRequest(false);
        }
    }

    /// <summary>
    /// Retrieves the current pressure of the gas container.
    /// </summary>
    /// <returns>The current pressure as a double value.</returns>
    [HttpGet("/getPressure")]
    public ActionResult<double> GetPressure()
    {
        var sc = new ServiceCollection();
        sc
            .AddSimpleRpcClient(
                "GasPressureService",
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/gasrpc",
                    Serializer = "HyperionMessageSerializer"
                }
            )
            .AddSimpleRpcHyperionSerializer();

        sc.AddSimpleRpcProxy<IGasContainerService>("GasPressureService");

        var sp = sc.BuildServiceProvider();

        Console.WriteLine("Routing REST GetPressure Call");

        try
        {
            var gasService = sp.GetService<IGasContainerService>();
            return gasService.GetPressure();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPressure: {ex.Message}");
            return BadRequest(-1);
        }
    }

    /// <summary>
    /// Checks if the gas container is destroyed.
    /// </summary>
    /// <returns>True if the container is destroyed; otherwise, false.</returns>
    [HttpGet("/isContainerDestroyed")]
    public ActionResult<bool> IsContainerDestroyed()
    {
        var sc = new ServiceCollection();
        sc
            .AddSimpleRpcClient(
                "GasPressureService",
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5000/gasrpc",
                    Serializer = "HyperionMessageSerializer"
                }
            )
            .AddSimpleRpcHyperionSerializer();

        sc.AddSimpleRpcProxy<IGasContainerService>("GasPressureService");

        var sp = sc.BuildServiceProvider();

        Console.WriteLine("Routing REST IsContainerDestroyed Call");

        try
        {
            var gasService = sp.GetService<IGasContainerService>();
            return gasService.IsDestroyed();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsContainerDestroyed: {ex.Message}");
            return BadRequest(false);
        }
    }
}
