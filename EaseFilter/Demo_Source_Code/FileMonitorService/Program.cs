﻿///////////////////////////////////////////////////////////////////////////////
//
//    (C) Copyright 2012 EaseFilter Technologies Inc.
//    All Rights Reserved
//
//    This software is part of a licensed software product and may
//    only be used or copied in accordance with the terms of that license.
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;
using System.Configuration.Install;

using EaseFilter.CommonObjects;

namespace FileMonitorService
{
    static class program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool mutexCreated = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "EaseFilter", out mutexCreated);

            try
            {
                if (!mutexCreated)
                {
                    Console.WriteLine("There are another EaseFilter service instance running, can't start the second one.");
                    return;
                }

                Utils.CopyOSPlatformDependentFiles();

                if (Environment.UserInteractive)
                {


                    if (args.Length > 0)
                    {
                        string command = args[0];
                        switch (command.ToLower())
                        {
                            case "-installdriver":
                                {
                                    bool ret = FilterAPI.InstallDriver();

                                    if (!ret)
                                    {
                                        Console.WriteLine("Install driver failed:" + FilterAPI.GetLastErrorMessage());
                                    }

                                    break;
                                }

                            case "-uninstalldriver":
                                {
                                    FilterAPI.StopFilter();

                                    bool ret = FilterAPI.UnInstallDriver();

                                    if (!ret)
                                    {
                                        Console.WriteLine("UnInstall driver failed:" + FilterAPI.GetLastErrorMessage());
                                    }

                                    break;
                                }

                            case "-installservice":
                                {
                                    try
                                    {
                                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Install service failed:" + ex.Message);
                                    }

                                    break;
                                }

                            case "-uninstallservice":
                                {
                                    try
                                    {
                                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("UnInstall service failed:" + ex.Message);
                                    }

                                    break;
                                }

                            case "-console":
                                {
                                    try
                                    {
                                        Console.WriteLine("Starting FileMonitor console application...");
                                      
                                        GlobalConfig.EventLevel = EventLevel.Trace;
                                        GlobalConfig.EventOutputType = EventOutputType.Console;

                                        FilterWorker.StartService();
                                        Console.WriteLine("\n\nPress any key to stop program");
                                        Console.Read();
                                        FilterWorker.StopService();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Start FileMonitor service failed:" + ex.Message);
                                    }

                                    break;
                                }

                            default: Console.WriteLine("The command " + command + " doesn't exist"); PrintUsage(); break;

                        }

                    }
                    else
                    {
                        PrintUsage();
                    }

                }
                else
                {
                    Console.WriteLine("Starting EaseFilter windows service...");
                    EaseFilterService service = new EaseFilterService();
                    ServiceBase.Run(service);

                }
            }
            catch (Exception ex)
            {
                EventManager.WriteMessage(132, "EaseFilterService", EventLevel.Error, "EaseFilter failed with error " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Exiting EaseFilter service.");
                GlobalConfig.Stop();
                mutex.Close();
            }


        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: FileMonitorService command");
            Console.WriteLine("Commands:");
            Console.WriteLine("-InstallDriver       --Install EaseFilter filter driver.");
            Console.WriteLine("-UninstallDriver     --Uninstall EaseFilter filter driver.");
            Console.WriteLine("-InstallService      --Install EaseFilter Windows service.");
            Console.WriteLine("-UnInstallService    ---Uninstall EaseFilter Windows service.");
            Console.WriteLine("-Console             ----start the console application.");
        }

    }
}
