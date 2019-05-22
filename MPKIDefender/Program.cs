using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;


namespace MPKIDefender
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern int WinExec(string exeName, int operType);
        static void Main(string[] args)
        {

            string logPath  = ConfigurationManager.AppSettings["logPath"];
            string exeName = ConfigurationManager.AppSettings["exeName"];
            string exeRoot = ConfigurationManager.AppSettings["exeRoot"];
            string MPKIProcessName = ConfigurationManager.AppSettings["MPKIProcessName"];
            int MPKIRunDelay = Convert.ToInt32(ConfigurationManager.AppSettings["MPKIRunDelay"]);
            string latFn = "";
            DateTime latAt = Convert.ToDateTime("1970-1-1 12:00:00");

            Console.WriteLine("#################  MPKIDefender  ##################");
            Console.WriteLine("Ver: 1.00 Build: 2019-05-22 Develop By Ping_yi");
            Console.WriteLine("Configuraton:");
            Console.WriteLine("LogPath: {0}", logPath);
            Console.WriteLine("ExecuteName: {0}", exeName);
            Console.WriteLine("ExecuteRootPath: {0}", exeRoot);
            Console.WriteLine("MPKIProcessName: {0}", MPKIProcessName);
            Console.WriteLine("StartDelya(S): {0}", MPKIRunDelay);
            Console.WriteLine("IdleTimeOut(S): {0}", Convert.ToInt32(ConfigurationManager.AppSettings["logModifyTimeOut"]));


            killProcess(MPKIProcessName);
            WinExec(exeRoot + @"\" + exeName, 6);


            while (true)
            {
                Thread.Sleep(1000);
                if (getNewestFile(logPath, out latFn, out latAt) != true)
                {
                    Console.SetCursorPosition(0, 10);
                    Console.WriteLine("CurrentLog: {0}, LastAccessTime: {1}", latFn, latAt.ToString());
                    Console.SetCursorPosition(0, 12);

                    if (chkTimeout(latAt) == true)
                    {
                        Console.Write("Open MPKI Client");
                        killProcess(MPKIProcessName);
                        WinExec(exeRoot + @"\" + exeName, 6);
                        Console.WriteLine(".......Complete!");
                        Console.WriteLine("Wait for client creatr log.");
                        Thread.Sleep(MPKIRunDelay * 1000);
                    }
                    else
                    {

                        TimeSpan lt = DateTime.Now - latAt;
                        Console.WriteLine("                                         ");
                        Console.WriteLine("                                         ");
                        Console.SetCursorPosition(0, 12);
                        Console.WriteLine(lt.ToString("c").Substring(0, 8));

                        if(chkProcessExist(MPKIProcessName) == false)   //檢查MPKI進程是否存在, 不在的話重新打開
                        {
                            WinExec(exeRoot + @"\" + exeName, 6);
                        }

                    }
                } else
                {
                    Console.WriteLine("Log path not exist, check it pls.");
                    Console.ReadKey();
                }
            }
        }

        //public static int getPidByProcessName(string processName)
        //{
        //    Process[] ps = Process.GetProcessesByName(processName);
        //    foreach(Process p in ps)
        //    {
        //        return p.Id;
        //    }
        //    return 0;
        //}
        //public static void runCMD(string cmd)
        //{
        //    Process p = new Process();
        //    p.StartInfo.FileName = "cmd.exe";       //设置要启动的应用程序
        //    p.StartInfo.UseShellExecute = false;    //是否使用操作系统的shell启动
        //    p.StartInfo.RedirectStandardInput = true;   //接受来自条用程序的输入信息
        //    p.StartInfo.RedirectStandardOutput = false; //输出信息
        //    p.StartInfo.RedirectStandardError = true;   //输出错误
        //    p.StartInfo.CreateNoWindow = true;  //不显示程序窗口
        //    p.Start();  //启动程序
        //    p.StandardInput.WriteLine(cmd + "&exit");   //向cmd窗口输入指令
        //    p.StandardInput.AutoFlush = true;
        //    p.WaitForExit();    //等待程序执行完退出
        //    p.Close();  //完毕引用
        //}
        public static bool getNewestFile(string dir, out string fn, out DateTime lat)
        {
            string newestFileName = "";
            bool result = false;
            DateTime lastAccessTime = Convert.ToDateTime("1970-1-1 12:00:00");
            DirectoryInfo root = new DirectoryInfo(dir);
            if (root.Exists)
            {
                foreach (FileInfo f in root.GetFiles())
                {
                    f.Refresh();
                    if (f.LastAccessTime >= lastAccessTime)
                    {
                        lastAccessTime = f.LastWriteTime;
                        newestFileName = f.Name;
                    }
                }
                result = false;
            } else
            {
                result = true;
            }
            fn = newestFileName;
            lat = lastAccessTime;
            return result;
        }
        public static bool chkTimeout(DateTime lat)
        {
            TimeSpan ts = DateTime.Now - lat;
            if (ts.TotalSeconds >= Convert.ToInt32(ConfigurationManager.AppSettings["logModifyTimeOut"])) return true;
            return false;
        }

        public static void killProcess(string exeName)
        {
            Process[] allPses = Process.GetProcessesByName(exeName);
            foreach(Process ps in allPses)
            {
                if(ps.ProcessName.Equals(exeName))
                {
                    ps.Kill();      //殺死進程
                    ps.WaitForExit();   //等待退出
                    break;
                }
            }
        }

        public static bool chkProcessExist(string proName)
        {
            Process[] ps = Process.GetProcessesByName(proName);
            if (ps.Length == 0) return false;
            return true;
        }
    }
}
