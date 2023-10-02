using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server;
using Clients;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;

namespace Kursach
{
    public partial class GlobalForm : Form
    {
        private Client client;
        public GlobalForm(Client client)
        {
            InitializeComponent();
            this.client = client;
        }
        private void buttonDelete(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                // Получение выбранного файла из ListView
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string fileName = selectedItem.Text;
                // Отправка запроса на удаление файла
                client.SendData($"3_{fileName}");
                // Получение ответа от сервера
                string responseMessage = client.ReceiveString();

                // Вывод сообщения об удалении файла
                MessageBox.Show(responseMessage);

                // Обновление списка файлов
                update_listview();
            }
            else
            {
                MessageBox.Show("Выберите файл для удаления.");
            }
        }




        public void buttonRename(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                // Получение выбранного файла из ListView
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string fileName = selectedItem.Text;

                // Запрос пользователю на ввод нового имени файла
                string newFileName = Interaction.InputBox("Введите новое имя файла:", "Переименование файла", fileName);

                if (!string.IsNullOrWhiteSpace(newFileName))
                {

                    client.SendData($"2_{selectedItem.Text}_{newFileName}");

                    // Получение ответа от сервера
                    string responseMessage = client.ReceiveString();

                    // Вывод сообщения о переименовании файла
                    MessageBox.Show(responseMessage);



                    // Обновление списка файлов
                    update_listview();
                }
                else
                {
                    MessageBox.Show("Введите новое имя файла.");
                }
            }
            else
            {
                MessageBox.Show("Выберите файл для переименования.");
            }
        }

        private void buttonCheck(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                // Получение выбранного файла из ListView
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string fileName = selectedItem.Text;


                // Формирование запроса для проверки целостности файла вместе с путем и именем файла
                string request = $"1_{fileName}";
                // Отправка запроса на проверку целостности файла серверу
                client.SendData($"{request}");

                // Получение ответа от сервера

                string responseMessage = client.ReceiveString();

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    MessageBox.Show(responseMessage);
                }
                else
                {
                    MessageBox.Show("Не удалось получить хеш-сумму файла от сервера.");
                }

             
            }
            else
            {
                MessageBox.Show("Выберите файл для проверки целостности.");
            }

            update_listview();
        }

        private void update_listview() 
        {
            client.Connect();
            listView1.Items.Clear();
            client.Send("files");
            string files = client.ReceiveString();
            foreach (string file in files.Split('\n'))
            {
                ListViewItem item = new ListViewItem(file.Split(' ')[0]);
                if (file.StartsWith("expectedHashes.json"))
                {
                    item.ForeColor = Color.RoyalBlue;
                }
                else if (file.Split(' ')[1] == "invalid")
                {
                    item.ForeColor = Color.Red;
                }
                else
                {
                    item.ForeColor = Color.Green;
                }
                listView1.Items.Add(item);
            }
        }





        private void buttonAddFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
            string filePath = openFileDialog1.FileName;
            var filename = Path.GetFileName(filePath);
            string in_file = File.ReadAllText(filePath);

            client.Send($"save/{filename}/{in_file}");
            string response = client.ReceiveString();
            if (response.StartsWith("ошибка "))
            {
                MessageBox.Show($"Произошла ошибка: {response}");
                return;
            }
            else
            {
                MessageBox.Show(response);
                update_listview();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            update_listview();
        }


    }
}
