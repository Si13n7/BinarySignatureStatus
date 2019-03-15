namespace BinarySignatureStatus
{
    using System;
    using System.IO;
    using Properties;
    using SilDev;

    public class Program
    {
        private const ConsoleColor DefaultBgColor = ConsoleColor.Black;
        private const ConsoleColor DefaultCaptionFgColor = ConsoleColor.DarkGreen;
        private const ConsoleColor DefaultFgColor = ConsoleColor.White;
        private const ConsoleColor ErrorCaptionFgColor = ConsoleColor.DarkRed;
        private const ConsoleColor WarnCaptionFgColor = ConsoleColor.DarkYellow;
        private static bool _active;

        private static void Main()
        {
            InitializeConsole();
            foreach (var arg in EnvironmentEx.CommandLineArgs(true, 1, false))
            {
                if (_active)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }
                var file = PathEx.Combine(arg);
                if (!File.Exists(file))
                {
                    WriteInnerLine($"Cannot find \'{file}\'", "Error", ErrorCaptionFgColor);
                    continue;
                }
                try
                {
                    var name = Path.GetFileName(file);
                    if (!string.IsNullOrWhiteSpace(name))
                        WriteInnerLine(name, "File", !name.EndsWithEx(".exe", ".dll") ? WarnCaptionFgColor : DefaultCaptionFgColor);

                    var version = FileEx.GetFileVersion(file)?.ToString();
                    if (!string.IsNullOrWhiteSpace(version) && !version.Equals("0.0.0.0"))
                        WriteInnerLine(version, "Version", DefaultCaptionFgColor);

                    var subject = FileEx.GetSignatureSubject(file);
                    if (!string.IsNullOrWhiteSpace(subject))
                        WriteInnerLine(subject, "Subject", DefaultCaptionFgColor);

                    var status = FileEx.GetSignatureStatus(file);
                    if (!string.IsNullOrWhiteSpace(status))
                        WriteInnerLine(status, "Status", status.ContainsEx("Unknown", "Error") ? ErrorCaptionFgColor : status.ContainsEx("NotSigned") ? WarnCaptionFgColor : DefaultCaptionFgColor);
                }
                catch (Exception ex)
                {
                    WriteInnerLine(ex.Message, "Error", ErrorCaptionFgColor);
                }
            }
            ExitConsole();
        }

        private static void InitializeConsole()
        {
            Console.Title = AssemblyInfo.Title;
            var longTitle = $"{Console.Title} v{AssemblyInfo.Version}";
            SetColors(ConsoleColor.DarkCyan);
            Console.WriteLine(Resources.Logo.UnzipText());
            SetColors(ConsoleColor.DarkGray);
            for (var i = longTitle.Length / 2; i < 25; i++)
                Console.Write(@" ");
            Console.WriteLine(longTitle);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            SetColors();
        }

        private static void ExitConsole()
        {
            if (!_active)
                return;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            SetColors(ConsoleColor.DarkGray);
            Console.WriteLine(@"Press any key to close this window . . .");
            Console.ReadKey();
        }

        private static void SetColors(ConsoleColor fgColor = DefaultFgColor, ConsoleColor bgColor = DefaultBgColor)
        {
            if (Console.ForegroundColor != fgColor)
                Console.ForegroundColor = fgColor;
            if (Console.BackgroundColor != bgColor)
                Console.BackgroundColor = bgColor;
        }

        private static void WriteInnerLine(string message = default(string), string caption = default(string), ConsoleColor captionFgColor = DefaultFgColor, ConsoleColor captionBgColor = DefaultBgColor)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine();
                return;
            }
            if (!_active)
                _active = true;
            message = message.Trim();
            if (!string.IsNullOrWhiteSpace(caption))
            {
                caption = " " + caption.Trim();
                if (!caption.EndsWith(":"))
                    caption = $"{caption}:";
                SetColors(captionFgColor, captionBgColor);
                Console.Write(caption);
                SetColors();
                for (var i = caption.Length; i < 10; i++)
                    Console.Write(@" ");
                if (message.ContainsEx("\r", "\n"))
                {
                    var lines = TextEx.FormatNewLine(message).SplitNewLine();
                    for (var i = 1; i < lines.Length; i++)
                        lines[i] = lines[i].PadLeft(lines[i].Length + 11);
                    message = lines.Join(Environment.NewLine);
                }
            }
            Console.WriteLine(@" " + message);
        }
    }
}
