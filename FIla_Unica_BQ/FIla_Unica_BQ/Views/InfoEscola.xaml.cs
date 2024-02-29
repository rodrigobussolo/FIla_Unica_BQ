using Fila_Unica_BQ.Models;
using Fila_Unica_BQ.Resources;
using Fila_Unica_BQ.Services;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Android.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fila_Unica_BQ.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoEscola : ContentPage
    {
        readonly FirebaseService fbService = new FirebaseService();
        //readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        private static readonly List<string> list = new List<string>();
        readonly List<string> listaString = list;

        string EscolaEnd = "";
        string EscolaBairro = "";
        string EscolaCEP = "";
        string EscolaCNPJ = "";
        string EscolaFone = "";
        string EscolaEmail = "";
        string EscolaDiretor = "";
        string EscolaCoord = "";
        //string EscolaPresidente = "";

        public InfoEscola()
        {
            InitializeComponent();

            Info_Escola.Title = AppResources.AppName;

            lbl_Titulo.Text = "ESCOLA";
            lbl_SubTitulo.Text = "Informações de Contato";

            ToolbarItem item = new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("refresh.png"),
                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };
            item.Clicked += OnItemAtualiza_Lista;
            this.ToolbarItems.Add(item);
                
            Busca_EscolaAsync();
        }

        void OnItemAtualiza_Lista(object sender, EventArgs e)
        {
            AIndicator.IsRunning = true;
            Busca_EscolaAsync();
        }

        public async void Busca_EscolaAsync()
        {
            try
            {
                await Busca_HTML_Escola();

                var escola = await fbService.GetEscola(Dados_gerais.CodigoEscola);
                if (escola != null)
                {
                    lbl_Escola.Text = escola.EscolaCod.ToString() + " - " + escola.EscolaNome;
                    lbl_Endereco.Text = escola.EscolaEnd + " - " + escola.EscolaBairro + "\nCEP " + escola.EscolaCEP;
                    lbl_CNPJ.Text = escola.EscolaCNPJ;
                    lbl_Fone.Text = escola.EscolaFone;
                    lbl_Email.Text = escola.EscolaEmail;
                    lbl_Diretor.Text = escola.EscolaDiretor;
                    lbl_Coordenador.Text = escola.EscolaCoord;
                }
            }
            catch { }

            AIndicator.IsRunning = false;
        }

        public async Task Busca_HTML_Escola()
        {
            try
            {
                listaString.Clear();
                int nitem = 1;
                string item = "";
                string pageHtml = "";
                string line;
                string texto;
                string auxiliar;

                using (HttpClient httpClient = new HttpClient(new AndroidClientHandler()))
                {
                    pageHtml = await httpClient.GetStringAsync("https://sge.brusque.sc.gov.br/sge/pub/index.php?i=" + Dados_gerais.CodigoEscola.ToString());
                    pageHtml = pageHtml.Replace("P&#341;esidente:", "Presidente");
                    pageHtml = pageHtml.Replace("Endere&ccedil;o:", "Endereco");
                }

                using (StringReader reader = new StringReader(pageHtml))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != string.Empty)
                        {
                            if (line.Contains("Endereco")) { item = "<End>"; }
                            if (line.Contains("Bairro")) { item = "<Bai>"; }
                            if (line.Contains("CEP")) { item = "<CEP>"; }
                            if (line.Contains("CNPJ")) { item = "<CNP>"; }
                            if (line.Contains("Fone")) { item = "<Fon>"; }
                            if (line.Contains("Email")) { item = "<Ema>"; }
                            if (line.Contains("Diretor")) { item = "<Dir>"; }
                            if (line.Contains("Coordenador")) { item = "<Coo>"; }
                            if (line.Contains("Presidente")) { item = "<Pre>"; }
                            if (line.Contains("input"))
                            {
                                texto = line.Substring(line.IndexOf('"') + 14);
                                texto = texto.Substring(0, texto.IndexOf('"'));

                                if (nitem > 6)
                                {
                                    auxiliar = texto.Substring(0, 1);
                                    if (Regex.IsMatch(auxiliar, @"^[0-9]+$")) { texto = ""; }
                                }

                                if (texto != "")
                                {
                                    listaString.Add(item.Trim() + texto.Trim());
                                    item = "";
                                    auxiliar = "";
                                    nitem++;
                                }
                            }
                        }
                    }
                }


                int cont = 0;

                if (listaString.Count > 0)
                {
                    foreach (string str in listaString)
                    {
                        if (str.Length > 4)
                        {
                            if (str.Trim().Substring(0, 5) == "<End>") { EscolaEnd = str.Trim().Replace("<End>", ""); }
                            if (str.Trim().Substring(0, 5) == "<Bai>") { EscolaBairro = str.Trim().Replace("<Bai>", ""); }
                            if (str.Trim().Substring(0, 5) == "<CEP>") { EscolaCEP = str.Trim().Replace("<CEP>", ""); }
                            if (str.Trim().Substring(0, 5) == "<CNP>") { EscolaCNPJ = str.Trim().Replace("<CNP>", ""); }
                            if (str.Trim().Substring(0, 5) == "<Fon>") { EscolaFone = str.Trim().Replace("<Fon>", ""); }
                            if (str.Trim().Substring(0, 5) == "<Ema>") { EscolaEmail = str.Trim().Replace("<Ema>", ""); }
                            if (str.Trim().Substring(0, 5) == "<Dir>") { EscolaDiretor = str.Trim().Replace("<Dir>", ""); }
                            if (str.Trim().Substring(0, 5) == "<Coo>" && EscolaCoord == "") { EscolaCoord = str.Trim().Replace("<Coo>", ""); }
                        }


                        cont++;
                    }

                    var escola = await fbService.GetEscola(Dados_gerais.CodigoEscola);
                    if (escola != null)
                    {
                        await fbService.UpdateEscola(Dados_gerais.CodigoEscola,
                                        EscolaEnd,
                                        EscolaBairro,
                                        EscolaCEP,
                                        EscolaCNPJ,
                                        EscolaFone,
                                        EscolaEmail,
                                        EscolaDiretor,
                                        EscolaCoord,
                                        "");
                    }
                    else
                    {
                        await fbService.AddInfoEscola(Dados_gerais.CodigoEscola,
                                        EscolaEnd,
                                        EscolaBairro,
                                        EscolaCEP,
                                        EscolaCNPJ,
                                        EscolaFone,
                                        EscolaEmail,
                                        EscolaDiretor,
                                        EscolaCoord,
                                        "");
                    }
                }
                else { }
            }
            catch { }
        }
    }
}