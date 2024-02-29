using Android.Content;
using Android.Widget;
using Fila_Unica_BQ.Models;
using Fila_Unica_BQ.Resources;
using Fila_Unica_BQ.Services;
using Firebase.Database;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Android.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fila_Unica_BQ.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Content_main : ContentPage
    {
        private readonly string Origem = (Models.OrigemInscricao.Origem).Trim();

        private static readonly List<string> list = new List<string>();
        readonly List<string> listaString = list;

        readonly FirebaseService fbService = new FirebaseService();
        readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        private int protocolo = 0;
        private string datahora = "";
        private int posicao = 0;
        private string escolha_1 = "";
        private string escolha_2 = "";
        private string escolha_3 = "";
        private int contador = 0;
        private int contador_lista = 0;

        private readonly string codURL = (Models.OrigemInscricao.Origem).Trim();
        private readonly string url = "https://filaunica.brusque.sc.gov.br/dmd/pub/filapub.php?f=" + Models.OrigemInscricao.Origem;

        public Content_main()
        {
            InitializeComponent();

            Pagina_Conteudo.Title = AppResources.AppName;

            txt_Protocolo.Text = "";

            switch (Origem)
            {
                case "B1":
                    lbl_Pagina.Text = "BERÇÁRIO 1";
                    break;

                case "B2":
                    lbl_Pagina.Text = "BERÇÁRIO 2";
                    break;
                case "I1":
                    lbl_Pagina.Text = "INFANTIL 1";
                    break;
                case "I2":
                    lbl_Pagina.Text = "INFANTIL 2";
                    break;
            }

            lbl_SubTitulo.Text = "Detalhes sobre o Candidato";


            Listar();

            Atualizar();
        }

        public async void Listar() { await ListaCandidatos(); }

        private async Task ListaCandidatos()
        {
            AIndicator.IsRunning = true;
            Info_Status("Analisando lista de candidatos... Aguarde!");
            try
            {
                var GetItems = (await firebase.Child("Candidatos" + codURL)
                   .OnceAsync<Models.Candidato>()).Select(item => new Models.Candidato
                   {
                       Posicao = item.Object.Posicao,
                       ProtocoloId = item.Object.ProtocoloId,
                       Data_Hora = item.Object.Data_Hora,
                       Opcao1 = item.Object.Opcao1,
                       Opcao2 = item.Object.Opcao2,
                       Opcao3 = item.Object.Opcao3
                   });
                contador_lista = 0;
                foreach (var item in GetItems)
                {
                    string linha = item.Posicao.ToString() + ";" + item.ProtocoloId.ToString() + ";" + item.Data_Hora + ";" + item.Opcao1 + ";" + item.Opcao2 + ";" + item.Opcao3;
                    listaString.Add(linha);
                    contador_lista += 1;
                }

                Info_Status("Total de candidatos cadastrados: " + contador_lista.ToString());
            }
            catch { Info_Status("Erro desconhecido ao coletar lista de candidatos!"); }
            AIndicator.IsRunning = false;
        }

        private async void Atualizar()
        {
            string testeHtml = null;
            try
            {
                using (HttpClient httpClient = new HttpClient(new AndroidClientHandler()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    testeHtml = await httpClient.GetStringAsync(url);
                }
            }
            catch { };

            try
            {
                if (testeHtml != null)
                {
                    AIndicator.IsRunning = true;
                    Info_Status("Verificando última atualização... Aguarde!");

                    DateTime DataAtualizacao = Convert.ToDateTime("01/01/0001");
                    DateTime Data_aux = DateTime.Now.Date;

                    var Data = (await firebase.Child("DataAtualizacao" + codURL)
                       .OnceAsync<Models.Atualizacao>()).Select(item => new Models.Atualizacao
                       {
                           Ultima_atualizacao = item.Object.Ultima_atualizacao,
                       });

                    if (Data != null)
                    {
                        foreach (var item in Data)
                        {
                            DataAtualizacao = item.Ultima_atualizacao;
                            lbl_DataAtualizaco.Text = "Última atualização da lista geral em:" + DataAtualizacao.ToString("dd/MM/yyyy");
                        }
                    }

                    if (Data_aux > DataAtualizacao)
                    {
                        await fbService.DeletaTudo();
                        await Busca_htmlAsync();
                        await ListaCandidatos();
                    }
                    AIndicator.IsRunning = false;
                }
            }
            catch { Info_Status("Sem comunicação com a base de dados!"); }

            if (contador_lista > 0) { Info_Status("Total de candidatos cadastrados: " + contador_lista.ToString()); }
        }

        public async void Atualiza_Lista()
        {
            await Busca_htmlAsync();
        }

        async void OnItemAtualiza_Lista(object sender, EventArgs e)
        {
            Limpa_Textos();
            await fbService.DeletaTudo();
            await Busca_htmlAsync();
            await ListaCandidatos();
        }

        private async Task Busca_htmlAsync()
        {
            Info_Status("Atualizando lista de candidatos... Aguarde!");
            try
            {
                AIndicator.IsRunning = true;
                string pageHtml = "";

                using (HttpClient httpClient = new HttpClient(new AndroidClientHandler()))
                {
                    pageHtml = await httpClient.GetStringAsync(url);
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);

                string Texto = "";
                var node = doc.DocumentNode.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("pop"));

                listaString.Clear();
                foreach (var testes in node)
                {
                    Texto = testes.InnerText.Trim();

                    Texto = Texto.Replace("Ordem:", "");
                    Texto = Texto.Replace("op&ccedil;ao:", ";");
                    Texto = Texto.Replace("Inscri&ccedil;ao: ", ";");
                    Texto = Texto.Replace("&nbsp;", "");
                    Texto = Texto.Replace("[X]", "");
                    Texto = Texto.Replace(" Protocolo: ", ";");
                    Texto = Texto.Replace("	 ", " ");
                    Texto = Texto.Replace("	", " ");
                    Texto = Texto.Replace("        ", "");
                    Texto = Texto.Replace("  ", "");
                    Texto = Texto.Replace("     ", "");
                    Texto = Texto.Replace(" 1&ordf;", "");
                    Texto = Texto.Replace(" 2&ordf;", "");
                    Texto = Texto.Replace(" 3&ordf;", "");
                    Texto = Texto.Replace("1S ", "");
                    Texto = Texto.Replace("2S ", "");
                    Texto = Texto.Replace("3S ", "");
                    Texto = Texto.Replace("\r\n", "");

                    listaString.Add(Texto);
                }

                if (listaString.Count > 0)
                {
                    foreach (string item in listaString)
                    {
                        GravaDados(item);
                    }

                    await fbService.Atualiza_Data();
                }
            }
            catch
            {
                Mensagens("Falha na coleta dos dados. Verifique sua conexão!", 1, 1);
            }
            Info_Status("");
        }

        private async void OnBuscarClick(object sender, EventArgs e)
        {
            AIndicator.IsRunning = true;
            Limpa_Textos();

            if (txt_Protocolo.Text != "")
            {
                try
                {
                    if (contador_lista == 0)
                    {
                        await ListaCandidatos();
                    }

                    BuscaCandidato(Int32.Parse(txt_Protocolo.Text));
                }
                catch
                {

                }
            }
            else
            {
                Mensagens("Informe o número do Protocolo!", 3, 1);
            };
        }

        private async void BuscaCandidato(int CandidatoId)
        {
            try
            {
                var candidato = await fbService.GetCandidato(CandidatoId);
                if (candidato != null)
                {
                    lbl_Protocolo.Text = candidato.ProtocoloId.ToString();
                    lbl_Posicao.Text = candidato.Posicao.ToString();
                    lbl_datahora.Text = candidato.Data_Hora;
                    lbl_Opc1.Text = candidato.Opcao1.Trim();
                    lbl_Opc2.Text = candidato.Opcao2.Trim();
                    lbl_Opc3.Text = candidato.Opcao3.Trim();

                    posicao = candidato.Posicao;

                    contador = 0;
                    Analisa_Posicao(candidato.Opcao1.Trim());
                    lbl_Opc1.Text += "\r\n" + contador.ToString() + " candidatos na frente";

                    contador = 0;
                    Analisa_Posicao(candidato.Opcao2.Trim());

                    lbl_Opc2.Text += "\r\n" + contador.ToString() + " candidatos na frente";

                    contador = 0;
                    Analisa_Posicao(candidato.Opcao3.Trim());
                    lbl_Opc3.Text += "\r\n" + contador.ToString() + " candidatos na frente";
                }
                else
                {
                    Mensagens("Nenhum registro para o protocolo informado!", 3, 0);
                }
            }
            catch { Mensagens("Falha na validação dos dados!", 1, 1); }
            AIndicator.IsRunning = false;
        }

        public void Limpa_Textos()
        {
            lbl_Posicao.Text = null;
            lbl_Protocolo.Text = null;
            lbl_datahora.Text = null;
            lbl_Opc1.Text = null;
            lbl_Opc2.Text = null;
            lbl_Opc3.Text = null;
        }

        private async void GravaDados(string texto)
        {
            string phrase = texto;
            string[] words = phrase.Split(';');
            int item = 1;

            foreach (var word in words)
            {
                if (item == 1) { posicao = Int32.Parse(word); }
                if (item == 2) { protocolo = Int32.Parse(word); }
                if (item == 3) { datahora = word.ToString().Trim(); }
                if (item == 4) { escolha_1 = word.ToString().Trim(); }
                if (item == 5) { escolha_2 = word.ToString().Trim(); }
                if (item == 6) { escolha_3 = word.ToString().Trim(); }

                item += 1;
            }
            await fbService.AddCandidato(posicao, protocolo, datahora, escolha_1, escolha_2, escolha_3);
        }

        private void Analisa_Posicao(string opcao)
        {
            int posicoes = 0;
            foreach (string itens in listaString)
            {
                posicoes += 1;
                if (posicoes < posicao)
                {
                    string phrase = itens;

                    string[] words = phrase.Split(';');

                    int item = 1;

                    foreach (var word in words)
                    {
                        if (word.ToString().Trim() != "14980")
                        {
                            //if (item == 4 && word.ToString().Trim() == opcao) >>> análise considerando a mesma ordem da opção
                            if (word.ToString().Trim() == opcao) // análise geral - considera todos os candidatos com a mesma opção independente da ordem das opções
                            {
                                contador += 1;
                            }

                            item += 1;
                        }
                    }
                }
            }
        }

        public async void Mensagens(string msg, int tipo_titulo, int tipo_msg)
        {
            string titulo = "";

            switch (tipo_titulo)
            {
                case 1:
                    titulo = "Erro: ";
                    break;
                case 2:
                    titulo = "Alerta: ";
                    break;
                case 3:
                    titulo = "Aviso: ";
                    break;
            }

            switch (tipo_msg)
            {
                case 1:
                    await DisplayAlert(titulo, msg, "OK");
                    break;
                default:
                    Android.Widget.Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Long).Show();
                    break;
            }
        }

        public void Info_Status(string txtStatus)
        {
            lbl_status.Text = txtStatus;
        }
    }
}