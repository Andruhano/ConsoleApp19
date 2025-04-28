using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string serverAddress = "127.0.0.1";
        int port = 5000;

        using TcpClient client = new TcpClient();
        await client.ConnectAsync(serverAddress, port);
        Console.WriteLine("Connected to server!");

        NetworkStream stream = client.GetStream();

        while (true)
        {
            Console.Write("Enter two currencies (e.g., USD EUR) or 'exit': ");
            string input = Console.ReadLine();

            if (input?.ToLower() == "exit")
            {
                break;
            }

            byte[] data = Encoding.UTF8.GetBytes(input + "\n");
            await stream.WriteAsync(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response.Trim()}");
        }

        Console.WriteLine("Disconnected from server.");
    }
}
