using System;
using System.Diagnostics;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace MultiFaceRec
{
   /// <summary>
   /// Lớp dùng để nhận diện khuôn mặt bằng thuật toán PCA (Principle Components Analysis)
   /// </summary>
   [Serializable]
   public class FaceRecognizer
   {
      private Image<Gray, Single>[] _eiImgs;
      private Image<Gray, Single> _avgImg;
      private Matrix<float>[] _eiVals;
      private List<Face> _faces;
   
      private double _eiDistanceThreshold;

      public FaceRecognizer(Image<Gray, Byte>[] images, List<Face> trainedFaces, double eigenDistanceThreshold, ref MCvTermCriteria termCrit)
      {
         CalcEigenObjects(images, ref termCrit, out _eiImgs, out _avgImg);

         _eiVals = Array.ConvertAll<Image<Gray, Byte>, Matrix<float>>(images,
             delegate(Image<Gray, Byte> img)
             {
                return new Matrix<float>(EigenDecomposite(img, _eiImgs, _avgImg));
             });

         _faces = trainedFaces;

         _eiDistanceThreshold = eigenDistanceThreshold;
      }

      private void CalcEigenObjects(Image<Gray, Byte>[] trainingImages, ref MCvTermCriteria termCrit, out Image<Gray, Single>[] eigenImages, out Image<Gray, Single> avg)
      {
         int width = trainingImages[0].Width;
         int height = trainingImages[0].Height;

         IntPtr[] inObjs = Array.ConvertAll<Image<Gray, Byte>, IntPtr>(trainingImages, delegate(Image<Gray, Byte> img) { return img.Ptr; });

         if (termCrit.max_iter <= 0 || termCrit.max_iter > trainingImages.Length)
            termCrit.max_iter = trainingImages.Length;
         
         int maxEigenObjs = termCrit.max_iter;

         eigenImages = new Image<Gray, float>[maxEigenObjs];
         for (int i = 0; i < eigenImages.Length; i++)
            eigenImages[i] = new Image<Gray, float>(width, height);
         IntPtr[] eigObjs = Array.ConvertAll<Image<Gray, Single>, IntPtr>(eigenImages, delegate(Image<Gray, Single> img) { return img.Ptr; });

         avg = new Image<Gray, Single>(width, height);

         CvInvoke.cvCalcEigenObjects(
             inObjs,
             ref termCrit,
             eigObjs,
             null,
             avg.Ptr);
      }

      private float[] EigenDecomposite(Image<Gray, Byte> src, Image<Gray, Single>[] eigenImages, Image<Gray, Single> avg)
      {
          return CvInvoke.cvEigenDecomposite(
              src.Ptr,
              Array.ConvertAll<Image<Gray, Single>, IntPtr>(eigenImages, delegate(Image<Gray, Single> img) { return img.Ptr; }),
              avg.Ptr);
      }

      private float[] GetEigenDistances(Image<Gray, Byte> image)
      {
         using (Matrix<float> eigenValue = new Matrix<float>(EigenDecomposite(image, _eiImgs, _avgImg)))
            return Array.ConvertAll<Matrix<float>, float>(_eiVals,
                delegate(Matrix<float> eigenValueI)
                {
                   return (float)CvInvoke.cvNorm(eigenValue.Ptr, eigenValueI.Ptr, Emgu.CV.CvEnum.NORM_TYPE.CV_L2, IntPtr.Zero);
                });
      }

      private void FindMostSimilarObject(Image<Gray, Byte> image, out int index, out float eigenDistance)
      {
         float[] dist = GetEigenDistances(image);

         index = 0;
         eigenDistance = dist[0];
         for (int i = 1; i < dist.Length; i++)
         {
            if (dist[i] < eigenDistance)
            {
               index = i;
               eigenDistance = dist[i];
            }
         }
      }

      public Face RecognizeFaces(Image<Gray, Byte> image)
      {
         int index;
         float eigenDistance;

         FindMostSimilarObject(image, out index, out eigenDistance);

         return (_eiDistanceThreshold <= 0 || eigenDistance < _eiDistanceThreshold )  ? _faces[index] :null;
      }
   }
}
