using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
// Start the server
var server = new TcpListener(IPAddress.Any, 4221);
server.Start();
using TcpClient client = await server.AcceptTcpClientAsync();
await using NetworkStream stream = client.GetStream();
// Read incoming data
byte[] requestBuffer = new byte[1024];
int bytesRead = await stream.ReadAsync(requestBuffer, 0, requestBuffer.Length);
string request = System.Text.Encoding.UTF8.GetString(requestBuffer, 0, bytesRead);
Console.WriteLine($"Received request: {request}");
var requestLines = request.Split("\r\n");
var requests = requestLines[0].Split("/");
for (int i = 0; i < requestLines.Length; i++)
{
    Console.WriteLine(requestLines[i]);
}
// Prepare and send response
string response = "";
if (requests[1] == " HTTP")
{
    response = "HTTP/1.1 200 OK\r\n\r\n";
}
else if (requests[1] == "user-agent HTTP")
{
    string str = requestLines[3].Split(": ")[1];
    int len = str.Length;
    response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: " + len + "\r\n\r\n" + str;
}
else
{
    response = "HTTP/1.1 404 Not Found\r\n\r\n";
}
byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(response);
stream.Write(responseBuffer, 0, responseBuffer.Length);