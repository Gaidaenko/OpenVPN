using Renci.SshNet;
using Renci.SshNet.Security;
using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace OpenVPN
{
    internal class Program
    {
        public static string newVPNclient;
        public static string forConfig;

        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1 - Создать новый сертификат и ключ.");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("2 - Отозвать существующий сертификат.");

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
            forConfig = newVPNclient;
           

            string buildkey = $"cd /etc/openvpn/easy-rsa/ && source ./vars && ./build-key --batch ";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{buildkey} {newVPNclient}\""; 
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();     
            process.Close();
            
            mkDir();
        }

        public static void mkDir()
        {
            string mkdir = $"mkdir /tmp/";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{mkdir}{newVPNclient}\"";
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.RedirectStandardError = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true; 

            process.Start();;
            process.WaitForExit();
            process.Close();

            createConfig();
        }
      
        public static void createConfig()
        {

            using (StreamWriter writer = new StreamWriter(@$"/tmp/{forConfig}/Connect.ovpn"))
            {
                writer.WriteLine("client\r\n" +
                    "dev tun\r\n" +
                    "proto udp\r\n" +
                    "auth-nocache\r\n" +
                    "remote vpn2.cehavekorm.com.ua 18194\r\n" +
                    "ca ca.crt\r\n" +
                    "cert " + forConfig + ".crt\r\n" +
                    "key " + forConfig + ".key \r\n" +
                    "resolv-retry infinite\r\n" +
                    "nobind\r\n" +
                    "persist-key\r\n" +
                    "persist-tun\r\n" +
                    "#ns-cert-type server\r\n" +
                    "remote-cert-tls server\r\n" +
                    "comp-lzo\r\n" +
                    "log openvpn.log\r\n" +
                    "verb 3");
            }

            copyAllFiles();
        }

        public static void copyAllFiles()
        {
            string cpAllFiles = $"cp /etc/openvpn/easy-rsa/keys/{newVPNclient}.crt /tmp/{newVPNclient} " +
                $"&& cp /etc/openvpn/easy-rsa/keys/{newVPNclient}.key /tmp/{newVPNclient} " +
                $"&& cp /etc/openvpn/easy-rsa/keys/ca.crt /tmp/{newVPNclient} " +
                $"&& cd /tmp/{newVPNclient} && tar -cvf ../$(basename \"$PWD\").tar * && cd .. && rm -fr /tmp/{newVPNclient}";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{cpAllFiles}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
            process.Close();

            sendMail();
        }
        public static void sendMail()
        {
            Console.WriteLine("Введите адрес почты на который отправить сертификат и ключ:");
            string mail = Console.ReadLine();

            bool validationMail = mail.Contains('@');
            bool validationMail2 = mail.Contains('.');

            if (validationMail == true && validationMail2 == true)
            {
                Console.WriteLine("..подождите, архив отправляется на почту!");

                string send = $"mpack -s 'Created OpenVPN certificates {forConfig}' -a /tmp/{forConfig}.tar " + mail;

                Process process = new Process();
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{send}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start(); ;
                process.WaitForExit();
                process.Close();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Отправлено!\nАрхив {forConfig}.tar временно сохранен /temp/{forConfig}.tar");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Введенный текст не является адресом!");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Архив {forConfig}.tar временно сохранен /temp/{forConfig}.tar");             
                Console.ForegroundColor = ConsoleColor.White;
            }          
        }

        static void revokeCert()
        {

            Console.WriteLine("Сертификат отозван!");
        
        }

       
    }
    public static class OpenvpnManagement
    {
        public static void certNameCheck()
        { 
        
        }


    }
}
