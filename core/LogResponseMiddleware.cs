// <copyright file="LogResponseMiddleware.cs" company="Rosen Group">
// Copyright (c) Rosen Group. All rights reserved.
// </copyright>

namespace research_redis_key_pattern.core
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// LogResponseMiddleware class.
    /// </summary>
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate nextDelegate;
        private readonly ILogger logger;

        //---### ### ### REGION: PUBLIC METHOD ### ### ###

        /// <summary>
        /// Initializes a new instance of the <see cref="LogResponseMiddleware"/> class.
        /// </summary>
        /// <param name="nextDelegate">request delegate.</param>
        /// <param name="loggerFactory">logger factory.</param>
        /// <param name="localizerLogMesTemp">string localizer for log message template.</param>
        public LogResponseMiddleware(RequestDelegate nextDelegate, ILoggerFactory loggerFactory)
        {
            this.nextDelegate = nextDelegate;
            this.logger = loggerFactory.CreateLogger("Rosen-GlobalLogger");
        }

        /// <summary>
        /// Invoke default method in Middleware.
        /// </summary>
        /// <param name="httpContext">http context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var watch = Stopwatch.StartNew();
            await this.nextDelegate.Invoke(httpContext);
            watch.Stop();

            // log response
            this.LogResponse(context: httpContext, elapsedTime: watch);
            this.LogErrorResponse(context: httpContext);
        }

        //---### ### ### REGION: PRIVATE METHOD ### ### ###
        private void LogResponse(HttpContext context, Stopwatch elapsedTime)
        {
            var statusCode = context.Response.StatusCode;

            var logMessage = "RequestInfo: {RequestPath} responded {StatusCode} in {ElapsedTime} ms with size {ResponseSize}.";
            var logMessageValue = new string[]
            {
                context.Request.Path,
                context.Response.StatusCode.ToString() + $" ({ReasonPhrases.GetReasonPhrase(statusCode)})",
                elapsedTime.Elapsed.TotalMilliseconds.ToString(),
                context.Response.ContentLength?.FormatByteToString(),
            };

            if (statusCode >= 500)
            {
                this.logger.LogR(
                   currentContext: context,
                   loglevel: LogLevel.Error,
                   customMessage: logMessage,
                   customeMessageValue: logMessageValue);
            }
            else if (statusCode >= 400)
            {
                this.logger.LogR(
                   currentContext: context,
                   loglevel: LogLevel.Warning,
                   customMessage: logMessage,
                   customeMessageValue: logMessageValue);
            }
            else
            {
                this.logger.LogR(
                   currentContext: context,
                   loglevel: LogLevel.Information,
                   customMessage: logMessage,
                   customeMessageValue: logMessageValue);
            }
        }

        private void LogErrorResponse(HttpContext context)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionDetails?.Error;

            if (exception == null)
            {
                return;
            }

            string logMessage = $"{exception.GetType().ToString()}: {exception.Message}";

            // - Handled Custom Errors
            if (exception is null)
            {
                this.logger.LogR(
                  currentContext: context,
                  loglevel: LogLevel.Warning,
                  logEventId: LogEvent.LogicErrorExceptionCode.ToString(),
                  customMessage: logMessage);
            }// - Unhandled Exceptions
            else
            {
                if (

                    // Very common exception related to calling a method on a null object reference
                    exception is NullReferenceException

                    // If your app runs out of memory
                    || exception is OutOfMemoryException

                    // Used around many file I/O type operations
                    || exception is System.IO.IOException

                    // Occurs attempting to access an array element that does not exist
                    || exception is IndexOutOfRangeException

                    // Common generic exception in a lot of libraries
                    || exception is InvalidOperationException

                    // One of the arguments provided to a method is not valid
                    || exception is ArgumentException)
                {
                    this.logger.LogR(
                      currentContext: context,
                      loglevel: LogLevel.Critical,
                      logEventId: LogEvent.CriticalExceptionCode.ToString(),
                      exception: exception,
                      customMessage: logMessage);
                }// Others
                else
                {
                    this.logger.LogR(
                      currentContext: context,
                      loglevel: LogLevel.Error,
                      logEventId: LogEvent.CommonExceptionCode.ToString(),
                      exception: exception,
                      customMessage: logMessage);
                }
            }
        }
    }

    public static class LogEvent
    {
        /// <summary>
        /// Background service exception.
        /// </summary>
        public const int BackgroundServiceExceptionCode = 4001;

        /// <summary>
        /// InvalidOperationException occurred in BackgroundService.
        /// </summary>
        public const int InvalidOperationExceptionCode = 4004;

        /// <summary>
        /// NullReferenceExceptionCode occurred in BackgroundService.
        /// </summary>
        public const int NullReferenceExceptionCode = 4005;

        /// <summary>
        /// Logic Error Exception.
        /// </summary>
        public const int LogicErrorExceptionCode = 5001;

        /// <summary>
        /// Critical Exception.
        /// </summary>
        public const int CriticalExceptionCode = 5002;

        /// <summary>
        /// Common Exception.
        /// </summary>
        public const int CommonExceptionCode = 5003;
    }
}