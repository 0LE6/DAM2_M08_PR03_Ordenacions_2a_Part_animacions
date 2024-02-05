using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace DAM2_M08_PR03_Ordenacions_2a_Part_animacions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] elementos;
        private int delay;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private double tamañoCiculito = 15; // aqui ajusto el tamaño del circulito
        private bool isMuted = false;

        // meter 4 pincells (solid color brush)
        SolidColorBrush scbCorrecte;
        SolidColorBrush scbIncorrecte;
        SolidColorBrush scbIntercambio;
        SolidColorBrush scbFondo;

        private DispatcherTimer delayTimer;

        public MainWindow()
        {
            InitializeComponent();

            // he tendio que poner este apaño del gpt porque no me cargaba el default del iupPausa
            iudPausa_ValueChanged(iudPausa, new RoutedPropertyChangedEventArgs<object>(null, iudPausa.Value));

            // Establecer colores por defecto para los ColorPickers
            // TODO: aqui meter los de lo pincells
            scbCorrecte = new SolidColorBrush(Colors.Green);
            scbIncorrecte = new SolidColorBrush(Colors.Red);
            scbIntercambio = new SolidColorBrush(Colors.Yellow);
            scbFondo = new SolidColorBrush(Colors.White);

            // le doy a mis color pickers el color de mis pincells
            colorCorrecte.SelectedColor = scbCorrecte.Color;
            colorIncorrecter.SelectedColor = scbIncorrecte.Color;
            colorIntercanvi.SelectedColor = scbIntercambio.Color;
            colorFons.SelectedColor = scbFondo.Color;

            // controladores de eventos para que los cambios en los IntegerUpDown se vayan reflejando en mis 4 pincells
            colorCorrecte.SelectedColorChanged += ColorPicker_SelectedColorChanged;
            colorIncorrecter.SelectedColorChanged += ColorPicker_SelectedColorChanged;
            colorIntercanvi.SelectedColorChanged += ColorPicker_SelectedColorChanged;
            colorFons.SelectedColorChanged += ColorPicker_SelectedColorChanged;

            // he usado dos controladores de eventos para los valores de la pausa y el radi
            iudPausa.ValueChanged += iudPausa_ValueChanged;
            iudRadi.ValueChanged += iudRadi_ValueChanged;
            colorFons.SelectedColorChanged += ColorFons_SelectedColorChanged;


            delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromMilliseconds(500);
            delayTimer.Tick += DelayTimer_Tick;


        }

        ////////////////////// CONTROLADORES DE EVENTOS /////////////////////////
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            delayTimer.Stop();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (sender is ColorPicker colorPicker)
            {
                switch (colorPicker.Name)
                {
                    case "colorCorrecte":
                        if (e.NewValue.HasValue)
                        {
                            scbCorrecte.Color = e.NewValue.Value;
                        }
                        break;
                    case "colorIncorrecter":
                        if (e.NewValue.HasValue)
                        {
                            scbIncorrecte.Color = e.NewValue.Value;
                        }
                        break;
                    case "colorIntercanvi":
                        if (e.NewValue.HasValue)
                        {
                            scbIntercambio.Color = e.NewValue.Value;
                        }
                        break;
                    case "colorFons":
                        if (e.NewValue.HasValue)
                        {
                            scbFondo.Color = e.NewValue.Value;
                            cvCanvas.Background = scbFondo;
                        }
                        break;
                }
            }
        }

        private void ColorFons_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                cvCanvas.Background = scbFondo;
            }
        }

        private void iudRadi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is int nuevoRadio)
            {
                foreach (var child in cvCanvas.Children)
                {
                    if (child is Rectangle rect)
                    {
                        rect.RadiusX = nuevoRadio;
                        rect.RadiusY = nuevoRadio;
                    }
                }
            }
        }

        private void iudPausa_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            delay = (int)e.NewValue; // actualizamos el delay segun lo que haga el usuario
        }

        // Método auxiliar para controlar los checked y unchecked entre los 2 checkbox
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                if (checkBox.Name == "checkInvertit" && checkInvertit.IsChecked == true)
                {
                    checkAleatori.IsChecked = false;
                }
                else if (checkBox.Name == "checkAleatori" && checkAleatori.IsChecked == true)
                {
                    checkInvertit.IsChecked = false;
                }
            }
        }

        // Codigo sacado del Moodle, autor: Xavier Sanmartí
        private void Retraso(int milliseconds)
        {
            var frame = new DispatcherFrame();
            new Thread(() =>
            {
                Thread.Sleep(milliseconds);
                frame.Continue = false; // Esto finaliza el DispatcherFrame
            }).Start();

            Dispatcher.PushFrame(frame); // Esto bloquea la ejecución hasta que el frame se detenga
        }


        ////////////////////// ORDENAR /////////////////////////

        private void btnOrdenar_Click(object sender, RoutedEventArgs e)
        {
            // no se si funcionara esta tonteria (funciona lol)
            mediaPlayer.Open(new Uri("music/tetris.mp3", UriKind.Relative));
            mediaPlayer.Play();


            string metodoSeleccionado = (cbOrdenacion.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (metodoSeleccionado)
            {
                case "Bubble sort":
                    BubbleSort();
                    break;
                case "Selection sort":
                    SelectionSort();
                    break;
                case "Insertion sort":
                    InsertionSort();
                    break;
                case "Counting sort":
                    CountingSort();
                    break;
            }
            mediaPlayer.Stop();
        }

        private void IntercambiarFiguras(int index1, int index2)
        {
            // Asegúrate de que los índices son válidos
            if (index1 < 0 || index1 >= cvCanvas.Children.Count || index2 < 0 || index2 >= cvCanvas.Children.Count) return;

            // Obtiene las figuras por su índice
            var figura1 = cvCanvas.Children[index1] as Shape;
            var figura2 = cvCanvas.Children[index2] as Shape;

            // Calcula las nuevas posiciones basadas en el ancho del canvas y el número de elementos
            double nuevaPosX1 = index2 * (cvCanvas.ActualWidth / elementos.Length);
            double nuevaPosX2 = index1 * (cvCanvas.ActualWidth / elementos.Length);

            // Prepara las animaciones
            DoubleAnimation animacion1 = new DoubleAnimation
            {
                To = nuevaPosX2,
                Duration = TimeSpan.FromMilliseconds(2)
            };

            DoubleAnimation animacion2 = new DoubleAnimation
            {
                To = nuevaPosX1,
                Duration = TimeSpan.FromMilliseconds(2)
            };

            // Cambia los colores de las figuras a scbIntercambio antes de iniciar la animación
            if (figura1 != null && figura2 != null)
            {
                figura1.Fill = scbIntercambio;
                figura2.Fill = scbIntercambio;

                // Inicia las animaciones
                figura1.BeginAnimation(Canvas.LeftProperty, animacion1);
                figura2.BeginAnimation(Canvas.LeftProperty, animacion2);

                // Espera a que la animación termine
                Retraso(delay);

                // Intercambia los elementos en el arreglo 'elementos'
                int temp = elementos[index1];
                elementos[index1] = elementos[index2];
                elementos[index2] = temp;

                // Restablece los colores originales y actualiza la altura si es necesario
                ActualizarColorFigura(index1);
                ActualizarColorFigura(index2);
                ActualizarAlturaFigura(index1, elementos[index1]);
                ActualizarAlturaFigura(index2, elementos[index2]);
            }
        }


        private void CambiarColorFiguraTemporal(int index)
        {
            if (cvCanvas.Children[index] is Shape figura)
            {
                figura.Fill = scbIntercambio;
            }
        }

        private void ActualizarAlturaFigura(int index, int valor)
        {
            if (cvCanvas.Children.Count > index)
            {
                if (cvCanvas.Children[index] is Shape figura)
                {
                    double alturaCanvas = cvCanvas.ActualHeight;
                    int valorMaximo = elementos.Max();
                    double nuevaAltura = (valor / (double)valorMaximo) * alturaCanvas;

                    // Ajustar la altura y la posición vertical de la figura
                    if (figura is Rectangle rect)
                    {
                        rect.Height = nuevaAltura;
                        Canvas.SetTop(rect, alturaCanvas - nuevaAltura);
                    }
                    else if (figura is Ellipse elipse)
                    {
                        // Para elipses, puedes decidir cambiar su tamaño o posición
                        // Aquí, como ejemplo, cambio el tamaño
                        // Mantener la proporción si es necesario
                        Canvas.SetTop(elipse, alturaCanvas - nuevaAltura);
                    }
                }
            }
        }

        private void ActualizarColorFigura(int index)
        {
            int[] elementosOrdenados = elementos.OrderBy(x => x).ToArray();

            if (cvCanvas.Children[index] is Shape figura)
            {
                figura.Fill = elementos[index] == elementosOrdenados[index]
                    ? scbCorrecte
                    : scbIncorrecte;
            }
        }

        ////////////////////// POSICIONAR /////////////////////////

        private void btnPosicionar_Click(object sender, RoutedEventArgs e)
        {
            // siempre limpiamos el canvas antes de pintar de nuevo
            cvCanvas.Children.Clear();

            int numeroDeElementos = iudElements.Value ?? 0;
            bool invertido = checkInvertit.IsChecked == true;
            bool aleatorio = checkAleatori.IsChecked == true;

            elementos = Enumerable.Range(1, numeroDeElementos).ToArray();

            if (invertido)
            {
                Array.Reverse(elementos);
            }
            else if (aleatorio)
            {
                Random rng = new Random();
                // ordenamos de forma aleatoria y pasamos a arrauy
                elementos = elementos.OrderBy(x => rng.Next()).ToArray();
            }

            DibujarFiguras(elementos);
        }

        private void DibujarFiguras(int[] elementos)
        {
            // Obtenemos las dimensiones del Canvas
            double alturaCanvas = cvCanvas.ActualHeight;
            double anchoCanvas = cvCanvas.ActualWidth;

            // calculo del espacio entre figuras
            double espacioEntreFiguras = anchoCanvas / elementos.Length;

            // buscamos el elemento máximo 
            int valorMaximo = elementos.Any() ? elementos.Max() : 1; // Evitamos la división por cero

            // cogemos los colores de los ColorPickels
            Color colorCorrecto = this.colorCorrecte.SelectedColor ?? Colors.Green;
            Color colorIncorrecto = this.colorIncorrecter.SelectedColor ?? Colors.Red;

            // solucion burra: crear un array ordenado apra comparar
            int[] elementosOrdenados = (int[])elementos.Clone();
            Array.Sort(elementosOrdenados);

            for (int i = 0; i < elementos.Length; i++)
            {
                double alturaFigura = (elementos[i] / (double)valorMaximo) * alturaCanvas;

                if ((cbFiguras.SelectedItem as ComboBoxItem)?.Content.ToString() == "Círculos")
                {
                    // pintamos círculos (elipses)
                    Ellipse circulo = new Ellipse
                    {
                        Width = tamañoCiculito,
                        Height = tamañoCiculito,
                        Fill = elementos[i] == elementosOrdenados[i]
                            ? scbCorrecte
                            : scbIncorrecte // con esto estara de color correcto o incorrectto
                    };

                    // lo situamos en el Canvas
                    Canvas.SetLeft(circulo, i * espacioEntreFiguras + (espacioEntreFiguras - tamañoCiculito) / 2);
                    Canvas.SetTop(circulo, alturaCanvas - alturaFigura - tamañoCiculito / 2); // alineacion desde la parte superior
                    cvCanvas.Children.Add(circulo);
                }
                else
                {
                    // pintamos rectángulos
                    Rectangle rectangulo = new Rectangle
                    {
                        Width = espacioEntreFiguras,
                        Height = alturaFigura,
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = iudGrosor.Value ?? 0,
                        RadiusX = iudRadi.Value ?? 0,
                        RadiusY = iudRadi.Value ?? 0,
                        Fill = elementos[i] == elementosOrdenados[i]
                            ? scbCorrecte
                            : scbIncorrecte
                    };

                    // lo situamos en el Canvas y ajustamos la posicion
                    Canvas.SetLeft(rectangulo, i * espacioEntreFiguras);
                    Canvas.SetTop(rectangulo, alturaCanvas - alturaFigura);
                    cvCanvas.Children.Add(rectangulo);
                }
            }
        }

        ////////////////////// SORTING ALGORITHMS /////////////////////////  

        private void BubbleSort()
        {
            int n = elementos.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (elementos[j] > elementos[j + 1])
                    {
                        IntercambiarFiguras(j, j + 1);
                        //cvCanvas.UpdateLayout();

                        // Este es mi primer intento con mi primer sorting algorithm,
                        // probamos dandole un delay segun selecciones el usuario
                        //await Task.Delay(delay);

                        // un breve retraso
                        Retraso(delay);
                    }
                }
            }
        }

        private void SelectionSort()
        {
            int n = elementos.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (elementos[j] < elementos[minIndex])
                    {
                        minIndex = j;
                    }
                }
                if (minIndex != i)
                {
                    IntercambiarFiguras(i, minIndex);
                }
            }
        }

        private void InsertionSort()
        {
            int n = elementos.Length;
            for (int i = 1; i < n; i++)
            {
                int key = elementos[i];
                int j = i - 1;

                while (j >= 0 && elementos[j] > key)
                {
                    elementos[j + 1] = elementos[j];
                    IntercambiarFiguras(j, j + 1);
                    j = j - 1;
                }
                elementos[j + 1] = key;
            }
        }

        // Deprecated
        private void CountingSort()
        {
            int RANGE = elementos.Max() + 1; // Asegúrate de que el rango cubra todos los valores posibles
            int[] count = new int[RANGE];
            int[] output = new int[elementos.Length];

            // Inicializa el array de conteo
            for (int i = 0; i < RANGE; ++i)
            {
                count[i] = 0;
            }

            // Almacena el conteo de cada elemento
            for (int i = 0; i < elementos.Length; ++i)
            {
                ++count[elementos[i]];
            }

            // Cambia count[i] para que contenga la posición actual en output de este elemento
            for (int i = 1; i < RANGE; ++i)
            {
                count[i] += count[i - 1];
            }

            // Construye el array de salida
            for (int i = elementos.Length - 1; i >= 0; i--)
            {
                output[count[elementos[i]] - 1] = elementos[i];
                --count[elementos[i]];
            }

            // Copia los elementos a elementos[]
            for (int i = 0; i < elementos.Length; ++i)
            {
                int indexAnterior = Array.IndexOf(elementos, output[i]);
                elementos[i] = output[i];
                IntercambiarFiguras(i, indexAnterior);
            }
        }

        ////////////////////// MUSIC ///////////////////////// 

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if (!isMuted)
            {
                mediaPlayer.Volume = 0;
                btnMute.Content = "Unmute";
            }
            else
            {
                mediaPlayer.Volume = 1; // O restablecer al volumen anterior si lo has guardado
                btnMute.Content = "Mute";
            }
            isMuted = !isMuted;
        }

        private void CheckColorSiONo_Checked(object sender, RoutedEventArgs e)
        {
            scbIntercambio.Color = scbIncorrecte.Color; // O cualquier otro color para indicar "desactivado"
        }

        private void CheckColorSiONo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (colorIntercanvi.SelectedColor.HasValue)
            {
                scbIntercambio.Color = colorIntercanvi.SelectedColor.Value;
            }
        }


    }
}
