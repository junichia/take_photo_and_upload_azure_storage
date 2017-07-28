using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;


// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace capturePhoto
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();


        }


        private async void button_Click(object sender, RoutedEventArgs e)
        {

        CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(
                softwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            imageControl.Source = bitmapSource;
            textBlock1.Text = photo.Path;

            string imageFilePath = photo.Path;
            string Filename = photo.Name;

            Uri fileUri = new Uri(imageFilePath);

            await App.container.CreateIfNotExistsAsync();
            var blob = App.container.GetBlockBlobReference(Filename);
            Stream fileStream = File.OpenRead(imageFilePath);
            await blob.UploadFromStreamAsync(fileStream);
            fileStream.Dispose();

            textBlock1.Text = photo.Path + ":アップロード完了" ;

            //Emotion[] emotionResult = await UploadAndDetectEmotions(imageFilePath);

            ///Scores emotionScore = emotionResult[0].Scores;
            //float scoreAnger = emotionScore.Anger;
            //float scoreContempt = emotionScore.Contempt; //軽蔑
            //float scoreDisgust = emotionScore.Disgust; //嫌悪感
            //float scoreFear = emotionScore.Fear;
            //float scoreHappiness = emotionScore.Happiness;
            //float scoreNeutral = emotionScore.Neutral;
            //float scoreSadness = emotionScore.Sadness;
            //float scoreSurprise = emotionScore.Surprise;

            //string txtScore = "Anger:" + scoreAnger + Environment.NewLine
            //                + "Contempt:" + scoreContempt + Environment.NewLine 
            //                + "Disgust:" + scoreDisgust + Environment.NewLine 
            //                + "Fear:" + scoreFear + Environment.NewLine
            //                + "Hapiness:" + scoreHappiness + Environment.NewLine
            //                + "Neutral:" + scoreNeutral + Environment.NewLine
            //                + "Sadness:" + scoreSadness + Environment.NewLine
            //                + "Surprise:" + scoreSurprise;


            //textBlock_score.Text = txtScore;

        }

        private async Task<Emotion[]> UploadAndDetectEmotions(string imageFilePath)
        {
            string subscriptionKey = "[key of cognitive service]";

            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(subscriptionKey);

            try
            {
                Emotion[] emotionResult;
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {

                    emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    return emotionResult;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }
    }
}
