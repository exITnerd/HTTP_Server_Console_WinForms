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
        // Implementuj logikę generowania kodu HTML na podstawie pliku CSV
        // Wczytaj dane z pliku CSV, a następnie stwórz kod HTML

        // Poniżej znajdziesz przykładową implementację, zakładając, że CSV zawiera jedną kolumnę
        string[] lines = File.ReadAllLines(csvFilePath);
        Random random = new Random();
        int randomIndex = random.Next(lines.Length);

        string randomCsvValue = lines[randomIndex];

        // Wygeneruj kod HTML na podstawie wartości z pliku CSV
        string htmlContent = $"<html><body><h1>{randomCsvValue}</h1></body></html>";

        return htmlContent;
    }

    private string GetRandomCsvFile()
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
            // Brak plików CSV, zwróć pusty ciąg znaków (możesz dostosować zwracaną wartość)
            return string.Empty;
        }
    }
}
