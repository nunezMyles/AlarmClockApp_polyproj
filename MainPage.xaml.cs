using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Project2_203149T
{
    public class Alarm      //  alarm object
    {
        public bool isOn { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public List<int> Days { get; set; }
        public List<string> scheduleId { get; set; }
    }

    //  Weather:        loadWeather(), reloadWeather()
    //  Alarm:          AddAlarm(), ReloadSelectedAlarm(), ReloadAllAlarm(), AddAlarmScheduleByDate(), AddAlarmScheduleByDay()
    //  News:           loadNews()
    //  Translator:     loadTranslator()
    //  Bus:            loadBus(), loadIconColor(), computeArrival()

    public sealed partial class MainPage : Page
    {
        Uri busIconUri;
        int selectedSetting = 0;
        int specialAlarmId = 0;
        bool isInteractionReady = false;
        DispatcherTimer Timer = new DispatcherTimer();

        List<Alarm> myAlarms = new List<Alarm>();
        Alarm trophyAlarm1, trophyAlarm2, trophyAlarm3;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            isInteractionReady = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0];
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(attribute.Title);
            Clipboard.SetContent(dataPackage);

            // Specification 1
            ApplicationView.PreferredLaunchViewSize = new Size(800, 480);
            ApplicationView.PreferredLaunchWindowingMode =
            ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // Specification 2
            ApplicationViewTitleBar titleBar =
            ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar =
            CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            tbDate.Text = DateTime.Now.ToString("ddd, dd MMM, yyyy");
            tbWeatherPgDate.Text = DateTime.Now.ToString("ddd, dd MMMM");

            cbGreetTextLanguage.Items.Add("English");
            cbGreetTextLanguage.Items.Add("Indonesian");
            cbGreetTextLanguage.Items.Add("Simplified Chinese, 中文 (简体)");
            cbGreetTextLanguage.Items.Add("Traditional Chinese, 繁體中文 (繁體)");
            cbGreetTextLanguage.Items.Add("Vietnamese, Tiếng Việt");
            cbGreetTextLanguage.Items.Add("Malay, Melayu");
            cbGreetTextLanguage.Items.Add("Thai, ไทย");
            cbGreetTextLanguage.Items.Add("Korean, 한국어");
            cbGreetTextLanguage.Items.Add("Japanese, 日本語");
            cbGreetTextLanguage.Items.Add("Hindi, हिन्दी");
            cbGreetTextLanguage.Items.Add("Tamil, தமிழ்");
            cbGreetTextLanguage.Items.Add("Myanmar (Burmese), မြန်မာ");

            cbGreetTextLanguage.SelectedIndex = 0;

            cbTranslatorTargetLanguage.Items.Add("English");
            cbTranslatorTargetLanguage.Items.Add("Indonesian");
            cbTranslatorTargetLanguage.Items.Add("Simplified Chinese, 中文 (简体)");
            cbTranslatorTargetLanguage.Items.Add("Traditional Chinese, 繁體中文 (繁體)");
            cbTranslatorTargetLanguage.Items.Add("Vietnamese, Tiếng Việt");
            cbTranslatorTargetLanguage.Items.Add("Malay, Melayu");
            cbTranslatorTargetLanguage.Items.Add("Thai, ไทย");
            cbTranslatorTargetLanguage.Items.Add("Korean, 한국어");
            cbTranslatorTargetLanguage.Items.Add("Japanese, 日本語");
            cbTranslatorTargetLanguage.Items.Add("Hindi, हिन्दी");
            cbTranslatorTargetLanguage.Items.Add("Tamil, தமிழ்");
            cbTranslatorTargetLanguage.Items.Add("Myanmar (Burmese), မြန်မာ");

            cbTranslatorTargetLanguage.SelectedIndex = 1;

            busIconUri = new Uri(this.BaseUri, "../images/icon_bus2.png");

            //tbAlarm1HomeName.Text = "-";
            //tbAlarm2HomeName.Text = "-";
            //tbAlarm3HomeName.Text = "-";

            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

            loadWeather();
            loadNews();

            //  Clear all scheduled toast notifications upon page loaded for demo purposes
            IReadOnlyList<ScheduledToastNotification> scheduledAlarms = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();

            foreach (ScheduledToastNotification alarm in scheduledAlarms)
            {
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(alarm);
            }

            IReadOnlyList<ScheduledToastNotification> scheduledAlarms2 = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            int x = 0;
            foreach (ScheduledToastNotification alarm in scheduledAlarms2)
            {
                x++;
            }
            tbNotificationQty.Text = x.ToString();
            
        }

        private void Timer_Tick(object sender, object e)
        {
            tbTime.Text = DateTime.Now.ToString("h:mm:ss");
            tbTime2.Text = DateTime.Now.ToString(" tt");

            IReadOnlyList<ScheduledToastNotification> scheduledAlarms = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            int x = 0;
            foreach (ScheduledToastNotification alarm in scheduledAlarms)
            {
                x++;
            }
            tbNotificationQty.Text = x.ToString();
        }

        private void AddAlarmScheduleByDay(Alarm alarmWithDays)     
        {
            int daysListIndex = 0;
            DateTime currentDay = DateTime.Today;
            List<DateTime> datesOfDays = new List<DateTime>();

            //  convert selected days into dates and put inside datesOfDays list
            foreach (int isDaySelected in alarmWithDays.Days)
            {
                if (isDaySelected == 0)     //  0-on, 1-off, oops lol
                {
                    int daysTillSelectedDay = 0;

                    switch (daysListIndex)
                    {
                        case 0:
                            daysTillSelectedDay = 1 - (int)currentDay.DayOfWeek;    //  mon = 1
                            break;
                        case 1:
                            daysTillSelectedDay = 2 - (int)currentDay.DayOfWeek;
                            break;
                        case 2:
                            daysTillSelectedDay = 3 - (int)currentDay.DayOfWeek;
                            break;
                        case 3:
                            daysTillSelectedDay = 4 - (int)currentDay.DayOfWeek;
                            break;
                        case 4:
                            daysTillSelectedDay = 5 - (int)currentDay.DayOfWeek;
                            break;
                        case 5:
                            daysTillSelectedDay = 6 - (int)currentDay.DayOfWeek;  //  sat = 6
                            break;
                        case 6:
                            daysTillSelectedDay = 7 - (int)currentDay.DayOfWeek;    //  sun = 0
                            break;
                        default:
                            break;
                    }
                    if (daysTillSelectedDay >= 0)   //  only add notifications if selected days are ahead of today this week
                    {
                        var dayIntoList = currentDay.AddDays(daysTillSelectedDay);
                        datesOfDays.Add(dayIntoList);
                    }
                }
                daysListIndex++;
            }
            
            //  once days are converted to dates, schedule each date in final date list
            foreach (DateTime scheduledDate in datesOfDays)
            {
                DateTime scheduledDateTime = scheduledDate + DateTime.Parse(alarmWithDays.Time).TimeOfDay;  // only if scheduled datetime is after now
                if (DateTime.Now < scheduledDateTime) AddAlarmScheduleByDate(alarmWithDays, scheduledDateTime);
            }
        }

        private void AddAlarmScheduleByDate(Alarm myAlarm, DateTime alarmTimeDay)   //  pls enable notifications in windows settings
        {
            string greetingText;
            string alarmName = myAlarm.Name;
            string alarmTimeStr = alarmTimeDay.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            string alarmTimeStr2 = alarmTimeDay.ToString("HH:mm", CultureInfo.InvariantCulture);

            if (TimeSpan.Parse(alarmTimeStr2) < TimeSpan.FromHours(12))
            {
                greetingText = "Good morning";      //  bef 12pm
            }
            else if (TimeSpan.Parse(alarmTimeStr2) > TimeSpan.FromHours(12) && TimeSpan.Parse(alarmTimeStr2) < TimeSpan.FromHours(17))
            {
                greetingText = "Good afternoon";    //  aft 12pm & bef 5pm
            }
            else if (TimeSpan.Parse(alarmTimeStr2) > TimeSpan.FromHours(17) && TimeSpan.Parse(alarmTimeStr2) < TimeSpan.FromHours(22))
            {
                greetingText = "Good evening";      //  aft 5pm & bef 10pm
            }
            else
            {
                greetingText = "Good night";        //  aft 10pm
            }

            string toastXmlString = "<toast>"
                                  +     "<visual>"
                                  +         "<binding template='ToastText04'>"
                                  +             "<text id='1'>" + alarmName + "</text>"
                                  +             "<text id='2'>" + greetingText + "</text>"
                                  +             "<text id='3'>" + alarmTimeStr + "</text>"
                                  +         "</binding>"
                                  +     "</visual>"
                                  +     "<commands scenario='alarm'>"
                                  +         "<command id='snooze' />"
                                  +         "<command id='dismiss' />"
                                  +     "</commands>"
                                  +     "<audio loop='true' />"
                                  + "</toast>";

            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml(toastXmlString);

            var offset = new DateTimeOffset(alarmTimeDay);  //  get scheduled alarm time

            myAlarm.scheduleId.Add("IdTostone" + specialAlarmId.ToString());

            //  must have minimum 1min snooze duration
            ScheduledToastNotification toast = new ScheduledToastNotification(toastDOM, offset, TimeSpan.FromMinutes(1), 1)
            {
                Id = "IdTostone" + specialAlarmId.ToString(),
                Tag = "NotificationOne",
                Group = "AlarmToasts",
                ExpirationTime = new DateTimeOffset(DateTime.Now + TimeSpan.FromMinutes(5))
            };

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);

            IReadOnlyList<ScheduledToastNotification> scheduledAlarms = 
                ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            int x = 0;
            foreach (ScheduledToastNotification alarm in scheduledAlarms)
            {
                x++;
            }
            tbNotificationQty.Text = x.ToString();

            specialAlarmId++;
        }

        private void reloadWeather(int daySelect)
        {
            switch(daySelect)
            {
                case 1:
                    tbWeatherPgDate.Text = DateTime.ParseExact(myWeather.forecast.forecastday[0].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        .ToString("ddd, dd MMMM", CultureInfo.InvariantCulture);
                    bmiCurrentWeatherIcon.UriSource = new Uri("https:" + myWeather.current.condition.icon, UriKind.Absolute);
                    tbCurrentWeatherTemp.Text = Convert.ToInt32(myWeather.current.temp_c).ToString() + "°";
                    tbCurrentConditionText.Text = myWeather.current.condition.text;
                    tbWeatherPrecipitation.Text = "Precipitation: " + myWeather.forecast.forecastday[0].day.daily_chance_of_rain.ToString() + "%";
                    tbWeatherHumidity.Text = "Humidity: " + myWeather.current.humidity.ToString() + "%";
                    tbWeatherWind.Text = "Wind: " + Convert.ToInt32(myWeather.current.wind_kph).ToString() + "km/h";

                    tbWeatherDay1.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    bmiWeatherDay1Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay1MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay1MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));

                    tbWeatherDay2.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay2Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay2MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay2MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    tbWeatherDay3.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay3Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay3MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay3MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    break;
                    
                case 2:
                    tbWeatherPgDate.Text = DateTime.ParseExact(myWeather.forecast.forecastday[1].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        .ToString("ddd, dd MMMM", CultureInfo.InvariantCulture);
                    bmiCurrentWeatherIcon.UriSource = new Uri("https:" + myWeather.forecast.forecastday[1].day.condition.icon, UriKind.Absolute);
                    tbCurrentWeatherTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[1].day.maxtemp_c).ToString() + "°";
                    tbCurrentConditionText.Text = myWeather.forecast.forecastday[1].day.condition.text;
                    tbWeatherPrecipitation.Text = "Precipitation: " + myWeather.forecast.forecastday[1].day.daily_chance_of_rain.ToString() + "%";
                    tbWeatherHumidity.Text = "Humidity: " + Convert.ToInt32(myWeather.forecast.forecastday[1].day.avghumidity).ToString() + "%";
                    tbWeatherWind.Text = "Wind: " + Convert.ToInt32(myWeather.forecast.forecastday[1].day.maxwind_kph).ToString() + "km/h";

                    tbWeatherDay1.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay1Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay1MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay1MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    tbWeatherDay2.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    bmiWeatherDay2Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay2MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay2MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));

                    tbWeatherDay3.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay3Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay3MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay3MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    break;

                case 3:
                    tbWeatherPgDate.Text = DateTime.ParseExact(myWeather.forecast.forecastday[2].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        .ToString("ddd, dd MMMM", CultureInfo.InvariantCulture);
                    bmiCurrentWeatherIcon.UriSource = new Uri("https:" + myWeather.forecast.forecastday[2].day.condition.icon, UriKind.Absolute);
                    tbCurrentWeatherTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[2].day.maxtemp_c).ToString() + "°";
                    tbCurrentConditionText.Text = myWeather.forecast.forecastday[2].day.condition.text;
                    tbWeatherPrecipitation.Text = "Precipitation: " + myWeather.forecast.forecastday[2].day.daily_chance_of_rain.ToString() + "%";
                    tbWeatherHumidity.Text = "Humidity: " + Convert.ToInt32(myWeather.forecast.forecastday[2].day.avghumidity).ToString() + "%";
                    tbWeatherWind.Text = "Wind: " + Convert.ToInt32(myWeather.forecast.forecastday[2].day.maxwind_kph).ToString() + "km/h";


                    tbWeatherDay1.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay1Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay1MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay1MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    tbWeatherDay2.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    bmiWeatherDay2Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay2MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                    tbWeatherDay2MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                    tbWeatherDay3.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    bmiWeatherDay3Icon.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay3MaxTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                    tbWeatherDay3MinTemp.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));

                    break;

                default:
                    break;
            }
        }

        WeatherRoot myWeather;
        private async void loadWeather()
        {
            var position = await LocationManager.GetPosition();
            myWeather = await WeatherApiProxy.GetWeather(position.Coordinate.Latitude, position.Coordinate.Longitude);

            //  home page
            
            bmiHomeWeatherIconDay1.UriSource = new Uri("https:" + myWeather.current.condition.icon, UriKind.Absolute);
            bmiHomeWeatherIconDay2.UriSource = new Uri("https:" + myWeather.forecast.forecastday[1].day.condition.icon, UriKind.Absolute);
            bmiHomeWeatherIconDay3.UriSource = new Uri("https:" + myWeather.forecast.forecastday[2].day.condition.icon, UriKind.Absolute);

            tbHomeWeatherIconDay2.Text = DateTime.ParseExact(myWeather.forecast.forecastday[1].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .ToString("ddd", CultureInfo.InvariantCulture);
            tbHomeWeatherIconDay3.Text = DateTime.ParseExact(myWeather.forecast.forecastday[2].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .ToString("ddd", CultureInfo.InvariantCulture);

            tbHomeCurrentTemp.Text = Convert.ToInt32(myWeather.current.temp_c).ToString() + "°";
            tbHomeCurrentHumidity.Text = "Humidity: " + myWeather.current.humidity.ToString() + "%";
            tbHomeCurrentWind.Text = "Wind: " + Convert.ToInt32(myWeather.current.wind_kph).ToString() + "km/h";
            

            //  weather page

            tbWeatherLocation.Text = myWeather.location.name + ", " + myWeather.location.country;
            bmiCurrentWeatherIcon.UriSource = new Uri("https:" + myWeather.current.condition.icon, UriKind.Absolute);
            tbCurrentWeatherTemp.Text = Convert.ToInt32(myWeather.current.temp_c).ToString() + "°";
            tbCurrentConditionText.Text = myWeather.current.condition.text;
            tbWeatherPrecipitation.Text = "Precipitation: " + myWeather.forecast.forecastday[0].day.daily_chance_of_rain.ToString() + "%";
            tbWeatherHumidity.Text = "Humidity: " + myWeather.current.humidity.ToString() + "%";
            tbWeatherWind.Text = "Wind: " + Convert.ToInt32(myWeather.current.wind_kph).ToString() + "km/h";

            tbWeatherDay2.Text = DateTime.ParseExact(myWeather.forecast.forecastday[1].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .ToString("ddd", CultureInfo.InvariantCulture);
            tbWeatherDay3.Text = DateTime.ParseExact(myWeather.forecast.forecastday[2].date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                .ToString("ddd", CultureInfo.InvariantCulture);

            bmiWeatherDay1Icon.UriSource = new Uri("https:" + myWeather.current.condition.icon, UriKind.Absolute);
            bmiWeatherDay2Icon.UriSource = new Uri("https:" + myWeather.forecast.forecastday[1].day.condition.icon, UriKind.Absolute);
            bmiWeatherDay3Icon.UriSource = new Uri("https:" + myWeather.forecast.forecastday[2].day.condition.icon, UriKind.Absolute);

            tbWeatherDay1MaxTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[0].day.maxtemp_c).ToString() + "°";
            tbWeatherDay2MaxTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[1].day.maxtemp_c).ToString() + "°";
            tbWeatherDay3MaxTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[2].day.maxtemp_c).ToString() + "°";

            tbWeatherDay1MinTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[0].day.mintemp_c).ToString() + "°";
            tbWeatherDay2MinTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[1].day.mintemp_c).ToString() + "°";
            tbWeatherDay3MinTemp.Text = Convert.ToInt32(myWeather.forecast.forecastday[2].day.mintemp_c).ToString() + "°";
        }

        private async void loadTranslator(string input)
        {
            TranslateRoot myTranslatedText;

            if (cbTranslatorTargetLanguage.SelectedItem != null)
            {
                switch (cbTranslatorTargetLanguage.SelectedItem.ToString())
                {
                    case "English":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "en");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Indonesian":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "id");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Simplified Chinese, 中文 (简体)":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "zh-Hans");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Traditional Chinese, 繁體中文 (繁體)":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "zh-Hant");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Vietnamese, Tiếng Việt":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "vi");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Thai, ไทย":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "th");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Malay, Melayu":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ms");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Korean, 한국어":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ko");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Japanese, 日本語":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ja");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Hindi, हिन्दी":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "hi");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Tamil, தமிழ்":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ta");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Myanmar (Burmese), မြန်မာ":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "my");
                        tbTranslateOutput.Text = myTranslatedText.translations[0].text;
                        break;

                    default:
                        break;
                }
            }
        }

        private async void loadNews()
        {
            NewsRoot myNews = await NewsApiProxy.GetNews();


            if (myNews.status == "error")
            {
                tbNewsLoadError.Text = myNews.code + ":\n\n" + myNews.message;
                tbNewsLoadError.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (var article in myNews.articles)
                {
                    if (article.title == null || article.title == "") article.title = "No title available";
                    if (article.description == null || article.description == "") article.description = "No description available";
                    if (article.urlToImage == null) article.urlToImage = "";
                    if (article.source.name == null || article.source.name == "") article.source.name = "-";

                    Grid grid = new Grid
                    {
                        Width = 370,
                        Height = 280,
                        //BorderBrush = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144)),
                        BorderThickness = new Thickness(1.5),
                        CornerRadius = new CornerRadius(15),
                        Margin = new Thickness(5)
                    };

                    RowDefinition row0 = new RowDefinition
                    {
                        Height = new GridLength(75, GridUnitType.Pixel)
                    };
                    grid.RowDefinitions.Add(row0);

                    RowDefinition row1 = new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    };
                    grid.RowDefinitions.Add(row1);

                    RowDefinition row2 = new RowDefinition
                    {
                        Height = new GridLength(30, GridUnitType.Pixel)
                    };
                    grid.RowDefinitions.Add(row2);

                    Border bdr0 = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 29, 29, 31)),
                        BorderThickness = new Thickness(0, 2, 0, 2),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144)),
                    };

                    TextBlock tbNews0 = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.GhostWhite),
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 350,
                        Text = article.title,
                        FontSize = 14,
                        Margin = new Thickness(7, 7, 7, 7)
                    };

                    bdr0.Child = tbNews0;
                    Grid.SetRow(bdr0, 0);
                    grid.Children.Add(bdr0);

                    Border bdr2 = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 29, 29, 31))
                    };

                    StackPanel sp2 = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    TextBlock tbNews2 = new TextBlock
                    {
                        Opacity = 0.8,
                        Foreground = new SolidColorBrush(Colors.White),
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 190,
                        FontSize = 13,
                        Text = article.description,
                        Margin = new Thickness(10, 5, 2, 2)
                    };
                    sp2.Children.Add(tbNews2);

                    if (article.urlToImage != "")
                    {
                        try
                        {
                            Image img2 = new Image
                            {
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(10, 10, 10, 0),
                                Width = 150,
                                Height = 130,
                                Source = new BitmapImage(new Uri(string.Format(article.urlToImage), UriKind.Absolute)),
                            };
                            sp2.Children.Add(img2);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }

                    bdr2.Child = sp2;
                    Grid.SetRow(bdr2, 1);
                    grid.Children.Add(bdr2);

                    Border bdr1 = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 29, 29, 31)),
                    };

                    TextBlock tbNews1 = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79)),
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 350,
                        Text = article.source.name,
                        FontSize = 16,
                        Margin = new Thickness(10, 2, 2, 2)
                    };

                    bdr1.Child = tbNews1;
                    Grid.SetRow(bdr1, 2);
                    grid.Children.Add(bdr1);

                    gvNews.Items.Add(grid);
                }
            }
            
        }

        string computeArrival(string estimatedArrivalString)
        {
            //  300 IQ MOMENT, since JsonSerializer only allows 1 datetime format setting, we juz define estimatedArrival as a
            //  string in order to check for 2 datetime format setting easily with no errors EZPZ
            if (estimatedArrivalString == "") return "-";

            DateTime estimatedArrivalDt = Convert.ToDateTime(estimatedArrivalString);
            TimeSpan difference = estimatedArrivalDt - DateTime.Now;
            int differenceInt = Convert.ToInt32(difference.TotalMinutes);
            if (differenceInt < 1) return "Arriving";
            return differenceInt.ToString() + "m";
        }
        Color loadIconColor(string load)
        {
            if (load == "SEA") return Colors.LimeGreen;
            if (load == "SDA") return Colors.Orange;
            if (load == "LSD") return Colors.OrangeRed;
            return Colors.Transparent;
        }

        private async void loadBus()
        {
            BusRoot myBus = await BusArrivalProxy.GetBusArrival(tbBusCodeInput.Text);

            foreach (var bus in myBus.Services)
            {
                StackPanel spBus = new StackPanel
                {
                    Height = 50,
                    Orientation = Orientation.Horizontal
                };

                TextBlock tbServiceNo = new TextBlock
                {
                    Width = 100,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 24,
                    Text = bus.ServiceNo,
                    Foreground = new SolidColorBrush(Colors.White),
                };

                BitmapIcon bmiNextBusLoad = new BitmapIcon
                {
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus.Load)),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(80, 0, 0, 0),
                    UriSource = busIconUri
                };

                TextBlock tbNextBusTime = new TextBlock
                {
                    Width = 100,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 17,
                    Text = computeArrival(bus.NextBus.EstimatedArrival),
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus.Load)),
                };

                BitmapIcon bmiNextBus2Load = new BitmapIcon
                {
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus2.Load)),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(20, 0, 0, 0),
                    UriSource = busIconUri
                };

                TextBlock tbNextBus2Time = new TextBlock
                {
                    Width = 100,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 17,
                    Text = computeArrival(bus.NextBus2.EstimatedArrival),
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus2.Load)),
                };

                BitmapIcon bmiNextBus3Load = new BitmapIcon
                {
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus3.Load)),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(20, 0, 0, 0),
                    UriSource = busIconUri
                };

                TextBlock tbNextBus3Time = new TextBlock
                {
                    Width = 100,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 17,
                    Text = computeArrival(bus.NextBus3.EstimatedArrival),
                    Foreground = new SolidColorBrush(loadIconColor(bus.NextBus3.Load)),
                };

                spBus.Children.Add(tbServiceNo);
                spBus.Children.Add(bmiNextBusLoad);
                spBus.Children.Add(tbNextBusTime);
                spBus.Children.Add(bmiNextBus2Load);
                spBus.Children.Add(tbNextBus2Time);
                spBus.Children.Add(bmiNextBus3Load);
                spBus.Children.Add(tbNextBus3Time);

                lvBus.Items.Add(spBus);
            }
        }

        private void abbSettings_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex > 0) fvHoriz.SelectedIndex--;
        }

        private void abbNews_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 1) fvHoriz.SelectedIndex++;
            while (fvHoriz.SelectedIndex > 1) fvHoriz.SelectedIndex--;
        }

        private void abbWeather_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 2) fvHoriz.SelectedIndex++;
            while (fvHoriz.SelectedIndex > 2) fvHoriz.SelectedIndex--;
        }

        private void abbClock_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 3) fvHoriz.SelectedIndex++;
            while (fvHoriz.SelectedIndex > 3) fvHoriz.SelectedIndex--;
        }

        private void abbAlarm_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 4) fvHoriz.SelectedIndex++;
            while (fvHoriz.SelectedIndex > 4) fvHoriz.SelectedIndex--;
        }

        private void abbTranslator_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 5) fvHoriz.SelectedIndex++;
            while (fvHoriz.SelectedIndex > 5) fvHoriz.SelectedIndex--;
        }

        private void abbBus_Click(object sender, RoutedEventArgs e)
        {
            while (fvHoriz.SelectedIndex < 6) fvHoriz.SelectedIndex++;
        }

        private void fvHoriz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInteractionReady)
            {
                switch (fvHoriz.SelectedIndex)
                {
                    case 0:
                        abbSettings.Foreground = new SolidColorBrush(Colors.White);
                        abbSettingsIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbNews.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbNewsIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 1:
                        abbSettings.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbSettingsIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbNews.Foreground = new SolidColorBrush(Colors.White);
                        abbNewsIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbWeather.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbWeatherIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 2:
                        abbNews.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbNewsIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbWeather.Foreground = new SolidColorBrush(Colors.White);
                        abbWeatherIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbClock.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbClockIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 3:
                        abbWeather.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbWeatherIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbClock.Foreground = new SolidColorBrush(Colors.White);
                        abbClockIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbAlarm.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbAlarmIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 4:
                        abbClock.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbClockIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbAlarm.Foreground = new SolidColorBrush(Colors.White);
                        abbAlarmIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbTranslator.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbTranslatorIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 5:
                        abbAlarm.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbAlarmIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbTranslator.Foreground = new SolidColorBrush(Colors.White);
                        abbTranslatorIcon.Foreground = new SolidColorBrush(Colors.White);

                        abbBus.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbBusIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        break;

                    case 6:
                        abbTranslator.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                        abbTranslatorIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));

                        abbBus.Foreground = new SolidColorBrush(Colors.White);
                        abbBusIcon.Foreground = new SolidColorBrush(Colors.White);
                        break;

                    default:
                        break;
                }
            }
        }

        int selectMon = 0;
        private void btnSelectMon_Click(object sender, RoutedEventArgs e)
        {
            if (selectMon == 0)
            {
                tbSelectMon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectMon = 1;
            }
            else
            {
                tbSelectMon.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectMon = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectTue = 1;
        private void btnSelectTue_Click(object sender, RoutedEventArgs e)
        {
            if (selectTue == 0)
            {
                tbSelectTue.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectTue = 1;
            }
            else
            {
                tbSelectTue.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectTue = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectWed = 1;
        private void btnSelectWed_Click(object sender, RoutedEventArgs e)
        {
            if (selectWed == 0)
            {
                tbSelectWed.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectWed = 1;
            }
            else
            {
                tbSelectWed.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectWed = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectThu = 0;
        private void btnSelectThu_Click(object sender, RoutedEventArgs e)
        {
            if (selectThu == 0)
            {
                tbSelectThu.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectThu = 1;
            }
            else
            {
                tbSelectThu.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectThu = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectFri = 0;
        private void btnSelectFri_Click(object sender, RoutedEventArgs e)
        {
            if (selectFri == 0)
            {
                tbSelectFri.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectFri = 1;
            }
            else
            {
                tbSelectFri.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectFri = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectSat = 1;
        private void btnSelectSat_Click(object sender, RoutedEventArgs e)
        {
            if (selectSat == 0)
            {
                tbSelectSat.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectSat = 1;
            }
            else
            {
                tbSelectSat.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectSat = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        int selectSun = 1;
        private void btnSelectSun_Click(object sender, RoutedEventArgs e)
        {
            if (selectSun == 0)
            {
                tbSelectSun.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
                selectSun = 1;
            }
            else
            {
                tbSelectSun.Foreground = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
                selectSun = 0;
            }
            tbSetDate.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbRepeatEvery.Foreground = new SolidColorBrush(Colors.White);
            dpSetDate.SelectedDate = null;
        }

        private void DatePicker_GotFocus(object sender, RoutedEventArgs e)
        {
            selectMon = 1;
            selectTue = 1;
            selectWed = 1;
            selectThu = 1;
            selectFri = 1;
            selectSat = 1;
            selectSun = 1;
            tbSetDate.Foreground = new SolidColorBrush(Colors.White);
            tbRepeatEvery.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectMon.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectTue.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectWed.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectThu.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectFri.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectSat.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
            tbSelectSun.Foreground = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
        }

        private void btnAddAlarm_Click(object sender, RoutedEventArgs e)
        {
            gdAddAlarm.Visibility = Visibility.Visible;
            gvAlarm.Opacity = 0.3;
            btnAddAlarm.Opacity = 0.3;
            btnTrashAlarm.Opacity = 0.3;
            spNotificationQty.Opacity = 0.3;
        }

        private void btnSaveAlarm_Click(object sender, RoutedEventArgs e)
        {
            gdAddAlarm.Visibility = Visibility.Collapsed;
            gvAlarm.Opacity = 1;
            btnAddAlarm.Opacity = 1;
            btnTrashAlarm.Opacity = 1;
            spNotificationQty.Opacity = 1;
            
            AddAlarm();

            tpAlarmTime.Time = TimeSpan.Parse("07:00:00");
            tbNameAlarm.Text = "";
        }

        private void btnCancelAddAlarm_Click(object sender, RoutedEventArgs e)
        {
            gdAddAlarm.Visibility = Visibility.Collapsed;
            gvAlarm.Opacity = 1;
            btnAddAlarm.Opacity = 1;
            btnTrashAlarm.Opacity = 1;
            spNotificationQty.Opacity = 1;
        }

        int alarmNameNum = 1;
        bool isDtInputInvalid = false;
        private async void AddAlarm()
        {
            string alarmName, alarmDate, timeAMPM;
            List<int> alarmDays = new List<int>();

            isDtInputInvalid = false;

            GridViewItem gvi0 = new GridViewItem
            {
                Margin = new Thickness(3, 0, 0, 0)
            };

            Border bd0 = new Border
            {
                Margin = new Thickness(10),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 187, 161, 79))
            };

            StackPanel sp0 = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            StackPanel sp1 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            alarmName = tbNameAlarm.Text;
            if (alarmName == "")
            {
                alarmName = "Alarm " + alarmNameNum.ToString();
                alarmNameNum++;
            }

            TextBlock tbAlarmName = new TextBlock
            {
                Text = alarmName,
                Foreground = new SolidColorBrush(Colors.White),
                Width = 120,
                Height = 30,
                TextAlignment = TextAlignment.Left,
                FontSize = 15,
                Margin = new Thickness(10,5,0,0)
            };

            Canvas cv0 = new Canvas
            {
                Width = 50,
                Height = 30
            };

            Button btnDelAlarm = new Button
            {
                Visibility = Visibility.Collapsed,
                Background = new SolidColorBrush(Colors.Transparent),
                RequestedTheme = ElementTheme.Light
            };

            BitmapIcon bmiDelAlarm = new BitmapIcon
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 227, 36, 43)),
                UriSource = new Uri(BaseUri, "../images/trash_icon1.png"),
                Width = 20,
                Height = 20
            };

            ToggleSwitch tsAlarm = new ToggleSwitch
            {
                Width = 50,
                IsOn = true,
                RequestedTheme = ElementTheme.Dark,
            };

            DateTime dt = new DateTime(2022, 06, 07);   //  placeholder date just to convert timespan to datetime format
            dt += tpAlarmTime.Time;
            timeAMPM = dt.ToString("hh:mm tt", CultureInfo.InvariantCulture);

            TextBlock tbAlarmTime = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                Width= 110,
                Height= 40,
                TextAlignment = TextAlignment.Center,
                Text = timeAMPM, 
                FontSize = 25
            };

            sp0.Children.Add(sp1);
            sp0.Children.Add(tbAlarmTime);

            if (dpSetDate.SelectedDate != null)
            {

                alarmDate = dpSetDate.Date.ToString("ddd, dd MMM", CultureInfo.InvariantCulture);

                TextBlock tbAlarmDate = new TextBlock
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = alarmDate, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = 30
                };
                sp0.Children.Add(tbAlarmDate);
            }
            else
            {
                alarmDate = "";

                StackPanel sp2 = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = 30
                };

                TextBlock tbMon = new TextBlock
                {
                    Foreground = btnDayColor(selectMon),
                    Text = "M",
                    FontSize = 15,
                    Margin = new Thickness(0),
                };
                TextBlock tbTue = new TextBlock
                {
                    Foreground = btnDayColor(selectTue),
                    Text = "T",
                    FontSize = 15,
                    Margin = new Thickness(10,0,0,0),
                };
                TextBlock tbWed = new TextBlock
                {
                    Foreground = btnDayColor(selectWed),
                    Text = "W",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbThu = new TextBlock
                {
                    Foreground = btnDayColor(selectThu),
                    Text = "T",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbFri = new TextBlock
                {
                    Foreground = btnDayColor(selectFri),
                    Text = "F",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbSat = new TextBlock
                {
                    Foreground = btnDayColor(selectSat),
                    Text = "S",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbSun = new TextBlock
                {
                    Foreground = btnDayColor(selectSun),
                    Text = "S",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };

                alarmDays.Add(selectMon);
                alarmDays.Add(selectTue);
                alarmDays.Add(selectWed);
                alarmDays.Add(selectThu);
                alarmDays.Add(selectFri);
                alarmDays.Add(selectSat);
                alarmDays.Add(selectSun);

                sp2.Children.Add(tbMon);
                sp2.Children.Add(tbTue);
                sp2.Children.Add(tbWed);
                sp2.Children.Add(tbThu);
                sp2.Children.Add(tbFri);
                sp2.Children.Add(tbSat);
                sp2.Children.Add(tbSun);

                sp0.Children.Add(sp2);
            }

            tsAlarm.Toggled += tsAlarm_Toggled;
            btnDelAlarm.Click += btnDelAlarm_Click;
            gvi0.PointerEntered += gvi0_PointerEntered;

            btnDelAlarm.Content = bmiDelAlarm;
            cv0.Children.Add(btnDelAlarm);
            cv0.Children.Add(tsAlarm);
            sp1.Children.Add(tbAlarmName);
            sp1.Children.Add(cv0);
            bd0.Child = sp0;
            gvi0.Content = bd0;

            Alarm newAlarm = new Alarm
            {
                isOn = true,
                Name = alarmName,
                Time = timeAMPM,
                Date = alarmDate,
                Days = alarmDays,
                scheduleId = new List<string>()
            };

            //  check if no day/date selected: if alarmdays = [1,1,1,1,1,1,1]

            if (Enumerable.SequenceEqual(alarmDays, new List<int> { 1, 1, 1, 1, 1, 1, 1 }))
            {
                ContentDialog invalidDtDialog = new ContentDialog()
                {
                    Title = "Invalid Datetime Input",
                    Content = "Please select a date.",
                    CloseButtonText = "Ok"
                };

                await invalidDtDialog.ShowAsync();
            }
            else
            {
                if (newAlarm.Date == "")
                {
                    AddAlarmScheduleByDay(newAlarm);
                }
                else
                {
                    DateTime scheduledDateTime = DateTime.Parse(newAlarm.Date) + DateTime.Parse(newAlarm.Time).TimeOfDay;
                    if (DateTime.Now < scheduledDateTime) AddAlarmScheduleByDate(newAlarm, scheduledDateTime);
                    else
                    {
                        isDtInputInvalid = true;

                        ContentDialog invalidDtDialog = new ContentDialog()
                        {
                            Title = "Invalid Datetime Input",
                            Content = "Can't set alarms for times in the past.",
                            CloseButtonText = "Ok"
                        };

                        await invalidDtDialog.ShowAsync();
                    }
                }
                if (!isDtInputInvalid)
                {
                    gvAlarm.Items.Add(gvi0);
                    myAlarms.Add(newAlarm);

                    cbSettingsAlarm1.Items.Add(newAlarm.Name);
                    cbSettingsAlarm2.Items.Add(newAlarm.Name);
                    cbSettingsAlarm3.Items.Add(newAlarm.Name);
                }
            }
        }

        private void gvi0_PointerEntered(object sender, RoutedEventArgs e)
        {
            var cursorInGVI = sender as GridViewItem;
            var cursorSelectedIndex = gvAlarm.Items.IndexOf(cursorInGVI);
            gvAlarm.SelectedIndex = cursorSelectedIndex;
        }

        private void btnDelAlarm_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<ScheduledToastNotification> scheduledAlarms = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();

            foreach (ScheduledToastNotification alarm in scheduledAlarms)
            {
                foreach (string scheduleId in myAlarms[gvAlarm.SelectedIndex].scheduleId)
                {
                    if (alarm.Id == scheduleId)
                    {
                        ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(alarm);

                        IReadOnlyList<ScheduledToastNotification> scheduledAlarms2 = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                        int x = 0;
                        foreach (ScheduledToastNotification alarm2 in scheduledAlarms2)
                        {
                            x++;
                        }
                        tbNotificationQty.Text = x.ToString();
                    }
                }
            }

            if (myAlarms[gvAlarm.SelectedIndex] != trophyAlarm1 && myAlarms[gvAlarm.SelectedIndex] != trophyAlarm2 &&
                myAlarms[gvAlarm.SelectedIndex] != trophyAlarm3)
            {
                cbSettingsAlarm1.Items.Remove(myAlarms[gvAlarm.SelectedIndex].Name);
                cbSettingsAlarm2.Items.Remove(myAlarms[gvAlarm.SelectedIndex].Name);
                cbSettingsAlarm3.Items.Remove(myAlarms[gvAlarm.SelectedIndex].Name);
            }
            if (myAlarms[gvAlarm.SelectedIndex] == trophyAlarm1)
            {
                tbAlarm1HomeName.Text = "-";

                RemoveTrophyAlarm(trophyAlarm1);

                tbAlarm1HomeName.Foreground = GreyToGold(false);        // .toString("dd")
                bmiAlarm1HomeIcon.Foreground = GreyToGold(false);

                trophyAlarm1 = null;
            }
            if (myAlarms[gvAlarm.SelectedIndex] == trophyAlarm2)
            {
                tbAlarm2HomeName.Text = "-";

                RemoveTrophyAlarm(trophyAlarm2);

                tbAlarm2HomeName.Foreground = GreyToGold(false);
                bmiAlarm2HomeIcon.Foreground = GreyToGold(false);

                trophyAlarm2 = null;
            }
            if (myAlarms[gvAlarm.SelectedIndex] == trophyAlarm3)
            {
                tbAlarm3HomeName.Text = "-";

                RemoveTrophyAlarm(trophyAlarm3);

                tbAlarm3HomeName.Foreground = GreyToGold(false);
                bmiAlarm3HomeIcon.Foreground = GreyToGold(false);

                trophyAlarm3 = null;
            }

            myAlarms.RemoveAt(gvAlarm.SelectedIndex);
            gvAlarm.Items.Remove(gvAlarm.Items[gvAlarm.SelectedIndex]);
        }

        private void RemoveTrophyAlarm(Alarm trophyAlarm)
        {
            if (cbSettingsAlarm1.SelectedItem != null)
            {
                if (cbSettingsAlarm1.SelectedItem.ToString() == trophyAlarm.Name) cbSettingsAlarm1.SelectedItem = null;
            }
            if (cbSettingsAlarm2.SelectedItem != null)
            {
                if (cbSettingsAlarm2.SelectedItem.ToString() == trophyAlarm.Name) cbSettingsAlarm2.SelectedItem = null;
            }
            if (cbSettingsAlarm3.SelectedItem != null)
            {
                if (cbSettingsAlarm3.SelectedItem.ToString() == trophyAlarm.Name) cbSettingsAlarm3.SelectedItem = null;
            }

            cbSettingsAlarm1.Items.Remove(trophyAlarm.Name);
            cbSettingsAlarm2.Items.Remove(trophyAlarm.Name);
            cbSettingsAlarm3.Items.Remove(trophyAlarm.Name);
        }

        bool delAlarmFlag = false;
        private void btnTrashAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (!delAlarmFlag)
            {
                delAlarmFlag = true;
                bmiTrashAlarm.Foreground = new SolidColorBrush(Color.FromArgb(255, 227, 36, 43));
            }
            else if (delAlarmFlag)
            {
                delAlarmFlag = false;
                bmiTrashAlarm.Foreground = new SolidColorBrush(Colors.White);
            }

            ReloadAllAlarm();
        }

        private void tsAlarm_Toggled(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<ScheduledToastNotification> scheduledAlarms = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();

            if (myAlarms[gvAlarm.SelectedIndex].isOn)
            {
                myAlarms[gvAlarm.SelectedIndex].isOn = false;

                foreach (ScheduledToastNotification alarm in scheduledAlarms)
                {
                    foreach (string scheduleId in myAlarms[gvAlarm.SelectedIndex].scheduleId)
                    {
                        if (alarm.Id == scheduleId)
                        {
                            ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(alarm);

                            IReadOnlyList<ScheduledToastNotification> scheduledAlarms2 = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
                            int x = 0;
                            foreach (ScheduledToastNotification alarm2 in scheduledAlarms2)
                            {
                                x++;
                            }
                            tbNotificationQty.Text = x.ToString();
                        }
                    }
                }
            }
            else
            {
                myAlarms[gvAlarm.SelectedIndex].isOn = true;

                if (myAlarms[gvAlarm.SelectedIndex].Date != "")
                {
                    DateTime scheduledDateTime = DateTime.Parse(myAlarms[gvAlarm.SelectedIndex].Date) + DateTime.Parse(myAlarms[gvAlarm.SelectedIndex].Time).TimeOfDay;  // only if scheduled datetime is after now
                    if (DateTime.Now < scheduledDateTime) AddAlarmScheduleByDate(myAlarms[gvAlarm.SelectedIndex], scheduledDateTime);
                }
                else
                {
                    AddAlarmScheduleByDay(myAlarms[gvAlarm.SelectedIndex]);
                }
            }

            updateAlarmDisplay();
            ReloadSelectedAlarm();
        }

        private void ReloadSelectedAlarm()
        {
            GridViewItem gvi0 = new GridViewItem
            {
                Margin = new Thickness(3, 0, 0, 0)
            };
            StackPanel sp0 = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            StackPanel sp1 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            Canvas cv0 = new Canvas
            {
                Width = 50,
                Height = 30
            };
            Button btnDelAlarm = new Button
            {
                Visibility = Visibility.Collapsed,
                Background = new SolidColorBrush(Colors.Transparent),
                RequestedTheme = ElementTheme.Light
            };
            BitmapIcon bmiDelAlarm = new BitmapIcon
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 227, 36, 43)),
                UriSource = new Uri(BaseUri, "../images/trash_icon1.png"),
                Width = 20,
                Height = 20
            };
            ToggleSwitch tsAlarm = new ToggleSwitch
            {
                Width = 50,
                Visibility =Visibility.Visible,
                IsOn = myAlarms[gvAlarm.SelectedIndex].isOn,
                RequestedTheme = ElementTheme.Dark,
            };
            Border bd0 = new Border
            {
                Margin = new Thickness(10),
                BorderThickness = new Thickness(2),
                BorderBrush = GreyToGold(myAlarms[gvAlarm.SelectedIndex].isOn)    
            };

            TextBlock tbAlarmName = new TextBlock
            {
                Text = myAlarms[gvAlarm.SelectedIndex].Name,
                Foreground = WhiteToGray(myAlarms[gvAlarm.SelectedIndex].isOn),     
                Width = 120,
                Height = 30,
                TextAlignment = TextAlignment.Left,
                FontSize = 15,
                Margin = new Thickness(10, 5, 0, 0)
            };

            TextBlock tbAlarmTime = new TextBlock
            {
                Foreground = WhiteToGray(myAlarms[gvAlarm.SelectedIndex].isOn),     
                Width = 110,
                Height = 40,
                TextAlignment = TextAlignment.Center,
                Text = myAlarms[gvAlarm.SelectedIndex].Time,
                FontSize = 25
            };

            sp0.Children.Add(sp1);
            sp0.Children.Add(tbAlarmTime);

            if (myAlarms[gvAlarm.SelectedIndex].Date != "")
            {
                TextBlock tbAlarmDate = new TextBlock
                {
                    Foreground = WhiteToGold(myAlarms[gvAlarm.SelectedIndex].isOn),     
                    Text = myAlarms[gvAlarm.SelectedIndex].Date,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = 30
                };
                sp0.Children.Add(tbAlarmDate);
            }
            else
            {
                StackPanel sp2 = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = 30
                };

                TextBlock tbMon = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[0]),
                    Text = "M",
                    FontSize = 15,
                    Margin = new Thickness(0),
                };
                TextBlock tbTue = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[1]),
                    Text = "T",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbWed = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[2]),
                    Text = "W",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbThu = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[3]),
                    Text = "T",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbFri = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[4]),
                    Text = "F",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbSat = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[5]),
                    Text = "S",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };
                TextBlock tbSun = new TextBlock
                {
                    Foreground = btnDayColor(myAlarms[gvAlarm.SelectedIndex].Days[6]),
                    Text = "S",
                    FontSize = 15,
                    Margin = new Thickness(10, 0, 0, 0),
                };

                sp2.Children.Add(tbMon);
                sp2.Children.Add(tbTue);
                sp2.Children.Add(tbWed);
                sp2.Children.Add(tbThu);
                sp2.Children.Add(tbFri);
                sp2.Children.Add(tbSat);
                sp2.Children.Add(tbSun);

                sp0.Children.Add(sp2);
            }

            tsAlarm.Toggled += tsAlarm_Toggled;
            btnDelAlarm.Click += btnDelAlarm_Click;
            gvi0.PointerEntered += gvi0_PointerEntered;

            btnDelAlarm.Content = bmiDelAlarm;
            cv0.Children.Add(btnDelAlarm);
            cv0.Children.Add(tsAlarm);
            sp1.Children.Add(tbAlarmName);
            sp1.Children.Add(cv0);
            bd0.Child = sp0;
            gvi0.Content = bd0;

            gvAlarm.Items[gvAlarm.SelectedIndex] = gvi0;
        }

        private void ReloadAllAlarm()
        {
            foreach (var alarm in gvAlarm.Items)
            {
                GridViewItem gvi0 = new GridViewItem
                {
                    Margin = new Thickness(3, 0, 0, 0)
                };
                StackPanel sp0 = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };
                StackPanel sp1 = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                Canvas cv0 = new Canvas
                {
                    Width = 50,
                    Height = 30
                };
                Button btnDelAlarm = new Button
                {
                    Visibility = TrashIconVisibility(delAlarmFlag),
                    Background = new SolidColorBrush(Colors.Transparent),
                    RequestedTheme = ElementTheme.Light
                };
                BitmapIcon bmiDelAlarm = new BitmapIcon
                {
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 227, 36, 43)),
                    UriSource = new Uri(BaseUri, "../images/trash_icon1.png"),
                    Width = 20,
                    Height = 20
                };
                ToggleSwitch tsAlarm = new ToggleSwitch
                {
                    Width = 50,
                    Visibility = ToggleSwitchVisibility(delAlarmFlag),
                    IsOn = myAlarms[gvAlarm.Items.IndexOf(alarm)].isOn,
                    RequestedTheme = ElementTheme.Dark,
                };
                Border bd0 = new Border
                {
                    Margin = new Thickness(10),
                    BorderThickness = new Thickness(2),
                    BorderBrush = GreyToGold(myAlarms[gvAlarm.Items.IndexOf(alarm)].isOn)
                };

                TextBlock tbAlarmName = new TextBlock
                {
                    Text = myAlarms[gvAlarm.Items.IndexOf(alarm)].Name,
                    Foreground = WhiteToGray(myAlarms[gvAlarm.Items.IndexOf(alarm)].isOn),
                    Width = 120,
                    Height = 30,
                    TextAlignment = TextAlignment.Left,
                    FontSize = 15,
                    Margin = new Thickness(10, 5, 0, 0)
                };

                TextBlock tbAlarmTime = new TextBlock
                {
                    Foreground = WhiteToGray(myAlarms[gvAlarm.Items.IndexOf(alarm)].isOn),
                    Width = 110,
                    Height = 40,
                    TextAlignment = TextAlignment.Center,
                    Text = myAlarms[gvAlarm.Items.IndexOf(alarm)].Time,
                    FontSize = 25
                };

                sp0.Children.Add(sp1);
                sp0.Children.Add(tbAlarmTime);

                if (myAlarms[gvAlarm.Items.IndexOf(alarm)].Date != "")
                {
                    TextBlock tbAlarmDate = new TextBlock
                    {
                        Foreground = WhiteToGold(myAlarms[gvAlarm.Items.IndexOf(alarm)].isOn),
                        Text = myAlarms[gvAlarm.Items.IndexOf(alarm)].Date,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Height = 30
                    };
                    sp0.Children.Add(tbAlarmDate);
                }
                else
                {
                    StackPanel sp2 = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Height = 30
                    };

                    TextBlock tbMon = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[0]),
                        Text = "M",
                        FontSize = 15,
                        Margin = new Thickness(0),
                    };
                    TextBlock tbTue = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[1]),
                        Text = "T",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };
                    TextBlock tbWed = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[2]),
                        Text = "W",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };
                    TextBlock tbThu = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[3]),
                        Text = "T",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };
                    TextBlock tbFri = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[4]),
                        Text = "F",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };
                    TextBlock tbSat = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[5]),
                        Text = "S",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };
                    TextBlock tbSun = new TextBlock
                    {
                        Foreground = btnDayColor(myAlarms[gvAlarm.Items.IndexOf(alarm)].Days[6]),
                        Text = "S",
                        FontSize = 15,
                        Margin = new Thickness(10, 0, 0, 0),
                    };

                    sp2.Children.Add(tbMon);
                    sp2.Children.Add(tbTue);
                    sp2.Children.Add(tbWed);
                    sp2.Children.Add(tbThu);
                    sp2.Children.Add(tbFri);
                    sp2.Children.Add(tbSat);
                    sp2.Children.Add(tbSun);

                    sp0.Children.Add(sp2);
                }

                tsAlarm.Toggled += tsAlarm_Toggled;
                btnDelAlarm.Click += btnDelAlarm_Click;
                gvi0.PointerEntered += gvi0_PointerEntered;

                btnDelAlarm.Content = bmiDelAlarm;
                cv0.Children.Add(btnDelAlarm);
                cv0.Children.Add(tsAlarm);
                sp1.Children.Add(tbAlarmName);
                sp1.Children.Add(cv0);
                bd0.Child = sp0;
                gvi0.Content = bd0;

                gvAlarm.Items[gvAlarm.Items.IndexOf(alarm)] = gvi0;
            }
        }

        Visibility TrashIconVisibility(bool delFlag)
        {
            if (delFlag) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        Visibility ToggleSwitchVisibility(bool delFlag)
        {
            if (delFlag) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        SolidColorBrush GreyToGold(bool isOn)
        {
            if (isOn) return new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
            return new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
        }

        SolidColorBrush WhiteToGray(bool isOn)
        {
            if (isOn) return new SolidColorBrush(Colors.White);
            return new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
        }

        SolidColorBrush WhiteToGold(bool isOn)
        {
            if (isOn) return new SolidColorBrush(Colors.White);
            return new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
        }

        SolidColorBrush btnDayColor(int status)
        {
            if (status == 0) return new SolidColorBrush(Color.FromArgb(255, 187, 161, 79));
            return new SolidColorBrush(Color.FromArgb(255, 112, 128, 144));
        }

        private void btnUpdateBus_Click(object sender, RoutedEventArgs e)
        {
            lvBus.Items.Clear();
            loadBus();
        }

        private void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            loadTranslator(tbTranslateInput.Text);
        }

        private void btnWeatherDay1_Click(object sender, RoutedEventArgs e)
        {
            reloadWeather(1);
        }

        private void btnWeatherDay2_Click(object sender, RoutedEventArgs e)
        {
            reloadWeather(2);
        }

        private void btnWeatherDay3_Click(object sender, RoutedEventArgs e)
        {
            reloadWeather(3);
        }

        private void btnSettingsText_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSetting != 0)
            {
                selectedSetting = 0;

                gdSettingsText.Visibility = Visibility.Visible;
                gdSettingsAlarm.Visibility = Visibility.Collapsed;

                tbSettingsText.Foreground = new SolidColorBrush();

                btnSettingsAlarm.BorderBrush = GreyToGold(false);
                bmiSettingsAlarm.Foreground = GreyToGold(false);
                tbSettingsAlarm.Foreground = GreyToGold(false);

                btnSettingsText.BorderBrush = GreyToGold(true);
                bmiSettingsText.Foreground = GreyToGold(true);
                tbSettingsText.Foreground = GreyToGold(true);
            }
        }
        private void btnSettingsAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSetting != 1)
            {
                selectedSetting = 1;

                gdSettingsText.Visibility = Visibility.Collapsed;
                gdSettingsAlarm.Visibility = Visibility.Visible;

                btnSettingsAlarm.BorderBrush = GreyToGold(true);
                bmiSettingsAlarm.Foreground = GreyToGold(true);
                tbSettingsAlarm.Foreground = GreyToGold(true);

                btnSettingsText.BorderBrush = GreyToGold(false);
                bmiSettingsText.Foreground = GreyToGold(false);
                tbSettingsText.Foreground = GreyToGold(false);
            }
        }

        private void btnGreetTextSaveChange_Click(object sender, RoutedEventArgs e)
        {
            TranslateGreetText(tbNewGreetText.Text);
            tbGreeting.Foreground = new SolidColorBrush(cpGreetText.Color);
            if (sliderGreetTextFontSize.Value != 0) tbGreeting.FontSize = sliderGreetTextFontSize.Value;
            while (fvHoriz.SelectedIndex < 3) fvHoriz.SelectedIndex++;
        }

        private void btnHomeEditGreeting_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSetting != 0)
            {
                selectedSetting = 0;

                gdSettingsText.Visibility = Visibility.Visible;
                gdSettingsAlarm.Visibility = Visibility.Collapsed;

                btnSettingsAlarm.BorderBrush = GreyToGold(false);
                bmiSettingsAlarm.Foreground = GreyToGold(false);
                tbSettingsAlarm.Foreground = GreyToGold(false);

                btnSettingsText.BorderBrush = GreyToGold(true);
                bmiSettingsText.Foreground = GreyToGold(true);
                tbSettingsText.Foreground = GreyToGold(true);
            }
            
            while (fvHoriz.SelectedIndex > 0) fvHoriz.SelectedIndex--;
        }

        private void btnHomeEditAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSetting != 1)
            {
                selectedSetting = 1;

                gdSettingsText.Visibility = Visibility.Collapsed;
                gdSettingsAlarm.Visibility = Visibility.Visible;

                btnSettingsAlarm.BorderBrush = GreyToGold(true);
                bmiSettingsAlarm.Foreground = GreyToGold(true);
                tbSettingsAlarm.Foreground = GreyToGold(true);

                btnSettingsText.BorderBrush = GreyToGold(false);
                bmiSettingsText.Foreground = GreyToGold(false);
                tbSettingsText.Foreground = GreyToGold(false);
            }

            while (fvHoriz.SelectedIndex > 0) fvHoriz.SelectedIndex--;
        }

        private void updateAlarmDisplay()   // toggle, delete, creation, dropdown change
        {
            if (trophyAlarm1 != null)
            {
                tbAlarm1HomeName.Text = trophyAlarm1.Name;

                if (trophyAlarm1.isOn)
                {
                    tbAlarm1HomeName.Foreground = GreyToGold(true);
                    bmiAlarm1HomeIcon.Foreground = GreyToGold(true);
                }
                else
                {
                    tbAlarm1HomeName.Foreground = GreyToGold(false);
                    bmiAlarm1HomeIcon.Foreground = GreyToGold(false);
                }
            }

            if (trophyAlarm2 != null)
            {
                tbAlarm2HomeName.Text = trophyAlarm2.Name;

                if (trophyAlarm2.isOn)
                {
                    tbAlarm2HomeName.Foreground = GreyToGold(true);
                    bmiAlarm2HomeIcon.Foreground = GreyToGold(true);
                }
                else
                {
                    tbAlarm2HomeName.Foreground = GreyToGold(false);
                    bmiAlarm2HomeIcon.Foreground = GreyToGold(false);
                }
            }

            if (trophyAlarm3 != null)
            {
                tbAlarm3HomeName.Text = trophyAlarm3.Name;

                if (trophyAlarm3.isOn)
                {
                    tbAlarm3HomeName.Foreground = GreyToGold(true);
                    bmiAlarm3HomeIcon.Foreground = GreyToGold(true);
                }
                else
                {
                    tbAlarm3HomeName.Foreground = GreyToGold(false);
                    bmiAlarm3HomeIcon.Foreground = GreyToGold(false);
                }
            }
        }

        private void cbSettingsAlarm1_DropDownClosed(object sender, object e)
        {
            if (cbSettingsAlarm1.SelectedItem != null)
            {
                foreach (Alarm alarm in myAlarms)
                {
                    if (cbSettingsAlarm1.SelectedItem.ToString() == alarm.Name)
                    {
                        trophyAlarm1 = alarm;
                    }
                }
                updateAlarmDisplay();
            }
        }

        private void cbSettingsAlarm2_DropDownClosed(object sender, object e)
        {
            if (cbSettingsAlarm2.SelectedItem != null)
            {
                foreach (Alarm alarm in myAlarms)
                {
                    if (cbSettingsAlarm2.SelectedItem.ToString() == alarm.Name)
                    {
                        trophyAlarm2 = alarm;
                    }
                }
                updateAlarmDisplay();
            }
        }

        private void cbSettingsAlarm3_DropDownClosed(object sender, object e)
        {
            if (cbSettingsAlarm3.SelectedItem != null)
            {
                foreach (Alarm alarm in myAlarms)
                {
                    if (cbSettingsAlarm3.SelectedItem.ToString() == alarm.Name)
                    {
                        trophyAlarm3 = alarm;
                    }
                }
                updateAlarmDisplay();
            }
        }

        private async void TranslateGreetText(string input)
        {
            TranslateRoot myTranslatedText;

            if (cbGreetTextLanguage.SelectedItem != null)
            {
                switch (cbGreetTextLanguage.SelectedItem.ToString())
                {
                    case "English":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "en");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Indonesian":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "id");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Simplified Chinese, 中文 (简体)":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "zh-Hans");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Traditional Chinese, 繁體中文 (繁體)":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "zh-Hant");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Vietnamese, Tiếng Việt":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "vi");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Thai, ไทย":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "th");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Malay, Melayu":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ms");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Korean, 한국어":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ko");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Japanese, 日本語":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ja");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Hindi, हिन्दी":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "hi");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Tamil, தமிழ்":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "ta");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    case "Myanmar (Burmese), မြန်မာ":
                        myTranslatedText = await TranslatorProxy.TranslateText(input, "my");
                        tbNewGreetText.Text = myTranslatedText.translations[0].text;
                        tbGreeting.Text = myTranslatedText.translations[0].text;
                        break;

                    default:
                        break;
                }
            }
        }


    }
}
