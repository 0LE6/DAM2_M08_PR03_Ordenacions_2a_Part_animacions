using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
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
        private double tamañoCiculito = 35; // aqui ajusto el tamaño del circulito
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
            delay = (int)e.NewValue == 0 ? 1 : (int)e.NewValue; // actualizamos el delay segun lo que haga el usuario
            // ternario porque si no petaba con el maldito 0
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
                case "Cocktail sort":
                    CocktailShakerSort();
                    break;
                case "Quick sort":
                    QuickSortWrapper();
                    break;
                case "Heap sort":
                    HeapSort();
                    break;
            }
            mediaPlayer.Stop();
        }

        private string GetSelectedEasingFunction()
        {
            if (cbFuncioEasing.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }
            else
            {
                return "Lineal"; // el de FOL
            }
        }

        private IEasingFunction GetEasingFunction(string easingFunctionName)
        {
            switch (easingFunctionName)
            {
                case "EaseInOut":
                    return new PowerEase { EasingMode = EasingMode.EaseInOut };
                case "EaseIn":
                    return new PowerEase { EasingMode = EasingMode.EaseIn };
                case "EaseOut":
                    return new PowerEase { EasingMode = EasingMode.EaseOut };
                case "Lineal":
                    return null; // Sin función de easing para una animación lineal
                case "BounceEase":
                    return new BounceEase();
                default:
                    return null; // O cualquier otro valor por defecto
            }
        }

        private void IntercambiarFiguras(int index1, int index2)
        {
            DependencyProperty propDeLaAnimacion;
            var tipoDeAnimacion = cbTipusAnimacio.Text;
            var nombreDeLaFuncionEasingSeleccionada = GetSelectedEasingFunction();

            var figura1 = cvCanvas.Children[index1] as Shape;
            var figura2 = cvCanvas.Children[index2] as Shape;

            // calculamos las nuevas posiciones basadas en el ancho del canvas y el número de elementos
            var nuevaPosision1 = index2 * (cvCanvas.ActualWidth / elementos.Length);
            var nuevaPosicion2 = index1 * (cvCanvas.ActualWidth / elementos.Length);
            double primeraPosicion;
            double segundaPosicion;

            // para que la animacion sea vertical o de lado
            if (tipoDeAnimacion == "Vertical")
            {
                primeraPosicion = Canvas.GetTop(cvCanvas.Children[index1]);
                segundaPosicion = Canvas.GetTop(cvCanvas.Children[index2]);
                propDeLaAnimacion = Canvas.TopProperty;
            }
            else
            {
                primeraPosicion = Canvas.GetLeft(cvCanvas.Children[index1]);
                segundaPosicion = Canvas.GetLeft(cvCanvas.Children[index2]);
                propDeLaAnimacion = Canvas.LeftProperty;
            }


            // Prepara las animaciones
            DoubleAnimation animacion1 = new DoubleAnimation
            {
                From = primeraPosicion,
                To = nuevaPosision1,
                Duration = TimeSpan.FromMilliseconds(delay),
                EasingFunction = GetEasingFunction(nombreDeLaFuncionEasingSeleccionada),
                FillBehavior = FillBehavior.Stop
            };

            DoubleAnimation animacion2 = new DoubleAnimation
            {
                From = segundaPosicion,
                To = nuevaPosicion2,
                Duration = TimeSpan.FromMilliseconds(delay),
                EasingFunction = GetEasingFunction(nombreDeLaFuncionEasingSeleccionada),
                FillBehavior = FillBehavior.Stop
            };            
            
            // reciclado del anterior codigo
            CambiarColorFiguraTemporal(index1);
            CambiarColorFiguraTemporal(index2);

            animacion1.Completed += (sender, e) =>
            {
                Canvas.SetZIndex(cvCanvas.Children[index1], 0);
                Canvas.SetZIndex(cvCanvas.Children[index2], 0);
            };

            // ejecutamos la animacion
            cvCanvas.Children[index1].BeginAnimation(propDeLaAnimacion, animacion1);
            cvCanvas.Children[index2].BeginAnimation(propDeLaAnimacion, animacion2);

            // para que no se vaya de madres
            Canvas.SetZIndex(cvCanvas.Children[index1], 1);
            Canvas.SetZIndex(cvCanvas.Children[index2], 1);

            // lo que me dijo Xevi, el delay antes de actualizar
            Retraso(delay);

            // intercambiamois los elementos en el arry 'elementos'
            int temp = elementos[index1];
            elementos[index1] = elementos[index2];
            elementos[index2] = temp;

            // Restablecemos los colores originales y actualiza la altura 
            ActualizarColorFigura(index1);
            ActualizarColorFigura(index2);
            ActualizarAlturaFigura(index1, elementos[index1]);
            ActualizarAlturaFigura(index2, elementos[index2]);
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

        # region algoritmo heap sort
        private void HeapSort()
        {
            int n = elementos.Length;

            // Construir el heap (reorganizar el array)
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(n, i);

            // Extraer elementos uno por uno del heap
            for (int i = n - 1; i >= 0; i--)
            {
                // Mover la raíz actual al final
                IntercambiarFiguras(0, i);

                // Llamar a Heapify en el heap reducido
                Heapify(i, 0);
            }
        }

        private void Heapify(int n, int i)
        {
            int largest = i; // Inicializar el más grande como raíz
            int l = 2 * i + 1; // izquierda = 2*i + 1
            int r = 2 * i + 2; // derecha = 2*i + 2

            // Si el hijo izquierdo es más grande que la raíz
            if (l < n && elementos[l] > elementos[largest])
                largest = l;

            // Si el hijo derecho es más grande que el más grande hasta ahora
            if (r < n && elementos[r] > elementos[largest])
                largest = r;

            // Si el más grande no es la raíz
            if (largest != i)
            {
                IntercambiarFiguras(i, largest);
                // Recursivamente heapify el subárbol afectado
                Heapify(n, largest);
            }
        }

        #endregion

        #region algoritmo quick sort
        private void QuickSortWrapper()
        {
            QuickSort(0, elementos.Length - 1);
        }

        private void QuickSort(int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(low, high);

                QuickSort(low, pi - 1);
                QuickSort(pi + 1, high);
            }
        }
                
        private int Partition(int low, int high)
        {
            int pivot = elementos[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (elementos[j] < pivot)
                {
                    i++;
                    // Intercambiar elementos[i] y elementos[j]
                    IntercambiarFiguras(i, j);
                }
            }
            // Intercambiar elementos[i+1] y elementos[high] (o pivot)
            IntercambiarFiguras(i + 1, high);
            return i + 1;
        }
        #endregion

        #region algoritmo cocktail sort

        private void CocktailShakerSort()
        {
            bool swapped = true;
            int start = 0;
            int end = elementos.Length;

            while (swapped)
            {
                // Reseteamos el flag swapped a false, ya que podría ser true desde la iteración anterior.
                swapped = false;

                // Loop de izquierda a derecha igual que el Bubble Sort
                for (int i = start; i < end - 1; ++i)
                {
                    if (elementos[i] > elementos[i + 1])
                    {
                        IntercambiarFiguras(i, i + 1);
                        swapped = true;
                    }
                }

                // Si no se movió nada, entonces el array está ordenado.
                if (!swapped)
                    break;

                // De lo contrario, necesitamos resetear el flag swapped para la siguiente iteración
                swapped = false;

                // Mueve el punto final atrás por uno, ya que el elemento en el final está en su lugar
                end = end - 1;

                // Loop de derecha a izquierda, haciendo lo mismo que el loop de izquierda a derecha
                for (int i = end - 1; i >= start; i--)
                {
                    if (elementos[i] > elementos[i + 1])
                    {
                        IntercambiarFiguras(i, i + 1);
                        swapped = true;
                    }
                }

                // Incrementa el punto inicial, ya que el último elemento de la iteración anterior está en su lugar
                start = start + 1;
            }
        }


        #endregion

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
