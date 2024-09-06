using Android.Content;
using Android.Icu.Util;
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
using static Android.Provider.UserDictionary;
using static System.Net.Mime.MediaTypeNames;

namespace Fila_Unica_BQ.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Content_main : ContentPage
    {
        private readonly string Origem = (Models.OrigemInscricao.Origem).Trim();

        private static readonly List<string> list = new List<string>();
        readonly List<string> listaString = list;

        private static readonly List<string> listB = new List<string>();
        readonly List<string> listaStringB = listB;

        readonly FirebaseService fbService = new FirebaseService();
        readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        private int protocolo = 0;
        private string datahora = "";
        private int posicao = 0;
        private string escolha_1 = "";
        private string escolha_2 = "";
        private string escolha_3 = "";
        private string escolha_4 = "";
        private string chamadas = "";
        private int contador = 0;
        private int contador_lista = 0;

        private readonly string codURL = (Models.OrigemInscricao.Origem).Trim();
        private readonly string url = "https://filaunica.brusque.sc.gov.br/dmd/pub/filapub.php?f=" + Models.OrigemInscricao.Origem;

        public Content_main()
        {
            InitializeComponent();

            Pagina_Conteudo.Title = AppResources.AppName;

            txt_Protocolo.Text = "";
            lbl_Chamadas.IsVisible = false;

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
            listaString.Clear();
            AIndicator.IsRunning = true;
            Info_Status("Analisando lista de candidatos... Aguarde!");
            try
            {
                var GetItems = (await firebase.Child("Candidato" + codURL)
                   .OnceAsync<Candidato>()).Select(item => new Candidato
                   {
                       Posicao = item.Object.Posicao,
                       ProtocoloId = item.Object.ProtocoloId,
                       Data_Hora = item.Object.Data_Hora,
                       Opcao1 = item.Object.Opcao1,
                       Opcao2 = item.Object.Opcao2,
                       Opcao3 = item.Object.Opcao3,
                       Opcao4 = item.Object.Opcao4,
                       Chamadas = item.Object.Chamadas
                   });
                contador_lista = 0;
                foreach (var item in GetItems)
                {
                    string linha = item.Posicao.ToString() + ";" + item.ProtocoloId.ToString() + ";" + item.Data_Hora + ";" + item.Opcao1 + ";" + item.Opcao2 + ";" + item.Opcao3 + ";" + item.Opcao4 + ";" + item.Chamadas;
                    listaString.Add(linha);
                    contador_lista += 1;
                }

                Info_Status("Total de candidatos: " + contador_lista.ToString());
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

                    var Data = (await firebase.Child("DtaAtualizacao" + codURL)
                       .OnceAsync<Atualizacao>()).Select(item => new Atualizacao
                       {
                           Ultima_atualizacao = item.Object.Ultima_atualizacao,
                       });

                    if (Data != null)
                    {
                        foreach (var item in Data)
                        {
                            DataAtualizacao = item.Ultima_atualizacao;
                            lbl_DataAtualizaco.Text = "Última atualização em:" + DataAtualizacao.ToString("dd/MM/yyyy");
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

            if (contador_lista > 0) { Info_Status("Total de candidatos: " + contador_lista.ToString()); }
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
                    Texto = Texto.Replace("Protocolo:", ";");
                    Texto = Texto.Replace("	 ", " ");
                    Texto = Texto.Replace("	", " ");
                    Texto = Texto.Replace("        ", "");
                    Texto = Texto.Replace("  ", "");
                    Texto = Texto.Replace("     ", "");
                    Texto = Texto.Replace("1&ordf;", "");
                    Texto = Texto.Replace("2&ordf;", "");
                    Texto = Texto.Replace("3&ordf;", "");
                    Texto = Texto.Replace("4&ordf;", "");
                    Texto = Texto.Replace("1S ", "");
                    Texto = Texto.Replace("2S ", "");
                    Texto = Texto.Replace("3S ", "");
                    Texto = Texto.Replace("4S ", "");
                    Texto = Texto.Replace("\r\n", "");

                    listaString.Add(Texto);
                }

                listaStringB.Clear();
                doc.LoadHtml(pageHtml);


                Texto = "";
                int n = 0;
                foreach (var dnode in doc.DocumentNode.SelectNodes("//table/tbody/tr/td"))
                {
                    Texto += dnode.InnerText;

                    Texto = Texto.Replace("Ordem:", "");
                    Texto = Texto.Replace("op&ccedil;ao:", ";");
                    Texto = Texto.Replace("Inscri&ccedil;ao: ", ";");
                    Texto = Texto.Replace("&nbsp;", "");
                    Texto = Texto.Replace("[X]", "");
                    Texto = Texto.Replace("Protocolo:", ";");
                    Texto = Texto.Replace("	 ", " ");
                    Texto = Texto.Replace("	", " ");
                    Texto = Texto.Replace("        ", "");
                    Texto = Texto.Replace("  ", "");
                    Texto = Texto.Replace("     ", "");
                    Texto = Texto.Replace("1&ordf;", "");
                    Texto = Texto.Replace("2&ordf;", "");
                    Texto = Texto.Replace("3&ordf;", "");
                    Texto = Texto.Replace("4&ordf;", "");
                    Texto = Texto.Replace("1S ", "");
                    Texto = Texto.Replace("2S ", "");
                    Texto = Texto.Replace("3S ", "");
                    Texto = Texto.Replace("4S ", "");
                    Texto = Texto.Replace("\r\n", "");

                    Texto += ";";
                    n++;

                    if (n == 6)
                    {
                        listaStringB.Add(Texto.Trim());
                        n = 0;
                        Texto = "";
                    }
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

                if (candidato != null && candidato.Chamadas == null)
                {
                    /*
                     * Testa existência do campo Chamadas. Este campo está presente a partir da versão 17.
                     * Se acaso estiver nulo (versão anteriores não geram esta informação), recria a lista.
                     */
                    await fbService.DeletaTudo();
                    await Busca_htmlAsync();
                    await ListaCandidatos();

                    candidato = await fbService.GetCandidato(CandidatoId);
                }

                if (candidato != null)
                {
                    lbl_Protocolo.Text = candidato.ProtocoloId.ToString();
                    lbl_Posicao.Text = candidato.Posicao.ToString();
                    lbl_datahora.Text = candidato.Data_Hora;
                    lbl_Opc1.Text = candidato.Opcao1.Trim();
                    lbl_Opc2.Text = candidato.Opcao2.Trim();
                    lbl_Opc3.Text = candidato.Opcao3.Trim();
                    lbl_Opc4.Text = candidato.Opcao4.Trim();

                    string Texto = "";
                    if (candidato.Chamadas == null)
                        Texto = "";
                    else { Texto = candidato.Chamadas.Trim(); }

                    if (Texto != "")
                    {
                        if (Texto.Length <= 3)
                        {
                            lbl_Chamadas.Text = "ATENÇÃO: Identificadas " + Texto + " tentativas de contato para este protocolo!";
                        }
                        else { lbl_Chamadas.Text = "ATENÇÃO: Identificadas tentativas de contato para este protocolo!"; }

                        lbl_Chamadas.IsVisible = true;
                    }
                    else { lbl_Chamadas.Text = ""; lbl_Chamadas.IsVisible = false; }

                    posicao = candidato.Posicao;

                    Analisa_Posicao(candidato.Opcao1.Trim());
                    lbl_Opc1.Text += "\r\nTotal de Candidatos: " + contador.ToString();

                    Analisa_Posicao(candidato.Opcao2.Trim());
                    lbl_Opc2.Text += "\r\nTotal de Candidatos: " + contador.ToString();

                    Analisa_Posicao(candidato.Opcao3.Trim());
                    lbl_Opc3.Text += "\r\nTotal de Candidatos: " + contador.ToString();

                    Analisa_Posicao(candidato.Opcao4.Trim());
                    lbl_Opc4.Text += "\r\nTotal de Candidatos: " + contador.ToString();


                    // A rotina abaixo busca o total de inscrições para cada opção, na escola

                    /*
                    var escolas = await fbService.GetEscolaNome(candidato.Opcao1.Trim());
                    if (escolas != null)
                    {
                        lbl_Opc1.Text += "\r\n" + escolas.Opcoes;
                    }

                    escolas = await fbService.GetEscolaNome(candidato.Opcao2.Trim());
                    if (escolas != null)
                    {
                        lbl_Opc2.Text += "\r\n" + escolas.Opcoes;
                    }

                    escolas = await fbService.GetEscolaNome(candidato.Opcao3.Trim());
                    if (escolas != null)
                    {
                        lbl_Opc3.Text += "\r\n" + escolas.Opcoes;
                    }
                    */

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
            lbl_Opc4.Text = null;
        }

        private async void GravaDados(string texto)
        {
            string phrase = texto;
            string[] words = phrase.Split(';');
            int item = 1;

            foreach (var word in words)
            {
                if (item == 1) { posicao = int.Parse(word); }
                if (item == 2) { protocolo = int.Parse(word); }
                if (item == 3) { datahora = word.ToString().Trim(); }
                if (item == 4) { escolha_1 = word.ToString().Trim(); }
                if (item == 5) { escolha_2 = word.ToString().Trim(); }
                if (item == 6) { escolha_3 = word.ToString().Trim(); }

                if (item == 7) {
                    if (word.ToString().Trim() == "")
                    {
                        escolha_4 = "NENHUM";
                    }
                    else 
                    { 
                        escolha_4 = word.ToString().Trim(); 
                    }
                }

                item += 1;
            }
            
            chamadas = "";
            int nProtocolo;

            foreach (string itemB in listaStringB)
            {
                words = itemB.Split(';');
                nProtocolo  = int.Parse(words[2]);  
                if (nProtocolo == protocolo)
                {
                    chamadas = words[5].Trim();
                    break;
                };
            }

            await fbService.AddCandidato(posicao, protocolo, datahora, escolha_1, escolha_2, escolha_3, escolha_4, chamadas);
        }

        private void Analisa_Posicao(string opcao)
        {
            /*
                A pedido dos responsáveis pelo programa Fila Única, da prefeitura, foi ajustado para não exibir a posição do candidato sobre cada instituição
            */
            int posicoes = 0;
            int cont = 0;
            foreach (string itens in listaString)
            {
                posicoes += 1;

                string phrase = itens;

                string[] words = phrase.Split(';');

                int item = 1;

                foreach (var word in words)
                {
                    if (word.ToString().Trim() == opcao)
                    {
                        cont += 1;
                        break;
                    }

                    item += 1;
                }

                /*
                if (posicoes < posicao)
                {
                    string phrase = itens;

                    string[] words = phrase.Split(';');

                    int item = 1;

                    foreach (var word in words)
                    {
                        if (word.ToString().Trim() != lbl_Protocolo.Text.Trim())
                        {
                            if (word.ToString().Trim() == opcao) //análise geral - considera todos os candidatos com a mesma opção independente da ordem das opções
                            {
                                contador += 1;
                            }

                            item += 1;
                        }
                    }
                }
                */
            }
            contador = cont;
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