using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Threading;


namespace MPKIDefender
{
    class Program
    {
        static void Main(string[] args)
        {
            string logPath  = ConfigurationManager.AppSettings["logPath"];
            string exeName = ConfigurationManager.AppSettings["exeName"];
            string exeRoot = ConfigurationManager.AppSettings["exeRoot"];
            string MPKIProcessName = ConfigurationManager.AppSettings["MPKIProcessName"];
            int MPKIRunDelay = Convert.ToInt32(ConfigurationManager.AppSettings["MPKIRunDelay"]);
            string latFn = "";
            DateTime latAt = Convert.ToDateTime("1970-1-1 12:00:00");

            Console.WriteLine("#################  MPKI守护者  ##################");
            Console.WriteLine("Ver: 1.00 更新时间: 2019-05-21 Develop By Ping_yi");
            Console.WriteLine("当前配置");
            Console.WriteLine("记录路径: {0}", logPath);
            Console.WriteLine("执行档名称: {0}", exeName);
            Console.WriteLine("执行档所在路径: {0}", exeRoot);
            Console.WriteLine("执行档所在路径: {0}", exeRoot);
            Console.WriteLine("运行等待时间(秒): {0}", MPKIRunDelay);
            Console.WriteLine("运行等待超时(秒): {0}", Convert.ToInt32(ConfigurationManager.AppSettings["logModifyTimeOut"]));

            //getNewestFile(logPath, out latFn, out latAt);
            //Console.WriteLine("当前最新记录档:{0}, 最后修改时间{1}", latFn, latAt.ToString());

            runCMD("taskkill /f /im " + exeName);   //关闭MPKI
            runCMD("start " + exeRoot + @"\" + exeName);    //重新开启


            while(true)
            {
                Thread.Sleep(1000);
                getNewestFile(logPath, out latFn, out latAt);
                Console.SetCursorPosition(0, 10);
                Console.WriteLine("当前最新记录档: {0}, 最后修改时间: {1}", latFn, latAt.ToString());
                Console.SetCursorPosition(0, 12);
                
                if (chkTimeout(latAt) == true)
                {
                    Console.Write("重新开启MPKI程序");
                    runCMD("taskkill /f /im " + exeName);   //关闭MPKI
                    runCMD("start " + exeRoot + @"\" + exeName);    //重新开启
                    Console.WriteLine(".......完成!");
                    Console.WriteLine("等待MPKI运行产生记录.");
                    Thread.Sleep(MPKIRunDelay * 1000);
                } else
                {

                        TimeSpan lt = DateTime.Now - latAt;
                        Console.WriteLine("                                         ");
                        Console.WriteLine("                                         ");
                        Console.SetCursorPosition(0, 12);
                        Console.WriteLine(lt.ToString("c").Substring(0, 8));

                }
            }
        }

        public static int getPidByProcessName(string processName)
        {
            Process[] ps = Process.GetProcessesByName(processName);
            foreach(Process p in ps)
            {
                return p.Id;
            }
            return 0;
        }
        public static void runCMD(string cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";       //设置要启动的应用程序
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统的shell启动
            p.StartInfo.RedirectStandardInput = true;   //接受来自条用程序的输入信息
            p.StartInfo.RedirectStandardOutput = false; //输出信息
            p.StartInfo.RedirectStandardError = true;   //输出错误
            p.StartInfo.CreateNoWindow = true;  //不显示程序窗口
            p.Start();  //启动程序
            p.StandardInput.WriteLine(cmd + "&exit");   //向cmd窗口输入指令
            p.StandardInput.AutoFlush = true;   
            p.WaitForExit();    //等待程序执行完退出
            p.Close();  //完毕引用
        }
        public static void getNewestFile(string dir, out string fn, out DateTime lat)
        {
            string newestFileName = "";
            DateTime lastAccessTime = Convert.ToDateTime("1970-1-1 12:00:00");
            DirectoryInfo root = new DirectoryInfo(dir);
            foreach(FileInfo f in root.GetFiles())
            {
                f.Refresh();
                if (f.LastAccessTime >= lastAccessTime)
                {
                    lastAccessTime = f.LastWriteTime;
                    newestFileName = f.Name;
                }
            }
            fn = newestFileName;
            lat = lastAccessTime;
        }
        public static bool chkTimeout(DateTime lat)
        {
            TimeSpan ts = DateTime.Now - lat;
            if (ts.TotalSeconds >= Convert.ToInt32(ConfigurationManager.AppSettings["logModifyTimeOut"])) return true;
            return false;
        }
    }
}
