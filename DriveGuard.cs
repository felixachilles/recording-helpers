// DriveGuard periodically checks the number of occupied bytes on a specific drive and deletes the oldest files if a maximum number exceeded.
// Tested on Windows 8.1 with MSVC2012 only.
//
// Usage: $> DriveGuard.exe <drive letter>:\ [maximum size in bytes] [number of files to be deleted if maximum size is exceeded]
//
// Example: '$> DriveGuard.exe G:\ 11000000000000 10' will erase the 10 oldest files of each filetype in 'string[] allPatterns', if data on drive G:\ exceeds 11000000000000 Bytes = 11*10^12 Bytes = circa 10TB.
//
// v0.2: only compile-time user defined filetypes will be erased
// v0.1: program is filetype-agnostic, period is hard-coded (180 seconds)
//
// written by Felix Achilles, 2016

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace DriveGuard
{
    class Program
    {
        static void Main(string[] args)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Change filetypes that are deleted and sleep duration in seconds here.
            ////////////////////////////////////////////////////////////////////////////////////////////////
            string[] allPatterns = { "*.avi", "*.mov", "*.png", "*.jpg" };
            const int sleepSeconds = 180;
			////////////////////////////////////////////////////////////////////////////////////////////////
            long driveSize = 0, maxSize = Convert.ToInt64(args[1]);
            int numDelFiles = Convert.ToInt16(args[2]);
            // init drive information
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            // check from now until the end of time
            while(true)
            {
                foreach (DriveInfo d in allDrives)
                {
                    if(d.Name.Equals(args[0]))
                    {
                        driveSize = GetDriveSize(d);
                        Console.WriteLine(DateTime.Now.ToString() + ": Drive size is " + driveSize.ToString() + " Bytes.");
                    }
                }

                foreach (string pattern in allPatterns)
                {
                    DeleteOldFiles(driveSize, maxSize, args[0], numDelFiles, pattern);
                }
                
                Console.WriteLine("DriveGuard now sleeping for " + (sleepSeconds/60).ToString() + " minutes...");
                
                // check every 180 seconds = 3 minutes
                Thread.Sleep(sleepSeconds*1000);
            }

        }

        static long GetDriveSize(DriveInfo d)
        {
            long b = 0;
            b = d.TotalSize - d.AvailableFreeSpace;
            return b;
        }

        static void DeleteOldFiles(long dirSize, long maxSize, string dir, int numDelFiles, string pattern)
        { 
            if(dirSize > maxSize)
            {
                Console.WriteLine(DateTime.Now.ToString() + ": Drive size too large! Deleting oldest " + numDelFiles.ToString() + " files of type " + pattern);
                var oldfiles = new DirectoryInfo(dir).EnumerateFiles(pattern,SearchOption.AllDirectories).OrderBy(f => f.LastWriteTime).Take(numDelFiles).ToList();
                foreach (FileInfo file in oldfiles)
                {
                    if (file.Exists)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("{0} Exception caught.", e);
                        }
                        catch (System.UnauthorizedAccessException e)
                        {
                            Console.WriteLine("{0} Exception caught.", e);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Unknown Exception!!! {0} Exception caught.", e);
                        }
                        
                    }
                }
            }
        }
    }
}
