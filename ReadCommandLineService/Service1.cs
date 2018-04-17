using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ReadCommandLineService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Index();
        }
        public void Index()
        {
            string filePath = "csv.niagara.data.csv";
            using (var redis = new RedisClient("localhost", 6379))
            {
                Read(filePath, redis);
                //return "done";
            }

        }
        public void Read(string filePath, RedisClient redis)
        {
            Int64 i = 0;
            try
            {
                // set columns
                string columns;

                // set data
                List<string> data = new List<string>();
                Process cmd = new Process();
                cmd.StartInfo.FileName = "C:\\Program Files\\Redis\\redis-cli.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                //cmd.StartInfo.Arguments = "Set key " + reader.ReadLine();
                cmd.Start();

                using (var reader = new StreamReader(filePath))
                {
                    int j = 0;
                    //first line is always columns
                    cmd.StandardInput.WriteLine("Select 3");
                    cmd.StandardInput.WriteLine("Set Columns " + reader.ReadLine());
                    cmd.StandardInput.WriteLine("MULTI");
                    // read remaining data
                    while (!reader.EndOfStream)
                    {
                        string s = $"Set {j}:{i} \"{reader.ReadLine().ToString()}\"";
                        if (i > 100)
                        {
                            i = 0; j++;
                            cmd.StandardInput.WriteLine("EXEC");
                            var sa = cmd.StandardOutput.ReadLine();
                            if (cmd.StandardOutput.ReadLine() != "OK" || cmd.StandardOutput.ReadLine() != "QUEUED")
                            {
                                Console.WriteLine(cmd.StandardOutput.ReadLine());

                            }
                            //Console.WriteLine(cmd.StandardOutput.ReadToEnd());
                        }
                        cmd.StandardInput.WriteLine(s);
                        i++;
                    }


                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                }

                //new CsvHelperResult(columns, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);


            }
        }
        /// <summary>
        /// Model class to reduce read/write errors. Always check the Success property to see if read/write is successful.
        /// </summary>
        public class CsvHelperResult
        {
            /// <summary>
            /// Flag to determine if helper result is successful.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// The success/fail message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// The columns of the csv file.
            /// </summary>
            public string Columns { get; set; }

            /// <summary>
            /// The data of the csv file.
            /// </summary>
            public IEnumerable<string> Data { get; set; }

            /// <summary>
            /// The path where the csv was written to.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Standard constructor for read/write success/fail.
            /// </summary>
            /// <param name="message"></param>
            public CsvHelperResult(string message, string path = "", bool success = false)
            {
                Message = message;
                Success = success;
                Path = path;
            }

            /// <summary>
            /// Successful read constructor.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="columns"></param>
            /// <param name="data"></param>
            public CsvHelperResult(string columns, IEnumerable<string> data)
            {
                Success = true;
                Columns = columns;
                Data = Data;
            }
        }
        protected override void OnStop()
        {
        }
    }
}
