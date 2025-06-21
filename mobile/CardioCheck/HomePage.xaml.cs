using System.Collections.ObjectModel; // Usado para cole��es que notificam a UI

namespace CardioCheck;

// Modelo de dados para cada slide do carrossel
public class Slide
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public partial class HomePage : ContentPage
{
    // Propriedade que armazena os dados dos slides
    public List<Slide> Slides { get; set; }

    // Vari�vel para controlar o timer
    private IDispatcherTimer _timer;

    public HomePage()
    {
        InitializeComponent();

        // Cria a lista de slides com 3 conte�dos diferentes
        Slides = new List<Slide>
        {
            new Slide { Icon = "icone_analise.gif", Title = "An�lise R�pida", Description = "Utilize CardioCheck para uma avalia��o de risco card�aco em minutos." },
            new Slide { Icon = "icone_dados.gif", Title = "Baseado em Dados", Description = "Nosso algoritmo � treinado com casos reais para oferecer um apoio � sua decis�o cl�nica." },
            new Slide { Icon = "icone_apoio.gif", Title = "Apoio � Decis�o M�dica", Description = "Uma ferramenta moderna para auxiliar na triagem e no encaminhamento de pacientes." }
        };

        // Define o BindingContext para que o XAML possa encontrar a lista "Slides"
        this.BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Associa o IndicatorView (bolinhas) com o CarouselView
        CarouselIndicator.ItemsSource = FeaturesCarousel.ItemsSource;
        CarouselIndicator.BindingContext = FeaturesCarousel;
        CarouselIndicator.SetBinding(IndicatorView.PositionProperty, "Position");


        // Cria e inicia o timer para rota��o autom�tica
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5); // Define o intervalo de 4 segundos
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Para o timer quando a p�gina n�o est� vis�vel para economizar recursos
        _timer?.Stop();
    }

    // Este m�todo � chamado a cada 4 segundos pelo timer
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (Slides is not null && Slides.Count > 0)
        {
            // Avan�a para o pr�ximo slide
            // O operador '%' garante que ele volte ao in�cio (0) ap�s o �ltimo slide
            int nextPosition = (FeaturesCarousel.Position + 1) % Slides.Count;
            FeaturesCarousel.Position = nextPosition;
        }
    }

    // L�gica para o bot�o de logout (mantenha a sua implementa��o)
    private async void OnLogoutClicked(object sender, TappedEventArgs e)
    {
        // Exemplo:
        // await Navigation.PopToRootAsync();
    }
}
