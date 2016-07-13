using System;
using System.Collections.Generic;
using System.Text;

namespace TransferDesk.Contracts.Logging
{
    public enum LogTarget
    {

        File, Database, EventLog

    }

    /// <summary>
    /// Defines the common logging interface
    /// </summary>
    public interface ILogger:IDisposable
    {

        /// <summary>
        /// Writes a message to the log
        /// </summary>
        /// <param name="exception">exception to write</param>
        /// <param name="stringbuilder">write pending writes</param>
        void LogException(Exception exception, StringBuilder stringbuilder = null);

        void Log(string message);

        void UserLog(string userId, string message);

    }


    /// <summary>
    /// Defines the File logging interface
    /// Following the interface segration principle, inherited from a base interface 
    /// </summary>
    public interface IFileLogger:ILogger
    {
        /// <summary>
        /// Writes a message to the file log
        /// </summary>
     
        string FileName { get; set; }
        string FilePath { get; set; }
        string FileNameSuffix { get; set; }

        //long LogLineCounter { get; set; }

        void WriteStringBuilderToDiskAndClear(StringBuilder stringBuilder);

    }

  




}
