using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;

namespace MultiFaceRec
{
    public partial class FrmPrincipal : Form
    {
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<Face> trainedFaces = new List<Face>();


        public FrmPrincipal()
        {
            InitializeComponent();
            //khởi động camera
            StartCam();

            //load dữ liệu cho việc phát hiện khuôn mặt
            face = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                // đọc dữ liệu từ database
                LoadData();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error:" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void LoadData()
        {
            try
            {
                // lấy dữ liệu từ database
                trainedFaces = SQLServerHelper.getInstance().getFaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            foreach (Face f in trainedFaces)
            {
                trainingImages.Add(f.Img);
            }
        }

        private void FrameGrabber(object sender, EventArgs e)
        {
            //lấy hình ảnh hiện tại của camera
            currentFrame = grabber.QueryFrame().Resize(imageBoxFrameGrabber.Width, imageBoxFrameGrabber.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //chuyển thành Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //phát hiện các khuôn mặt trong camera
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                  new Size(20, 20));

            
            Face detectedFace = null;
            bool flag = false;

            //với mỗi khuôn mặt được phát hiện, vẽ khung hình vuông màu đỏ
            foreach (MCvAvgComp f in facesDetected[0])
            {
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(Constants.DETECTED_FRAME_WIDTH, Constants.DETECTED_FRAME_HEIGHT, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                // vẽ khung phát hiện khuôn mặt
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);


                if (trainingImages.ToArray().Length != 0)
                {
                    //TermCriteria for face recognition with numbers of trained images like maxIteration
                    MCvTermCriteria termCrit = new MCvTermCriteria(1, 0.001);

                    //khởi tạo đối tượng để nhận diện khuôn mặt
                    FaceRecognizer recognizer = new FaceRecognizer(
                       trainingImages.ToArray(),
                        // labels.ToArray(),
                      trainedFaces,
                       3000,
                       ref termCrit);

                    //nhận diện khuôn mặt
                    Face recognizedFace = recognizer.RecognizeFaces(result);

                    if (recognizedFace != null)
                    {
                        //chỉ hiển this thông tin của khuôn mặt đầu tiên đc nhận diện
                        if (!flag)
                        {
                            detectedFace = recognizedFace;
                            flag = true;
                        }

                        //vẽ tên của khuôn mặt được nhận diện
                        currentFrame.Draw(recognizedFace.Name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                    }

                }

            }

            // gán lại frame hiện tại cho camera
            imageBoxFrameGrabber.Image = currentFrame;

            if (detectedFace != null)
            {
                // hiển thị thông tin của khuôn mặt đầu tiên đc nhận diện
                lName.Text = detectedFace.Name;
                lPhone.Text = detectedFace.Phone;
                lEmail.Text = detectedFace.Email;
                lAge.Text = Utility.GetAge(detectedFace.Dob).ToString();
                pbImage.BackgroundImage = detectedFace.Img.ToBitmap();
            }
        }

        private void StartCam()
        {
            //khởi tạo camera
            grabber = new Capture();
            //hiển thị camera
            grabber.QueryFrame();
            //bắt đầu phát hiện và nhận diện khuôn mặt
            StartDetecting();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // hủy việc nhận diện của form chính
            StopDetecting();
            // show form thêm khuôn mặt
            new AddFacesForm().ShowDialog(this);
            // đọc lại csdl
            ReLoad();
            // bắt đầu nhận diện
            StartDetecting();
        }

        private void StartDetecting()
        {
            Application.Idle += new EventHandler(FrameGrabber);
        }

        private void StopDetecting()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
        }

        private void ReLoad()
        {
            LoadData();
        }

        private void lEmail_Click(object sender, EventArgs e)
        {

        }

    }
}