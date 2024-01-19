using System;
using System.IO;
using System.Net;
using System.Threading;

class SimpleHttpServer
{
    private string baseDirectory;
    private HttpListener listener;

    public bool IsRunning { get; private set; }

    public void StartServer(string port, string directory)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Server is already running.");
        }

        int portNumber;
        if (!int.TryParse(port, out portNumber))
        {
            throw new ArgumentException("Invalid port number.");
        }

        baseDirectory = directory;

        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{portNumber}/");
        listener.Start();

        Console.WriteLine($"Server started at http://localhost:{portNumber}/");

        IsRunning = true;

        while (IsRunning)
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

    public void StopServer()
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("Server is not running.");
        }

        IsRunning = false;

        listener.Stop();
        listener.Close();

        Console.WriteLine("Server stopped.");
    }

    private void HandleRequest(object state)
    {
        HttpListenerContext context = (HttpListenerContext)state;
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            if (request.HttpMethod == "GET")
            {
                string fileName;

                // Extracting the requested file name from the URL
                string requestedFileName = request.Url.Segments.LastOrDefault()?.Trim('/');
                if (string.IsNullOrWhiteSpace(requestedFileName))
                {
                    // If no specific file is requested, default to "index.html"
                    fileName = Path.Combine(baseDirectory, "index.html");
                }
                else
                {
                    fileName = Path.Combine(baseDirectory, requestedFileName);
                }

                HandleFileRequest(request, response, fileName);
            }
        }
        catch (FileNotFoundException)
        {
            // File not found (404)
            response.StatusCode = (int)HttpStatusCode.NotFound;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("<html><body><h1>404 Not Found</h1></body></html>");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            // Other exceptions (500)
            Console.WriteLine($"Error handling request: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private void HandleFileRequest(HttpListenerRequest request, HttpListenerResponse response, string fileName)
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

    private string GenerateHtmlContent(string csvFilePath)
    {
        string[] lines = File.ReadAllLines(csvFilePath);
        Random random = new Random();
        int randomIndex = random.Next(lines.Length);
        string randomCsvValue = lines[randomIndex];
        string htmlContent = $"<html><body><h1>{randomCsvValue}</h1></body></html>";
        return htmlContent;
    }
}
