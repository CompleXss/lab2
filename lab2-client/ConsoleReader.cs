namespace Lab2_client;

public static class ConsoleReader
{
	public static (string host, int port) ReadAdressInputs()
	{
		string host;
		int port;

		do
		{
			Console.Write("Введите адрес сервера (ipv4:port): ");
			string? input = Console.ReadLine()?.Trim();

			if (string.IsNullOrEmpty(input))
				continue;

			int delimeterIndex = input.IndexOf(':');
			if (delimeterIndex == -1) continue;

			host = input[..delimeterIndex].Trim();
			if (host.Length < 7) continue;
			if (host.Count(x => x == '.') != 3) continue;

			if (int.TryParse(input[(delimeterIndex + 1)..], out port))
				break;
		}
		while (true);

		return (host, port);
	}

	public static string ReadFileNameInput()
	{
		string? fileName;

		do
		{
			Console.Write("Введите путь к файлу для передачи его на сервер: ");
			fileName = Console.ReadLine();
		}
		while (!File.Exists(fileName));

		return fileName;
	}
}
