using Spectre.Console;
using BehindU.Services;
using Figgle;

class Program
{
static void Main(string[] args)
{
    if (Services.TryConnect() == true)
    {
        while (true)
        {
            Console.WriteLine(FiggleFonts.Slant.Render("Online :)"));
            AnsiConsole.WriteLine("");
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("¿What u wanna do?")
                    .AddChoices(new[]
                    {
                        "Check health",
                        "Ram / CPU",
                        "Nmap",
                        "File Transfer",
                        "Quit"
                    }));
            switch (opcion)
            {
                case "Check health":
                    Console.WriteLine("Checking server status...");
                    MostrarBarraDeProgreso(() => Services.CheckStatusServer());
                    break;
                case "Ram / CPU":
                    Console.WriteLine("Verifying resources...");
                    MostrarBarraDeProgreso(() => Services.ReviewResources());
                    break;
                case "Nmap":
                    Console.WriteLine("Scanning ports...");
                    MostrarBarraDeProgreso(() => Services.ScanPorts());
                    break;
                case "File Transfer":
                    Console.WriteLine("Starting file transfer...");
                    MostrarBarraDeProgreso(() => Services.CopyFile());
                    break;
                case "Quit":
                    Console.WriteLine("Leaving...");
                    AnsiConsole.MarkupLine("[bold red]DELETE ALL[/]");
                    return;
            }

            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
    else
    {
        Console.WriteLine(FiggleFonts.Slant.Render("Offline :( Try Later"));
    }
}

    static void MostrarBarraDeProgreso(Action accion)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Circle)
            .Start("Processing...", ctx =>
            {
                accion();
            });
    }
}