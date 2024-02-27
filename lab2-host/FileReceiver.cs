using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab2_host;

public class FileReceiver(IConfiguration config, ILogger<FileReceiver> logger) : BackgroundService
{
	private const int BUFFER_SIZE = 1024 * 1024;
	private readonly byte[] CONNECT_BYTES = Encoding.UTF8.GetBytes("CONNECT");

	private volatile int clientsCount;
	private string savePath = string.Empty;
	private long maxFileSize;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		savePath = config.GetValue<string>("SavePath") ?? "out";
		maxFileSize = config.GetValue<long>("MaxFileSize");
		var maxClientsCount = config.GetValue<int>("MaxClientsCount");

		var port = config.GetValue<int>("Port");
		var ipPoint = new IPEndPoint(IPAddress.Any, port);

		using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Bind(ipPoint);
		socket.Listen();

		logger.LogInformation("File receiver service running on {endpoint}.", socket.LocalEndPoint);



		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				while (clientsCount >= maxClientsCount)
				{
					await Task.Delay(1000, stoppingToken);
				}

				var client = await socket.AcceptAsync(stoppingToken);
				await HandleClient(client, stoppingToken);
			}
			catch (OperationCanceledException)
			{
			}
			catch (SocketException se)
			{
				if (se.SocketErrorCode != SocketError.ConnectionReset)
					logger.LogError("Could not connect to client. {code}: {message}", se.SocketErrorCode, se.Message);
			}
			catch (Exception e)
			{
				logger.LogError("Error: {message}", e.Message);
			}
		}
	}



	public async Task HandleClient(Socket client, CancellationToken stoppingToken)
	{
		await client.SendAsync(CONNECT_BYTES); // handshake

		Interlocked.Increment(ref clientsCount);
		logger.LogInformation("Client connected: {}. Currently connected: {clientCount} client(s)", client.RemoteEndPoint, clientsCount);

		// get new thread from thread pool
		var worker = Task.Run(async () =>
		{
			await ReadData(client, stoppingToken);
		}, stoppingToken);

		// executes when worker completes his work
		_ = worker.ContinueWith(x =>
		{
			logger.LogInformation("Client disconnected: {}", client.RemoteEndPoint);

			client.Dispose();
			Interlocked.Decrement(ref clientsCount);
		}, CancellationToken.None);
	}

	public async Task ReadData(Socket client, CancellationToken stoppingToken)
	{
		var buffer = new byte[BUFFER_SIZE];

		while (!stoppingToken.IsCancellationRequested)
		{
			// read data length
			int bytesRead = await client.ReceiveAsync(buffer, stoppingToken);
			if (bytesRead == 0) break;

			long dataLength = BitConverter.ToInt64(buffer, 0);

			bool allowedToContinue = dataLength <= maxFileSize;
			await client.SendAsync(BitConverter.GetBytes(allowedToContinue));

			if (!allowedToContinue)
				continue;

			long totalBytesRead = 0;

			// read fileName
			bytesRead = await client.ReceiveAsync(buffer, stoppingToken);
			if (bytesRead == 0) break;

			string fileName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

			// read and save data
			string path = Path.Combine(savePath, fileName);
			using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);

			logger.LogInformation("Receiving file \"{fileName}\". Total {dataLength} bytes.", fileName, dataLength);

			do
			{
				bytesRead = await client.ReceiveAsync(buffer, stoppingToken);
				if (bytesRead == 0) break;

				totalBytesRead += bytesRead;
				fs.Write(buffer, 0, bytesRead);
			}
			while (totalBytesRead < dataLength);

			logger.LogInformation("File \"{fileName}\" was successfully received!", fileName);
		}
	}
}
