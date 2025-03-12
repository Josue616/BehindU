# BehindU <img src="https://i.imgur.com/kUNonqv.png" width="50" height="50" align="right">

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![Multiplatform](https://img.shields.io/badge/Platform-Cross--Platform-brightgreen)
![Console Application](https://img.shields.io/badge/Type-Console--Application-yellow)
![License](https://img.shields.io/badge/License-MIT-informational)

## Project Description 🚀

**BehindU** is a cross-platform console tool developed in .NET 8, designed to simplify the management and monitoring of your servers directly from your local computer. With **BehindU**, you can perform essential server administration tasks quickly and efficiently, all from the convenience of your terminal.

Imagine having at your fingertips the ability to:

* **🕵️‍♂️ Monitor your server status in real-time:** Know the uptime, resource usage such as CPU, memory, and disk to ensure optimal performance.
* **🗂️ Transfer files without complications:** Upload and download files between your local machine and the server securely and easily, ideal for managing configurations, backups, or deployments.
* **🚪 Explore open ports:** Perform a quick scan to identify open ports on your server, a crucial step for security and diagnostics.

**BehindU** is the perfect tool for system administrators, developers, and anyone who needs to interact with Linux servers remotely and effectively.

## Main Features ✨

* **Server Monitoring:**
    * **Uptime:** Check how long the server has been running.
    * **Resource Usage:** Visualize CPU, memory (total, used, free, available), and disk space consumption in a clear and concise table.
* **File Transfer (SCP):**
    * **File Upload (Local to Server):** Upload files from your local machine to the server, with interactive file and directory selector.
    * **File Download (Server to Local):** Download files from the server to your local machine, also with interactive selector.
* **Port Scanning (Nmap):**
    * **Full Port Scan:** Perform a TCP/UDP port scan (1-65535) on the server to identify active services.
    * **Results Table:** Displays open ports, their status, and associated service in an organized table.
* **Interactive Console Interface:**
    * **Clear and Easy-to-Use Menus:** Intuitive navigation through selection prompts in the console.
    * **Data Presentation with Spectre.Console:** Use of tables and styles for an attractive and readable display of information.
* **Cross-Platform:** Developed in .NET 8, **BehindU** is compatible with Windows, macOS, and Linux.
* **Simple Configuration:** Server connection settings are configured through a `.env` file, keeping sensitive information out of the code.

## Getting Started 🚀

### Prerequisites 📋

Before running **BehindU**, make sure you have the following installed:

* **.NET 8 SDK:** You can download it from the [official .NET website](https://dotnet.microsoft.com/download/dotnet/8.0).
* **SSH access to a Linux server:** You will need the credentials (server, username, password) to connect to your Linux server.
* **Nmap installed on the server (optional for port scanning):** If you want to use the port scanning feature, ensure that `nmap` is installed on the target server. You can install it with: `sudo apt-get install nmap` (on Debian/Ubuntu) or `sudo yum install nmap` (on CentOS/RHEL).

### Configuration ⚙️

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Josue616/BehindU.git
   cd BehindU
   cd BehindU # Go into the subdirectory where the .csproj file is located