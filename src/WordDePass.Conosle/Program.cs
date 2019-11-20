//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass.Conosle
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program
    {
        private readonly PasswordHints hints;

        private Program(PasswordHints hints)
        {
            this.hints = hints ?? new PasswordHints();
        }

        private static async Task Main(string[] args)
        {
            (PasswordHints hints, IList<string> filenames) parsed;
            try
            {
                parsed = Parse(args);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                PrintUsage();
                return;
            }

            var program = new Program(parsed.hints);
            using (var tokenSource = new CancellationTokenSource())
            {
                var tasks = parsed.filenames
                    .Where(filename => !string.IsNullOrWhiteSpace(filename))
                    .Select(filename => program.StartAsync(filename, tokenSource.Token));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine(Strings.Usage);
        }

        private static (PasswordHints hints, IList<string> filenames) Parse(string[] args)
        {
            args = args ?? Array.Empty<string>();
            var hints = new PasswordHints();
            Alphabet alphabet = null;
            var filenames = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }

                if (arg[0] != '-')
                {
                    filenames.Add(arg);
                    continue;
                }

                var offset = 0;
                foreach (var flag in arg.Skip(1))
                {
                    offset++;
                    var value = args.Length > i + offset ? args[i + offset] : null;

                    switch (flag)
                    {
                        case '?':
                        case 'h':
                        case 'H':
                            PrintUsage();
                            return default;

                        case 'a':
                            Alphabet bet;
                            try
                            {
                                bet = Alphabet.GetByName(value);
                            }
                            catch (NotSupportedException)
                            {
                                bet = new Alphabet(value);
                            }

                            alphabet = alphabet != null ? alphabet + bet : bet;
                            break;

                        case 'p':
                            hints.AddPrefix(value);
                            break;

                        case 'i':
                            hints.AddInfix(value);
                            break;

                        case 's':
                            hints.AddSuffix(value);
                            break;

                        case 'l':
                            try
                            {
                                hints.MinLength = int.Parse(value, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture);
                                hints.MaxLength = hints.MinLength;
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException(
                                    string.Format(Thread.CurrentThread.CurrentCulture, Strings.Args_InvalidLength, Strings.Minimum),
                                    e);
                            }

                            break;

                        case 'n':
                            try
                            {
                                hints.MinLength = int.Parse(value, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture);
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException(
                                    string.Format(Thread.CurrentThread.CurrentCulture, Strings.Args_InvalidLength, Strings.Minimum),
                                    e);
                            }

                            break;

                        case 'x':
                            try
                            {
                                hints.MaxLength = int.Parse(value, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture);
                            }
                            catch (Exception e)
                            {
                                throw new ArgumentException(
                                    string.Format(Thread.CurrentThread.CurrentCulture, Strings.Args_InvalidLength, Strings.Maximum),
                                    e);
                            }

                            break;

                        default:
                            throw new ArgumentException(
                                string.Format(Thread.CurrentThread.CurrentCulture, Strings.Args_UnknownOption, flag));
                    }
                }

                i += offset;
            }

            hints.Alphabet = alphabet;

            if (!filenames.Any())
            {
                throw new ArgumentException(Strings.Args_FilenameRequired);
            }

            return (hints, filenames);
        }

        private async Task StartAsync(string filename, CancellationToken token)
        {
            string password;

            using (var checker = new PasswordChecker(filename))
            using (var passFinder = new DePass(checker, false))
            {
                password = await passFinder.FindPasswordAsync(this.hints, token).ConfigureAwait(false);
            }

            Console.WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, Strings.OutputFormat, filename, password ?? "(unknown)"));
        }
    }
}
