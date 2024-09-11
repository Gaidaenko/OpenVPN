using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

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
    }
}
