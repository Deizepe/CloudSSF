
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;



namespace Mestrado
{
    public partial class Principal : Form
    {

        Stopwatch stopwatch = new Stopwatch();

        string _dir = Properties.Settings.Default.diretorio;
        static CancellationTokenSource cts;

        int encontrou = 0;
        private string finalString = ""; // String inicial vazia
        List<string> acoesEncontradas = new List<string>();

        List<Privilegio> privilegiosEncontrados = new List<Privilegio>();
        List<Privilegio> privilegiosChecados = new List<Privilegio>();
        List<Smell> smellsEncontrados=new List<Smell>();
        List<Smell> smellsChecados = new List<Smell>();


        public Principal()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Principal_Load(object sender, EventArgs e)
        {
            txtKey.Text = Properties.Settings.Default.secretKeyIAM;
            txtUser.Text = Properties.Settings.Default.accessKeyIAM;
            txtPlatform.Text = Properties.Settings.Default.linguagem;
            txtPath.Text = Properties.Settings.Default.diretorio;
            txtTempoDecorrido.Text = "00:00:00";
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialogPath = new FolderBrowserDialog();
            folderBrowserDialogPath.ShowDialog();
            txtPath.Text = folderBrowserDialogPath.SelectedPath;
            _dir = folderBrowserDialogPath.SelectedPath;
            Properties.Settings.Default.diretorio = _dir;
            Properties.Settings.Default.Save();
        }

        private void btnChagePlatform_Click(object sender, EventArgs e)
        {
            FormLinguagem f = new FormLinguagem();
            f.ShowDialog();
            txtPlatform.Text = Properties.Settings.Default.linguagem;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormCredenciais f = new FormCredenciais();
            f.ShowDialog();
            txtKey.Text = Properties.Settings.Default.accessKeyIAM;
            txtUser.Text = Properties.Settings.Default.secretKeyIAM;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Visible = true;

            string directoryPath = _dir; // Especifique o caminho do diretório aqui
            //string searchString = "<?php"; // Especifique a string a ser pesquisada aqui

            pbProgresso.Minimum = 0;
            pbProgresso.Maximum = CountFilesInDirectory(_dir);
            pbProgresso.Step = 1;
            pbProgresso.Value = 0;
            //object ss = new { _dir, searchString };
            object[] myArray = new object[5]; // Cria um array de objetos com tamanho 5

            cts = new CancellationTokenSource();
            stopwatch.Start();

            //privilegios codesmell
            string jsonFilePath = "./libraryCodeSmell.json";
            if (!File.Exists(jsonFilePath))
            {
                MessageBox.Show("O arquivo JSON não foi encontrado.");
                return;
            }
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<Privilegio> privilegios = JsonConvert.DeserializeObject<List<Privilegio>>(jsonContent);

            //smells
            string jsonFilePathS = "./librarySmell.json";
            if (!File.Exists(jsonFilePathS))
            {
                MessageBox.Show("O arquivo JSON não foi encontrado.");
                return;
            }
            string jsonContentS = File.ReadAllText(jsonFilePathS);
            List<Smell> smells = JsonConvert.DeserializeObject<List<Smell>>(jsonContentS);

            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { SearchInDirectory(directoryPath, privilegios, cts); }), null);

            //smells


            //addTB("\nA pesquisa foi concluída.");
            Console.ReadLine();
            btnStart.Enabled = true;

        }

























        void SearchInDirectory(string directoryPath, List<Privilegio> privilegios, CancellationTokenSource cancellationToken)
        // void SearchInDirectory(object parameter)
        {
            // setTempo();
            if (cancellationToken.IsCancellationRequested)
            {
                // Realizar as ações necessárias para encerrar o processamento
                addTB("Processamento abortado pelo usuário.");
                return;
            }

            try
            {
                //object[] parameters = (object[])parameter;
                //string directoryPath = parameters[0].ToString();
                //string searchString = parameters[1].ToString();
                //CancellationTokenSource cancellationToken = (CancellationTokenSource)parameters[2];
                foreach (string file in Directory.GetFiles(directoryPath))
                {
                    setTempo();
                    // Verificar se a string existe no conteúdo do arquivo

                    addTB("\nFile: " + file);
                    foreach (var privilegio in privilegios)
                    {
                        string searchWord = privilegio.Method;

                        if (File.ReadAllText(file).Contains(searchWord))
                        {
                            addResultadoAcoes(privilegio);
                            addAcoesEncontradas(searchWord);
                            //setTempo();
                            addTB("\nString '" + searchWord + "' found in file. ");
                            addEncontrou();
                        }
                        else
                        {
                            //setTempo();
                            //addTB("\nNot Found " + file);
                        }
                    }
                    addPBProgresso();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        setTempo();
                        // Realizar as ações necessárias para encerrar o processamento
                        addTB("Processamento abortado pelo usuário.");
                        return;
                    }

                }

                foreach (string subDirectory in Directory.GetDirectories(directoryPath))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Realizar as ações necessárias para encerrar o processamento
                        //setTempo();
                        addTB("Processamento abortado pelo usuário.");
                        return;
                    }
                    SearchInDirectory(subDirectory, privilegios, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Realizar as ações necessárias para encerrar o processamento
                        //setTempo();
                        addTB("Processamento abortado pelo usuário.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

                addTB("\nOcorreu um erro: " + ex.Message);
                //setTempo();
            }
            //setTempo();
            
        }

        private void addResultadoAcoes(Privilegio privilegio)
        {
            privilegiosEncontrados.Add(privilegio);
        }

        void addAcoesEncontradas(string searchWord)
        {
            if (!acoesEncontradas.Contains(searchWord))
            {
                acoesEncontradas.Add(searchWord);
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
                if (pbProgresso.Value == pbProgresso.Maximum)
                {
                    stopwatch.Stop();
                    setTempo();
                    stopwatch.Reset();
                    CheckAWSAsync();
                    //MessageBox.Show("Encerrado");
                    Console.WriteLine(acoesEncontradas);
                    checkPrivilegiosCloud();
                    printSaida();
                }
            }));
        }

        void setTempo()
        {
            txtTempoDecorrido.Invoke(new Action(() =>
            {
                TimeSpan ts = stopwatch.Elapsed;
                txtTempoDecorrido.Text = ts.ToString();
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

        private void button5_Click(object sender, EventArgs e)
        {
            pbProgresso.Value = 0;
            cts.Cancel();
            stopwatch.Reset();
            tbBusca.Text = "Processamento abortado pelo usuário.";
            encontrou = 0;
        }



        AmazonIdentityManagementServiceClient iamClient = new AmazonIdentityManagementServiceClient(Properties.Settings.Default.accessKeyIAM, Properties.Settings.Default.secretKeyIAM);


        private async void CheckAWSAsync()
        {
            //AKIATL4MDI4DCIEUIV4O
            //X8X0ERLNdz0729TMWbTOwZ7xHrYJp350ivAIKf2i

            string secretKey = Properties.Settings.Default.accessKeyIAM;
            string accessKey = Properties.Settings.Default.secretKeyIAM;

            //string accessKeyId = Properties.Settings.Default.accessKeyIAM;
            //string secretAccessKey = Properties.Settings.Default.secretKeyIAM;
            string userName = "mestrado";
            //string region = "us-east-1"; // Defina a região apropriada para o serviço SES


            //var iamClient = new AmazonIdentityManagementServiceClient(accessKeyId, secretAccessKey);

            try
            {
                GetUserResponse userResponse = await iamClient.GetUserAsync(new GetUserRequest { UserName = userName });

                if (userResponse.User != null)
                {
                    string userId = userResponse.User.UserId;

                    ListUserPoliciesResponse policiesResponse = await iamClient.ListUserPoliciesAsync(new ListUserPoliciesRequest { UserName = userName });
                    foreach (string policyName in policiesResponse.PolicyNames)
                    {
                        tbBusca.AppendText($"Política: {policyName}");
                    }

                    ListAttachedUserPoliciesResponse attachedPoliciesResponse = await iamClient.ListAttachedUserPoliciesAsync(new ListAttachedUserPoliciesRequest { UserName = userName });
                    foreach (var policy in attachedPoliciesResponse.AttachedPolicies)
                    {

                        //var getUserPolicyRequest = new ListAttachedUserPoliciesRequest
                        //{
                        //    UserName = "mestrado",
                        //    PolicyName = "ttt"
                        //};

                        //var g = iamClient.ListAttachedUserPolicies(getUserPolicyRequest);

                        var getUserPolicyRequest = new GetUserPolicyRequest
                        {
                            UserName = "mestrado",
                            PolicyName = "ttt"
                        };

                        var getUserPolicyResponse = iamClient.GetUserPolicy(getUserPolicyRequest);

                        //Aqui você pode analisar a política para encontrar as ações permitidas
                        //tbResult.AppendText($"Política Anexada: {policy.PolicyName }");
                       // tbResult.AppendText($"Política Anexada: {getUserPolicyResponse.PolicyDocument }");



                       // tbResult.AppendText($"Política Anexada: {policy.PolicyName}");
                       // tbResult.AppendText($"Política Anexada: {policy.PolicyArn }");
                       // tbResult.AppendText($"Política Anexada: {policy }");







                        //var policyDocument = JObject.Parse(getUserPolicyResponse.PolicyDocument);
                        //var statementArray = policyDocument["Statement"] as JArray;

                        //if (statementArray != null)
                        //{
                        //    // Collect allowed actions
                        //    var allowedActions = new List<string>();
                        //    foreach (var statement in statementArray)
                        //    {
                        //        var actionArray = statement["Action"] as JArray;
                        //        if (actionArray != null)
                        //        {
                        //            foreach (var action in actionArray)
                        //            {
                        //                allowedActions.Add(action.ToString());
                        //            }
                        //        }
                        //    }

                        //    // Print the allowed actions for the policy
                        //    listBoxPermissoes.Items.Add($"Policy: {policy.PolicyName}");
                        //    foreach (var action in allowedActions)
                        //    {
                        //        listBoxPermissoes.Items.Add($"- {action}");
                        //    }

                        //}







                    }
                    ListRolesResponse listRol = await iamClient.ListRolesAsync(new ListRolesRequest { });

                    foreach (var policy in listRol.Roles)
                    {
                       // tbResult.AppendText($"Política Anexada: {policy.PermissionsBoundary}");
                    }
                }
                else
                {
                   // tbResult.AppendText($"O usuário '{userName}' não foi encontrado.");
                }
            }
            catch (AmazonIdentityManagementServiceException ex)
            {
                tbResult.AppendText($"Erro: {ex.Message}");
            }

        }

























        private void button5_Click_1(object sender, EventArgs e)
        {
            //smells
            string jsonFilePath = "./librarySmell.json";
            if (!File.Exists(jsonFilePath))
            {
                MessageBox.Show("O arquivo JSON LibrarySmell não foi encontrado.");
                return;
            }
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<Smell> s = JsonConvert.DeserializeObject<List<Smell>>(jsonContent);


            //smells

            // Action you want to check (e.g., "s3:GetObject")
            var actionToCheck = "s3:PutObject";

            // Get the user's policies
            var listUserPoliciesRequest = new ListUserPoliciesRequest
            {
                UserName = "mestrado"
            };

            var listUserPoliciesResponse = iamClient.ListUserPolicies(listUserPoliciesRequest);

            // Iterate through the policies and check if the action is allowed
            foreach (var policyName in listUserPoliciesResponse.PolicyNames)
            {
                var getUserPolicyRequest = new GetUserPolicyRequest
                {
                    UserName = "mestrado",
                    PolicyName = policyName
                };

                var getUserPolicyResponse = iamClient.GetUserPolicy(getUserPolicyRequest);

                var x = HttpUtility.UrlDecode(getUserPolicyResponse.PolicyDocument).ToUpper();

                //privilege
                if (x.Contains(actionToCheck.ToUpper()))
                {

                    tbBusca.AppendText($"Action '{actionToCheck}' is allowed for the user mestrado in policy '{policyName}'.");
                    return; // Found the action, no need to continue checking

                }

                //smell
                foreach (Smell sItem in s)
                {
                    if (x.Contains(sItem.Smells.ToUpper()))
                    {
                        tbResult.AppendText(sItem.Smells);
                        tbBusca.AppendText(Environment.NewLine);

                    }
                }
            }

            // The action was not found in any policy
            tbResult.AppendText($"Action '{actionToCheck}' is not allowed for the user mestrado.");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tbResult.AppendText("CODE SMELLS");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("==== ======\n");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("- Privileges in code");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);

            string output = String.Format("{0,-12} | {1,-5} | {2,-15} | {3,-12} | {4,-12} | {5,-50}", "Method", "ARN", "Language", "Cloud", "Status", "Description");
            tbResult.AppendText(output);
                   output = String.Format("{0,-12} | {1,-5} | {2,-15} | {3,-12} | {4,-12} | {5,-50}", "------", "---", "--------", "-----", "------", "-----------");
            tbResult.AppendText(output);

            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("SECURITY SMELLS");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("======== ======");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("-Privileges in CLoud");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);

            output = String.Format("{0,-12} | {1,-5} | {2,-12} | {3,-12} | {4,-50}", "Smell", "ARN", "Cloud", "Status", "Description");
            tbResult.AppendText(output);
            tbResult.AppendText(Environment.NewLine);
            output = String.Format("{0,-12} | {1,-5} | {2,-12} | {3,-12} | {4,-50}", "-----", "---", "-----", "------", "-----------");
            tbResult.AppendText(output);
            tbResult.AppendText(Environment.NewLine);


        }

        void checkPrivilegiosCloud()
        {
            //smells
            string jsonFilePath = "./librarySmell.json";
            if (!File.Exists(jsonFilePath))
            {
                MessageBox.Show("O arquivo JSON LibrarySmell não foi encontrado.");
                return;
            }
            string jsonContent = File.ReadAllText(jsonFilePath);
            List<Smell> s = JsonConvert.DeserializeObject<List<Smell>>(jsonContent);


            //smells

            // Action you want to check (e.g., "s3:GetObject")
           // var actionToCheck = "s3:PutObject";

            // Get the user's policies
            var listUserPoliciesRequest = new ListUserPoliciesRequest
            {
                UserName = Properties.Settings.Default.usuarioIAM
            };

            var listUserPoliciesResponse = iamClient.ListUserPolicies(listUserPoliciesRequest);

            // Iterate through the policies and check if the action is allowed
            foreach (var policyName in listUserPoliciesResponse.PolicyNames)
            {
                var getUserPolicyRequest = new GetUserPolicyRequest
                {
                    UserName = Properties.Settings.Default.usuarioIAM,
                    PolicyName = policyName
                };

                var getUserPolicyResponse = iamClient.GetUserPolicy(getUserPolicyRequest);

                var x = HttpUtility.UrlDecode(getUserPolicyResponse.PolicyDocument).ToUpper();

                //privilege
                foreach (Privilegio actionToCheck in privilegiosEncontrados)
                {
                    if (x.Contains(actionToCheck.Method.ToUpper()) && actionToCheck.Language==Properties.Settings.Default.linguagem)
                    {
                        actionToCheck.Status = "OK";
                        if (!privilegiosChecados.Contains(actionToCheck))
                        {
                            privilegiosChecados.Add(actionToCheck);
                        }
                        
                        tbBusca.AppendText($"Action '{actionToCheck}' is allowed for the user in policy.");
                        //tbBusca.AppendText($"Action '{actionToCheck}' is allowed for the user in policy '{policyName}'.");
                        // Found the action, no need to continue checking

                    }
                    else if(actionToCheck.Language == Properties.Settings.Default.linguagem)
                    {
                        actionToCheck.Status = "Absent";
                        if (!privilegiosChecados.Contains(actionToCheck))
                        {
                            privilegiosChecados.Add(actionToCheck);
                        }
                    }
                }
                //smell
                foreach (Smell sItem in s)
                {
                    if (x.Contains(sItem.Smells.ToUpper()))
                    {
                        sItem.Status = "Over Privileges";
                        tbBusca.AppendText(sItem.Smells);
                        tbBusca.AppendText(Environment.NewLine);
                        smellsChecados.Add(sItem);

                    }
                }
            }

            // The action was not found in any policy
            //tbBusca.AppendText($"Action '{actionToCheck}' is not allowed for the user.");
        }

        void printSaida()
        {

            tbResult.AppendText("CODE SMELLS");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("==== ======\n");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("- Privileges in code");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);

            string output = String.Format("{0,-12} | {1,-5} | {2,-15} | {3,-12} | {4,-12} | {5,-50}", "Method", "ARN", "Language", "Cloud", "Status", "Description");
            tbResult.AppendText(output);
            output = String.Format("{0,-12} | {1,-5} | {2,-15} | {3,-12} | {4,-12} | {5,-50}", "------", "---", "--------", "-----", "------", "-----------");
            tbResult.AppendText(output);
            tbResult.AppendText(Environment.NewLine);

            foreach (var item in privilegiosChecados)
            {
                output = String.Format("{0,-12} | {1,-5} | {2,-15} | {3,-12} | {4,-12} | {5,-50}", item.Method, item.ARN, item.Language, item.Cloud, item.Status, item.Description);
                tbResult.AppendText(output);
                tbResult.AppendText(Environment.NewLine);

            }

            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("SECURITY SMELLS");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("======== ======");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText("-Privileges in CLoud");
            tbResult.AppendText(Environment.NewLine);
            tbResult.AppendText(Environment.NewLine);

            output = String.Format("{0,-32} | {1,-4} | {2,-5} | {3,-15} | {4,-12}", "Smell", "ARN", "Cloud", "Status", "Description");
            tbResult.AppendText(output);
            tbResult.AppendText(Environment.NewLine);
            output = String.Format("{0,-32} | {1,-4} | {2,-5} | {3,-15} | {4,-12}", "-----", "---", "-----", "------", "-----------");
            tbResult.AppendText(output);
            tbResult.AppendText(Environment.NewLine);


            foreach (var item in smellsChecados)
            {
                output = String.Format("{0,-32} | {1,-4} | {2,-5} | {3,-15} | {4,-12}", item.Smells, item.ARN,  item.Cloud, item.Status, item.Desc);
                tbResult.AppendText(output);
                tbResult.AppendText(Environment.NewLine);

            }

            tbResult.AppendText(Environment.NewLine);


        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Set the file filter and default extension
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";

                // Show the SaveFileDialog
                DialogResult result = saveFileDialog.ShowDialog();

                // If the user clicked the "Save" button in the dialog
                if (result == DialogResult.OK)
                {
                    // Get the selected file path from the dialog
                    string filePath = saveFileDialog.FileName;

                    // Save the content of the TextBox to the selected file path
                    SaveTextBoxContentToFile(tbBusca.Text, filePath);

                    //MessageBox.Show("Content saved to: " + filePath);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Set the file filter and default extension
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";

                // Show the SaveFileDialog
                DialogResult result = saveFileDialog.ShowDialog();

                // If the user clicked the "Save" button in the dialog
                if (result == DialogResult.OK)
                {
                    // Get the selected file path from the dialog
                    string filePath = saveFileDialog.FileName;

                    // Save the content of the TextBox to the selected file path
                    SaveTextBoxContentToFile(tbResult.Text, filePath);

                    //MessageBox.Show("Content saved to: " + filePath);
                }
            }
        }
        private void SaveTextBoxContentToFile(string content, string filePath)
        {
            try
            {
                // Write the content to the text file
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }
    }


}

