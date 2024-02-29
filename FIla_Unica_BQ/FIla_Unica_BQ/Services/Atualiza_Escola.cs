using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fila_Unica_BQ.Services;
using Firebase.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using Xamarin.Android.Net;
using HtmlAgilityPack;
using Org.Json;
using Fila_Unica_BQ.Resources;
using Fila_Unica_BQ.Models;
using System.Net;
using System.IO;

namespace Fila_Unica_BQ.Services
{

    public class Atualiza_Escola
    {
        private readonly string url = "https://sge.brusque.sc.gov.br/sge/pub/index.php?i=122";

        string texto = "";
        string html = "";
        string line = "";

        readonly FirebaseService fbService = new FirebaseService();
        readonly FirebaseClient firebase = new FirebaseClient("https://fila-unica-brusque-default-rtdb.firebaseio.com/");

        private static readonly List<string> list = new List<string>();
        readonly List<string> listaString = list;

        public void Busca_HTML()
        {
            listaString.Clear();

            using (var client = new WebClient())
            {
                var conteudo = client.DownloadString(url);

                html = conteudo;
            }

            using (StringReader reader = new StringReader(html))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != string.Empty)
                    {
                        if (line.Contains("input"))
                        {
                            texto = line.Substring(line.IndexOf('"') + 14);
                            listaString.Add(texto.Substring(0, texto.IndexOf('"')));
                        }
                    }
                }
            }

            foreach (string str in listaString)
            {
                
            }
        }
    }
}
