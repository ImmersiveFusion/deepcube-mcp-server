# DeepCube (TM) MCP Server
Welcome to DeepCube MCP Server, an open-source Model Context Protocol (MCP) server designed to seamlessly integrate with Visual Studio Code (VS Code) and GitHub Copilot in agent mode. DeepCube is the spatial observability platform, and this server brings its telemetry to you: interact with application telemetry directly within the VS Code editor, streamlining workflows and enhancing productivity with real-time insights into your software's performance.

## 🚀 Features
* **Seamless VS Code Integration**: Interact with your application's telemetry data right from the VS Code editor using GitHub Copilot's agent mode.
* **Open Source**: Freely available under the MIT license, encouraging community contributions and transparency.
* **Easy Telemetry Access**: Query and analyze application performance metrics effortlessly using natural language prompts in agent mode.
* **Extensible MCP Support**: Connects to the Model Context Protocol ecosystem, enabling integration with a variety of tools and services.
* **Autonomous Workflows**: Leverage GitHub Copilot’s agent mode to automate multi-step tasks, such as retrieving telemetry data, analyzing performance, and generating reports.
* **Developer-Friendly**: Designed for ease of use, with minimal setup required to start interacting with telemetry data.
## 🌟 Why DeepCube MCP Server?
DeepCube MCP Server makes it incredibly easy to monitor and interact with your application's telemetry directly within your coding environment. By integrating with VS Code and GitHub Copilot’s agent mode, you can use natural language to query metrics, debug performance issues, and optimize your application without leaving your editor. Whether you're a solo developer or part of a large team, this tool simplifies access to critical telemetry data, saving you time and effort.
## 🛠️ Installation

The server ships as a single self-contained executable named **`dcp`**. It has no
dependencies: you do not need the .NET runtime, or anything else, on the machine
that runs it.

### 1. Get an API key

You need a DeepCube Grid API key. Your grids and keys live in the
[DeepCube portal](https://portal.deepcube.ai). If you do not have a subscription yet,
start with the [Getting Started](https://docs.deepcube.ai/Getting-Started) guide.

The key identifies which grid the server reads, so anyone holding it can read that
grid's telemetry. Treat it as a secret: keep it out of source control and out of
anything you share.

### 2. Download the binary

Grab the archive for your platform from the
[latest release](https://github.com/ImmersiveFusion/deepcube-mcp-server/releases/latest):

| Platform | Archive |
|----------|---------|
| Windows (Intel/AMD) | `dcp-windows-amd64.zip` |
| Windows (ARM) | `dcp-windows-arm64.zip` |
| Linux (Intel/AMD) | `dcp-linux-amd64.zip` |
| Linux (ARM) | `dcp-linux-arm64.zip` |
| macOS (Apple silicon) | `dcp-darwin-arm64.zip` |
| macOS (Intel) | `dcp-darwin-amd64.zip` |

Unzip it and put `dcp` somewhere stable, because your MCP client config points at its
full path.

**Windows** (PowerShell):

```powershell
Expand-Archive dcp-windows-amd64.zip -DestinationPath "$env:LOCALAPPDATA\Programs\dcp"
```

**macOS and Linux**:

```bash
unzip dcp-darwin-arm64.zip -d ~/.local/bin
chmod +x ~/.local/bin/dcp
```

On macOS the binary is not notarized, so Gatekeeper will block the first run. Clear the
quarantine flag once:

```bash
xattr -d com.apple.quarantine ~/.local/bin/dcp
```

### 3. Point your MCP client at it

The server speaks MCP over stdio and reads its key from the `IF_ApiKey` environment
variable. Use the full path to the binary you unzipped.

**VS Code with GitHub Copilot** — `.vscode/mcp.json` in your workspace. This form prompts
for the key and stores it in VS Code's secret storage rather than in the file:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "if-key",
      "description": "DeepCube API Key",
      "password": true
    }
  ],
  "servers": {
    "deepcube": {
      "type": "stdio",
      "command": "C:\\Users\\you\\AppData\\Local\\Programs\\dcp\\dcp.exe",
      "env": {
        "IF_ApiKey": "${input:if-key}"
      }
    }
  }
}
```

**Claude Code**:

```bash
claude mcp add --scope local deepcube -e IF_ApiKey=<your-key> -- ~/.local/bin/dcp
```

**Claude Desktop, Cursor, and other clients** take the same shape in their own config
file:

```json
{
  "mcpServers": {
    "deepcube": {
      "command": "/absolute/path/to/dcp",
      "env": { "IF_ApiKey": "<your-key>" }
    }
  }
}
```

By default the server talks to `https://api-azure.deepcube.ai`. Set `IF_BaseUrl` in the
same `env` block to point it somewhere else.

### 4. Verify

Restart your client and confirm the server connects. In Claude Code:

```bash
claude mcp list
```

You should see `deepcube: ... - ✓ Connected`, which means the key was accepted and the
server read your grid. Then ask your agent something like *"is everything ok?"* and it
should call `GetSystemHealth`.

If it fails to start, run `dcp` directly in a terminal with `IF_ApiKey` set: it writes
startup errors, including an invalid or expired key, to standard error.

## 🧑‍💻 Building from source

You only need this if you want to modify the server. Requires the
[.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/ImmersiveFusion/deepcube-mcp-server.git
cd deepcube-mcp-server
dotnet test src/IF.APM.App.MCP.slnx
dotnet publish src/IF.APM.App.MCP.Server/IF.APM.App.MCP.Server.csproj -c Release -o ./artifacts/local
```

That writes `dcp` into `./artifacts/local`. Point your MCP client at it exactly as above.

The repository ships a `.vscode/mcp.json` that runs the server straight from source with
`dotnet run`, so you can iterate without republishing.

## 🌍 Open Source
DeepCube MCP Server is proudly open source under the [MIT License](LICENSE). We believe in the power of community-driven development and welcome contributions from developers worldwide. You can:

* Contribute: Submit pull requests, report issues, or suggest features on our GitHub repository.
* Customize: Extend the server to support additional telemetry services or integrate with other tools in your development stack.
* Collaborate: Join our community to help shape the future of telemetry integration in VS Code.

By open-sourcing the DeepCube MCP Server, we aim to foster a vibrant ecosystem where developers can build, share, and improve tools for spatial observability.

## 🔧 Usage Example

Here’s a quick example of how to use DeepCube MCP Server with GitHub Copilot in VS Code:
* Open a project in VS Code.
* Ensure the DeepCube MCP Server is running and configured.
* In the Copilot Chat pane, select Agent mode and enter:

    In the Copilot Chat window, use natural language prompts to query telemetry data. For example:

    ```
    Were there any errors in yesterday?
    ```
    
    or

    ```
    Show me SQL errors in the last hour
    ```

    Once you have a session and have discovered errors, try:

    ```
    Show me the stack trace
    ```

    or

    ```
    How do I fix it?
    ```

## 🧰 Tools

The server exposes the same tool set as the DeepCube Unity client's assistant, under the same names, so a prompt written against one works against the other.

### Health and diagnosis

| Tool | Answers |
|------|---------|
| `GetSystemHealth` | "Is everything ok?" Per-service state, error rates, Apdex, top errors. Start here. |
| `GetDiagnosis` | "What's wrong?" Recent errors grouped by root cause and ranked by severity. |
| `GetServiceMap` | "What services are running?" Topology with per-service health and dependencies. |
| `GetPressurePoints` | "What's about to break?" Services ranked by composite pressure against a baseline. |
| `GetTrendAnalysis` | "Is latency getting worse?" Per-service improving/stable/degrading classification. |
| `GetAlertSummary` | "Should someone be paged?" Alert status derived from health thresholds. |
| `GetIncidentTimeline` | "What happened?" Chronological reconstruction of an incident. |
| `GetServiceDetail` | "How is a given service doing?" One service in depth. |
| `GetSlowestEndpoints` | "What's slow?" Top 20 endpoints by P99 latency. |
| `GetDeploymentCorrelation` | "Was there a deployment?" New versions, services, endpoints, hosts. |
| `GetComparison` | "Is this normal?" Current window against a baseline window. |

### Traces and logs

| Tool | Returns |
|------|---------|
| `GetTraceFilterFacets` | Filterable dimensions for trace queries in a range. |
| `GetLogFilterFacets` | Filterable dimensions for log queries in a range. |
| `GetTraces` | Root trace entries in a time range. |
| `GetTrace` | One trace entry by span ID. |
| `GetTraceSpans` | The full call tree for a trace ID. |
| `GetTraceSpanTags` | OpenTelemetry attributes for a span. |
| `GetTraceSpanEvents` | Span events, including exceptions and stack traces. |
| `GetTraceErrors` | Error records for a trace. |
| `GetTraceLogs` | Log entries correlated to a trace. |
| `GetLogs` | Log entries in a time range. |
| `GetLog` | One log entry by span ID. |

### Charts and grid

| Tool | Returns |
|------|---------|
| `GetAvailableCharts` | The chart catalog. Call this first to get valid chart IDs. |
| `GetRangeData` | Time-series points for a chart ID over a range. |
| `GetDashboard` | A dashboard definition with its layout and charts. |
| `GetGridInfo` | Metadata about the current grid. |

Time ranges are capped at 7 days and are normalised to UTC before they reach the API. Tool failures return a JSON `error` field, never internal exception detail.

## 📚 Documentation
For detailed setup instructions, configuration options, and advanced usage, check out the full documentation at [docs.deepcube.ai](https://docs.deepcube.ai).

## 🤝 Contributing
We love contributions! To get started:
* Fork the repository.
* Create a new branch (git checkout -b feature/your-feature).
* Make your changes and commit (git commit -m "Add your feature").
* Push to your fork (git push origin feature/your-feature).
* Open a pull request.

## 📜 License
This project is licensed under the [MIT License](LICENSE), see the file for details.

## 🙌 Acknowledgments
Thanks to the GitHub Copilot team for their innovative work on agent mode and MCP support.
Shoutout to the VS Code community for their continuous feedback and contributions to open-source AI tools.

## 📬 Contact
Have questions or need support? Join our GitHub Discussions, reach out on social media, or join our [Discord](https://discord.gg/DKhCup3yDQ). Learn more at [deepcube.ai](https://deepcube.ai).
