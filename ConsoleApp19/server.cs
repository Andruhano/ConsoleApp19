using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class server
{
    static ConcurrentDictionary<string, double> exchangeRates = new ConcurrentDictionary<string, double>()
    {
        ["USD"] = 1.0,
        ["EUR"] = 0.92,
        ["UAH"] = 39.3,
        ["GBP"] = 0.78,
        ["JPY"] = 154.5
    };

    static async Task Main(string[] args)
    {
        int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client); 
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        string clientEndPoint = client.Client.RemoteEndPoint.ToString();
        Log($"Connected: {clientEndPoint} at {DateTime.Now}");

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"Received from {clientEndPoint}: {request}");

                string[] parts = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    await SendMessageAsync(stream, "Invalid format. Use: CURRENCY1 CURRENCY2");
                    continue;
                }

                string fromCurrency = parts[0].ToUpper();
                string toCurrency = parts[1].ToUpper();

                if (!exchangeRates.ContainsKey(fromCurrency) || !exchangeRates.ContainsKey(toCurrency))
                {
                    await SendMessageAsync(stream, "Unknown currency.");
                    continue;
                }

                double rate = exchangeRates[toCurrency] / exchangeRates[fromCurrency];
                string response = $"1 {fromCurrency} = {rate:F4} {toCurrency}";
                await SendMessageAsync(stream, response);

                Log($"Exchange rate requested: {request} => {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception with {clientEndPoint}: {ex.Message}");
        }
        finally
        {
            Log($"Disconnected: {clientEndPoint} at {DateTime.Now}");
            client.Close();
        }
    }

    static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        await stream.WriteAsync(data, 0, data.Length);
    }

    static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText("log.txt", $"{DateTime.Now}: {message}\n");
    }
}
