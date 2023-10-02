using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 12345;

            var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);



            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(5);
            Console.WriteLine($"Server is listening on {ip}:{port}");

            while (true)
            {
                var listener = tcpSocket.Accept();
                Console.WriteLine($"New client connected.");

                _ = Task.Run(() => HandleClient(listener)); // Запуск обработки клиента в отдельном потоке
            }
        }

        private static void HandleClient(Socket client)
        {
            while (true)
            {
                string request = ReceiveData(client);
                if (request.StartsWith("1"))
                {
                    string[] parts = request.Split(new char[] { '_' });
                    string path = "C:/kyrsach";
                    string fileName = parts[1];

                    // Формирование пути к файлу
                    string filePath = Path.Combine(path, fileName);

                    // Проверка наличия файла с указанным именем в папке
                    if (File.Exists(filePath))
                    {
                        // Вычисление хеш-суммы файла
                        string fileHash = Class1.GetFileHash(new FileInfo(filePath));
                        if(Class1.IsHashEquals(fileName, fileHash))
                        {
                            SendData(client, "Файл проверен, файл валидный");
                        } else
                        {
                            SendData(client, "Файл не валидный");
                        }
                        // Отправка хеш-суммы файла клиенту
                    }
                    else
                    {
                        // Отправка сообщения о том, что файла с указанным именем нет в папке
                        string message = $"Файл {fileName} не найден в папке {path}";
                        SendData(client, message);

                    }
                }
                if (request.StartsWith("2"))
                {
                    string[] parts = request.Split('_');
                    string path = "C:/kyrsach";
                    string oldName = parts[1];
                    string newName = parts[2];


                    // Получение списка файлов в указанной папке
                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] files = directory.GetFiles();

                    // Формирование строки со списком файлов и их статусом
                    string filesInfo = "";
                    foreach (FileInfo file in files)
                    {
                        filesInfo += $"{file.Name}\n";
                    }


                    // Получение расширения старого имени файла
                    string extension = Path.GetExtension(oldName);

                    // Получение только имени файла без расширения
                    string fileName = Path.GetFileNameWithoutExtension(oldName);

                    // Проверка наличия файла с указанным старым именем в папке
                    string oldFilePath = Path.Combine(path, oldName);
                    if (File.Exists(oldFilePath))
                    {
                        // Генерация нового пути и имени файла
                        string newFilePath = Path.Combine(path, $"{newName}{extension}");

                        // Попытка переименования файла
                        try
                        {
                            File.Move(oldFilePath, newFilePath);

                            // Отправка сообщения о успешном переименовании файла
                            string message = $"Файл {oldName} успешно переименован в {newName}{extension}";
                            SendData(client, message);
                        }
                        catch (Exception ex)
                        {
                            // Отправка сообщения об ошибке переименования файла
                            string message = $"Ошибка при переименовании файла {oldName}: {ex.Message}";
                            SendData(client, message);
                        }
                    }
                    else
                    {
                        // Отправка сообщения о том, что файла с указанным именем нет в папке
                        string message = $"Файл {oldName} не найден в папке {path}";
                        SendData(client, message);
                    }
                }

                if (request.StartsWith("3"))
                {
                    string[] parts = request.Split('_');
                    string path = "C:/kyrsach";
                    string fileName = parts[1];

                    // Формирование пути к файлу
                    string filePath = Path.Combine(path, fileName);

                    // Проверка наличия файла с указанным именем в папке
                    if (File.Exists(filePath))
                    {
                        // Попытка удаления файла
                        try
                        {
                            File.Delete(filePath);

                            // Отправка сообщения о успешном удалении файла
                            string message = $"Файл {fileName} успешно удалён";
                            SendData (client, message);
                        }
                        catch (Exception ex)
                        {
                            // Отправка сообщения об ошибке удаления файла
                            string message = $"Ошибка при удалении файла {fileName}: {ex.Message}";
                            SendData(client, message);
                        }
                    }
                    else
                    {
                        // Отправка сообщения о том, что файла с указанным именем нет в папке
                        string message = $"Файл {fileName} не найден в папке {path}";
                        SendData(client, message);
                    }
                    Console.WriteLine("was deleted");
                }


                else if (request == "4")
                {
                    SendData(client, "Выход из программы");
                    break;
                }
                else if (request == "files")
                {
                    string directoryPath = "C:/kyrsach";
                    string[] files = Directory.GetFiles(directoryPath);
                    string[] fileNames = files.Select(file => Path.GetFileName(file)).ToArray();

                    // Получение ожидаемых хешей файлов
                    Dictionary<string, string> expectedHashes = Class1.GenerateExpectedHashes(directoryPath);

                    // Вычисление хешей и добавление их к именам файлов
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string fileName = fileNames[i];
                        string filePath = Path.Combine(directoryPath, fileName);
                        FileInfo file = new FileInfo(filePath);

                        // Проверка наличия файла в ожидаемых хешах
                        if (expectedHashes.ContainsKey(fileName))
                        {
                            string expectedHash = expectedHashes[fileName];

                            // Проверка валидности файла
                            bool isValid = Class1.IsFileValid(file, expectedHash);
                            if (isValid)
                            {
                                // Файл валиден - добавление хеша к имени файла
                                string fileHash = Class1.GetFileHash(file);
                                fileNames[i] = $"{fileName} {fileHash}";
                            }
                            else
                            {
                                // Файл невалиден - помечаем его как недействительный
                                fileNames[i] = $"{fileName} invalid";
                            }
                        }
                        else
                        {
                            // Файл не найден в ожидаемых хешах - помечаем его как недействительный
                            fileNames[i] = $"{fileName} invalid";
                        }
                    }

                    string fileList = string.Join("\n", fileNames);
                    Console.WriteLine(fileList);
                    SendData(client, fileList);


                } else if (request.StartsWith("save")) {
                    var req = request.Split('/');
                    var filename = req[1];
                    var in_file = req[2];

                    string filePath = Path.Combine("C:/kyrsach", filename);
                    if (File.Exists(filePath))
                    {
                        using (var sha256 = SHA256.Create())
                        { 
                            string fileHash = Class1.GetFileHash(new FileInfo(filePath));
                            var hash = sha256.ComputeHash(UnicodeEncoding.UTF8.GetBytes(fileHash));
                            var hashBuilder = new StringBuilder(hash.Length * 2);
                            foreach (var b in hash)
                            {
                                hashBuilder.Append(b.ToString("x2"));
                            }
                            if (Class1.IsHashEquals(filename, hashBuilder.ToString()))
                            {
                                SendData(client, "Файл валидный");
                            }
                            else
                            {
                                SendData(client, "Файл валидный");
                            }
                        }
                    }
                    else
                    {
                        File.Create(filePath).Dispose();
                        File.WriteAllText(filePath, in_file);
                        SendData(client, "Файл создан");
                    }
                }
            }
        }

        private static string ReceiveData(Socket client)
        {
            var buffer = new byte[256];
            var size = 0;
            var data = new StringBuilder();
            do
            {
                size = client.Receive(buffer);
                data.Append(Encoding.UTF8.GetString(buffer, 0, size));
            } while (client.Available > 0);
            return data.ToString();
        }

        private static void SendData(Socket client, string data)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            client.Send(buffer);
        }
    }
}