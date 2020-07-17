﻿using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;

using Unknown6656.AutoIt3.ExpressionParser;
using Unknown6656.AutoIt3.Runtime.Native;
using Unknown6656.AutoIt3.Runtime;
using Unknown6656.Common;

namespace Unknown6656.AutoIt3.Extensibility.Plugins.Au3Framework
{
    using static MainProgram;
    using static AST;

    public sealed class FrameworkMacros
        : AbstractMacroProvider
    {
        internal const string MACRO_DISCARD = "DISCARD";
        private static readonly Regex REGEX_IPADDRESS = new Regex(@"ipaddress(?<num>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public FrameworkMacros(Interpreter interpreter)
            : base(interpreter)
        {
        }

        public unsafe override bool ProvideMacroValue(CallFrame frame, string name, out Variant? value)
        {
            SourceLocation location = frame.CurrentThread.CurrentLocation ?? Interpreter.MainThread?.CurrentLocation ?? SourceLocation.Unknown;

            value = name.ToUpperInvariant() switch
            {
                "APPDATACOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "APPDATADIR" => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AUTOITEXE" => ASM_FILE.FullName,
                "AUTOITPID" => Process.GetCurrentProcess().Id,
                "AUTOITVERSION" => __module__.InterpreterVersion?.ToString() ?? "0.0.0.0",
                "AUTOITX64" => sizeof(void*) > 4,
                "COMMONFILESDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                "COMPILED" => false,
                "COMPUTERNAME" => Environment.MachineName,
                "COMSPEC" => Environment.GetEnvironmentVariable(NativeInterop.DoPlatformDependent("comspec", "SHELL")),
                "CR" => "\r",
                "CRLF" => Environment.NewLine,
                "CPUARCH" or "osarch" => Environment.Is64BitOperatingSystem ? "X64" : "X86",
                "DESKTOPCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                "DESKTOPDIR" => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "DOCUMENTSCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "EXITCODE" => Interpreter.ExitCode,
                "ERROR" => Interpreter.ErrorCode,
                "EXTENDED" => Interpreter.ExtendedValue,
                "FAVORITESCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                "FAVORITESDIR" => Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                "HOMEDRIVE" => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).Root.FullName,
                "HOMEPATH" or "userprofiledir" => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "HOUR" => DateTime.Now.ToString("HH", null),
                "LOCALAPPDATADIR" => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LOGONDOMAIN" => Environment.UserDomainName,
                "LOGONSERVER" => @"\\" + Environment.UserDomainName,
                "MDAY" => DateTime.Now.ToString("dd", null),
                "MIN" => DateTime.Now.ToString("mm", null),
                "MON" => DateTime.Now.ToString("MM", null),
                "MSEC" => DateTime.Now.ToString("fff", null),
                "MYDOCUMENTSDIR" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NUMPARAMS" => (frame as AU3CallFrame)?.PassedArguments.Length ?? 0,
                "MUILANG" or "oslang" => NativeInterop.DoPlatformDependent(NativeInterop.GetUserDefaultUILanguage, () => default),
                "TAB" => "\t",
                "SW_DISABLE" => 65,
                "SW_ENABLE" => 64,
                "SW_HIDE" => 0,
                "SW_LOCK" => 66,
                "SW_MAXIMIZE" => 3,
                "SW_MINIMIZE" => 6,
                "SW_RESTORE" => 9,
                "SW_SHOW" => 5,
                "SW_SHOWDEFAULT" => 10,
                "SW_SHOWMAXIMIZED" => 3,
                "SW_SHOWMINIMIZED" => 2,
                "SW_SHOWMINNOACTIVE" => 7,
                "SW_SHOWNA" => 8,
                "SW_SHOWNOACTIVATE" => 4,
                "SW_SHOWNORMAL" => 1,
                "SW_UNLOCK" => 67,
                "TEMPDIR" => NativeInterop.DoPlatformDependent(Environment.GetEnvironmentVariable("temp"), "/tmp"),

                "OSBUILD" => Environment.OSVersion.Version.Build,
                "OSTYPE" => NativeInterop.DoPlatformDependent("WIN32_NT", "UNIX", "MACOS_X"),

                "PROGRAMFILESDIR" => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "PROGRAMSCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                "PROGRAMSDIR" => Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "SCRIPTDIR" => Path.GetDirectoryName(location.FullFileName),
                "SCRIPTFULLPATH" => location.FullFileName,
                "SCRIPTLINENUMBER" => location.StartLineNumber,
                "SCRIPTNAME" => Path.GetFileName(location.FullFileName),
                "STARTMENUCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                "STARTMENUDIR" => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "STARTUPCOMMONDIR" => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
                "STARTUPDIR" => Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "SYSTEMDIR" => Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                "WINDOWSDIR" => Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "SEC" => DateTime.Now.ToString("ss", null),
                "USERNAME" => Environment.UserName,
                "YDAY" => DateTime.Now.DayOfYear.ToString("D3", null),
                "YEAR" => DateTime.Now.ToString("yyyy", null),
                "WDAY" => (int)DateTime.Now.DayOfWeek + 1,
                "WORKINGDIR" => Directory.GetCurrentDirectory(),

                "LF" => "\n",

                _ when name.Equals(MACRO_DISCARD, StringComparison.InvariantCultureIgnoreCase) =>
                    frame.VariableResolver.TryGetVariable(VARIABLE.Discard, VariableSearchScope.Global, out Variable? discard) ? discard.Value : Variant.Null,
                _ => (Variant?)null,
            };

            if (value is null && name.Match(REGEX_IPADDRESS, out ReadOnlyIndexer<string, string>? g))
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                List<string> ips = new List<string>();
                int idx = (int)decimal.Parse(g["num"], null);

                foreach (IPAddress ip in host.AddressList)
                    if (ip.AddressFamily is AddressFamily.InterNetwork)
                        ips.Add(ip.ToString());

                value = idx < ips.Count ? ips[idx] : "0.0.0.0";
            }

            return value is Variant;
        }
    }

    public sealed class AdditionalMacros
        : AbstractMacroProvider
    {
        public AdditionalMacros(Interpreter interpreter)
            : base(interpreter)
        {
        }

        public override bool ProvideMacroValue(CallFrame frame, string name, out Variant? value) => (value = name.ToUpperInvariant() switch
        {
            "ESC" => "\x1b",
            "VTAB" => "\v",
            "NUL" => "\0",
            "DATE" => DateTime.Now.ToString("yyyy-MM-dd", null),
            "DATE_TIME" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff", null),
            "E" => Math.E,
            "NL" => Environment.NewLine,
            "PHI" => 1.618033988749894848204586834m,
            "PI" => Math.PI,
            "TAU" => Math.PI * 2,
            _ => (Variant?)null,
        }) is Variant;
    }
}