# Immersive APM MCP Server
Welcome to Immersive APM MCP Server, an open-source Model Context Protocol (MCP) server designed to seamlessly integrate with Visual Studio Code (VS Code) and GitHub Copilot in agent mode. This powerful tool empowers developers to interact with application telemetry directly within the VS Code editor, streamlining workflows and enhancing productivity with real-time insights into your software's performance.

# 🚀 Features
* **Seamless VS Code Integration**: Interact with your application's telemetry data right from the VS Code editor using GitHub Copilot's agent mode.
* **Open Source**: Freely available under the MIT license, encouraging community contributions and transparency.
* **Easy Telemetry Access**: Query and analyze application performance metrics effortlessly using natural language prompts in agent mode.
* **Extensible MCP Support**: Connects to the Model Context Protocol ecosystem, enabling integration with a variety of tools and services.
* **Autonomous Workflows**: Leverage Copilot’s agent mode to automate multi-step tasks, such as retrieving telemetry data, analyzing performance, and generating reports.
* **Developer-Friendly**: Designed for ease of use, with minimal setup required to start interacting with telemetry data.
# 🌟 Why Immersive APM MCP Server?
Immersive APM MCP Server makes it incredibly easy to monitor and interact with your application's telemetry directly within your coding environment. By integrating with VS Code and GitHub Copilot’s agent mode, you can use natural language to query metrics, debug performance issues, and optimize your application without leaving your editor. Whether you're a solo developer or part of a large team, this tool simplifies access to critical telemetry data, saving you time and effort.
# 🛠️ Installation
## Clone the Repository:
```powershell
git clone https://github.com/ImmersiveFusion/if-apm-mcp-server.git
cd if-apm-mcp-server
```

## Build and restore packages:

```powershell
cd src/IF.APM.App.MCP.Server
dotnet build
```

## Configure the MCP Server:

Update the configuration file (`.vscode\mcp.json`) with your Immersive APM Grid API key.

> If you need API key, review the [Getting Started](https://docs.immersivefusion.com/Getting-Started) documentation and start your subscription.

Example configuration:

```json
{
    "inputs": [
        {
        "type": "promptString",
        "id": "if-key",
        "description": "Immersive APM API Key",
        "password": true
        }
    ],
    "servers": {
        "IF.APM.App.MCP.Server": {
        "type": "stdio",
        "command": "dotnet",
        "args": [
            "run",
            "--project",
            "${workspaceFolder}/IF.APM.App.MCP.Server/IF.APM.App.MCP.Server.csproj"
        ],
        "env": {
            "IF_ApiKey": "${input:if-key}"
            }
        }
    }
}
```

## Run the Server:

* Follow the instructions in https://code.visualstudio.com/docs/copilot/chat/mcp-servers

* Enable Agent Mode:
    * Open the Copilot Chat window in VS Code (Ctrl+Alt+I or Cmd+Alt+I on macOS).
    * Select Agent from the mode dropdown in the Copilot Chat pane.
    * Ensure the Immersive APM MCP Server is running and configured as an MCP server in VS Code. Refer to the VS Code documentation for details on MCP server setup.

# 🌍 Open Source
Immersive APM MCP Server is proudly open source under the MIT License (LICENSE). We believe in the power of community-driven development and welcome contributions from developers worldwide. You can:

* Contribute: Submit pull requests, report issues, or suggest features on our GitHub repository.
* Customize: Extend the server to support additional telemetry services or integrate with other tools in your development stack.
* Collaborate: Join our community to help shape the future of telemetry integration in VS Code.

By open-sourcing Immersive APM, we aim to foster a vibrant ecosystem where developers can build, share, and improve tools for application performance monitoring.

# 🔧 Usage Example

Here’s a quick example of how to use Immersive APM MCP Server with GitHub Copilot in VS Code:
* Open a project in VS Code.
* Ensure the Immersive APM MCP Server is running and configured.
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

## 📚 Documentation
For detailed setup instructions, configuration options, and advanced usage, check out our full documentation (docs/README.md).

## 🤝 Contributing
We love contributions! To get started:
* Fork the repository.
* Create a new branch (git checkout -b feature/your-feature).
* Make your changes and commit (git commit -m "Add your feature").
* Push to your fork (git push origin feature/your-feature).
* Open a pull request.
* Please read our Contributing Guidelines (CONTRIBUTING.md) for more details.

## 📜 License
This project is licensed under the MIT License (LICENSE) - see the file for details.

## 🙌 Acknowledgments
Thanks to the GitHub Copilot team for their innovative work on agent mode and MCP support.
Shoutout to the VS Code community for their continuous feedback and contributions to open-source AI tools.

## 📬 Contact
Have questions or need supportទ
System: support? Join our GitHub Discussions or reach out on social media or join our [Discord](https://discord.com/channels/946854209272287333/1388020870890655844).