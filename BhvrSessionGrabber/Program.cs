using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Fiddler;

namespace BhvrSessionGrabber
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;

            Log("Isolumia.com - bhvrSession Grabber", ConsoleColor.Cyan);

            if (!IsRunningAsAdmin())
            {
                Log("[-] You must run this tool as Administrator.", ConsoleColor.Red);
                return;
            }

            if (!InitializeFiddler())
            {
                Log("[-] Failed to initialize Fiddler Proxy. Exiting...", ConsoleColor.Red);
                return;
            }

            Console.Clear();
            Log("[!] Fiddler Proxy initialized. Waiting for game to start...", ConsoleColor.Yellow);
            var gameProcess = WaitForGameProcess();
            Log("[!] Continue starting the game as usual...", ConsoleColor.Yellow);

            Console.ReadLine();
            ShutdownFiddler();
            TerminateGame();
        }

        // ensures the proxy gets disabled if user closes the app (x)
        private static void OnProcessExit(object sender, EventArgs e)
        {
            ShutdownFiddler();
        }

        // ensures the proxy gets disabled if user presses ctrl + c
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ShutdownFiddler();
        }

        // install/look for existing cert & enable proxy
        private static bool InitializeFiddler()
        {
            try
            {
                string certDirectory = Path.GetDirectoryName(CertFolder.CertLocation);
                if (!Directory.Exists(certDirectory))
                    Directory.CreateDirectory(certDirectory);

                BCCertMaker.BCCertMaker bC = new BCCertMaker.BCCertMaker();
                CertMaker.oCertProvider = bC;

                string cert = CertFolder.CertLocation;
                if (!File.Exists(cert))
                {
                    Log("[!] Creating and installing Fiddler root certificate...", ConsoleColor.Yellow);
                    bC.CreateRootCertificate();
                    bC.WriteRootCertificateAndPrivateKeyToPkcs12File(cert, "");
                }
                else
                {
                    Log("[?] Looking for existing Fiddler root certificate...", ConsoleColor.Green);
                    bC.ReadRootCertificateAndPrivateKeyFromPkcs12File(cert, "");
                }

                if (!CertMaker.rootCertIsTrusted())
                {
                    Log("[!] Trust the Fiddler root certificate...", ConsoleColor.Yellow);
                    CertMaker.trustRootCert();
                }

                if (!CertMaker.rootCertIsTrusted())
                {
                    Log("[-] Failed to trust the Fiddler root certificate.", ConsoleColor.Red);
                    return false;
                }

                var settings = new FiddlerCoreStartupSettingsBuilder()
                    .ListenOnPort(42069) // 420 69 hihihi
                    .RegisterAsSystemProxy()
                    .ChainToUpstreamGateway()
                    .DecryptSSL()
                    .OptimizeThreadPool()
                    .Build();

                FiddlerApplication.Startup(settings);
                FiddlerApplication.BeforeRequest += OnBeforeRequest;

                Log("[+] Fiddler Proxy started successfully.", ConsoleColor.Green);
                return true;
            }
            catch (Exception ex)
            {
                Log($"[-] Error initializing Fiddler Proxy: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        // request to get bhvrsession
        private static void OnBeforeRequest(Session session)
        {
            try
            {
                if (session.hostname.Contains("bhvrdbd.com") && session.uriContains("/api/v1/config"))
                {
                    if (session.oRequest["Cookie"]?.Contains("bhvrSession=") == true)
                    {
                        string bhvrSession = session.oRequest["Cookie"].Replace("bhvrSession=", "");
                        Console.Clear();
                        Log($"[+] bhvrSession found: ", ConsoleColor.Green, false);
                        Log($"{bhvrSession}", ConsoleColor.White);
                        SaveBhvrSession(bhvrSession);
                        PromptUserToTerminateGame();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[-] Error processing request: {ex.Message}", ConsoleColor.Red);
            }
        }

        // saves the bhvrsession in a txt (same location as the file)
        private static void SaveBhvrSession(string bhvrSession)
        {
            try
            {
                string exeDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string filePath = Path.Combine(exeDirectory, "bhvrsession.txt"); // since build as single exe (self contained) this ensures it doesnt get saved in the temp folder

                File.WriteAllText(filePath, bhvrSession);
                Log($"\n[+] bhvrSession saved to: ", ConsoleColor.Green, false);
                Log($"{filePath}", ConsoleColor.White);
            }
            catch (Exception ex)
            {
                Log($"\n[-] Error saving bhvrSession: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void PromptUserToTerminateGame()
        {
            Log("\n[!] Press any key to terminate the game and ensure the session won't change.", ConsoleColor.Yellow);
            Log("[!] Otherwise close this window to exit without terminating the game.", ConsoleColor.Yellow);

            Log("\n\n\n\n\n\n\n[$] This is completely free and was made by 'sha - isolumia.com", ConsoleColor.Cyan); // people need to know who this insane c# coder is (c# makes me sick)

            if (Console.ReadKey(intercept: true) != null)
            {
                TerminateGame();
            }

            ShutdownFiddler();
            Environment.Exit(0);
        }

        private static void TerminateGame()
        {
            try
            {
                string[] gameProcesses = { "DeadByDaylight-WinGDK-Shipping.exe", "DeadByDaylight-EGS-Shipping.exe" }; // tries to terminate both cuz im lazy, dont worry about it
                foreach (string processName in gameProcesses)
                {
                    Log($"[!] Trying to terminate {processName}...", ConsoleColor.Yellow);
                    Process.Start("cmd.exe", $"/c taskkill /F /IM {processName}");
                }
            }
            catch (Exception ex)
            {
                Log($"[-] Error terminating game: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void ShutdownFiddler()
        {
            if (FiddlerApplication.IsStarted())
            {
                FiddlerApplication.Shutdown();
                Log("[!] Fiddler Proxy shutdown.", ConsoleColor.Yellow);
            }
        }

        private static Process WaitForGameProcess()
        {
            while (true)
            {
                string[] gameProcesses = { "DeadByDaylight-WinGDK-Shipping", "DeadByDaylight-EGS-Shipping" };
                foreach (var processName in gameProcesses)
                {
                    var gameProcess = Process.GetProcessesByName(processName).FirstOrDefault();
                    if (gameProcess != null)
                    {
                        Console.Clear();
                        Log($"[+] Game process '", ConsoleColor.Green, false);
                        Log($"{processName}", ConsoleColor.White, false);
                        Log($"' detected.", ConsoleColor.Green);
                        return gameProcess;
                    }
                }
                Thread.Sleep(1000); // sleeps for 1 sec
            }
        }

        private static bool IsRunningAsAdmin() => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private static void Log(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ResetColor();
        }
    }

    // location where the cert is stored
    public static class CertFolder
    {
        public static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Isolumia", "BhvrSessionGrabber");
        public static readonly string CertLocation = Path.Combine(DataFolder, "FiddlerRootCertificate.p12");
    }
}