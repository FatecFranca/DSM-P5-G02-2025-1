using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CardioCheck.Models;
using CardioCheck.Model;
using System.Threading.Tasks;

namespace CardioCheck;

public partial class QuestionarioPage : ContentPage
{
    public QuestionarioPage()
    {
        InitializeComponent();

        MessagingCenter.Subscribe<object>(this, "LimparFormulario", async (sender) => // Adiciona o "async" aqui
        {
            await ClearForm(); // Agora o await funciona
        });
    }
    private void OnAnginaSwitchToggled(object sender, ToggledEventArgs e)
    {
        bool isToggled = e.Value;
        if (isToggled)
        {
            AnginaExercicioLabel.Text = "Sim";
            AnginaExercicioLabel.TextColor = Colors.Green;
        }
        else
        {
            AnginaExercicioLabel.Text = "N�o";
            AnginaExercicioLabel.TextColor = Colors.Gray; // Ou Colors.Red
        }
    }
    private void OnGlicemiaSwitchToggled(object sender, ToggledEventArgs e)
    {
        bool isToggled = e.Value;
        if (isToggled)
        {
            GlicemiaLabel.Text = "Sim";
            GlicemiaLabel.TextColor = Colors.Green;
        }
        else
        {
            GlicemiaLabel.Text = "N�o";
            GlicemiaLabel.TextColor = Colors.Gray;
        }
    }

    private async void OnInfoInclinacaoSTTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert(
            "Inclina��o do Pico do Segmento ST", // T�tulo do Pop-up
            "Este campo descreve a inclina��o do segmento ST no eletrocardiograma durante o pico do exerc�cio.\n\n" +
            "� Normal (Flat): Risco intermedi�rio.\n\n" +
            "� Ascendente (Upsloping): Geralmente considerado de baixo risco.\n\n" +
            "� Descendente (Downsloping): Geralmente indica um maior risco de isquemia mioc�rdica (fluxo sangu�neo inadequado para o cora��o).", // Mensagem explicativa
            "Entendi" // Texto do bot�o para fechar
        );
    }

    private async void OnEnviarQuestionarioClicked(object sender, EventArgs e)
    {
        try
        {
            // Valida��o simples de entrada
            if (!ValidateInputs())
            {
                ResultadoLabel.Text = "Por favor, preencha todos os campos corretamente.";
                ResultadoLabel.TextColor = Colors.Red;
                return;
            }

            await SetLoadingState(true);

            // Ativa o loader e esconde o formul�rio
            SetLoadingState(true);

            var questionario = new Questionario
            {
                Nome = NomePacienteEntry.Text,
                Age = int.Parse(IdadeEntry.Text),
                Sex = SexoMasculinoRadio.IsChecked ? 1 : 0,
                ChestPainType = TipoDorPeitoPicker.SelectedIndex + 1,
                RestingBloodPressure = float.Parse(PressaoArterialRepousoEntry.Text),
                SerumCholesterol = float.Parse(ColesterolSericoEntry.Text),
                FastingBloodSugar = GlicemiaSwitch.IsToggled ? 1 : 0,
                RestingECG = EletrocardiogramaRepousoPicker.SelectedIndex,
                MaxHeartRate = float.Parse(FrequenciaCardiacaMaximaEntry.Text),
                ExerciseAngina = AnginaExercicioSwitch.IsToggled ? 1 : 0,
                Oldpeak = float.Parse(OldpeakEntry.Text),
                StSlope = InclinacaoSTPicker.SelectedIndex
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessaoLogin.Token);
            var url = $"{SessaoLogin.UrlApi}/questionarios";

            var json = JsonSerializer.Serialize(questionario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();



            // ================== ALTERA��O PRINCIPAL AQUI ==================
            if (response.IsSuccessStatusCode)
            {
                var resultado = JsonSerializer.Deserialize<Resultado>(responseContent);

                // Navega para a nova p�gina de resultado, passando os dados
                await Navigation.PushAsync(new ResultadoPage(questionario, resultado));

                // Limpa o Label de erro da p�gina atual
                ResultadoLabel.Text = string.Empty;
            }
            else
            {
                // Em caso de erro, o resultado ainda � mostrado no Label da p�gina atual
                ResultadoLabel.Text = $"Erro ao processar: {responseContent}";
                ResultadoLabel.TextColor = Colors.Red;
            }
            // ==============================================================



        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
        finally
        {
            // Desativa o loader e mostra o formul�rio, independentemente do resultado
            await SetLoadingState(false);
        }
    }

    private async Task SetLoadingState(bool isLoading)
    {
        uint duration = 250; // Dura��o da anima��o em milissegundos
        Easing easing = Easing.CubicInOut; // Efeito de acelera��o/desacelera��o suave

        if (isLoading)
        {
            // Prepara o loader para a anima��o de FadeIn
            LoaderGrid.Opacity = 0;
            LoaderGrid.IsVisible = true;

            // Inicia as duas anima��es ao mesmo tempo
            await Task.WhenAll(
                MainContentScrollView.FadeTo(0.3, duration, easing), // Deixa o formul�rio semitransparente
                LoaderGrid.FadeTo(1, duration, easing) // Deixa o loader totalmente vis�vel
            );

            // Esconde o formul�rio do layout para n�o ser clic�vel por baixo do loader
            MainContentScrollView.IsVisible = false;
        }
        else
        {
            // Prepara o formul�rio para a anima��o de FadeIn
            MainContentScrollView.Opacity = 0;
            MainContentScrollView.IsVisible = true;

            // Inicia as duas anima��es de volta ao mesmo tempo
            await Task.WhenAll(
                LoaderGrid.FadeTo(0, duration, easing), // Deixa o loader transparente
                MainContentScrollView.FadeTo(1, duration, easing) // Deixa o formul�rio totalmente vis�vel
            );

            // Esconde o loader do layout ap�s a anima��o
            LoaderGrid.IsVisible = false;
        }
    }

    private bool ValidateInputs()
    {
        return !string.IsNullOrWhiteSpace(NomePacienteEntry.Text) &&
               int.TryParse(IdadeEntry.Text, out _) &&
               float.TryParse(PressaoArterialRepousoEntry.Text, out _) &&
               float.TryParse(ColesterolSericoEntry.Text, out _) &&
               float.TryParse(FrequenciaCardiacaMaximaEntry.Text, out _) &&
               float.TryParse(OldpeakEntry.Text, out _) &&
               TipoDorPeitoPicker.SelectedIndex != -1 &&             
               EletrocardiogramaRepousoPicker.SelectedIndex != -1 &&
               InclinacaoSTPicker.SelectedIndex != -1;
    }
    public async Task ClearForm()
    {
        // Limpa os campos de texto
        NomePacienteEntry.Text = string.Empty;
        IdadeEntry.Text = string.Empty;

        // Redefine os RadioButtons
        SexoMasculinoRadio.IsChecked = true;

        // Redefine os Pickers
        TipoDorPeitoPicker.SelectedIndex = -1;
        EletrocardiogramaRepousoPicker.SelectedIndex = -1;
        InclinacaoSTPicker.SelectedIndex = -1;

        // Redefine os Switches
        GlicemiaSwitch.IsToggled = false;
        AnginaExercicioSwitch.IsToggled = false;

        // Redefine os Sliders para seus valores padr�o
        PressaoSlider.Value = 120;
        ColesterolSlider.Value = 200;
        FreqCardiacaSlider.Value = 150;
        OldpeakSlider.Value = 1.0;

        await MainContentScrollView.ScrollToAsync(0, 0, true);
    }




}