using CardioCheck.Models;

namespace CardioCheck;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }


    //  fun��o para o bot�o Sair
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool resposta = await DisplayAlert("Sair", "Voc� tem certeza que deseja fazer o logout?", "Sim", "N�o");

        if (resposta)
        {
            // Apaga as informa��es de sess�o salvas
            SessaoLogin.Token = null;

            // Volta para a p�gina de Login, reiniciando o aplicativo
            Application.Current.MainPage = new LoginPage();
        }
    }
}
