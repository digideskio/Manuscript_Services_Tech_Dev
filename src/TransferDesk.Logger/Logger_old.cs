using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using TransferDesk.Contracts.Logging;
using System.Diagnostics;
using System.Text;

//Lazy instantiation is implemented for filelogger instance
//fileLogger variable follows a singleton instance as it is static
//interfaces for logger follow interface segeration principle
//each logger class e.g filelogger follows single responsibility principle
//to make the class thread safe, synchroziation is done with a lock object as static to persist locks for all calls to proc. 
//a Object factory method to get a logger type as per the target type.
//helper class works on the adapter pattern 
//Stopwatch property added to helper class to check performance while writing to log, commented due to mutliple threads
//each date will have a new log file in same folder
//plus filename suffix can create seperate file for a module.
//File size limited to 1000 lines per file
//helper can have extension methods for logger classes so it is extensible to change but core will be with logger classes.
//exception.Tostring will show all inner exceptions if not null
//will handle line breaks using a wrapper with environment.newline character

namespace TransferDesk.Logger
{
   public class LogHelper : IFileLogger

    {
        private static FileLogger _fileLogger = null; //a singleton pattern

        //interface defined below
       public string FilePath{get;set;}

        public string FileName { get; set; }

        public string FileNameSuffix { get; set; }

        //extra properties below
        public LogTarget LogTarget { get; set; }

        public LogHelper()
        {
            LogTarget = LogTarget.File; //default

        }

        public void Dispose()
        {
            _fileLogger.Dispose();
        }

       public void WriteStringBuilderToDiskAndClear(StringBuilder stringBuilder)
        {
            ILogger logger = GetLogger();
            IFileLogger fileLogger = logger as IFileLogger;
            fileLogger.FilePath = this.FilePath;
            fileLogger.FileName = this.FileName;
            fileLogger.WriteStringBuilderToDiskAndClear(stringBuilder);
       }


        public void Log(string message)

        {
            switch (LogTarget)

            {
                case LogTarget.File:
                    ILogger logger = GetLogger();
                    IFileLogger fileLogger = logger as IFileLogger;
                    fileLogger.FilePath = this.FilePath;
                    fileLogger.FileName = this.FileName;
                    fileLogger.Log(message);

                    break;
            }

        }


        public void LogException(Exception exception)

        {
            switch (LogTarget)

            {

                case LogTarget.File:

                    ILogger logger = GetLogger();
                    IFileLogger fileLogger = logger as IFileLogger;
                    fileLogger.FilePath = this.FilePath;
                    fileLogger.FileName = this.FileName;
                    fileLogger.LogException(exception);

                    break;


            }

        }


       private ILogger GetLogger() //factory method to return objects
       {
           switch (LogTarget)
           {
                case LogTarget.File:
                    if (_fileLogger == null)
                    {
                        _fileLogger = new FileLogger();

                    }
                    return _fileLogger;
           }
            return null;
       }

    }

    public class FileLogger : IFileLogger

        {
            protected static readonly object LockObj = new object();

            protected static Stopwatch PerformanceStopWatch;

          
            public string FileName { get; set; }

            public string FileNameSuffix { get; set; }

            public string FilePath { get; set; }

            public long LogLineCounter { get; private set; }

            public int FileNameSuffixCounter { get; private set; }

            public int LogLineLimitCount { get; set; }

            public FileLogger()
            {
                LogLineCounter = 1;
                LogLineLimitCount = 1000; 
                //this will help control file size limit 1 character = 1 byte appox. each line 100 char average
                PerformanceStopWatch = new Stopwatch();
                PerformanceStopWatch.Start();
               
            }
        

        public virtual void Dispose()
        {
            //dispose here
            if(PerformanceStopWatch.IsRunning)
            PerformanceStopWatch.Stop();
        }


        public void Log(string message)
        {
                try
                {
                    WriteToDisk(message);
                    
                }
                catch (Exception loggerException)
                {
                   TryWriteForLoggerException(loggerException,message);
                }
              
        }

            private void WriteToDisk(string message )
            {
                
                lock (LockObj)

                {
                    using (StreamWriter streamWriter = new StreamWriter(FilePath + FileName + FileNameSuffix 
                        + "_" + DateTime.Today.Day + "_" + DateTime.Today.Month + "_" + DateTime.Today.Year + "_" + FileNameSuffixCounter + "_log.txt", true))

                    {
                        string timeStamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff");
                        
                        streamWriter.WriteLine(LogLineCounter + " " +
                            timeStamp  + " " +
                            PerformanceStopWatch.ElapsedMilliseconds + " " + message);
                     
                        streamWriter.Close();

                    }

                    PerformanceStopWatch.Restart();
                   
                }//lock ends

            LogLineCounter += 1;

            if (LogLineCounter > LogLineLimitCount)
            {
                FileNameSuffixCounter += 1;
            }
            
            }

            public void WriteStringBuilderToDiskAndClear(StringBuilder stringBuilder )
            {
                try
                {
                    WriteToDisk(stringBuilder.ToString());
                }
                catch (Exception loggerException)
                {
                    TryWriteForLoggerException(loggerException, stringBuilder.ToString());
                }
                finally
                {
                    stringBuilder.Clear();
                   
                }
                
            }

            public void LogException(Exception exception)

            {
                string message = null;
                try
                {
                    var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                
                    //exception.tostring will include all inner exception details

                    message = "userID : " + userId + " exception : " + exception.ToString();

                    WriteToDisk(message);

                    //throw new Exception("test logger exception");
                }
                catch (Exception loggerException)
                {
                    TryWriteForLoggerException(loggerException, message);
                }
                    
            }
            


            public void TryWriteForLoggerException(Exception loggerException, string message)

            {
            string timeStamp = DateTime.Now.ToString("yyyy_MM_dd");
            string filePathForExceptionInLogger = (FilePath + "whenLoggerException" + timeStamp);

                using (StreamWriter streamWriter = new StreamWriter(filePathForExceptionInLogger))
                {
                    streamWriter.WriteLine(message);
              
                    streamWriter.WriteLine("ExceptionInLogger:" + loggerException.Message);
                    streamWriter.Close();

                }
            }

        }

   
    }

