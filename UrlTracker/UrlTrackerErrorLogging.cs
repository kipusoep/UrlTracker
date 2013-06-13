using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoCaster.Umbraco.UrlTracker
{
	/// <summary>
	/// The Error Logger interface
	/// </summary>
	public interface IUrlTrackerErrorLogger
	{
		/// <summary>
		/// The Log method
		/// </summary>
		/// <param name="message">The error message</param>
		void Log(string message);
		/// <summary>
		/// The Log method
		/// </summary>
		/// <param name="exception">The exception</param>
		void Log(Exception exception);
	}

	/// <summary>
	/// The Error Logging class
	/// </summary>
	public static class UrlTrackerErrorLogging
	{
		/// <summary>
		/// The active Error Logger instance
		/// </summary>
		internal static IUrlTrackerErrorLogger ErrorLogger { get; set; }
		/// <summary>
		/// Registers the provided implemented <see cref="IUrlTrackerErrorLogger"/> as active ErrorLogger
		/// </summary>
		/// <param name="errorLogger">The implemented <see cref="IUrlTrackerErrorLogger"/></param>
		public static void RegisterLogger(IUrlTrackerErrorLogger errorLogger)
		{
			ErrorLogger = errorLogger;
		}
		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="message">The message</param>
		internal static void Log(string message)
		{
			if (ErrorLogger != null)
				ErrorLogger.Log(message);
		}
		/// <summary>
		/// Log an <see cref="Exception"/>
		/// </summary>
		/// <param name="exception">The <see cref="Exception"/></param>
		internal static void Log(Exception exception)
		{
			if (ErrorLogger != null)
				ErrorLogger.Log(exception);
		}
	}
}
