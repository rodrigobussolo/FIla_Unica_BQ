using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Fila_Unica_BQ.Views;
using Fila_Unica_BQ.Resources;

namespace Fila_Unica_BQ
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();
        public ICommand HelpCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();

            BindingContext = this;
        }

        void RegisterRoutes()
        {
            Routes.Add("activity_main", typeof(activity_main));
            Routes.Add("Content_main", typeof(Content_main));
            Routes.Add("Home", typeof(Home));
            Routes.Add("Pagina", typeof(Pagina));
            //Routes.Add("Teste", typeof(MainShell));

            foreach (var item in Routes)
            {
                Routing.RegisterRoute(item.Key, item.Value);
            }
        }

        private async void Click_Sobre(object sender, EventArgs e)
        {
            Limpar();

            Models.Dados_gerais.Titulo = "SOBRE";
            Models.Dados_gerais.SubTitulo = "Informativo";
            Models.Dados_gerais.Texto = AppResources.Sobre;
            Models.Dados_gerais.Origem = 4;
            Current.FlyoutIsPresented = false;
            await Navigation.PushAsync(new Pagina());
        }

        private async void Click_Doador(object sender, EventArgs e)
        {
            Limpar();

            Models.Dados_gerais.Titulo = "INCENTIVO";
            Models.Dados_gerais.SubTitulo = "Este app é gratuito, e sempre será";
            Models.Dados_gerais.Texto = AppResources.Incentivo;
            Models.Dados_gerais.Origem = 2;
            Current.FlyoutIsPresented = false;
            await Navigation.PushAsync(new Pagina());
        }

        private async void Click_Docs(object sender, EventArgs e)
        {
            Limpar();

            Models.Dados_gerais.Titulo = "INSCRIÇÃO";
            Models.Dados_gerais.SubTitulo = "Documentação Necessária";
            Models.Dados_gerais.Texto = AppResources.Documentos;
            Models.Dados_gerais.Origem = 3;
            Current.FlyoutIsPresented = false;
            await Navigation.PushAsync(new Pagina());
        }

        private async void Click_UEscolar(object sender, EventArgs e)
        {
            Limpar();

            Models.Dados_gerais.Titulo = "UNIDADES ESCOLARES";
            Models.Dados_gerais.SubTitulo = "Total de Alunos por Opção";
            Models.Dados_gerais.Origem = 1;
            Current.FlyoutIsPresented = false;
            await Navigation.PushAsync(new Pagina());
        }

        private void Limpar()
        {
            Models.Dados_gerais.Titulo = "";
            Models.Dados_gerais.Texto = "";
            //Models.Dados_gerais.list.Clear();
        }
    }
}