using Fila_Unica_BQ.Models;
using Fila_Unica_BQ.Resources;
using Fila_Unica_BQ.Services;
using Firebase.Database;
using HtmlAgilityPack;
using Plugin.Clipboard;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fila_Unica_BQ.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pagina : ContentPage
    {
        private readonly string url = "https://filaunica.brusque.sc.gov.br/dmd/pub/qfilapub.php";

        readonly FirebaseService fbService = new FirebaseService();
        readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        int codigo = 0;
        string descricao = "";
        int opc_1 = 0;
        int opc_2 = 0;
        int opc_3 = 0;
        //string msg = "";
        bool atualizar = false;
        DateTime DataAtualizacao = Convert.ToDateTime("01/01/0001");

        public Pagina()
        {
            InitializeComponent();

            Pagina_Conteudo.Title = AppResources.AppName;

            lbl_Titulo.Text = Dados_gerais.Titulo;
            lbl_SubTitulo.Text = Dados_gerais.SubTitulo;

            int valor = Dados_gerais.Origem;

            switch (valor)
            {
                case 0: //Outras Páginas
                    Layout_Lista.IsVisible = false;
                    listaEscola.IsVisible = false;
                    lbl_DataAtualizaco.IsVisible = false;
                    btn_UE.IsVisible = false;

                    lbl_Texto.Text = Dados_gerais.Texto;

                    AIndicator.IsRunning = false;

                    Layout_Doador.IsVisible = false;
                    break;
                case 1: //Página Unidades Escolares

                    OrigemInscricao.Origem = "ListaEscolas";

                    scr_Texto.IsVisible = false;
                    lbl_Texto.IsVisible = false;
                    scr_Extra.IsVisible = false;

                    AIndicator.IsRunning = false;

                    //atualizar = false;
                    Valida_Data();
                    if (atualizar == true)
                    {
                        //Atualizar_Escola();
                    }

                    Layout_Doador.IsVisible = false;

                    Listar();
                    break;
                case 2: //Página doações
                    Layout_Lista.IsVisible = false;
                    listaEscola.IsVisible = false;
                    lbl_DataAtualizaco.IsVisible = false;
                    Layout_Doador.IsVisible = true;
                    scr_Extra.IsVisible = false;

                    lbl_Texto.Text = Dados_gerais.Texto;
                    AIndicator.IsRunning = false;
                    lbl_Pix.Text = "rodrigobussolo@gmail.com";

                    break;
                case 3: //Página Documentação para Inscrição
                    Layout_Lista.IsVisible = false;
                    listaEscola.IsVisible = false;
                    lbl_DataAtualizaco.IsVisible = false;
                    scr_Extra.IsVisible = true;

                    lbl_Texto.Text = Dados_gerais.Texto;
                    AIndicator.IsRunning = false;

                    Layout_Doador.IsVisible = false;

                    break;
            }
            

            //if (Dados_gerais.Origem == 1)
            //{
            //    OrigemInscricao.Origem = "ListaEscolas";

            //    scr_Texto.IsVisible = false;
            //    lbl_Texto.IsVisible = false;

            //    ToolbarItem item = new ToolbarItem
            //    {
            //        IconImageSource = ImageSource.FromFile("refresh.png"),
            //        Order = ToolbarItemOrder.Primary,
            //        Priority = 0
            //    };
            //    item.Clicked += OnItemAtualiza_ListaAsync;
            //    this.ToolbarItems.Add(item);

            //    //atualizar = false;
            //    Valida_Data();
            //    if (atualizar == true)
            //    {
            //        //Atualizar_Escola();
            //    }

            //    Listar();
            //}
            //else
            //{
            //    Layout_Lista.IsVisible = false;
            //    listaEscola.IsVisible = false;
            //    lbl_DataAtualizaco.IsVisible = false;

            //    lbl_Texto.Text = Dados_gerais.Texto;

            //    AIndicator.IsRunning = false;
            //}

            //if (Dados_gerais.Origem != 2)
            //{
            //    Copiar.IsVisible = false;
            //}
            //else { lbl_Pix.Text = "rodrigobussolo@gmail.com"; }
        }

        async void OnItemAtualiza_ListaAsync(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Pergunta?", "Deseja executar atualização da lista de escolas agora? A última atualização foi em " + DataAtualizacao.ToString("dd/MM/yyyy") + ".", "Sim", "Não");
            if (answer == true)
            {

                AIndicator.IsRunning = true;
                Atualizar_Escola();
                //Listar();
            }
        }

        private async void Click_Copiar(object sender, EventArgs e)
        {
            CrossClipboard.Current.SetText("rodrigobussolo@gmail.com");

            await DisplayAlert("Atenção", "A chave PIX \""
                               + lbl_Pix.Text
                               + "\" foi copiada.", "OK");
        }

        private async void Click_UEscolar(object sender, EventArgs e)
        {
            Limpar();

            Dados_gerais.Titulo = "UNIDADES ESCOLARES";
            Dados_gerais.SubTitulo = "Total de Alunos por Opção";
            Dados_gerais.Origem = 1;
            //Current.FlyoutIsPresented = false;
            await Navigation.PushAsync(new Pagina());
        }

        private void Limpar()
        {
            Dados_gerais.Titulo = "";
            Dados_gerais.Texto = "";
        }

        public async void Valida_Data()
        {
            try
            {
                var Data = (await firebase.Child("DataAtualizacaoListaEscolas")
                   .OnceAsync<Models.Atualizacao>()).Select(item => new Models.Atualizacao
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

                if (DateTime.Now.Date > DataAtualizacao)
                {
                    atualizar = true;
                }
                else { }
            }
            catch { }
        }

        private async void Atualizar_Escola()
        {
            try
            {
                var httpClient = new HttpClient();

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    await fbService.DeletaEscolas();
                    var s = await response.Content.ReadAsStringAsync();

                    var apenasDigitos = new Regex(@"[^\d]");
                    string texto;

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(s);

                    var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//table/tbody/tr/td");

                    int position = 0;
                    int linha = 1;
                    foreach (var node in htmlNodes)
                    {
                        if (linha == 1)
                        {
                            texto = node.InnerText.Substring(0, 3).Trim();

                            String.Join("", Regex.Split(texto, @"[^\d]"));

                            position = node.InnerText.IndexOf("-");
                            //codigo = Int32.Parse(node.InnerText.Substring(0, 3).Trim());
                            codigo = Int32.Parse(String.Join("", Regex.Split(texto, @"[^\d]")));
                            descricao = node.InnerText.Substring(position + 1).Trim();
                        }
                        else
                        {
                            if (linha == 2)
                            {
                                if (node.InnerText.Length > 0) { opc_1 = Int32.Parse(node.InnerText); }
                            }

                            if (linha == 3)
                            {
                                if (node.InnerText.Length > 0) { opc_2 = Int32.Parse(node.InnerText); }
                            }

                            if (linha == 4)
                            {
                                if (node.InnerText.Length > 0) { opc_3 = Int32.Parse(node.InnerText); }

                                await fbService.AddEscolaCandidato(codigo, descricao, opc_1, opc_2, opc_3);

                                opc_1 = 0;
                                opc_2 = 0;
                                opc_3 = 0;
                                linha = 0;
                            }
                        }

                        linha++;
                    }
                    await fbService.Atualiza_Data();

                    await ListaEscolas();
                }

            }
            catch { }
        }

        public async void Listar() { await ListaEscolas(); }

        private async Task ListaEscolas()
        {
            try
            {
                var escolas = await fbService.GetEscolaCandidatos();

                listaEscola.ItemsSource = escolas.OrderBy(a => a.EscolaNome);
            }
            catch { }
            AIndicator.IsRunning = false;
        }

        public void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var contato = e.SelectedItem as EscolaCandidato;
            DisplayAlert("Item Selecionado (SelectedItem) ", contato.EscolaCod.ToString(), "Ok");
        }
        public void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var escola = e.Item as EscolaCandidato;

            codigo = escola.EscolaCod;
            _ = Busca_EscolaAsync();
        }

        public async Task Busca_EscolaAsync()
        {
            try
            {
                Dados_gerais.CodigoEscola = codigo;
                await Navigation.PushAsync(new InfoEscola());
            }
            catch { }
        }
    }
}