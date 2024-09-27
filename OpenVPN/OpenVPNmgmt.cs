using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        public static void checkName()
        {
         
            string chkName = $"test -f /etc/openvpn/easy-rsa/keys/{Program.forConfig}.crt && echo 'exist' || echo 'doesNotExist'";

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
                Console.WriteLine("Сертификат с таикм именем уже существует!\n");
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

        public static void revokeCrt()
        {

            Console.WriteLine("Введите имя сертификата!");
            string revokeCertName = Console.ReadLine();

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

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Сертификат отозван!\n");
                Console.ForegroundColor = ConsoleColor.White;

                process.WaitForExit();
                process.Close();

                Program.startMenu();

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Сертификат с таикм отсутствует в папке сертификатов!\n");
                Console.ForegroundColor = ConsoleColor.White;

                process.WaitForExit();
                process.Close();

                Program.startMenu();

            }
        }
    }
}
