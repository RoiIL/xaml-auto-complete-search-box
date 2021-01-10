using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using CoordinateSharp;
using Newtonsoft.Json;

namespace AutoCompleteSearchBoxWithOption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitialzeLocationsData();
            OnViewInitialze();
        }

        private List<GeocodingData> geocodingData = new List<GeocodingData>();
        private string coordinateDisplay = string.Empty;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;

                Coordinate coordinateParser;

                if (string.IsNullOrEmpty(SearchText))
                {
                    HasSuggestions = false;
                    IsSearchEnabled = false;
                }
                else if (Coordinate.TryParse(searchText, out coordinateParser))
                {
                    GeocodingData geocodingLocation = new GeocodingData();
                    geocodingLocation.display_name = coordinateParser.ToString();
                    geocodingLocation.CoordinatesData.lat = "32.222";
                    geocodingLocation.CoordinatesData.lon = "35.532";

                    QueryText = new List<GeocodingData> { geocodingLocation };
                    HasSuggestions = true;
                    IsSearchEnabled = true;
                    coordinateDisplay = geocodingLocation.display_name;
                }
                else if (geocodingData != null)
                {
                    QueryText = geocodingData.Where(x => x.display_name.ToUpper().Replace(",", " ").Replace(" ", "").Trim().Contains(SearchText.ToUpper().Replace(",", " ").Replace(" ", "").Trim())).Take(5).ToList();
                    HasSuggestions = suggestions.Any(); 
                }
                else
                {
                    GetLocationGeocode(SearchText);
                }

                OnPropertyChanged("SearchText");
            }
        }

        private GeocodingData selectedSearch;

        public GeocodingData SelectedSearch
        {
            get { return selectedSearch; }
            set
            {
                selectedSearch = value;
                if (value != null && !string.IsNullOrEmpty(selectedSearch.display_name))
                {
                    searchText = selectedSearch.display_name;
                    IsSearchEnabled = true;
                }
                else
                {
                    IsSearchEnabled = false;
                }

                OnPropertyChanged("SelectedSearch");
                OnPropertyChanged("SearchText");
            }
        }

        private bool isSearchEnabled;

        public bool IsSearchEnabled
        {
            get { return isSearchEnabled; }
            set
            {
                isSearchEnabled = value;
                OnPropertyChanged("IsSearchEnabled");
            }
        }

        private bool hasSuggestions;

        public bool HasSuggestions
        {
            get { return hasSuggestions; }
            set
            {
                hasSuggestions = value;
                OnPropertyChanged("HasSuggestions");
            }
        }

        private List<GeocodingData> suggestions;
        public List<GeocodingData> QueryText
        {
            get
            { return suggestions; }
            set
            {
                suggestions = value;
                OnPropertyChanged("QueryText");
            }
        }

        private ICommand goToLocationCommand;
        public ICommand GoToLocationCommand
        {
            get { return goToLocationCommand; }
            set
            {
                goToLocationCommand = value;
                OnPropertyChanged("GoToLocationCommand");
            }
        }

        private static ICommand closeView;
        public ICommand CloseViewCommand
        {
            get { return closeView; }
            set
            {
                closeView = value;
                OnPropertyChanged("CloseViewCommand");
            }
        }

        private ICommand keyDown;
        public ICommand KeyDownCommand
        {
            get { return keyDown; }
            set
            {
                keyDown = value;
                OnPropertyChanged("KeyDownCommand");
            }
        }

        private static ICommand escapeCommand;
        public ICommand EscapeCommand
        {
            get { return escapeCommand; }
            set
            {
                escapeCommand = value;
                OnPropertyChanged("EscapeCommand");
            }
        }

        private static ICommand hideSuggestions;

        public ICommand HideSuggestionsCommand
        {
            get { return hideSuggestions; }
            set
            {
                hideSuggestions = value;
                OnPropertyChanged("HideSuggestionsCommand");
            }
        }

        private static ICommand selectionCommand;

        public ICommand SelectionCommand
        {
            get { return selectionCommand; }
            set
            {
                selectionCommand = value;
                OnPropertyChanged("SelectionCommand");
            }
        }

        private void InitialzeLocationsData()
        {
            string geocodingJson;
            using (StreamReader streamReader = new StreamReader(@"../../IsraelGeocode.json", Encoding.UTF8))
            {
                geocodingJson = streamReader.ReadToEnd();
            }
            geocodingData = JsonConvert.DeserializeObject<List<GeocodingData>>(geocodingJson);
        }

        private void OnViewInitialze()
        {
            GoToLocationCommand = new DelegateCommand(ExecuteGoToLocation);
            CloseViewCommand = new DelegateCommand(ExecuteCloseView);
            KeyDownCommand = new DelegateCommand(ExecuteKeyDown);
            EscapeCommand = new DelegateCommand(ExecuteDeleteSearchText);
            HideSuggestionsCommand = new DelegateCommand(ExecuteHideSuggestionsCommand);
            SelectionCommand = new DelegateCommand(ExecuteSelectionCommand);
        }

        public void ExecuteGoToLocation()
        {
            if (!string.IsNullOrEmpty(coordinateDisplay))
            {
                SearchText = coordinateDisplay;
                coordinateDisplay = string.Empty;
            }

            HasSuggestions = false;
        }

        public void ExecuteCloseView()
        {
            this.Close();
        }

        public void ExecuteKeyDown()
        {
            if (HasSuggestions)
            {
                this.LocationListBox.Focus();
                LocationListBox.SelectedItem = LocationListBox.Items[0];
                var listBoxItem = (ListBoxItem)LocationListBox.ItemContainerGenerator.ContainerFromItem(LocationListBox.SelectedItem);
                listBoxItem.Focus();
            }
        }

        public void ExecuteDeleteSearchText()
        {
            SearchText = string.Empty;
            HasSuggestions = false;
        }

        public void ExecuteHideSuggestionsCommand()
        {
            SearchTextBox.Focus();
            SearchTextBox.CaretIndex = SearchText.Length;
            HasSuggestions = false;
        }

        public void ExecuteSelectionCommand()
        {
            if (LocationListBox.SelectedItem != null)
            {
                SelectedSearch = LocationListBox.SelectedItem as GeocodingData;
                ExecuteGoToLocation();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        //         private string GetLocationGeocode(string streetName)
        //         {
        //             try
        //             {
        //                 using (var client = new HttpClient())
        //                 {
        //                     string uri = "http://192.168.99.100:7070/search?&q=" +
        //                                  streetName + "&format=json";
        //                     var request = new HttpRequestMessage
        //                     {
        //                         RequestUri = new Uri(uri),
        //                         Method = HttpMethod.Get,
        //                     };
        //                     // request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        // 
        //                     var result = client.SendAsync(request).Result;
        //                     result.EnsureSuccessStatusCode();
        // 
        //                     var responseBody = result.Content.ReadAsStringAsync().ConfigureAwait(false);
        // 
        //                     return responseBody.GetAwaiter().GetResult();
        //                 }
        //             }
        //             catch (Exception e)
        //             {
        //                 return null;
        //             }
        //         }

        private string GetLocationGeocode(string locationName)
        {
            try
            {
                string responseString = string.Empty;
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ResponseFromOsmContainerCallback);
                    client.DownloadStringAsync(new Uri("http://192.168.99.100:7070/search?&q=" +
                                                           locationName + "&format=json"));
                }

                return responseString;
            }
            catch (Exception ex)
            {
                
                return string.Empty;
            }
        }

        private void ResponseFromOsmContainerCallback(object obj, DownloadStringCompletedEventArgs args)
        {
            try
            {
                bool test =!string.IsNullOrEmpty(args.Result);
                string locationResults = args.Result;
                locationResults = locationResults.Replace("no, ", "");
                QueryText = JsonConvert.DeserializeObject<List<GeocodingData>>(locationResults);
                if (suggestions != null)
                {
                    HasSuggestions = suggestions.Any();
                }
            }
            catch (Exception e)
            {
                string result = e.InnerException.Message;
            }
        }

        private void LocationListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key == Key.Space))
            {
                SearchTextBox.Focus();
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                SearchTextBox.Focus();
                SearchText = SearchText.Remove(SearchText.Length - 1);
                SearchTextBox.CaretIndex = SearchText.Length;
            }
        }

        private void LocationListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                SelectedSearch = item.Content as GeocodingData;
                ExecuteGoToLocation();
            }
        }
    }

    public class Coordinates
    {
        public string lat;
        public string lon;
    }

    public class GeocodingData
    {
        public GeocodingData()
        {
            CoordinatesData = new AutoCompleteSearchBoxWithOption.Coordinates();
        }
        public string display_name { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public Coordinates CoordinatesData;
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action executeMethod;
        public DelegateCommand(Action executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            executeMethod();
        }

        public event EventHandler CanExecuteChanged;
    }
}
