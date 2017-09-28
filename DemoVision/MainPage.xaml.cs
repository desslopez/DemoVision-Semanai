using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Threading.Tasks;

using Windows.Storage.Pickers;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace DemoVision
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CameraCaptureUI captureUI = new CameraCaptureUI();
        StorageFile photo;
        IRandomAccessStream imageStream;

        FileOpenPicker openPicker = new FileOpenPicker();

        public MainPage()
        {
            this.InitializeComponent();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
        }

        private async void GetPhotoButton_Click(object sender, RoutedEventArgs e)
        {

            // This is to select a file to open.
            //Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            // Open the file picker .
            photo = await openPicker.PickSingleFileAsync();

            // The file is null if user cancels the file picker.
            if (photo != null)
            {
                // Here we open a stream for the selected file.
                // The 'using' block ensures the stream is disposed
                // after the image is loaded.
                imageStream = await photo.OpenAsync(FileAccessMode.Read);
                Windows.Graphics.Imaging.BitmapDecoder decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(imageStream);
                Windows.Graphics.Imaging.SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                Windows.Graphics.Imaging.SoftwareBitmap softwareBitmapBRG = Windows.Graphics.Imaging.SoftwareBitmap.Convert(softwareBitmap, Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                    Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                await bitmapSource.SetBitmapAsync(softwareBitmapBRG);

                image.Source = bitmapSource;
            }
        }

        
        private async void TakePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

                if (photo == null)
                {
                    return;
                }
                else
                {
                    imageStream = await photo.OpenAsync(FileAccessMode.Read);
                    Windows.Graphics.Imaging.BitmapDecoder decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(imageStream);
                    Windows.Graphics.Imaging.SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    Windows.Graphics.Imaging.SoftwareBitmap softwareBitmapBRG = Windows.Graphics.Imaging.SoftwareBitmap.Convert(softwareBitmap, Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                        Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                    SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                    await bitmapSource.SetBitmapAsync(softwareBitmapBRG);

                    image.Source = bitmapSource;
                }
            }
            catch
            {
                output.Text = "Error carnal";
            }
        }

        private async Task<AnalysisResult> GetImageDescription(Stream imageStream)
        {
            VisionServiceClient visionClient = new VisionServiceClient("d24c1150205c4e8fb8ef981cfc91bc13", "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
            VisualFeature[] features = { VisualFeature.Tags };
            return await visionClient.AnalyzeImageAsync(imageStream, features.ToList(), null);
        }

        private async Task<OcrResults> GetImageText(Stream imageStream)
        {
            VisionServiceClient visionClient = new VisionServiceClient("d24c1150205c4e8fb8ef981cfc91bc13", "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
            //VisualFeature[] features = { VisualFeature.Description };
             return await visionClient.RecognizeTextAsync(imageStream);
        }

        private async void Analiza_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await GetImageDescription(imageStream.AsStream());
                foreach (var tag in result.Tags)
                {
                    output.Text = output.Text + tag.Name + "\n";
                }
            }
            catch (ClientException ex)
            {
                output.Text = ex.Message;
            }
        }

        private async void AnalizaTexto_Click(object sender, RoutedEventArgs e)
        {
            output.Text = "";
            try
            {
                var result = await GetImageText(imageStream.AsStream());
                foreach (var region in result.Regions)
                {
                    // Iterate lines per region
                    foreach (var line in region.Lines)
                    {
                        // Iterate words per line and add the word
                        // to the StackLayout
                        foreach (var word in line.Words)
                        {
                            output.Text = output.Text + word.Text+ " ";
                        }
                    }
                }
            }
            catch (ClientException ex)
            {
                output.Text = ex.Message;
            }
        }

        private void GetPhotoButton_Click(System.Object sender, RoutedEventArgs e)
        {

        }
    }
}
