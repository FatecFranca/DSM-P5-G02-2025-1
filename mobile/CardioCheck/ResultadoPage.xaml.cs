using CardioCheck.Model;

namespace CardioCheck;

public partial class ResultadoPage : ContentPage
{

    public ResultadoPage(Questionario questionario, Resultado resultado)
    {
        InitializeComponent();
        PopulateData(questionario, resultado);
    }

    private void PopulateData(Questionario questionario, Resultado resultado)
    {
        // --- Define as vari�veis com base no resultado da predi��o ---
        bool isHighRisk = resultado.Predicao == 1;

        var corResultado = isHighRisk ? Colors.Red : Colors.Green;
        var corFundoIcone = isHighRisk ? Color.FromRgba(255, 0, 0, 0.1) : Color.FromRgba(0, 128, 0, 0.1);
        var textoResultado = isHighRisk ? "ALTO RISCO" : "BAIXO RISCO";
        var iconeGlyph = isHighRisk ? "\uE808" : "\uE807"; // E808 para triste, E807 para feliz

        // --- Popula o card de resultado em destaque ---
        ResultadoBorder.Stroke = new SolidColorBrush(corResultado);
        ResultadoPredicaoLabel.Text = textoResultado;
        ResultadoPredicaoLabel.TextColor = corResultado;
        ResultadoRecomendacaoLabel.Text = resultado.Recomendacao;

        // --- L�GICA DO �CONE ADICIONADA AQUI ---
        ResultadoIconCircle.BackgroundColor = corFundoIcone;
        ResultadoIcon.Source = new FontImageSource
        {
            Glyph = iconeGlyph,
            FontFamily = "FontIcons", 
            Size = 45,
            Color = corResultado
        };
        // --- FIM DA L�GICA DO �CONE ---

        // --- Popula o resumo dos dados do paciente ---
        NomeLabel.Text = questionario.Nome;
        IdadeLabel.Text = questionario.Age.ToString();
        SexoLabel.Text = questionario.Sex == 1 ? "Masculino" : "Feminino";

        string[] tiposDorPeito = { "1: Angina T�pica", "2: Angina At�pica", "3: Dor N�o-anginosa", "4: Assintom�tico" };
        DorPeitoLabel.Text = tiposDorPeito[questionario.ChestPainType - 1];

        PressaoLabel.Text = $"{questionario.RestingBloodPressure} mm Hg";
        ColesterolLabel.Text = $"{questionario.SerumCholesterol} mg/dl";
        GlicemiaLabel.Text = questionario.FastingBloodSugar == 1 ? "Sim" : "N�o";

        string[] resultadosECG = { "0: Normal", "1: Anormalidade da onda ST-T", "2: Hipertrofia ventricular esquerda" };
        EcgLabel.Text = resultadosECG[questionario.RestingECG];

        FreqCardiacaLabel.Text = questionario.MaxHeartRate.ToString();
        AnginaExercicioLabel.Text = questionario.ExerciseAngina == 1 ? "Sim" : "N�o";
        OldpeakLabel.Text = questionario.Oldpeak.ToString("F1");

        string[] inclinacoesST = { "0: Normal", "1: Ascendente", "2: Descendente" };
        InclinacaoStLabel.Text = inclinacoesST[questionario.StSlope];
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        // 1. Envia a mensagem para quem estiver ouvindo (neste caso, a QuestionarioPage)
        MessagingCenter.Send<object>(this, "LimparFormulario");

        // 2. Navega de volta para a tela raiz do aplicativo (a tela de abas)
        await Navigation.PopToRootAsync();
    }
}