using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    public class Class1
    {

        public static bool IsFileValid(FileInfo file, string expectedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = file.OpenRead())
                {
                    var hash = sha256.ComputeHash(stream);
                    var hashBuilder = new StringBuilder(hash.Length * 2);
                    foreach (var b in hash)
                    {
                        hashBuilder.Append(b.ToString("x2"));
                    }

                    // Сравниваем вычисленную хеш-сумму с ожидаемой
                    return hashBuilder.ToString().Equals(expectedHash);
                }
            }
        }


        public static bool IsHashEquals(string filename, string hash)
        {
            var expectedHashes = new Dictionary<string, string>();

            // Проверяем наличие файла expectedHashes.json
            var jsonFile = Path.Combine("C:/kyrsach/expectedHashes.json");
            if (File.Exists(jsonFile))
            {
                // Читаем хеши из файла
                var json = File.ReadAllText(jsonFile);
                expectedHashes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                // Проверяем соответствие вычисленного хеша ожидаемому хешу
                if (expectedHashes.TryGetValue(filename, out var expectedHash) && expectedHash == hash)
                {
                    // Хеш совпадает - добавляем его в список ожидаемых хешей
                    expectedHashes[filename] = hash;
                    return true;
                }
                else
                {
                    // Хеш не совпадает или ожидаемый хеш отсутствует - помечаем файл как недействительный
                    expectedHashes[filename] = "invalid";
                    return false;
                }
            }
            else return false;

        }

        public static string GetFileHash(FileInfo file)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = file.OpenRead())
                {
                    var hash = sha256.ComputeHash(stream);
                    var hashBuilder = new StringBuilder(hash.Length * 2);
                    foreach (var b in hash)
                    {
                        hashBuilder.Append(b.ToString("x2"));
                    }
                    return hashBuilder.ToString();
                }
            }
        }

        public static Dictionary<string, string> GenerateExpectedHashes(string path)
        {
            var expectedHashes = new Dictionary<string, string>();

            // Проверяем наличие файла expectedHashes.json
            var jsonFile = Path.Combine("C:/kyrsach/expectedHashes.json");
            if (File.Exists(jsonFile))
            {
                // Читаем хеши из файла
                var json = File.ReadAllText(jsonFile);
                expectedHashes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            else
            {
                // Создаем новый файл expectedHashes.json и записываем в него хеши
                var directory = new DirectoryInfo(path);
                var files = directory.GetFiles();

                foreach (var file in files)
                {
                    // Получаем имя файла и его путь
                    string fileName = file.Name;
                    string filePath = file.FullName;

                    // Вычисляем хеш файла
                    string fileHash = Class1.GetFileHash(file);

                    // Проверяем соответствие вычисленного хеша ожидаемому хешу
                    if (expectedHashes.TryGetValue(fileName, out var expectedHash) && expectedHash == fileHash)
                    {
                        // Хеш совпадает - добавляем его в список ожидаемых хешей
                        expectedHashes[fileName] = fileHash;
                    }
                    else
                    {
                        // Хеш не совпадает или ожидаемый хеш отсутствует - помечаем файл как недействительный
                        expectedHashes[fileName] = fileHash;
                    }
                }

                var json = JsonSerializer.Serialize(expectedHashes);
                File.WriteAllText(jsonFile, json);
            }

            return expectedHashes;
        }

    }
}
