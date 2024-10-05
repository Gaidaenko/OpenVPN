using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenVPN
{
    public static class OpenVPNmgmt
    {
        public static string revokeCertName;

        public static void checkName()
        {
         
            string chkName = $"test -f /etc/openvpn/easy-rsa/keys/{Program.newVPNclient}.crt && echo 'exist' || echo 'doesNotExist'";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{chkName}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();
               
            if (result.Contains("exist"))
            {
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Сертифікат з таким ім'ям вже існеє!\n");
                Console.ForegroundColor = ConsoleColor.White;

                Program.startMenu();
                       
            }
            else 
            {
                process.WaitForExit();
                process.Close();

                Program.createCrt();
            
            }         
        }

        public static void createLog()
        {
            DateTime dateTime = DateTime.Now;

            string log = "test -f /var/log/openvpnmgmt.log && echo 'exist' || echo 'doesNotExist'";
            string createLog = $"touch /var/log/openvpnmgmt.log && echo 'Certificate created {Program.newVPNclient} {dateTime}' >> /var/log/openvpnmgmt.log";                 
            string enrtyInLog = $"echo 'Certificate created {Program.newVPNclient} {dateTime}' >> /var/log/openvpnmgmt.log";

            Process processLog = new Process();
            processLog.StartInfo.FileName = "/bin/bash";
            processLog.StartInfo.Arguments = $"-c \"{log}\"";
            processLog.StartInfo.RedirectStandardOutput = true;
            processLog.StartInfo.RedirectStandardError = true;
            processLog.StartInfo.UseShellExecute = false;
            processLog.StartInfo.CreateNoWindow = true;
            processLog.Start();

            StreamReader logReader = processLog.StandardOutput;
            string result = logReader.ReadToEnd();

            if (result.Contains("exist"))
            {
                Process processWriteLog = new Process();
                processWriteLog.StartInfo.FileName = "/bin/bash";
                processWriteLog.StartInfo.Arguments = $"-c \"{enrtyInLog}\"";
                processWriteLog.StartInfo.RedirectStandardOutput = true;
                processWriteLog.StartInfo.RedirectStandardError = true;
                processWriteLog.StartInfo.UseShellExecute = false;
                processWriteLog.StartInfo.CreateNoWindow = true;
                processWriteLog.Start();
                processWriteLog.WaitForExit();
                processWriteLog.Close();

            }
            else
            {
                Process processCreateLog = new Process();
                processCreateLog.StartInfo.FileName = "/bin/bash";
                processCreateLog.StartInfo.Arguments = $"-c \"{createLog}\"";
                processCreateLog.StartInfo.RedirectStandardOutput = true;
                processCreateLog.StartInfo.RedirectStandardError = true;
                processCreateLog.StartInfo.UseShellExecute = false;
                processCreateLog.StartInfo.CreateNoWindow = true;
                processCreateLog.Start();
                processCreateLog.WaitForExit();
                processCreateLog.Close();

            }
            processLog.WaitForExit();
            processLog.Close();

            Program.sendMail();
        }

        public static void revokeCrt()
        {

            Console.WriteLine("Введіть назву сертифікату!");
            revokeCertName = Console.ReadLine();

            string chkName = $"test -f /etc/openvpn/easy-rsa/keys/{revokeCertName}.crt && echo 'exist' || echo 'doesNotExist'";
            string revoke = $"cd /etc/openvpn/easy-rsa/ && source ./vars && ./revoke-full {revokeCertName} && kill -HUP $(pgrep openvpn)";
            string rm = $"rm /etc/openvpn/easy-rsa/keys/{revokeCertName}.crt && rm /etc/openvpn/easy-rsa/keys/{revokeCertName}.csr && rm /etc/openvpn/easy-rsa/keys/{revokeCertName}.key";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{chkName}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();

            if (result.Contains("exist"))
            {
                Process processRevoke = new Process();
                processRevoke.StartInfo.FileName = "/bin/bash";
                processRevoke.StartInfo.Arguments = $"-c \"{revoke}\"";
                processRevoke.StartInfo.RedirectStandardOutput = true;
                processRevoke.StartInfo.RedirectStandardError = true;
                processRevoke.StartInfo.UseShellExecute = false;
                processRevoke.StartInfo.CreateNoWindow = true;
                processRevoke.Start();
                processRevoke.WaitForExit();
                processRevoke.Close();

                Process processRm = new Process();
                processRm.StartInfo.FileName = "/bin/bash";
                processRm.StartInfo.Arguments = $"-c \"{rm}\"";
                processRm.StartInfo.RedirectStandardOutput = true;
                processRm.StartInfo.RedirectStandardError = true;
                processRm.StartInfo.UseShellExecute = false;
                processRm.StartInfo.CreateNoWindow = true;
                processRm.Start();
                processRm.WaitForExit();
                processRm.Close(); 

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Сертифікат отозван!\n");
                Console.ForegroundColor = ConsoleColor.White;

                process.WaitForExit();
                process.Close();

                revokeLog();

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Сертифікат з такою назвою відсутній в папці з сертифікатами!\n");
                Console.ForegroundColor = ConsoleColor.White;

                process.WaitForExit();
                process.Close();

                Program.startMenu();

            }
            
        }

        public static void revokeLog()
        {
            DateTime dateTime = DateTime.Now;
            string log = "test -f /var/log/openvpnmgmt.log && echo 'exist' || echo 'doesNotExist'";
            string createLog = $"touch /var/log/openvpnmgmt.log && echo 'Certificate revoked {revokeCertName} {dateTime}' >> /var/log/openvpnmgmt.log";
            string enrtyInLog = $"echo 'Certificate revoked {revokeCertName} {dateTime}' >> /var/log/openvpnmgmt.log";

            Process processLog = new Process();
            processLog.StartInfo.FileName = "/bin/bash";
            processLog.StartInfo.Arguments = $"-c \"{log}\"";
            processLog.StartInfo.RedirectStandardOutput = true;
            processLog.StartInfo.RedirectStandardError = true;
            processLog.StartInfo.UseShellExecute = false;
            processLog.StartInfo.CreateNoWindow = true;
            processLog.Start();

            StreamReader logReader = processLog.StandardOutput;
            string result = logReader.ReadToEnd();

            if (result.Contains("exist"))
            {
                Process processWriteLog = new Process();
                processWriteLog.StartInfo.FileName = "/bin/bash";
                processWriteLog.StartInfo.Arguments = $"-c \"{enrtyInLog}\"";
                processWriteLog.StartInfo.RedirectStandardOutput = true;
                processWriteLog.StartInfo.RedirectStandardError = true;
                processWriteLog.StartInfo.UseShellExecute = false;
                processWriteLog.StartInfo.CreateNoWindow = true;
                processWriteLog.Start();
                processWriteLog.WaitForExit();
                processWriteLog.Close();

            }
            else
            {
                Process processCreateLog = new Process();
                processCreateLog.StartInfo.FileName = "/bin/bash";
                processCreateLog.StartInfo.Arguments = $"-c \"{createLog}\"";
                processCreateLog.StartInfo.RedirectStandardOutput = true;
                processCreateLog.StartInfo.RedirectStandardError = true;
                processCreateLog.StartInfo.UseShellExecute = false;
                processCreateLog.StartInfo.CreateNoWindow = true;
                processCreateLog.Start();
                processCreateLog.WaitForExit();
                processCreateLog.Close();

            }

            Program.startMenu();
        }

        public static void showActions()
        {
            string showLog = "cat /var/log/openvpnmgmt.log";

            Process processShowLog = new Process();
            processShowLog.StartInfo.FileName = "/bin/bash";
            processShowLog.StartInfo.Arguments = $"-c \"{showLog}\"";
            processShowLog.StartInfo.RedirectStandardOutput = true;
            processShowLog.StartInfo.RedirectStandardError = true;
            processShowLog.StartInfo.UseShellExecute = false;
            processShowLog.StartInfo.CreateNoWindow = true;
            processShowLog.Start();

            StreamReader readLog = processShowLog.StandardOutput;
            string result = readLog.ReadToEnd();
            Console.WriteLine(result.ToString());

            processShowLog.WaitForExit();
            processShowLog.Close();

            Program.startMenu();
        }

        public static void watchSessions()
        {
            string session = "cat /var/log/openvpn-status.log";

            Process processSessions = new Process();
            processSessions.StartInfo.FileName = "/bin/bash";
            processSessions.StartInfo.Arguments = $"-c \"{session}\"";
            processSessions.StartInfo.RedirectStandardOutput = true;
            processSessions.StartInfo.RedirectStandardError = true;
            processSessions.StartInfo.UseShellExecute = false;
            processSessions.StartInfo.CreateNoWindow = true;
            processSessions.Start();

            StreamReader readLog = processSessions.StandardOutput;
            string resultLog = readLog.ReadToEnd();
            Console.WriteLine("\n"+resultLog.ToString()+"\n");

            processSessions.WaitForExit();
            processSessions.Close();

            Program.startMenu();

        }

    }
}
