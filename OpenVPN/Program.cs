using Renci.SshNet;
using Renci.SshNet.Security;
using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;

namespace OpenVPN
{
    internal class Program
    {
        public static string newVPNclient;
        public static string buildkey = $"cd /etc/openvpn/easy-rsa/ && source ./vars && ./build-key --batch ";
        public static string mkdir = $"mkdir /tmp/";
        public static string cpCrt = $"cp /etc/openvpn/easy-rsa/keys/{newVPNclient}.crt /tmp/{newVPNclient}";
        public static string cpKey = $"cp /etc/openvpn/easy-rsa/keys/{newVPNclient}.key /tmp/{newVPNclient}";
        public static string cpCaCrt = $"cp /etc/openvpn/easy-rsa/keys/ca.crt /tmp/{newVPNclient}";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Создать ноый сертификат и ключ, нажмите - 1.");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Отозвать существующий сертификат нажмите - 2.");

            Console.ForegroundColor = ConsoleColor.White;

            int choice = int.Parse(Console.ReadLine());

            
            switch(choice)
            { 
                case 1: createCrt();
                break;

                case 2: revokeCert();
                break;            
            }
           
        }
        public static void createCrt()
        {
            Console.WriteLine("Введите название сертификата (пользователя) латинскими буквами:");

            newVPNclient = Console.ReadLine();

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{buildkey} {newVPNclient}\""; 
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true; 
           
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        
            process.Close();
            mkDir();
        }

        public static void mkDir()
        {

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{mkdir}{newVPNclient}\"";
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true; 

            process.Start();
            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            process.Close();

            copyCrt();
        }
        public static void copyCrt()
        {
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash"; 
            process.StartInfo.Arguments = $"-c \"{mkdir}{newVPNclient}\"";
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true; 

            process.Start();
            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            process.Close();


        }
        static void revokeCert()
        {

            Console.WriteLine("Сертификат отозван!");
        
        }
    }
}
