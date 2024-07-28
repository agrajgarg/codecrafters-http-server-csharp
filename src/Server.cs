using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    
    static async Task Main()
    {
        Console.WriteLine("Server starting...");
        var server = new TcpListener(IPAddress.Any, 4221);
        server.Start();
        Console.WriteLine("Server started. Listening for connections...");
    
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }
    
    static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var parts = request.Split("\r\n");
            var path = parts[0].Split(' ')[1];
            string method = parts[0].Split("/")[0].Trim();
    
            string message = String.Empty;
            if (path == "/")
            {
                message = "HTTP/1.1 200 OK\r\n\r\n";
            }
            else if (path.StartsWith("/echo/"))
            {
                var echoContent = path.Substring("/echo/".Length);
                message = $"HTTP/1.1 200 OK\r\n" +
                          $"Content-Type: text/plain\r\n" +
                          $"Content-Length: {echoContent.Length}\r\n" +
                          $"\r\n{echoContent}";
            }
            else if (path == "/user-agent")
            {
                var userAgent = parts.FirstOrDefault(p => p.StartsWith("User-Agent: "))
                    ?.Substring("User-Agent: ".Length);
                message = $"HTTP/1.1 200 OK\r\n" +
                          $"Content-Type: text/plain\r\n" +
                          $"Content-Length: {userAgent?.Length ?? 0}\r\n" +
                          $"\r\n{userAgent}";
            }
            else if (path.StartsWith("/files"))
            {
                var fileName = path.Split("/")[2];
                var env = Environment.GetCommandLineArgs();
                var currentDirectory = env[2];
                var filePath = currentDirectory + "/"+ fileName;

                if (method == "GET")
                {
                    if (File.Exists(filePath))
                    {
                        var fileContent = File.ReadAllText((filePath));
                        message =
                            $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n\r\n{fileContent}";
                    }
                    else
                    {
                        message = "HTTP/1.1 404 Not Found\r\n\r\n";
                    }
                }
                else if (method == "POST")
                {
                    // FileStream createdFile= File.Create(filePath);
                    // byte[] bufferNew = Encoding.UTF8.GetBytes(parts[parts.Length - 1]);
                    // await createdFile.WriteAsync(bufferNew, 0, bufferNew.Length);
                    File.WriteAllText(filePath, parts[parts.Length - 1]);
                    message = "HTTP/1.1 201 Created\r\n\r\n";
                }
            }
            else
            {
                message = "HTTP/1.1 404 Not Found\r\n\r\n";
            }
            var responseBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
    
}
