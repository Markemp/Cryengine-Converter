using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CgfConverter.Utilities;

public enum StringSizeEnum
{
    Int8 = 1,
    Int16 = 2,
    Int32 = 4,
}

public enum LogLevelEnum
{
    /// <summary>Log Everything</summary>
    Verbose = 0x01,
    /// <summary>Log Some Stuff Useful For Debugging</summary>
    Debug = 0x02,
    /// <summary>Log Information/Progress Updates </summary>
    Info = 0x04,
    /// <summary>Log Warnings (Default)</summary>
    Warning = 0x08,
    /// <summary>Log Errors</summary>
    Error = 0x0F,
    /// <summary>Log Critical Errors</summary>
    Critical = 0x10,
    /// <summary>Don't Log Anything</summary>
    None = 0xFF,
}

public static class HelperMethods
{
    public static LogLevelEnum LogLevel { get; set; }
    public static LogLevelEnum DebugLevel { get; set; }

    /// <summary>Private DataStore for the DateTimeFormats property.summary>
    private static string[] _dateTimeFormats = [@"yyyy-MM-dd\THHmm"]; // 2010-12-31T2359

    public static double Safe(double value)
    {
        if (value == double.NegativeInfinity)
            return double.MinValue;

        if (value == double.PositiveInfinity)
            return double.MaxValue;

        if (double.IsNaN(value))
            return 0;

        return value;
    }

    public static void CleanNumbers(StringBuilder sb)
    {
        sb.Replace("0.000000", "0");
        sb.Replace("-0.000000", "0");
        sb.Replace("1.000000", "1");
        sb.Replace("-1.000000", "-1");
    }

    internal static List<string> GetNullSeparatedStrings(int numberOfNames, BinaryReader b)
    {
        List<string> names = [];
        StringBuilder builder = new();

        for (int i = 0; i < numberOfNames; i++)
        {
            char c = b.ReadChar();
            while (c != 0)
            {
                builder.Append(c);
                c = b.ReadChar();
            }
            names.Add(builder.ToString());
            builder.Clear();
        }

        return names;
    }

    /// <summary>Custom DateTime formats supported by the parser.</summary>
    public static string[] DateTimeFormats {
        get { return _dateTimeFormats; }
        set { _dateTimeFormats = value; }
    }

    public static void Log(string? format = null, params object[] args) => Log(LogLevelEnum.Debug, format, args);

    public static void Log(LogLevelEnum logLevel, string? format = null, params object[] args)
    {
        if (LogLevel <= logLevel)
            Console.WriteLine(format ?? string.Empty, args);

        if (DebugLevel <= logLevel)
            Debug.WriteLine(format ?? string.Empty, args);
    }
}
