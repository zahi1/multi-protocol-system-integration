namespace Servers;

using Grpc.Core;
using Services;
using SimpleRpc.Transports.Http.Client;
using SimpleRpc.Serialization.Hyperion;
using Microsoft.Extensions.DependencyInjection;
using GasContract;
using SimpleRpc.Transports;

/// <summary>
/// Service for Gas Pressure Management
/// </summary>
public class GasContainerService : Services.GasContainerService.GasContainerServiceBase
{
    /// <summary>
    /// Increase the gas mass in the container.
    /// </summary>
    /// <param name="input">DoubleMsg with the amount of mass to increase.</param>
    /// <param name="context">gRPC server call context.</param>
    /// <returns>Empty message indicating completion.</returns>
    public override Task<Empty> IncreaseMass(DoubleMsg input, ServerCallContext context)
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
        var gasService = sp.GetService<IGasContainerService>();
        gasService.IncreaseMass(input.Value);

        Console.WriteLine("Routing GRPC IncreaseMass Call");

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Decrease the gas mass in the container.
    /// </summary>
    /// <param name="input">DoubleMsg with the amount of mass to decrease.</param>
    /// <param name="context">gRPC server call context.</param>
    /// <returns>Empty message indicating completion.</returns>
    public override Task<Empty> DecreaseMass(DoubleMsg input, ServerCallContext context)
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
        var gasService = sp.GetService<IGasContainerService>();
        gasService.DecreaseMass(input.Value);

        Console.WriteLine("Routing GRPC DecreaseMass Call");

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Retrieve the current pressure in the container.
    /// </summary>
    /// <param name="input">Empty message.</param>
    /// <param name="context">gRPC server call context.</param>
    /// <returns>DoubleMsg containing the current pressure.</returns>
    public override Task<DoubleMsg> GetPressure(Empty input, ServerCallContext context)
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
        var gasService = sp.GetService<IGasContainerService>();
        var pressure = gasService.GetPressure();

        Console.WriteLine("Routing GRPC GetPressure Call");

        return Task.FromResult(new DoubleMsg { Value = pressure });
    }

    /// <summary>
    /// Check if the gas container is destroyed.
    /// </summary>
    /// <param name="input">Empty message.</param>
    /// <param name="context">gRPC server call context.</param>
    /// <returns>BoolMsg indicating if the container is destroyed.</returns>
    public override Task<BoolMsg> IsDestroyed(Empty input, ServerCallContext context)
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
        var gasService = sp.GetService<IGasContainerService>();
        var isDestroyed = gasService.IsDestroyed();

        Console.WriteLine("Routing GRPC IsDestroyed Call");

        return Task.FromResult(new BoolMsg { Value = isDestroyed });
    }
}
