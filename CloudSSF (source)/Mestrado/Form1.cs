using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Mestrado
{
    public partial class Form1 : Form
    {
        static CancellationTokenSource cts;

        string _dir ="";
        int encontrou = 0;
        private string finalString = ""; // String inicial vazia

        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            label1.Text
                = folderBrowserDialog1.SelectedPath;
            _dir = folderBrowserDialog1.SelectedPath;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string directoryPath = "C:\\Caminho\\Para\\Diretorio"; // Especifique o caminho do diretório aqui
            string directoryPath = _dir; // Especifique o caminho do diretório aqui
            string searchString = "<?php"; // Especifique a string a ser pesquisada aqui

            pbProgresso.Minimum = 0;
            pbProgresso.Maximum = CountFilesInDirectory(_dir);
            pbProgresso.Step = 1;
            pbProgresso.Value = 0;
            object ss = new{ _dir, searchString };
            object[] myArray = new object[5]; // Cria um array de objetos com tamanho 5

            cts = new CancellationTokenSource();

            // ThreadPool.QueueUserWorkItem(SearchInDirectory, ss);
            ThreadPool.QueueUserWorkItem(
  new WaitCallback(delegate (object state)
  { SearchInDirectory(directoryPath, searchString, cts); }), null);

            ;

            addTB("\nA pesquisa foi concluída.");
            Console.ReadLine();
        }

         void SearchInDirectory(string directoryPath, string searchString, CancellationTokenSource cancellationToken)
        // void SearchInDirectory(object parameter)
        {
            try
            {
                //object[] parameters = (object[])parameter;
                //string directoryPath = parameters[0].ToString();
                //string searchString = parameters[1].ToString();
                //CancellationTokenSource cancellationToken = (CancellationTokenSource)parameters[2];
                foreach (string file in Directory.GetFiles(directoryPath))
                {
                    // Verificar se a string existe no conteúdo do arquivo
                    if (File.ReadAllText(file).Contains(searchString))
                    {
                        addTB("\nString encontrada no arquivo: " + file);
                        addEncontrou();
                    }
                    else {
                        addTB("\nNao encontrado " + file);
                    }
                    addPBProgresso();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Realizar as ações necessárias para encerrar o processamento
                        addTB("Processamento abortado pelo usuário.");
                        return;
                    }

                }

                foreach (string subDirectory in Directory.GetDirectories(directoryPath))
                {

                    SearchInDirectory(subDirectory, searchString, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                
                addTB("\nOcorreu um erro: " + ex.Message);
            }
        }
        void addTB(string inputT)
        {
            string input = inputT.Trim();

            if (!string.IsNullOrEmpty(input))
            {
                //finalString += input; // Concatena o novo texto com a string existente
                //tbBusca.AppendText(input); // Exibe o novo texto na caixa de texto
                //tbBusca.AppendText(Environment.NewLine); // Adiciona uma nova linha

                tbBusca.Invoke(new Action(() =>
                {
                    tbBusca.AppendText(input);
                    tbBusca.AppendText(Environment.NewLine);
                }));

            }
        }
        void addPBProgresso()
        {
            pbProgresso.Invoke(new Action(() =>
            {
                pbProgresso.Value++;
            }));
        }
        void addEncontrou()
        {
            pbProgresso.Invoke(new Action(() =>
            {
                encontrou++;
                lblEncontrou.Text = encontrou.ToString();   
            }));
        }

        static int CountFilesInDirectory(string directoryPath)
        {
            int count = 0;

            try
            {
                count += Directory.GetFiles(directoryPath).Length;

                foreach (string subDirectory in Directory.GetDirectories(directoryPath))
                {
                    count += CountFilesInDirectory(subDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }

            return count;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }
    }
}
