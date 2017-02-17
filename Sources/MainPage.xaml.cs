using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFShapeView;

namespace FaceXamarin
{
    /// <summary>
    /// 이미지 파일 선택해서 가져오기 : https://github.com/jamesmontemagno/MediaPlugin
    /// 도형 그리기 : https://github.com/vincentgury/XFShapeView
    /// </summary>
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Image.SizeChanged += Image_SizeChanged;
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            try
            {
                var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera,
                        Permission.Storage);
                    cameraStatus = results[Permission.Camera];
                    storageStatus = results[Permission.Storage];
                }

                if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
                {
                    //카메라로 사진 찍는 경우
                    //var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());
                    //if (file == null) return;
                    //Image.Source = ImageSource.FromFile(file.Path);

                    //갤러리에서 사진 가지고 오는 경우
                    var pic = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Full
                    });

                    if (pic == null) return;
                    Image.Source = ImageSource.FromFile(pic.Path);

                    //face detect
                    var faces = await FaceAPIHelper.Instance.GetDetectAsync(pic.GetStream());
                    pic.Dispose();

                    if (faces == null) return;
                    Result.Text = string.Format("Found {0} faces", faces.Length.ToString());

                    //찾은 얼굴 위치에 박스 표시 
                    //실제 이미지 크기를 알아야 비율로 위치를 잡을 텐데..실제 이미지 크기를 몰라서 비율 계산을 할 수 없음
                    //비율 계산만 할 수 있으면 정확하게 화면에 박스를 그릴수 있을 텐데...쿨럭
                    foreach (var face in faces)
                    {
                        var box = new ShapeView
                        {
                            ShapeType = ShapeType.Box,
                            HeightRequest = face.FaceRectangle.Height,
                            WidthRequest = face.FaceRectangle.Width,
                            Color = Color.Transparent,
                            BorderColor = Color.Red,
                            BorderWidth = 2f
                        };
                        AbsoluteLayout.Children.Add(box, new Point(face.FaceRectangle.Left, face.FaceRectangle.Top));
                    }
                }
                else
                {
                    await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                    //On iOS you may want to send your user to the settings screen.
                    //CrossPermissions.Current.OpenAppSettings();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }
}