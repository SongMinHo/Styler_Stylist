using System;
using System.Windows.Media;

namespace ProjectKinect
{
    public class ImageTrack
    {
        ImageSource img;
        int id;

        public ImageTrack(ImageSource img, int id)
        {
            this.img = img;
            this.id = id;
        }

        public ImageSource getImg()
        {
            return img;
        }

        public int getId()
        {
            return id;
        }
    }

}
