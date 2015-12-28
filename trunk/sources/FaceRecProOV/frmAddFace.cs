using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MultiFaceRec
{
    public partial class AddFacesForm : Form
    {
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        int captureTimes=0;

        public AddFacesForm()
        {
            InitializeComponent();
            //khởi động camera
            StartCam();

            //load dữ liệu cho việc phát hiện khuôn mặt
            face = new HaarCascade("haarcascade_frontalface_default.xml");
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            //lấy hình ảnh hiện tại của camera
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //chuyển thành Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //phát hiện các khuôn mặt trong camera
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.2,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));

            //với mỗi khuôn mặt được phát hiện, vẽ khung hình vuông màu đỏ
            foreach (MCvAvgComp f in facesDetected[0])
            {
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                // vẽ khung phát hiện khuôn mặt
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);

            }

            // gán lại frame hiện tại cho camera
            imageBoxFrameGrabber.Image = currentFrame;

        }

        private void btnAddFace_Click(object sender, EventArgs e)
        {
            try
            {
                // lấy dữ liệu từ các textbox
                string name = txtName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text.Trim();
                DateTime dob = dtpDOB.Value;

                // nếu người dùng đã nhập tên
                if (name.Length > 0)
                {
                    // nếu chức năng chụp hình trong 10s đã đc bật
                    if (cbxAutoCapture.Checked)
                    {
                        // ẩn nút Add face
                        btnAddFace.Enabled = false;
                        // bắt đầu tự động chụp khuôn mặt trong 10s
                        advanceCapture();
                    }
                    else // ngược lại, chụp bình thường
                    {
                        normalCapture(name, phone, email, dob);
                    }
                }
                else
                {
                    MessageBox.Show("Name is empty!");
                }

            }
            catch
            {
                MessageBox.Show("No detected face", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void advanceCapture()
        {
            // bắt đầu tự động chụp
            timer1.Enabled = true;
        }

        private void normalCapture(string name, string phone, string email, DateTime dob)
        {
            capture(name, phone, email, dob);
        }

        // hàm chụp khuôn mặt đc phát hiện và lưu vào database
        private void capture(string name, string phone, string email, DateTime dob)
        {
            //lấy frame hiện tại của camera
            gray = grabber.QueryGrayFrame().Resize(imageBoxFrameGrabber.Width, imageBoxFrameGrabber.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //phát hiện khuôn mặt
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
            face,
            1.2,
            10,
            Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
            new Size(20, 20));

            // nếu phát hiện được khuôn mặt nào
            if (facesDetected[0].Length > 0)
            {
                // lấy khuôn mặt đc phát hiện
                MCvAvgComp f = facesDetected[0][0];
                TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();//Convert<gray,>();

                // chỉnh lại kích thước khuôn mặt để so sánh với dữ liệu
                TrainedFace = result.Resize(Constants.DETECTED_FRAME_WIDTH, Constants.DETECTED_FRAME_HEIGHT, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                // tạo đối tượng Face
                Face newFace = new Face(name, phone, email, dob, TrainedFace);
                try
                {
                    // lưu vào cơ sở dữ liệu
                    SQLServerHelper.getInstance().insertFace(newFace);

                    // thêm khuôn mặt vào khung Training
                    lstFaces.Controls.Add(CreatePanel(TrainedFace));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        
        private Panel CreatePanel(Image<Gray, byte> img)
        {
            Panel p = new Panel();
            p.Width = Constants.DETECTED_FRAME_WIDTH;
            p.Height = Constants.DETECTED_FRAME_HEIGHT;
            p.BackColor = Color.Gray;
            p.BackgroundImage = img.ToBitmap();
            return p;
        }

        private void StartCam()
        {
            //khởi tạo camera
            grabber = new Capture();

            // hiển thị camera
            grabber.QueryFrame();

            //thêm sự kiện phát hiện khuôn mặt
            Application.Idle += new EventHandler(FrameGrabber);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // lấy dữ liệu từ các textbox
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            DateTime dob = dtpDOB.Value;

            // chụp khuôn mặt và lưu vào database
            capture(name, phone, email, dob);

            String timeStr = (captureTimes + 1).ToString() + "s";

            // nếu đã chụp 10s rồi thì tắt chức năng tự đông chụp
            if (captureTimes++ >= 10)
            {
                timeStr = "";
                timer1.Enabled = false;
                captureTimes = 0;
                btnAddFace.Enabled = true;
            }

            // hiển thị thời gian(giây)
            lTime.Text = timeStr;
        }

    }
}