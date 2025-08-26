# Multi-Protocol Distributed System with Adapters for an Interoperable Networked Architecture

A systems interoperability project that simulates gas pressure in a container and integrates three independently networked components into a single cohesive system via adapters: the container server exposes a **SimpleRPC** endpoint; the input client communicates via **gRPC** (through the input adapter); and the output client communicates via **RESTful HTTP** (through the output adapter).

Short description: An interoperability system that simulates gas pressure in a container by coordinating three independently networked components (Container, Input, Output) through adapters. Each component uses a different communication style, while adapters provide a uniform RPC-style boundary so original components remain unmodified.

Core idea: Pressure depends on gas mass and temperature. Every 2 seconds, the container changes its internal temperature by a random amount. When pressure is below a lower limit, input components increase mass. When pressure is above an upper limit, output components decrease mass. If pressure goes outside the safe range (implosion/explosion), the container is destroyed and the simulation is reset. Input components are not allowed to add mass if pressure is above the lower limit, and output components are not allowed to remove mass if pressure is below the upper limit.

- Container (server): owns the canonical state (mass, temperature, pressure) and applies autonomous temperature changes every 2 seconds.
- Input component: increases mass when pressure is below the configured lower limit.
- Output component: decreases mass when pressure is above the configured upper limit.
- Adapters: provide a uniform RPC-style boundary so each component can keep its own communication style without being modified.

No dependencies were altered in legacy components; only addresses were configured where needed.

## Project goals

- Keep original components intact (no code changes, except addresses).
- Integrate differently networked components in one system using adapters.
- Ensure the system is free of race conditions, deadlocks, and livelocks.
- Use RPC-style wrappers in the adaptation layer.

## Concept overview

- Pressure is proportional to mass and temperature.
- Every 2 seconds, the container adjusts its internal temperature by a random value and checks safety limits.
- If pressure < lower limit: input increases mass (random positive amount).
- If pressure > upper limit: output decreases mass (random positive amount).
- If pressure drops below implosion limit or rises above explosion limit: the container is destroyed and automatically reset to initial state; input/output pause until safe again.

## Project structure / architecture

- InteroperabilityGasPressure/
  - `GasContainerLogic.cs` — thread-safe state and rules (temperature loop, pressure calc, limits, reset)
  - `GasContainerService.cs` — RPC-facing service implementing the domain interface
  - `Server.cs` — hosts the container service on localhost:5000
- GasPressureContract/
  - `IGasContainerService.cs` — contract defining IncreaseMass, DecreaseMass, GetPressure, IsDestroyed
- Adapters/
  - `Server.cs` — hosts an adapter on localhost:5002
  - `GasContainerService.cs` — bridges calls from the adapter to the container service through RPC-style proxy
- Input/
  - `Client.cs` — connects to the adapter and periodically requests mass increase while pressure is below the lower limit
- OutputAdapter/
  - `Server.cs` — hosts a controller-based adapter on localhost:5001
  - `Controllers/GasPressureController.cs` — exposes controller actions and forwards to RPC-style proxy
- Output/
  - `Client.cs` — connects to the controller-based adapter and requests mass decrease while pressure is above the upper limit

Data flow:
1. Container runs and updates temperature every 2s, enforcing limits.
2. Input queries pressure and requests mass increase when allowed.
3. Output queries pressure and requests mass decrease when allowed.
4. Both Input and Output talk to adapters, which forward to the domain service via a uniform RPC-style boundary.

## Technologies used

- Container (server): SimpleRPC endpoint that exposes the domain interface (`IGasContainerService`) at http://127.0.0.1:5000/gasrpc.
- Input side:
  - Input Adapter: gRPC service listening on http://127.0.0.1:5002; forwards each request to the container via a SimpleRPC proxy.
  - Input Client: gRPC client (see `Input/Client.cs`) that connects to the Input Adapter and requests mass increase when pressure is below the lower limit.
- Output side:
  - Output Adapter: Controller-based web endpoint on http://127.0.0.1:5001; forwards calls to the container via a SimpleRPC proxy.
  - Output Client: RESTful HTTP client (generated client in `Output/OutputClient.cs` used by `Output/Client.cs`) that calls the Output Adapter to decrease mass when pressure is above the upper limit.

Note on message queues: This version integrates components via request/response endpoints. A message-queue variant (e.g., RabbitMQ) can be added as a decoupled event or command path if you want asynchronous buffering, retries, or fan-out, but it isn’t required for the current flow.

## How communication happens (high-level)

- Each component uses its own preferred client/server style.
- Adapters sit in front of the container’s domain service and translate client requests to a common RPC-style interface (`IGasContainerService`).
- The domain service applies thread-safe updates and returns the current pressure and container status.

### How the adapters work

- Input Adapter (gRPC):
  1) Receives Increase/Decrease/Get/IsDestroyed calls from the Input client via gRPC.
  2) Forwards each call to the container’s SimpleRPC endpoint using a typed proxy of `IGasContainerService`.
  3) Returns the response to the gRPC client.

- Output Adapter (Controller-based):
  1) Exposes HTTP endpoints: `/increaseMass`, `/decreaseMass`, `/getPressure`, `/isContainerDestroyed`.
  2) Forwards each HTTP request to the container’s SimpleRPC endpoint using the same typed proxy.
  3) Returns the result to the HTTP client.

## Run locally

Prerequisites:
- .NET SDK 8.x installed

### Ports used
- Container service: 5000
- Controller-based adapter: 5001
- Adapter for input: 5002

### macOS (zsh)

1) Build the solution

```
 dotnet build InteroperabilityGasPressure.sln
```

2) Start processes in separate terminals (order matters)

- Container (domain service)
```
 dotnet run --project InteroperabilityGasPressure/GasPressure.csproj
```

- Controller-based adapter
```
 dotnet run --project OutputAdapter/OutputAdapter.csproj
```

- Adapter for input
```
 dotnet run --project Adapters/InputAdapter.csproj
```

- Input client
```
 dotnet run --project Input/Input.csproj
```

- Output client
```
 dotnet run --project Output/Output.csproj
```

### Windows (PowerShell or CMD)

1) Build the solution

```
 dotnet build InteroperabilityGasPressure.sln
```

2) Start processes in separate terminals (order matters)

- Container (domain service)
```
 dotnet run --project InteroperabilityGasPressure/GasPressure.csproj
```

- Controller-based adapter
```
 dotnet run --project OutputAdapter/OutputAdapter.csproj
```

- Adapter for input
```
 dotnet run --project Adapters/InputAdapter.csproj
```

- Input client
```
 dotnet run --project Input/Input.csproj
```

- Output client
```
 dotnet run --project Output/Output.csproj
```

### What to expect
- Logs indicate temperature changes every ~2 seconds and show pressure, mass adjustments, and safety-limit events.
- When the container is destroyed due to implosion/explosion, it resets; clients pause operations until pressure returns to a safe range.

## Configuration
- Default addresses are configured in the source files:
  - Container service host: `127.0.0.1:5000`
  - Input adapter host: `127.0.0.1:5002`
  - Controller-based adapter host: `127.0.0.1:5001`
- If you need to change ports, update them in the respective server classes.

## Concurrency and safety
- The domain logic uses an internal lock around state changes to avoid race conditions.
- Long-running background temperature updates and short request handlers are isolated via the lock to avoid deadlocks/livelocks.

## License
This project is licensed under the MIT License. See [`LICENSE`](./LICENSE).
