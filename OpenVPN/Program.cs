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
    public class Program
    {
        public static string newVPNclient;
        public static int choice;

        static void Main(string[] args)
        {
             startMenu();

        }

        public static void startMenu()
        {
            newVPNclient = null;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n1 - Створити новий сертифікат і ключ.");
            Console.WriteLine("2 - Відізвати існуючий сертифікат.");
            Console.WriteLine("3 - Сеанси openvpn.");
            Console.WriteLine("4 - Історія.");
            Console.WriteLine("5 - Вихід.");

            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                int choice = int.Parse(Console.ReadLine());

                if (choice < 1 || choice > 5)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Введіть число від 1 до 5!\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    startMenu();
                }
                else
                {
                    switch (choice)
                    {
                        case 1:
                            enterName();
                            break;

                        case 2:
                            OpenVPNmgmt.revokeCrt();
                            break;

                        case 3:
                            OpenVPNmgmt.watchSessions(); 
                            break;

                        case 4:
                            OpenVPNmgmt.showActions();
                            break;

                        case 5:
                            exitMenu();
                            break;

                    }
                }
            }
            catch(FormatException ex)
            {
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("Введіть число від 1 до 5!\n");
                Console.ForegroundColor = ConsoleColor.White;
               
                startMenu();
            }
        }

        public static void enterName()
        {
            Console.WriteLine("Введіть назву сертифікату (користувача) латинськими літерами:");

            newVPNclient = Console.ReadLine();

            if (string.IsNullOrEmpty(newVPNclient))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Введіть назву сертифікату!");
                Console.ForegroundColor = ConsoleColor.White;

                startMenu();
            }
            else
            {

                OpenVPNmgmt.checkName();
            
            }
        }
        public static void createCrt()
        {          
            string buildkey = $"cd /etc/openvpn/easy-rsa/ && source ./vars && ./build-key --batch ";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{buildkey} {newVPNclient}\""; 
            process.StartInfo.RedirectStandardOutput = false; 
            process.StartInfo.RedirectStandardError = false; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
            process.Close();

            mkDir();
        }

        public static void mkDir()
        {
            string mkdir = $"mkdir /tmp/{newVPNclient}";

            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{mkdir}\"";
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

            using (StreamWriter writer = new StreamWriter(@$"/tmp/{newVPNclient}/Connect.ovpn"))
            {
                writer.WriteLine("client\r\n" +
                    "dev tun\r\n" +
                    "proto udp\r\n" +
                    "auth-nocache\r\n" +
                    "remote files.bit-cloud.eu 18194\r\n" +
                    "ca ca.crt\r\n" +
                    "cert " + newVPNclient + ".crt\r\n" +
                    "key " + newVPNclient + ".key \r\n" +
                    "resolv-retry infinite\r\n" +
                    "nobind\r\n" +
                    "persist-key\r\n" +
                    "persist-tun\r\n" +
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

            OpenVPNmgmt.createLog();
        }
        public static void sendMail()
        {

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("\nВведіть назву пошти на корту відправити сертифікат і ключ:");
            Console.ForegroundColor = ConsoleColor.White;
            string mail = Console.ReadLine();         

            bool validationMail = mail.Contains('@');
            bool validationMail2 = mail.Contains('.');
            string website = "https://files.bit-cloud.eu/index.php/s/Gd2HF6LbnpwM2MN";

            if (validationMail == true && validationMail2 == true && !String.IsNullOrEmpty(mail))
            {
                Console.WriteLine("..зачекайте, архів відправляється на пошту!");         

                string send = $"echo '\nЗавантажити OpenVPN клієнт.\n\n{website}' | mpack -s 'Created OpenVPN certificates {newVPNclient}' -d /dev/stdin /tmp/{newVPNclient}.tar " + mail;
           
                Process process = new Process();
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{send}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                process.Close();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Відправлено!\nАрхів {newVPNclient}.tar тимчасово збережено /temp/{newVPNclient}.tar");
                Console.ForegroundColor = ConsoleColor.White;
                startMenu();               
            }
            else
            {
                mail = null;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Не є адресою!");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nАрхів {newVPNclient}.tar тимчасово збережено /tmp/{newVPNclient}.tar");             
                Console.ForegroundColor = ConsoleColor.White;
                startMenu();             
            }          
        }
        static void exitMenu()
        {
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{"exit"}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start(); ;
            process.WaitForExit();
            process.Close();
            return; 
        }      
    }
}
