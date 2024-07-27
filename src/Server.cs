using System.Net;
using System.Net.Sockets;
using System.Text;
// You can use print statements as follows for debugging, they'll be visible
// when running tests.
Console.WriteLine("Logs from your program will appear here!");
// Uncomment this block to pass the first stage
var server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client
var message = "HTTP/1.1 404 Not Found\r\n\r\n";
var buffer = new byte[1024];
var bytes = socket.Receive(buffer);
var request = Encoding.UTF8.GetString(buffer);
var parts = request.Split("\r\n");
var path = parts[0].Split(' ')[1];
if (path == "/")
  message = "HTTP/1.1 200 OK\r\n\r\n";
else if (path.Contains("/echo")) {
  var random = path.Split("/echo/")[1];
  message = $"HTTP/1.1 200 OK\r\n" + $"Content-Type: text/plain\r\n" +
            $"Content-Length: {random.Length}\r\n" + $"\r\n{random}";
} else if (path == "/user-agent") {
  var userAgent = parts[2].Split(' ')[1];
  message = $"HTTP/1.1 200 OK\r\n" + $"Content-Type: text/plain\r\n" +
            $"Content-Length: {userAgent.Length}\r\n\r\n" + $"{userAgent}";
}
socket.Send(Encoding.UTF8.GetBytes(message));