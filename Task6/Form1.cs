using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace task6
{
    public partial class Form1 : Form
    {
		Bitmap bmp;
		Graphics g;
		Pen pen_shape = new Pen(Color.Blue); // для фигуры
		Pen pen_axis = new Pen(Color.Black); // для осей
		int centerX, centerY; // центр pictureBox
		List<face> shape = new List<face>(); // фигура - список граней
		List<my_point> points = new List<my_point>(); // список образующих точек

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox.Size.Width, pictureBox.Size.Height);
            pictureBox.Image = bmp; centerX = pictureBox.Width / 2; centerY = pictureBox.Height / 2;
        }

		private void Form1_Load(object sender, EventArgs e)
		{
			g = Graphics.FromImage(bmp);
            NoShape.Checked = true;
            countRotation.Value = 20;
        }

        private void draw_point(my_point p) // рисуем точку
        {
            g.FillEllipse(new SolidBrush(Color.Red), (int)Math.Round(p.X + centerX - 3), (int)Math.Round(-p.Y + centerY - 3), 6, 6);
        }

        private void draw_face(face f) // рисуем грань
        {
            int n = f.points.Count - 1;

			int x1 = (int)Math.Round(f.points[0].X + centerX);
            int x2 = (int)Math.Round(f.points[n].X + centerX);
			int y1 = (int)Math.Round(-f.points[0].Y + centerY);
            int	y2 = (int)Math.Round(-f.points[n].Y + centerY);
			g.DrawLine(pen_shape, x1, y1, x2, y2);

			for (int i = 0; i < n; i++)
            {
				x1 = (int)Math.Round(f.points[i].X + centerX);
                x2 = (int)Math.Round(f.points[i + 1].X + centerX);
                y1 = (int)Math.Round(-f.points[i].Y + centerY);
                y2 = (int)Math.Round(-f.points[i + 1].Y + centerY);
                g.DrawLine(pen_shape, x1, y1, x2, y2);
            }
        }


        private void redraw_image() // перерисовываем картинку
        {
			g.Clear(Color.White);
			g.DrawLine(pen_axis, new Point(0, centerY), new Point(pictureBox.Width, centerY));
            g.DrawLine(pen_axis, new Point(centerX, 0), new Point(centerX, pictureBox.Height));
            foreach (my_point p in points)
                draw_point(p);
            foreach (face f in shape)
                draw_face(f);
            pictureBox.Image = bmp;
        }

        private void build_tetrahedron()
        {
            double h = Math.Sqrt(3) / 2 * 100;

            my_point p1 = new my_point(-50, -h/2, 0);
            my_point p2 = new my_point(0, -h/2, -h);
            my_point p3 = new my_point(50, -h/2, 0);
            my_point p4 = new my_point(0, h/2, 0);

			face f1 = new face();	f1.add(p1);		f1.add(p2);		f1.add(p3);		shape.Add(f1);
			face f2 = new face();	f2.add(p1);		f2.add(p4);		f2.add(p2);		shape.Add(f2);
			face f3 = new face();	f3.add(p4);		f3.add(p2);		f3.add(p3);		shape.Add(f3); 
			face f4 = new face();	f4.add(p1);		f4.add(p4);		f4.add(p3);		shape.Add(f4);
        }
        private void build_cube()
        {
            double h = 50;
            my_point p1 = new my_point(0, 0, 0);
            my_point p2 = new my_point(h, 0, 0);
            my_point p3 = new my_point(h, 0, h);
            my_point p4 = new my_point(0, 0, h);
            my_point p5 = new my_point(0, h, 0);
            my_point p6 = new my_point(h, h, 0);
            my_point p7 = new my_point(h, h, h);
            my_point p8 = new my_point(0, h, h);

            face f1 = new face();   f1.add(p1); f1.add(p2); f1.add(p3); f1.add(p4); shape.Add(f1);
            face f2 = new face(); f2.add(p5); f2.add(p6); f2.add(p7); f2.add(p8); shape.Add(f2);
            face f3 = new face(); f3.add(p1); f3.add(p2); f3.add(p6); f3.add(p5); shape.Add(f3);
            face f4 = new face(); f4.add(p2); f4.add(p6); f4.add(p7); f4.add(p3); shape.Add(f4);
            face f5 = new face(); f5.add(p7); f5.add(p3); f5.add(p4); f5.add(p8); shape.Add(f5);
            face f6 = new face(); f6.add(p4); f6.add(p8); f6.add(p5); f6.add(p1); shape.Add(f6);
        }

        private void displacement(double kx, double ky, double kz) // сдвиг
        {
            Matrix displM = displMatrix(kx, ky, kz);
            foreach(face f in shape) 
            {
                foreach(my_point p in f.points)
                {
                    p.updateFromVector(displM.Multiple(p.getVector()));
                }
            }
        }

        private void rotate(double xangle, double yangle, double zangle) // поворот
        {
            Matrix mx = rotationXMatrix(xangle); 
            Matrix my = rotationYMatrix(yangle);
            Matrix mz = rotationZMatrix(zangle);
            foreach (face f in shape)
            {
                foreach (my_point p in f.points)
                {
                    p.updateFromVector(mx.Multiple(p.getVector()));
                    p.updateFromVector(my.Multiple(p.getVector()));
                    p.updateFromVector(mz.Multiple(p.getVector()));
                }
            }
        }


        private my_point center_point() // центр фигуры
        {
            double sumX = 0, sumY = 0, sumZ = 0;
            int count = 0;
            for (int i = 0; i < shape.Count; i++)
                for (int j = 0; j < shape[i].points.Count; j++)
                {
                    sumX += shape[i].points[j].X;
                    sumY += shape[i].points[j].Y;
                    sumZ += shape[i].points[j].Z;
                    ++count;
                }
            return new my_point(sumX / count, sumY / count, sumZ / count);
        }

        private void scaling(double xScale, double yScale, double zScale) // масштабирование
        {
            my_point center_P = center_point();
            Matrix scale = scaleMatrix(xScale, yScale, zScale);
            Matrix displ1 = displMatrix(-center_P.X, -center_P.Y, -center_P.Z);
            Matrix displ2 = displMatrix(center_P.X, center_P.Y, center_P.Z);
            foreach (face f in shape)
            {
                foreach (my_point p in f.points)
                {
                    p.updateFromVector(displ1.Multiple(p.getVector()));
                    p.updateFromVector(scale.Multiple(p.getVector()));
                    p.updateFromVector(displ2.Multiple(p.getVector()));
                }
            }
        }

        private void displacement_button_Click(object sender, EventArgs e) // перенос
        {
            int kx = (int)x_shift.Value, ky = (int)y_shift.Value, kz = (int)z_shift.Value;
            displacement(kx, ky, kz);
            redraw_image();
        }


        private void rotate_button_Click(object sender, EventArgs e) // поворот
        {
           double xangle = ((double)x_rotate.Value * Math.PI) / 180;
            //double xangle = (double)x_rotate.Value;
            double yangle = ((double)y_rotate.Value * Math.PI) / 180;
            double zangle = ((double)z_rotate.Value * Math.PI) / 180;
            rotate(xangle, yangle, zangle);
            redraw_image();
        }

        private void scale_button_Click(object sender, EventArgs e) // масштабирование
        {
			scaling((double)x_scale.Value, (double)y_scale.Value, (double)z_scale.Value);
            redraw_image();
        }

        public Matrix displMatrix(double tx, double ty, double tz)
        {
            Matrix result = new Matrix(4, 4);
            double[,] m = {
                { 1, 0, 0, tx },
                { 0, 1, 0, ty },
                { 0, 0, 1, tz },
                { 0, 0, 0, 1 },
            };
            result.matrix = m;
            return result;
        }

        public Matrix scaleMatrix(double tx, double ty, double tz)
        {
            Matrix result = new Matrix(4, 4);
            double[,] m = {
                { 1/tx, 0, 0, 0 },
                { 0, 1/ty, 0, 0 },
                { 0, 0, 1/tz, 0 },
                { 0, 0, 0, 1 },
            };
            result.matrix = m;
            return result;
        }
        public Matrix rotationXMatrix(double angle)
        {
            Matrix result = new Matrix(4, 4);
            double[,] m = {
                { 1, 0, 0, 0 },
                { 0, Math.Cos(angle), -Math.Sin(angle), 0 },
                { 0, Math.Sin(angle), Math.Cos(angle), 0 },
                { 0, 0, 0, 1 },
            };
            result.matrix = m;
            return result;
        }

        public Matrix rotationYMatrix(double angle)
        {
            Matrix result = new Matrix(4, 4);
            double[,] m = {
                { Math.Cos(angle), 0, Math.Sin(angle), 0 },
                { 0, 1, 0, 0 },
                { -Math.Sin(angle), 0, Math.Cos(angle), 0 },
                { 0, 0, 0, 1 },
            };
            result.matrix = m;
            return result;
        }

        public Matrix rotationZMatrix(double angle)
        {
            Matrix result = new Matrix(4, 4);
            double[,] m = {
                { Math.Cos(angle), - Math.Sin(angle), 0, 0 },
                {  Math.Sin(angle), Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            };
            result.matrix = m;
            return result;
        }


        //Указать образующие
        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if( SetPoints.Checked)
            {
                my_point p = new my_point(e.X - centerX, centerY - e.Y, 0);
                points.Add(p);
            }
            redraw_image();
        }

        //Открыть из файла
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                shape.Clear();
                var read = new System.IO.StreamReader(openFileDialog1.FileName);
                string text;
                int n = Int32.Parse(read.ReadLine());
                shape = new List<face>();
                for (int i = 0; i < n; i++)
                {
                    text = read.ReadLine();
                    string[] coords = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    face f = new face();
                    for (int j = 0; j < coords.Count(); j += 3)
                    {           
                        f.add(new my_point(double.Parse(coords[j]), double.Parse(coords[j + 1]), double.Parse(coords[j + 2])));
                    }
                    shape.Add(f);
                }
                read.Dispose();
                redraw_image();
            }
        }

        //Сохранить в файл
        private void button2_Click(object sender, EventArgs e)
        {
            string file_name = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                file_name = saveFileDialog1.FileName;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(file_name))
            {
                file.WriteLine(shape.Count());
                foreach (face f in shape)
                {
                    foreach(my_point p in f.points)
                    {
                        file.Write(p.X.ToString() + " " + p.Y.ToString() + " " + p.Z.ToString() + " ");
                    }
                    file.WriteLine();
                }
             
            }
        }

        
		private void shape_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked == false)
				return;
			shape.Clear();
			if (tetrahedron.Checked)
				build_tetrahedron();
            if (Cube.Checked)
                build_cube();
			redraw_image();
		}

        private void button4_Click(object sender, EventArgs e)
        {
            shape.Clear();
            int count = (int)countRotation.Value;
            double angle = (360/ count * Math.PI) / 180;
            Matrix m = rotationYMatrix(angle);
            for (int c = 0; c < count; c++)
            {
                List<my_point> nextPoints = new List<my_point>();
                for (int i = 0; i < points.Count; i++)
                {
                    my_point p = new my_point();
                    p.updateFromVector(m.Multiple(points[i].getVector()));
                    nextPoints.Add(p);
                }
                for (int i = 1; i < points.Count; i++)
                {
                    face f = new face();
                    f.add(points[i - 1]);
                    f.add(points[i]);
                    f.add(nextPoints[i]);
                    f.add(nextPoints[i - 1]);
                    shape.Add(f);
                }
                points = nextPoints;
            }
            points.Clear();
            redraw_image();
        }

    }

    public class my_point
    {
        public double X, Y, Z;

        public my_point()
        {
            this.X = this.Y = this.Z = 0;
        }

        public my_point(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Matrix getVector()
        {
            Matrix result = new Matrix(4, 1);
            double[,] m ={
                { X }, {Y}, {Z }, {1 } 
            };
            result.matrix = m;
            return result;
        }
        public void updateFromVector(Matrix vector)
        {
            X = vector.matrix[0, 0];
            Y = vector.matrix[1, 0];
            Z = vector.matrix[2, 0];
        }
    }

    public class face
    {
        public List<my_point> points;

        public face()
        {
            points = new List<my_point>();
        }

        public void add(my_point p)
        {
            points.Add(p);
        }
    }

    public class Matrix
    {
        public double[,] matrix;

        public int Row { get; protected set; }
        public int Column { get; protected set; }
        public Matrix(int row, int column)
        {
            Row = row;
            Column = column;
            matrix = new double[row, column];
        }


        public Matrix Multiple(Matrix value)
        {
            Matrix result = new Matrix(Row, value.Column);
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < value.Column; j++)
                    for (int k = 0; k < value.Row; k++)
                        result.matrix[i, j] += matrix[i, k] * value.matrix[k, j];
            return result;
        }
        
    }
  
}
