//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass.Conosle
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var program = new Program();

            if (args != null)
            {
                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        await Task.WhenAll(
                            args.Where(arg => !string.IsNullOrWhiteSpace(arg)).Select(arg => Task.Run(() => program.Start(arg))))
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        tokenSource.Cancel();
                        throw;
                    }
                }
            }
        }

        private async Task StartAsync(string filename, CancellationToken token)
        {
            string password;

            using (var checker = new PasswordChecker(filename))
            using (var passFinder = new DePass(checker, false))
            {
                password = await passFinder.FindPasswordAsync(token).ConfigureAwait(false);
            }

            Console.WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, Strings.OutputFormat, filename, password ?? "(none)"));
        }

        private void Start(string filename)
        {
            string password;

            using (var checker = new PasswordChecker(filename))
            using (var passFinder = new DePass(checker, false))
            {
                password = passFinder.FindPassword();
            }

            Console.WriteLine(string.Format(Thread.CurrentThread.CurrentCulture, Strings.OutputFormat, filename, password ?? "(none)"));
        }
    }
}
