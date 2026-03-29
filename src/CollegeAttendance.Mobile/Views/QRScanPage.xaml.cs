using CollegeAttendance.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace CollegeAttendance.Mobile.Views;

public partial class QRScanPage : ContentPage
{
    private readonly QRScanViewModel _viewModel;

    public QRScanPage(QRScanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID || IOS
        AddCameraScanner();
#endif
    }

    private void AddCameraScanner()
    {
        var barcodeReader = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.QrCode,
                AutoRotate = true,
                Multiple = false
            }
        };
        barcodeReader.BarcodesDetected += OnBarcodesDetected;

        // Replace the placeholder with actual scanner
        if (Content is Grid grid)
        {
            var placeholder = grid.Children.FirstOrDefault(c => c is Border b && Grid.GetRow(b) == 0);
            if (placeholder != null)
            {
                var index = grid.Children.IndexOf(placeholder);
                grid.Children.RemoveAt(index);
                grid.Children.Insert(index, barcodeReader);
                Grid.SetRow(barcodeReader, 0);
            }
        }
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var result = e.Results.FirstOrDefault();
        if (result == null || string.IsNullOrWhiteSpace(result.Value)) return;

        // Prevent duplicate processing
        if (sender is CameraBarcodeReaderView reader)
            reader.IsDetecting = false;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _viewModel.ProcessQRCodeCommand.ExecuteAsync(result.Value);
        });
    }
}
