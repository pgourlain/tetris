using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace GeniusTetris
{
    public static class Tetris3DModelProvider
    {
        /// <summary>
        /// Create a face 1x1
        /// </summary>
        /// <returns></returns>
        static private MeshGeometry3D CreateGeometryFace1x1()
        {
            /*
             * square definition
             *       1------2
             *       |      |
             *       |      |
             *       0------3
             * triangle indice definition
             *       2---1
             *       |  /     
             *       |/      
             *       0
             * 
             *           2
             *          /|
             *        /  |
             *       0---1
             * */
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.Positions.Add(new Point3D(1, 1, 0));
            mesh.Positions.Add(new Point3D(1, 0, 0));
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(0, 0));
            return mesh;
        }

        static private Model3D CreateFaceModel1x1(Brush br)
        {
            GeometryModel3D geometry = new GeometryModel3D();

            geometry.Material = new DiffuseMaterial(br);
            geometry.BackMaterial = new DiffuseMaterial(br);

            geometry.Geometry = CreateGeometryFace1x1();
            return geometry;
        }

        static private ModelVisual3D CreateFace1x1(Brush br)
        {
            ModelVisual3D Result = new ModelVisual3D();

            Result.Content = CreateFaceModel1x1(br);

            return Result;
        }

        /// <summary>
        /// Create a 3D tetris shape, each shape is contructed with several faces
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        static public ModelVisual3D CreateShape(GeniusTetris.Core.Shape shape)
        {
            ModelVisual3D model = new ModelVisual3D();
            Model3DGroup group = new Model3DGroup();
            model.Content = group;
            byte currentColor = 0;
            for (int i = 0; i < shape.Width; i++)
            {
                for (int j = 0; j < shape.Height; j++)
                {
                    byte b = shape[i, j];
                    if (b > 0)
                    {
                        currentColor = b;
                        DrawingBrush dbrush = (DrawingBrush)Application.Current.Resources[string.Format("Block{0}", b)];
                        //Brush dbrush = new SolidColorBrush(Colors.Red);
                        Model3D face = CreateFaceModel1x1(dbrush);
                        face.Transform = GetTransformation(i, j, false);
                        group.Children.Add(face);
                        face = CreateFaceModel1x1(dbrush);
                        face.Transform = GetTransformation(i, j, true);
                        group.Children.Add(face);
                        //ajout des cotés
                        AddCotes(shape, group, i, j, dbrush);
                    }
                }
            }
            return model;
        }

        static private void AddCotes(GeniusTetris.Core.Shape shape, Model3DGroup group, int i, int j, Brush dbrush)
        {
            //Gauche
            if (i - 1 < 0 || shape[i - 1, j] == 0)
            {
                group.Children.Add(CreateFaceDroiteGauche(i, j, dbrush, true));
            }
            //Droit
            if (i + 1 >= shape.Width || shape[i + 1, j] == 0)
            {
                group.Children.Add(CreateFaceDroiteGauche(i, j, dbrush, false));
            }
            //Haut
            if (j - 1 < 0 || shape[i, j - 1] == 0)
            {
                group.Children.Add(CreateFaceHautBas(i, j, dbrush, true));
            }
            //Bas
            if (j + 1 >= shape.Height || shape[i, j + 1] == 0)
            {
                group.Children.Add(CreateFaceHautBas(i, j, dbrush, false));
            }
        }

        static private Model3D CreateFaceDroiteGauche(int i, int j, Brush dbrush, bool isgauche)
        {
            Model3D face = CreateFaceModel1x1(dbrush);

            Transform3DGroup grouptr = new Transform3DGroup();
            RotateTransform3D rotate = new RotateTransform3D();
            rotate.CenterX = isgauche ? 0 : 1;
            rotate.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), isgauche ? 90 : -90);
            grouptr.Children.Add(rotate);

            face.Transform = grouptr;
            grouptr.Children.Add(GetTransformation(i, j, false));
            return face;
        }

        static private Model3D CreateFaceHautBas(int i, int j, Brush dbrush, bool isbas)
        {
            Model3D face = CreateFaceModel1x1(dbrush);

            Transform3DGroup grouptr = new Transform3DGroup();
            RotateTransform3D rotate = new RotateTransform3D();
            rotate.CenterY = isbas ? 0 : 1;
            rotate.Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), isbas ? 270 : 90);
            grouptr.Children.Add(rotate);

            face.Transform = grouptr;
            grouptr.Children.Add(GetTransformation(i, j, false));
            return face;
        }

        static private Transform3D GetTransformation(int i, int j, bool back)
        {
            Transform3DGroup group = new Transform3DGroup();

            TranslateTransform3D translate = new TranslateTransform3D(i, j, 0);
            if (back)
            {
                group.Children.Add(new TranslateTransform3D(0, 0, -1));
            }
            group.Children.Add(translate);
            return group;
        }
    }
}
