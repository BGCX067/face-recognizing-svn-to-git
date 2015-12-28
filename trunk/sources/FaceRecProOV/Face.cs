using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace MultiFaceRec
{
    public class Face
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private Image<Gray, byte> img;

        public Image<Gray, byte> Img
        {
            get { return img; }
            set { img = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string phone;

        public string Phone
        {
            get { return phone; }
            set { phone = value; }
        }
        private string email;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        private DateTime dob;

        public DateTime Dob
        {
            get { return dob; }
            set { dob = value; }
        }
        public Face()
        {
            name = "";
            phone = "";
            email = "";
        }
        public Face(string name, string phone, string email, DateTime dob, Image<Gray, byte> img)
        {
            this.name = name;
            this.phone = phone;
            this.email = email;
            this.img = img;
            this.dob = dob;
        }
    }
}
