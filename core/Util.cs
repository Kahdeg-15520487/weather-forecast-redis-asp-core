using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace research_redis_key_pattern.core
{
    public static class Util
    {
        /// <summary>
        /// Format a size by byte to suitable size unit.
        /// </summary>
        /// <param name="bytesize">input size count by Byte.</param>
        /// <returns>a string of size with suitable size unit.</returns>
        public static string FormatByteToString(this long bytesize)
        {
            double result;
            string strSizeUnit;

            if (bytesize >= 1073741824.00)
            {
                result = bytesize / 1073741824.00;
                strSizeUnit = "GB";
            }
            else if (bytesize >= 1048576.00)
            {
                result = bytesize / 1048576.00;
                strSizeUnit = "MB";
            }
            else if (bytesize >= 1024.00)
            {
                result = bytesize / 1024.00;
                strSizeUnit = "KB";
            }
            else
            {
                result = bytesize;
                strSizeUnit = "BYTE";
            }

            string strSize = result.ToString("0.00");

            return strSize + " " + strSizeUnit;
        }
    }

    public class LogEnum
    {
        public enum LogLevel
        {
            Trace = 0,
            Debug = 1,
            Info = 2,
            Warn = 3,
            Error = 4,
            Critical = 5,
            None = 6,
        }
    }

    /// <summary>
    /// RLogHelper class is a custom extension of ILogger.
    /// </summary>
    public static class RLogHelper
    {
        //---### ### ###
        //---### ### ### REGION: PUBLIC METHOD ### ### ###
        //---### ### ###

        /// <summary>
        /// Custom log as Logger extension.
        /// </summary>
        /// <param name="logger">ilogger extension.</param>
        /// <param name="currentContext">current http context.</param>
        /// <param name="loglevel">level of log.</param>
        /// <param name="currentLine">line of code need to log.</param>
        /// <param name="logEventId">log event id.</param>
        /// <param name="exception">exception.</param>
        /// <param name="jsonObj">any type of object with json type.</param>
        /// <param name="customMessage">custom message attach to the log.</param>
        /// <param name="customeMessageValue">value of above message.</param>
        public static void LogR(
            this ILogger logger,
            HttpContext currentContext,
            LogLevel loglevel,
            int currentLine = 0,
            string logEventId = "",
            Exception exception = null,
            object jsonObj = null,
            string customMessage = "",
            string[] customeMessageValue = null)
        {
            var logType = LogEnum.LogLevel.None;
            switch (loglevel)
            {
                case LogLevel.Information:
                    logType = LogEnum.LogLevel.Info;
                    break;
                case LogLevel.Warning:
                    logType = LogEnum.LogLevel.Warn;
                    break;
                case LogLevel.Critical:
                    logType = LogEnum.LogLevel.Critical;
                    break;
                case LogLevel.Error:
                    logType = LogEnum.LogLevel.Error;
                    break;
                case LogLevel.Debug:
                    logType = LogEnum.LogLevel.Debug;
                    break;
                case LogLevel.Trace:
                    logType = LogEnum.LogLevel.Trace;
                    break;
            }

            var rlog = new RLogHandler();

            logger.Log(
                logLevel: loglevel,
                message: rlog.LogMessage(
                    context: currentContext,
                    logType: logType,
                    currentLine: currentLine,
                    logEventId: logEventId,
                    exception: exception,
                    jsonObj: jsonObj,
                    customMessage: customMessage,
                    customeMessageValue: customeMessageValue),
                args: null);
        }
    }

    /// <inheritdoc/>
    public class RLogHandler
    {
        private readonly RLogFormatMsg rlogFormatMsg;

        /// <summary>
        /// Initializes a new instance of the <see cref="RLogHandler"/> class.
        /// </summary>
        /// <param name="rlogFormatMsg">log format class.</param>
        public RLogHandler()
        {
            this.rlogFormatMsg = new RLogFormatMsg();
        }

        /// <inheritdoc/>
        public string LogMessage(
            HttpContext context,
            LogEnum.LogLevel logType,
            int currentLine = 0,
            string logEventId = "",
            Exception exception = null,
            object jsonObj = null,
            string customMessage = "",
            string[] customeMessageValue = null)
        {
            var logLocation = this.rlogFormatMsg.GenerateLogLocation(logType: logType, context: context, currentLine: currentLine, exptionObj: exception);
            var logTitle = this.rlogFormatMsg.GenerateLogTitle(logType, context, logEventId, logLocation, customMessage, customeMessageValue);
            var logContent = this.rlogFormatMsg.GenerateLogContent(logTitle: logTitle, exception: exception, jsonObj: jsonObj);

            return logContent.ToString();
        }

        /// <inheritdoc/>
        public void LogToConsole(string logText, LogEnum.LogLevel logType = LogEnum.LogLevel.Trace)
        {
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;

            switch (logType)
            {
                case LogEnum.LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogEnum.LogLevel.Critical:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogEnum.LogLevel.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogEnum.LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            //--- Display log in output console
            Console.WriteLine(logText);
        }

        /// <inheritdoc/>
        public bool LogToFile(string fileName, string fileContent)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileContent))
                {
                    // Specify a "currently active folder"
                    string rootDir = AppDomain.CurrentDomain.BaseDirectory;

                    // Create a new subfolder under the current active folder
                    string logFolder = Path.Combine(rootDir, "Logs");

                    // Create the subfolder
                    Directory.CreateDirectory(logFolder);

                    // Combine the new file name with the path
                    logFolder = Path.Combine(logFolder, fileName);

                    // Check file not exist, create the file and write to it.
                    if (!File.Exists(logFolder))
                    {
                        File.WriteAllText(logFolder, fileContent);
                    }

                    // file exist, append to it.
                    else
                    {
                        File.AppendAllText(logFolder, fileContent);
                    }

                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }

            return false;
        }
    }

    /// <summary>
    /// RLog class is a custom extension of ILogger.
    /// </summary>
    public class RLogFormatMsg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RLogFormatMsg"/> class.
        /// </summary>
        public RLogFormatMsg()
        {
        }

        //---### ### ###
        //---### ### ### REGION: PUBLIC METHOD ### ### ###
        //---### ### ###

        /// <inheritdoc/>
        public StringBuilder GenerateLogContent(string logTitle, Exception exception = null, object jsonObj = null)
        {
            var logContent = new StringBuilder();
            logContent.AppendLine(string.Empty);
            logContent.AppendLine(logTitle);

            if (exception != null)
            {
                var exceptionContent = ProcessExceptionDetail(exception);
                logContent.AppendLine(exceptionContent);
            }

            if (jsonObj != null)
            {
                var detailContent = ProcessCommonDetail(jsonObj);
                logContent.AppendLine(detailContent);
            }

            return logContent;
        }

        /// <inheritdoc/>
        public string GenerateLogLocation(LogEnum.LogLevel logType, HttpContext context, int currentLine = 0, Exception exptionObj = null)
        {
            var route = context?.GetRouteData();

            var methodName = route?.Values.GetValueOrDefault("action")?.ToString() ?? string.Empty;

            //---Consider performance (only use in case of tracing & debug with exception)
            if (exptionObj as Exception != null && logType <= LogEnum.LogLevel.Debug)
            {
                //--- Get a StackTrace object for the exception
                var st = new StackTrace(exptionObj, true);

                //--- Get the last stack frame
                foreach (var fr in st.GetFrames())
                {
                    if ((fr.GetMethod().IsFinal && fr.GetMethod().IsPublic && !fr.GetMethod().ContainsGenericParameters)
                        || fr.GetMethod().Name.ToLower().Equals(methodName.ToLower()))
                    {
                        // - Located at
                        methodName = !string.IsNullOrEmpty(fr.GetMethod().Name) ? fr.GetMethod().Name : methodName;
                        currentLine = fr.GetFileLineNumber() != currentLine && fr.GetFileLineNumber() != 0
                            ? fr.GetFileLineNumber()
                            : currentLine;
                        break;
                    }
                }
            }

            var lineText = currentLine > 0 ? $"Line {currentLine}" : string.Empty;
            var dirText = !string.IsNullOrEmpty(methodName) ? $"From {methodName}() {lineText}" : string.Empty;

            return dirText;
        }

        /// <inheritdoc/>
        public string GenerateLogTitle(LogEnum.LogLevel logType, HttpContext context, string logEventId = "", string logLocation = "", string customMessage = "", string[] customeMessageValue = null)
        {
            var dateCode = DateTime.Today.ToString("yyyy-MM-dd");
            var timeNow = DateTime.Now;
            var localZone = TimeZoneInfo.Local.GetUtcOffset(timeNow);
            var timeCode = $"{timeNow.ToString("hh:mm:ss.ffffff tt")} - GMT/UTC {localZone}"; // to microseconds

            //--- If custom message has value
            customMessage = !string.IsNullOrEmpty(customMessage) ? $" | {customMessage}" : string.Empty;

            //--- If custom message is a string format with dynamic values
            if (customeMessageValue != null)
            {
                // - Replace all text value in bracket of Custom Message by increasing number to valid string format
                var regex = new Regex(@"{.*?}");
                var i = 0;
                foreach (Match match in regex.Matches(customMessage))
                {
                    customMessage = customMessage.Replace(match.Value, "{" + i.ToString() + "}");
                    i++;
                }

                customMessage = string.Format(customMessage, customeMessageValue);
            }

            logLocation = !string.IsNullOrEmpty(logLocation) ? $"| {logLocation}" : string.Empty;
            logEventId = !string.IsNullOrEmpty(logEventId) ? $"| Eid: {logEventId}" : string.Empty;

            var requestId = context != null ? $"| Rid: {context.TraceIdentifier}" : string.Empty;
            var apiPath = context?.Request?.Path.HasValue != null ? $"| {context.Request.Path.Value}" : string.Empty;
            var requestType = !string.IsNullOrEmpty(context?.Request?.Method) ? $"| {context.Request.Method}" : string.Empty;
            var apiQuerystring = !string.IsNullOrEmpty(context?.Request?.QueryString.Value) ? $"| {context.Request.QueryString.Value}" : string.Empty;

            var introLog = $"{dateCode} | {timeCode} | {logType.ToString().ToUpper()} {requestId} {logEventId} {requestType} {apiPath} {apiQuerystring} {logLocation} {customMessage}";

            return introLog;
        }

        //---### ### ###
        //---### ### ### REGION: PRIVATE METHOD ### ### ###
        //---### ### ###
        private static string ProcessExceptionDetail(object errorObj)
        {
            //--- Loop all content in Exception
            var exception = errorObj as Exception;

            var contentLog = new StringBuilder();
            contentLog.AppendLine("... BEGIN StackTrace ...");

            if (exception != null)
            {
                do
                {
                    foreach (DictionaryEntry kvp in exception.Data)
                    {
                        contentLog.AppendFormat("{0} : {1}\n", kvp.Key, kvp.Value);
                    }

                    contentLog.AppendLine("### Message : " + exception.Message);
                    contentLog.AppendLine("### StackTrace : " + exception.StackTrace);
                }
                while ((exception = exception.InnerException) != null);
            }
            else
            {
                var objSeri = JsonConvert.SerializeObject(errorObj);
                JToken parsedJson = JToken.Parse(objSeri);
                contentLog.AppendLine(parsedJson.ToString(Formatting.Indented));
            }

            contentLog.AppendLine("... END StackTrace ...");

            return contentLog.ToString();
        }

        private static string ProcessCommonDetail(object detaiilObj)
        {
            if (detaiilObj != null)
            {
                var contentLog = new StringBuilder();
                contentLog.AppendLine("... BEGIN Details ...");
                var objSeri = JsonConvert.SerializeObject(detaiilObj);
                JToken parsedJson = JToken.Parse(objSeri);
                contentLog.AppendLine(parsedJson.ToString(Formatting.Indented));
                contentLog.AppendLine("... END Details ...");

                return contentLog.ToString();
            }

            return string.Empty;
        }
    }
}
