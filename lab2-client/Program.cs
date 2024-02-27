using Lab2_client;
using System.Net.Sockets;
using System.Text;

var (host, port) = ConsoleReader.ReadAdressInputs();
Console.WriteLine($"\nПытаюсь подключиться, используя:\n\tадрес:\t{host}\n\tпорт:\t{port}\n");

//var host = "127.0.0.1";
//var port = 8888;



try
{
	using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	await socket.ConnectAsync(host, port);

	Console.WriteLine("Запрашиваю разрешение на подключение...");

	var handshakeBuffer = new byte[8];
	await socket.ReceiveAsync(handshakeBuffer);



	Console.WriteLine("Подключение установлено!");
	Console.WriteLine($"Адрес подключения {socket.RemoteEndPoint}");
	Console.WriteLine();

	while (true)
	{
		string fileNameToRead = ConsoleReader.ReadFileNameInput();
		using var fs = new FileStream(fileNameToRead, FileMode.Open, FileAccess.Read);

		if (fs.Length == 0)
		{
			Console.WriteLine("Файл пустой!");
			continue;
		}

		// send header
		await socket.SendAsync(BitConverter.GetBytes((long)fs.Length));

		int read = await socket.ReceiveAsync(handshakeBuffer);
		if (read == 0 || !BitConverter.ToBoolean(handshakeBuffer))
		{
			Console.WriteLine("Сервер отказался принимать такой большой файл!\n");
			continue;
		}

		string fileName = Path.GetFileName(fileNameToRead);
		await socket.SendAsync(Encoding.UTF8.GetBytes(fileName));

		byte[] buffer = new byte[1024 * 1024];
		long totalBytesSent = 0;
		int bytesRead = 0;

		// send data
		while ((bytesRead = await fs.ReadAsync(buffer)) != 0)
		{
			await socket.SendAsync(buffer.AsMemory(0, bytesRead));
			totalBytesSent += bytesRead;
			Console.WriteLine($"Отправлено {totalBytesSent} байт из {fs.Length}");
		}

		Console.WriteLine("Передача файла завершена!\n");
	}
}
catch (Exception e)
{
	Console.WriteLine("Ошибка: " + e.Message);
}
