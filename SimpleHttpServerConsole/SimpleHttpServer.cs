using System;
using System.IO;
using System.Net;
using System.Threading;

class SimpleHttpServer
{
    private static string baseDirectory;
    private static HttpListener listener;

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: SimpleHttpServer <port> <base_directory>");
            return;
        }

        int port;
        if (!int.TryParse(args[0], out port))
        {
            Console.WriteLine("Invalid port number.");
            return;
        }

        baseDirectory = args[1];

        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        Console.WriteLine($"Server started at http://localhost:{port}/");

        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(HandleRequest, context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void HandleRequest(object state)
    {
        HttpListenerContext context = (HttpListenerContext)state;
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            if (request.HttpMethod == "GET")
            {
                string fileName;
                if (request.Url.LocalPath.EndsWith(".html"))
                {
                    fileName = Path.Combine(baseDirectory, request.Url.LocalPath.TrimStart('/'));
                }
                else
                {
                    fileName = GetRandomCsvFile();
                }

                HandleFileRequest(request, response, fileName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private static void HandleFileRequest(HttpListenerRequest request, HttpListenerResponse response, string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                string content = GenerateHtmlContent(fileName);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("<html><body><h1>404 Not Found</h1></body></html>");
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling file request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private static string GenerateHtmlContent(string csvFilePath)
    {
        string[] lines = File.ReadAllLines(csvFilePath);
        Random random = new Random();
        int randomIndex = random.Next(lines.Length);
        string randomCsvValue = lines[randomIndex];
        string htmlContent = $"<html><body><h1>{randomCsvValue}</h1></body></html>";
        return htmlContent;
    }

    private static string GetRandomCsvFile()
    {
        string[] csvFiles = Directory.GetFiles(baseDirectory, "*.csv");
        if (csvFiles.Length > 0)
        {
            Random random = new Random();
            int randomIndex = random.Next(csvFiles.Length);
            return Path.Combine(baseDirectory, Path.GetFileName(csvFiles[randomIndex]));
        }
        else
        {
            return string.Empty;
        }
    }
}
