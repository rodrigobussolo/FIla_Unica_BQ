using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fila_Unica_BQ.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        public Home()
        {
            InitializeComponent();
        }

        private async void Click_Bercario1(object sender, EventArgs e)
        {
            Models.OrigemInscricao.Origem = "B1";
            await Navigation.PushAsync(new Content_main());
        }

        private async void Click_Bercario2(object sender, EventArgs e)
        {
            Models.OrigemInscricao.Origem = "B2";
            await Navigation.PushAsync(new Content_main());
        }

        private async void Click_Infantil1(object sender, EventArgs e)
        {
            Models.OrigemInscricao.Origem = "I1";
            await Navigation.PushAsync(new Content_main());
        }

        private async void Click_Infantil2(object sender, EventArgs e)
        {
            Models.OrigemInscricao.Origem = "I2";
            await Navigation.PushAsync(new Content_main());
        }
    }
}