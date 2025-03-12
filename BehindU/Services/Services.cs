using System;
using System.IO;
using Renci.SshNet;
using Spectre.Console;
using DotNetEnv;

namespace BehindU.Services
{

    public class Services
    {
        private static string? server;
        private static string? user;
        private static string? pass;

        static Services()
        {
            Env.Load();
            server = Environment.GetEnvironmentVariable("SERVER");
            user = Environment.GetEnvironmentVariable("USER");
            pass = Environment.GetEnvironmentVariable("PASS");
        }

        public static string EjecutarComandoSSH(string comando)
        {
            var connectionInfo = new ConnectionInfo(server, user, new PasswordAuthenticationMethod(user, pass));
            using (var client = new SshClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    var cmd = client.CreateCommand(comando);
                    var resultado = cmd.Execute();
                    client.Disconnect();
                    return resultado;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error connecting to the server: {ex.Message}");
                }
            }
        }

        public static bool TryConnect()
        {
            string salida = EjecutarComandoSSH("echo 'test'");
            if (salida.Contains("Error connecting"))
            {
                return false;
            }
            return true;
        }

        public static void CheckStatusServer()
        {
            try
            {
                var salida = EjecutarComandoSSH("uptime");
                AnsiConsole.MarkupLine("[bold yellow]Server status:[/]\n{0}", salida);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]{0}[/]", ex.Message);
            }
        }

        public static void ReviewResources()
        {
            try
            {
                var topSalida = EjecutarComandoSSH("top -n 1 -b");
                var topLineas = topSalida.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var cpuLinea = topLineas.FirstOrDefault(x => x.Contains("%Cpu(s):"));
                var cpuInfo = cpuLinea?.Split(':')[1].Trim();
                var memoriaLinea = topLineas.FirstOrDefault(x => x.Contains("MiB Mem :"));
                var memoriaInfo = memoriaLinea?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var totalMemoria = memoriaInfo?[2];
                var usadaMemoria = memoriaInfo?[4];
                var libreMemoria = memoriaInfo?[6];
                var disponibleMemoria = memoriaInfo?[9];
                var dfSalida = EjecutarComandoSSH("df -h /");
                var dfLineas = dfSalida.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var discoInfo = dfLineas[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var totalDisco = discoInfo[1];
                var usadoDisco = discoInfo[2];
                var libreDisco = discoInfo[3];
                var tabla = new Table();
                tabla.AddColumn("[bold blue]Resource[/]");
                tabla.AddColumn("[bold blue]Valor[/]");

                tabla.AddRow("Total Memory (MiB)", totalMemoria);
                tabla.AddRow("Memory Used (MiB)", usadaMemoria);
                tabla.AddRow("Free Memory (MiB)", libreMemoria);
                tabla.AddRow("Available Memory (MiB)", disponibleMemoria);
                tabla.AddRow("CPU Usage", cpuInfo);
                tabla.AddRow("Total Disk Space", totalDisco);
                tabla.AddRow("Used Disk Space", usadoDisco);
                tabla.AddRow("Free Disk Space", libreDisco);

                AnsiConsole.MarkupLine("[bold yellow]Resources in Use:[/]");
                AnsiConsole.Write(tabla);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]{0}[/]", ex.Message);
            }
        }

        public static void ScanPorts()
        {
            try
            {
                var salida = EjecutarComandoSSH("nmap -p 1-65535 -T4 --open localhost");

                var lineas = salida.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var tabla = new Table();
                tabla.AddColumn("[bold blue]Port[/]");
                tabla.AddColumn("[bold blue]Status[/]");
                tabla.AddColumn("[bold blue]Service[/]");

                foreach (var linea in lineas)
                {
                    if (linea.Contains("/tcp") || linea.Contains("/udp"))
                    {
                        var partes = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (partes.Length >= 2)
                        {
                            var puerto = partes[0].Split('/')[0];
                            var estado = partes[1];
                            var servicio = partes.Length > 2 ? partes[2] : "unknown";
                            tabla.AddRow(puerto, estado, servicio);
                        }
                    }
                }

                AnsiConsole.MarkupLine("[bold yellow]Port scan result:[/]");
                AnsiConsole.Write(tabla);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]{0}[/]", ex.Message);
            }
        }

        public static void CopyFile()
        {
            try
            {
                var connectionInfo = new ConnectionInfo(server, user, new PasswordAuthenticationMethod(user, pass));
                using (var client = new SshClient(connectionInfo))
                {
                    client.Connect();
                    var accion = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("What do you want to do?")
                            .AddChoices(new[] { "Upload file to server", "Download file from server" }));

                    if (accion == "Upload file to server")
                    {
                        var archivoLocal = SeleccionarArchivoLocal();
                        AnsiConsole.MarkupLine($"[yellow]Selected file:[/] {archivoLocal}");
                        var directorioRemoto = SeleccionarDirectorioRemoto(client, "/home/");
                        AnsiConsole.MarkupLine($"[yellow]Selected remote directory:[/] {directorioRemoto}");
                        var nombreArchivo = Path.GetFileName(archivoLocal);
                        var archivoRemoto = $"{directorioRemoto}/{nombreArchivo}";
                        AnsiConsole.MarkupLine($"[yellow]Generated remote route:[/] {archivoRemoto}");
                        using (var scp = new ScpClient(connectionInfo))
                        {
                            try
                            {
                                scp.Connect();
                                AnsiConsole.MarkupLine("[green]Connected to the server to upload the file...[/]");
                                if (!File.Exists(archivoLocal))
                                {
                                    AnsiConsole.MarkupLine($"[red]The local file does not exist: {archivoLocal}[/]");
                                    return;
                                }
                                var comandoVerificarDirectorio = $"test -d {directorioRemoto} && echo 'exists' || echo 'no exists'";
                                var resultado = EjecutarComandoSSH(comandoVerificarDirectorio);
                                if (resultado.Trim() != "existe")
                                {
                                    AnsiConsole.MarkupLine($"[red]The remote directory does not exist: {directorioRemoto}[/]");
                                    return;
                                }
                                using (var fileStream = new FileStream(archivoLocal, FileMode.Open))
                                {
                                    scp.Upload(fileStream, archivoRemoto);
                                }

                                AnsiConsole.MarkupLine($"[green]File successfully uploaded to {archivoRemoto}![/]");
                                var comandoVerificarArchivo = $"test -f {archivoRemoto} && echo 'exists' || echo 'no exists'";
                                var resultadoArchivo = EjecutarComandoSSH(comandoVerificarArchivo);
                                AnsiConsole.MarkupLine($"[yellow]Verification on the server: {resultadoArchivo.Trim()}[/]");
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Error uploading file: {ex.Message}[/]");
                            }
                            finally
                            {
                                scp.Disconnect();
                            }
                        }
                    }
                    else if (accion == "Download file from server")
                    {
                        var archivoRemoto = SeleccionarArchivoRemoto(client, "/home/");
                        AnsiConsole.MarkupLine($"[yellow]Selected file:[/] {archivoRemoto}");
                        archivoRemoto = archivoRemoto.Replace('\\', '/');
                        var comandoVerificarArchivo = $"test -f {archivoRemoto} && echo 'exists' || echo 'no exists'";
                        var resultadoArchivo = EjecutarComandoSSH(comandoVerificarArchivo);
                        if (resultadoArchivo.Trim() != "exists")
                        {
                            AnsiConsole.MarkupLine($"[red]The remote file does not exist: {archivoRemoto}[/]");
                            return;
                        }
                        var directorioLocal = SeleccionarDirectorioLocal();
                        AnsiConsole.MarkupLine($"[yellow]Selected local directory:[/] {directorioLocal}");
                        var nombreArchivo = Path.GetFileName(archivoRemoto);
                        var archivoLocal = Path.Combine(directorioLocal, nombreArchivo);
                        using (var scp = new ScpClient(connectionInfo))
                        {
                            try
                            {
                                scp.Connect();
                                AnsiConsole.MarkupLine("[green]Connected to the server to download the file...[/]");
                                using (var fileStream = new FileStream(archivoLocal, FileMode.Create))
                                {
                                    scp.Download(archivoRemoto, fileStream);
                                }

                                AnsiConsole.MarkupLine($"[green]File successfully downloaded to {archivoLocal}![/]");
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Error downloading file: {ex.Message}[/]");
                            }
                            finally
                            {
                                scp.Disconnect();
                            }
                        }
                    }

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            }
        }

        public static string SeleccionarArchivoLocal(string rutaInicial = ".")
        {
            var directorioActual = new DirectoryInfo(rutaInicial);

            while (true)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Current directory:[/] {directorioActual.FullName}");

                var opciones = directorioActual.GetDirectories()
                    .Select(d => d.Name + "/")
                    .Concat(directorioActual.GetFiles().Select(f => f.Name))
                    .Append("..")
                    .ToList();

                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a file or directory")
                        .AddChoices(opciones));

                if (opcion == "..")
                {
                    directorioActual = directorioActual.Parent ?? directorioActual;
                }
                else if (opcion.EndsWith("/"))
                {
                    directorioActual = new DirectoryInfo(Path.Combine(directorioActual.FullName, opcion.TrimEnd('/')));
                }
                else
                {
                    return Path.Combine(directorioActual.FullName, opcion);
                }
            }
        }

        public static string SeleccionarDirectorioRemoto(SshClient cliente, string rutaInicial = "/")
        {
            var directorioActual = rutaInicial;

            while (true)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Remote directory:[/] {directorioActual}");
                var cmd = cliente.CreateCommand($"ls -p {directorioActual}");
                var salida = cmd.Execute();
                var opciones = salida.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(linea => linea.EndsWith("/") || linea == "..")
                    .ToList();
                opciones.Add("Select this directory");

                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a directory")
                        .AddChoices(opciones));

                if (opcion == "..")
                {
                    directorioActual = Path.GetDirectoryName(directorioActual);
                    if (string.IsNullOrEmpty(directorioActual)) directorioActual = "/";
                }
                else if (opcion == "Select this directory")
                {
                    return directorioActual;
                }
                else
                {
                    directorioActual = Path.Combine(directorioActual, opcion.TrimEnd('/'));
                }
            }
        }

        public static string SeleccionarDirectorioLocal(string rutaInicial = ".")
        {
            var directorioActual = new DirectoryInfo(rutaInicial);

            while (true)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Current directory:[/] {directorioActual.FullName}");

                var opciones = directorioActual.GetDirectories()
                    .Select(d => d.Name + "/")
                    .Append("..")
                    .Append("Select this directory")
                    .ToList();

                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a directory")
                        .AddChoices(opciones));

                if (opcion == "..")
                {
                    directorioActual = directorioActual.Parent ?? directorioActual;
                }
                else if (opcion == "Select this directory")
                {
                    return directorioActual.FullName;
                }
                else
                {
                    directorioActual = new DirectoryInfo(Path.Combine(directorioActual.FullName, opcion.TrimEnd('/')));
                }
            }
        }

        public static string SeleccionarArchivoRemoto(SshClient cliente, string rutaInicial = "/")
        {
            var directorioActual = rutaInicial;

            while (true)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Remote directory:[/] {directorioActual}");
                var cmd = cliente.CreateCommand($"ls -p {directorioActual}");
                var salida = cmd.Execute();
                var opciones = salida.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Append("..")
                    .ToList();
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a file or directory")
                        .AddChoices(opciones));

                if (opcion == "..")
                {
                    directorioActual = Path.GetDirectoryName(directorioActual);
                    if (string.IsNullOrEmpty(directorioActual)) directorioActual = "/";
                }
                else if (opcion.EndsWith("/"))
                {
                    directorioActual = Path.Combine(directorioActual, opcion.TrimEnd('/'));
                }
                else
                {
                    return Path.Combine(directorioActual, opcion);
                }
            }
        }
    }
}