﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace CustomAppProtocol
{
    partial class Program()
    {
        private const string baseDirectory = "D:\\MyApps\\";

        // Parse the URL into app name and parameters then convert the parameters to a string to pass to the app
        static Task Main(string[] args)
        {
            string appProtocol = "myapp:/";

            if (args.Length > 0)
            {
                string url = args[0];

                try
                {
                    url = ProcessUrl(url, appProtocol);
                    var (appName, paramsStr) = ExtractAppNameAndParams(url);
                    RunApp(appName, paramsStr);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (FormatException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                throw new ArgumentException("No arguments received!", nameof(args));
            }

            return Task.CompletedTask;
        }

        static string ProcessUrl(string url, string appProtocol)
        {
            // Print the raw URL for debugging
            Console.WriteLine($"Raw URL: {url}");

            // Throw an exception if the URL is empty
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL is missing!");
            }

            // Remove the protocol from the URL
            url = url.Replace(appProtocol, "", StringComparison.Ordinal);

            // Print the URL after removing the protocol for debugging
            Console.WriteLine($"URL after removing protocol: {url}");

            return url;
        }

        static (string appName, string paramsStr) ExtractAppNameAndParams(string url)
        {
            // Split the URL by the '?' char to separate the base and query parts
            var urlParts = url.Split('?');

            // Throw an exception if the app name is missing
            if (urlParts[0].Length == 0)
            {
                throw new FormatException("App name is missing in the url!");
            }

            string appName = urlParts[0];
            string paramsStr = "";

            // Print the extracted app name for debugging
            Console.WriteLine($"Extracted App Name: {appName}");

            // Check if there are any parameters in the URL
            if (urlParts.Length > 1)
            {
                paramsStr = ProcessQueryParams(urlParts[1]);
            }

            return (appName, paramsStr);
        }

        static string ProcessQueryParams(string query)
        {
            // Parse the query string into a collection of key-value pairs
            var paramsCollection = HttpUtility.ParseQueryString(query);

            // Create a list of key-value pairs from the params collection
            var keyValuePairs = paramsCollection.AllKeys.Select(k => $"{k} => {paramsCollection[k]}");

            // Join the key-value pairs into a single string
            string paramsStr = string.Join(", ", keyValuePairs);

            // Print the parameters for debugging
            Console.WriteLine($"Recieved {paramsCollection.Count} parameters(s)");

            // The foreach loop will iterate over the params collection and print the key and value for each param
            foreach (var key in paramsCollection.AllKeys)
            {
                Console.WriteLine($"Key: {key} => Value: {paramsCollection[key]}");
            }

            return paramsStr;
        }

        // Define a regex pattern to match the app name
        [GeneratedRegex(@"^App(\d+)$")]
        private static partial Regex MyRegex();

        // Run the specified app with the given parameter
        static void RunApp(string app, string parameter)
        {
            if (string.IsNullOrWhiteSpace(app))
                throw new ArgumentException("App identifier cannot be empty!", nameof(app));

            var match = MyRegex().Match(app);
            if (!match.Success)
                throw new ArgumentException($"Invalid app identifier format: {app}", nameof(app));

            // Construct the path to the executable
            string appNumber = match.Groups[1].Value;
            string appName = $"App{appNumber}";
            string exePath = Path.Combine(baseDirectory, appName, appName, "bin", "Debug", "net8.0-windows", $"{appName}.exe");

            if (!File.Exists(exePath))
                throw new FileNotFoundException($"Executable not found for {app}", exePath);

            // Start the process with the command-line parameter
            ProcessStartInfo startInfo = new(exePath)
            {
                ArgumentList = { parameter }
            };
            Process.Start(startInfo);
        }

    }
}