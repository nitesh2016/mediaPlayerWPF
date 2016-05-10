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
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;


namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    [Serializable]
    public class Item
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public override string ToString()
        {
            return string.Format("Title:{0}", Title);
        }

    }


    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer hideUItimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            hideUItimer.Interval = TimeSpan.FromSeconds(1);
            hideUItimer.Tick += hideUITick;
        }
        public DateTime LastMouseMove { get; private set; }
        private void Window1_MouseMove(object sender, MouseEventArgs e)
        {
            LastMouseMove = DateTime.Now;
            if(isHidden)
            {
                Window1_MouseEnter(null, null);
                isHidden = false;
            }
        }

        bool isHidden;
        private void hideUITick(object sender, EventArgs e)
        {
            TimeSpan span = DateTime.Now - LastMouseMove;
            if(span>=TimeSpan.FromSeconds(5)&&!isHidden)
            {
               Window1_MouseLeave(null,null);
               isHidden = true;
            }
        }

        bool isClickedPlay = false;
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isClickedPlay)
            {
                Mediaplayer.Pause(); dispatcherTimer.Stop();
                Play.Background = null;
                playImage.Visibility = Visibility.Visible;
                isClickedPlay = false;
            }
            else
            {
                if (isEnd) Repeat_Click(null, null);

                if (Mediaplayer.Source == null)
                    add_Click(null, null);

                if (Mediaplayer.Source == null)
                    return;

                try
                {
                    Mediaplayer.Play();
                    dispatcherTimer.Start();
                    hideUItimer.Start();
                }
                catch (Exception a) { MessageBox.Show(a.Message); }
                playImage.Visibility = Visibility.Hidden;

                Play.Background = new ImageBrush(new BitmapImage(new Uri("../../pause.png", UriKind.RelativeOrAbsolute)));
                isClickedPlay = true;
                isEnd = false;
                expander.IsExpanded = false;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            string sec, min, hours;
            #region customizeTime
            if (Mediaplayer.Position.Seconds < 10)
                sec = "0" + Mediaplayer.Position.Seconds.ToString();
            else
                sec = Mediaplayer.Position.Seconds.ToString();

            if (Mediaplayer.Position.Minutes < 10)
                min = "0" + Mediaplayer.Position.Minutes.ToString();
            else
                min = Mediaplayer.Position.Minutes.ToString();

            if (Mediaplayer.Position.Hours < 10)
                hours = "0" + Mediaplayer.Position.Hours.ToString();
            else
                hours = Mediaplayer.Position.Hours.ToString();

            #endregion customizeTime

            //ProgressBar.Value = Mediaplayer.Position.TotalMilliseconds;
            ProgressSlider.Value = Mediaplayer.Position.Seconds;
            if (Mediaplayer.Position.Hours == 0)
            {
                TimeBlock.Text = min + ":" + sec;
            }
            else
            {
                TimeBlock.Text = hours + ":" + min + ":" + sec;
            }
        }

        private bool fullScreen = false;
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (!fullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                fullScreen = true;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                fullScreen = false;
            }
        }

        private bool isVolumeClicked = false;
        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if (!isVolumeClicked)
            {
                Mediaplayer.Volume = 0;
                volumeImage.Visibility = Visibility.Hidden;
                Volume.Background = new ImageBrush(new BitmapImage(new Uri("../../volume1.png", UriKind.RelativeOrAbsolute)));
                isVolumeClicked = true;
            }
            else
            {
                Mediaplayer.Volume = 0.5;
                Volume.Background = null;
                volumeImage.Visibility = Visibility.Visible;
                isVolumeClicked = false;
            }

        }

        bool isEnd = false;
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            isEnd = true;
            Repeat.Visibility = Visibility.Visible;
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            TimeBlock.Text = "00:00";
            Mediaplayer.Position = TimeSpan.Zero;
            Mediaplayer.Play(); dispatcherTimer.Start();
            Repeat.Visibility = Visibility.Hidden;
        }

        private void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && fullScreen == true)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                fullScreen = false;
            }
        }

        private void Window1_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.None;
            
            var da = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            var collection = Grid1.Children.OfType<Button>();
            foreach (var el in collection)
            {
                el.BeginAnimation(OpacityProperty, da);
                el.IsEnabled = false;
            }
            Slider.BeginAnimation(OpacityProperty, da);
            ProgressSlider.BeginAnimation(OpacityProperty, da);
            TimeBlock.BeginAnimation(OpacityProperty, da);
            expander.BeginAnimation(OpacityProperty, da);
        }

        private void Window1_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            var da = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            var collection = Grid1.Children.OfType<Button>();
            foreach (var el in collection)
            {
                el.BeginAnimation(OpacityProperty, da);
                el.IsEnabled = true;
            }
            Slider.BeginAnimation(OpacityProperty, da);
            TimeBlock.BeginAnimation(OpacityProperty, da);
            expander.BeginAnimation(OpacityProperty, da);
            ProgressSlider.BeginAnimation(OpacityProperty, da);
        }
      
        private void listView1_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(((Item)listView1.SelectedItem).Title))
            {
                Mediaplayer.Source = new Uri(((Item)listView1.SelectedItem).Title);
                expander.IsExpanded = false;
                Play_Click(null, null);
            }
            else
            {
                listView1.Items.Remove(listView1.SelectedItem);
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = true };
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            openFileDialog.Filter = "Video Files (*.mp4;*.avi)|*.mp4;*.avi|All files(*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Count(); i++)
                    ((ListView)expander.Content).Items.Add(new Item() { Title = openFileDialog.FileNames[i], Path = openFileDialog.SafeFileNames[i], });
                expander.IsExpanded = true;
                Mediaplayer.Source = new Uri(((Item)listView1.Items[0]).Title);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Mediaplayer.Volume = Slider.Value;
            if (Mediaplayer.Volume == 0)
            {
                volumeImage.Visibility = Visibility.Hidden;
                Volume.Background = new ImageBrush(new BitmapImage(new Uri("../../volume1.png", UriKind.RelativeOrAbsolute)));
            }
            else
            {
                Volume.Background = null;
                volumeImage.Visibility = Visibility.Visible;
            }
        }
        private void remove_Click(object sender, RoutedEventArgs e)
        {
            if (listView1.Items.Count == 0)
                return;
            if (listView1.SelectedIndex != -1)
                listView1.Items.Remove(listView1.SelectedItem);
            else { listView1.Items.RemoveAt(0); }
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Mediaplayer.Volume = Slider.Value;
        }

        private void Mediaplayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            ProgressSlider.Maximum = (int)Mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
            ProgressSlider.Value = 0;
            hideUItimer.Start();
        }

        private void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (listView1.Items.IsEmpty)
                return;
            using (FileStream fs = new FileStream("filmsLib.dat", FileMode.OpenOrCreate))
            {
                List<Item> films = new List<Item>();
                foreach (var el in listView1.Items)
                {
                    films.Add((Item)el);
                }

                formatter.Serialize(fs, films);
            }
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream("filmsLib.dat", FileMode.OpenOrCreate))
            {
                if (fs.Length == 0)
                    return;
                List<Item> films = (List<Item>)formatter.Deserialize(fs);
                foreach (var el in films)
                {
                    listView1.Items.Add(el);
                }
            }
        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Mediaplayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           // Mediaplayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }

        private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mediaplayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            if(isEnd)
            {
                dispatcherTimer.Start();
                Repeat.Visibility = Visibility.Hidden;
            }
        }
    }
}

