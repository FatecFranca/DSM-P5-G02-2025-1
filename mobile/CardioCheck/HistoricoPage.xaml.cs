using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using CardioCheck.Models;
using CardioCheck.Model;


namespace CardioCheck;

public partial class HistoricoPage : ContentPage
{
    public ObservableCollection<Avaliacao> Historico { get; set; }

    public HistoricoPage()
    {
        InitializeComponent();
        Historico = new ObservableCollection<Avaliacao>();

        // Define o 'BindingContext' para que o XAML possa encontrar a propriedade 'Historico'
        // e se ligar a ela. Isso permite que a CollectionView se atualize automaticamente.
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Limpa a lista sempre que a p�gina aparecer para n�o duplicar itens.
        Historico.Clear();
        await LoadHistorico();
    }

    private async Task LoadHistorico()
    {
        LoadingIndicator.IsRunning = true;
        HistoricoCollectionView.IsVisible = false; // Esconde a lista enquanto carrega

        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessaoLogin.Token);
            var url = $"{SessaoLogin.UrlApi}/historico";

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // PONTO CR�TICO: A API envia JSON com letras min�sculas (camelCase)
                // e o nosso modelo C# usa mai�sculas (PascalCase).
                // Esta op��o faz com que o desserializador ignore essa diferen�a.
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var historicoList = JsonSerializer.Deserialize<List<Avaliacao>>(content, options);

                if (historicoList != null)
                {
                    // Ordena os resultados do mais novo para o mais antigo.
                    var historicoOrdenado = historicoList.OrderByDescending(item => item.Data);
                    foreach (var item in historicoOrdenado)
                    {
                        Historico.Add(item);
                    }
                }
            }
            else
            {
                await DisplayAlert("Erro de API", "N�o foi poss�vel carregar o hist�rico. O servidor respondeu com um erro.", "OK");
            }
        }
        catch (JsonException jsonEx)
        {
            // Este erro acontece se o JSON da API n�o for compat�vel com o modelo 'Avaliacao.cs'.
            await DisplayAlert("Erro de Dados", $"O formato dos dados recebidos da API est� incorreto. Verifique se o backend foi atualizado para enviar nome e idade do paciente.\n\nDetalhe t�cnico: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Conex�o", $"Ocorreu um erro geral: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            HistoricoCollectionView.IsVisible = true; // Mostra a lista ap�s o carregamento
        }
    }
}