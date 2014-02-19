using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Speech.Synthesis;
using SKYPE4COMLib;
using System.Collections.Specialized;
using System.Threading;

namespace MYTWEETYMISS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;
        private SpeechSynthesizer reader;
        private string quizQuestionName;
        private List<QuizItem> quizSet = new List<QuizItem>();
        private const int QuizQuestionCount = 10;
        private const int NumberCount = 20;
        private const int ButtonHeight = 200;
        private const int QuestionIntervalInSeconds = 2;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            reader = new SpeechSynthesizer();
            reader.Rate *= 2;
        }


        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();                                
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {        
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {                    
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        error = true;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;                   
                }
            }

            if (!error)
            {
                kinectRegion.KinectSensor = args.NewSensor;
            }
        }

        private void ButtonNumberClick(object sender, RoutedEventArgs e)
        {            
            reader.SpeakAsync(StringConstants.NUMBER);
            scrollContent.Children.Clear();
            viewer1.Visibility = System.Windows.Visibility.Visible;
            for (int i = 1; i < NumberCount; i++)
            {
                var button = new KinectCircleButton
                                 {
                                     Content = i,
                                     Height = ButtonHeight
                                 };
                    
                int i1 = i;
                button.Click += (o, args) => reader.SpeakAsync(i1.ToString()); 
                
                scrollContent.Children.Add(button);
            }
        }

        private void ButtonAlphClick(object sender, RoutedEventArgs e)
        {
            reader.SpeakAsync(StringConstants.ALPHABET);
            scrollContent.Children.Clear();
            viewer1.Visibility = System.Windows.Visibility.Visible;
            for (char i = 'A'; i <= 'Z'; i++)
            {
                var button = new KinectCircleButton
                                 {
                                     Content = i,
                                     Height = ButtonHeight
                                 };
                
                char i1 = i;
                button.Click +=
                    (o, args) => reader.SpeakAsync(i1.ToString()); 

                scrollContent.Children.Add(button);
            }
        }

        private void ButtonFruitsClick(object sender, RoutedEventArgs e)
        {
            reader.SpeakAsync(StringConstants.FRUITS);
            scrollContent.Children.Clear();
            viewer1.Visibility = System.Windows.Visibility.Visible;

            foreach (Fruit fruit in Enum.GetValues(typeof(Fruit)))
            {
                var button = new KinectTileButton
                {
                    Background = new ImageBrush {ImageSource =
                                                    new BitmapImage(
                                                        new Uri(@"..\..\Images\fruits\" + fruit.ToString() + ".jpg", UriKind.Relative)
                                                    )},
                    Height = ButtonHeight
                };

                button.Click +=
                    (o, args) => reader.SpeakAsync(fruit.ToString());

                scrollContent.Children.Add(button);
            }    
        }

        private void ButtonColorsClick(object sender, RoutedEventArgs e)
        {
            reader.SpeakAsync(StringConstants.COLOUR);
            scrollContent.Children.Clear();
            viewer1.Visibility = System.Windows.Visibility.Visible;
            foreach (Colors color in Enum.GetValues(typeof(Colors)))
            {
                var button = new KinectTileButton
                {
                    Background = new SolidColorBrush { Color = GetColorValueForColorEnum(color) },
                    Height = ButtonHeight
                };

                button.Click +=
                    (o, args) => reader.SpeakAsync(color.ToString());

                scrollContent.Children.Add(button);
            }
        }

        private void ButtonAnimalsClick(object sender, RoutedEventArgs e)
        {
            reader.SpeakAsync(StringConstants.ANIMAL);
            scrollContent.Children.Clear();
            
            foreach (Animal animal in Enum.GetValues(typeof(Animal)))
            {
                var button = new KinectTileButton
                {
                    Background = new ImageBrush
                    {
                        ImageSource =
                           new BitmapImage(
                               new Uri(@"..\..\Images\animals\" + animal.ToString() + ".jpg", UriKind.Relative)
                           )
                    },
                    Height = ButtonHeight
                };

                button.Click +=
                    (o, args) => reader.SpeakAsync(animal.ToString());

                scrollContent.Children.Add(button);
            } 
        }

        private void ButtonQuizClick(object sender, RoutedEventArgs e)
        {
            quizSet = new List<QuizItem>();
            scrollContent.Children.Clear();            

            PrepareQuizSet();
            MapActionToQuizButtons();
            SetRandomQuestion();
        }

        private void MapActionToQuizButtons()
        {
            foreach (QuizItem item in quizSet)
            {
                KinectButtonBase button = new KinectTileButton();
                if (!String.IsNullOrEmpty(item.Text))
                {
                    button = new KinectCircleButton();
                    button.Content = item.Text;
                }
                else if (!String.IsNullOrEmpty(item.ImageUrl))
                {
                    button.Background = new ImageBrush
                    {
                        ImageSource =
                           new BitmapImage(
                               new Uri(item.ImageUrl, UriKind.Relative)
                           )
                    };
                }
                else if (item.Color != null)
                {
                    button.Background = new SolidColorBrush { Color = item.Color };
                }
                button.Height = ButtonHeight;
                button.Tag = item.Name;
                button.Click += QuizItemButtonClicked;
                scrollContent.Children.Add(button);
            }
        }

        private void PrepareQuizSet()
        {
            var r = new Random();

            for (int i = 0; i < QuizQuestionCount; i++)
            {
                int randomItem;
                int category = r.Next(1, 6);
                switch (category)
                {
                    case 1:
                        {
                            randomItem = r.Next(1, 21);
                            quizSet.Add(new QuizItem { Name = randomItem.ToString(), Text = randomItem.ToString() });
                            break;
                        }
                    case 2:
                        {
                            randomItem = r.Next(1, 27);
                            char c = Convert.ToChar('A' + randomItem - 1);
                            quizSet.Add(new QuizItem { Name = c.ToString(), Text = c.ToString() });
                            break;
                        }
                    case 3:
                        {
                            randomItem = r.Next(1, 11);
                            quizSet.Add(new QuizItem { Name = ((Animal)randomItem).ToString(), ImageUrl = @"..\..\Images\animals\" + ((Animal)randomItem).ToString() + ".jpg" });
                            break;
                        }
                    case 4:
                        {
                            randomItem = r.Next(1, 11);
                            quizSet.Add(new QuizItem { Name = ((Fruit)randomItem).ToString(), ImageUrl = @"..\..\Images\fruits\" + ((Fruit)randomItem).ToString() + ".jpg" });
                            break;
                        }
                    case 5:
                        {
                            randomItem = r.Next(1, 11);
                            quizSet.Add(new QuizItem { Name = ((Colors)randomItem).ToString(), Color = GetColorValueForColorEnum((Colors)randomItem) });
                            break;
                        }
                }

            }
        }

   

        private void QuizItemButtonClicked(object sender, EventArgs e)
        {
            string tag = ((KinectButtonBase)sender).Tag.ToString();
            if (tag.Equals(quizQuestionName))
            {
                reader.SpeakAsync(StringConstants.RIGHT);
            }
            else
            {
                reader.SpeakAsync(StringConstants.WRONG);
            }            
            SetRandomQuestion();   
        }

        private void SetRandomQuestion()
        {
            Thread.Sleep(QuestionIntervalInSeconds * 1000);
            var r = new Random();
            int i = r.Next(0, QuizQuestionCount);
            quizQuestionName = quizSet[i].Name;
            reader.SpeakAsync(StringConstants.FIND + quizSet[i].Name);
        }

        private System.Windows.Media.Color GetColorValueForColorEnum(Colors color)
        {
            System.Windows.Media.Color colorValue = new Color();
            switch (color)
            {
                case Colors.black:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.0f, ScG = 0.0f, ScR = 0.0f }; break;
                case Colors.blue:
                    colorValue = new Color { ScA = 1.0f, ScB = 1.0f, ScG = 0.0f, ScR = 0.0f }; break;
                case Colors.cyan:
                    colorValue = new Color { ScA = 1.0f, ScB = 1.0f, ScG = 1.0f, ScR = 0.0f }; break;
                case Colors.green:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.0f, ScG = 1.0f, ScR = 0.0f }; break;
                case Colors.orange:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.0f, ScG = 0.65f, ScR = 1.0f }; break;
                case Colors.purple:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.5f, ScG = 0.0f, ScR = 0.5f }; break;
                case Colors.red:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.0f, ScG = 0.0f, ScR = 1.0f }; break;
                case Colors.violet:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.93f, ScG = 0.51f, ScR = 0.93f }; break;
                case Colors.white:
                    colorValue = new Color { ScA = 1.0f, ScB = 1.0f, ScG = 1.0f, ScR = 1.0f }; break;
                case Colors.yellow:
                    colorValue = new Color { ScA = 1.0f, ScB = 0.0f, ScG = 1.0f, ScR = 1.0f }; break;
            }

            return colorValue;
        }
        
    }
}
