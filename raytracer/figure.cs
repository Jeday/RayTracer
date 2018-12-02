using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace raytracer
{
    public class Point3D
    {
        public float x, y, z;

        public Point3D()
        {
            x = 0;
            y = 0;
            z = 0;

        }
        public Point3D(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public float length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);

        }

        public Point3D(Point3D p)
        {
            if (p == null)
                return;
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public override string ToString()
        {
            return String.Format("X:{0:f1} Y:{1:f1} Z:{2:f1}", x, y, z);
        }
        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);

        }

       
        

        public static float scalar(Point3D p1, Point3D p2)
        {
            return p1.x * p2.x + p1.y * p2.y + p1.z * p2.z;
        }

        public static Point3D norm(Point3D p)
        {
            float z = (float)Math.Sqrt((float)(p.x * p.x + p.y * p.y + p.z * p.z));
            if (z == 0) z = 1;
            return new Point3D(p.x / z, p.y / z, p.z / z);
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);

        }


        public static Point3D operator *(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.y * p2.z - p1.z * p2.y, p1.z * p2.x - p1.x * p2.z, p1.x * p2.y - p1.y * p2.x);
        }

        public static Point3D operator *(Point3D p1,float t)
        {
            return new Point3D(p1.x *t, p1.y * t, p1.z *t);
        }

        public static Point3D operator * (float t, Point3D p1)
        {
            return new Point3D(p1.x * t, p1.y * t, p1.z * t);
        }

        public static Point3D operator-(Point3D p1, float t)
        {
            return new Point3D(p1.x - t, p1.y - t, p1.z - t);
        }

        public static Point3D operator -(float t, Point3D p1)
        {
            return new Point3D( t- p1.x, t - p1.y,  t - p1.z);
        }

        public static Point3D operator +(Point3D p1, float t)
        {
            return new Point3D(p1.x + t, p1.y + t, p1.z + t);
        }

        public static Point3D operator +(float t, Point3D p1)
        {
            return new Point3D(p1.x + t, p1.y + t, p1.z + t);
        }

        public static Point3D operator/(Point3D p1, float t)
        {
            return new Point3D(p1.x / t, p1.y / t, p1.z / t);
        }

        public static Point3D operator/(float t, Point3D p1)
        {
            return new Point3D(t / p1.x, t / p1.y, t / p1.z);
        }

        public static Point3D blend(Point3D c1, Point3D c2)
        {
            return new Point3D(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);

        }

        

    }


    public class Side
    {
        public Figure host = null;
        public List<int> points = new List<int>();
        public Pen drawing_pen = new Pen(Color.Black);
        public Point3D Normal;
        public bool IsVisible = false;

        public Side(Figure h = null)
        {
            host = h;
        }
        public Side(Side s)
        {
            points = new List<int>(s.points);
            host = s.host;
            drawing_pen = s.drawing_pen.Clone() as Pen;
            Normal = new Point3D(s.Normal);
            IsVisible = s.IsVisible;
        }
        public Point3D get_point(int ind)
        {
            if (host != null)
                return host.points[points[ind]];
            return null;
        }

        public static Point3D norm(Side S)
        {
            if (S.points.Count() < 3)
                return new Point3D(0, 0, 0);
            Point3D U = S.get_point(1) - S.get_point(0);
            Point3D V = S.get_point(S.points.Count - 1) - S.get_point(0);
            Point3D normal =  U*V;
            return Point3D.norm(normal);
        }

        public void CalculateSideNormal()
        {
            Normal = norm(this);
        }

        public void CalculateVisibilty(Point3D cntr)
        {
            if (Normal == null)
                IsVisible = true;
            else
                IsVisible = Point3D.scalar(cntr - get_point(0), Normal) < 0;

        }


    }

    public class Figure
    {

        public List<Point3D> points = new List<Point3D>(); // точки 
        public List<Side> sides = new List<Side>(); // стороны
        public Material mat;
        public Figure()
        {
        }

        // redo for new members
        public Figure(Figure f)
        {
            foreach (Point3D p in f.points)
            {
                points.Add(new Point3D(p));
            }
            foreach (Side s in f.sides)
            {
                sides.Add(new Side(s));
                sides.Last().host = this;
            }
           
        }

        public void CalculateNormals() {
            foreach (Side s in sides)
            {
                s.Normal = Side.norm(s);
            }
        }


        public virtual bool RayIntersection(Ray r, out float t, out Point3D normal) {
            t = 0;
            normal = null;
            Side best_side = null;
            
            foreach(Side s in sides)
            {
                switch (s.points.Count)
                {
                    case 3:
                        if(r.intersectTriangle(s.get_point(0), s.get_point(1), s.get_point(2),out float t1) && (t == 0 || t1 < t))
                        {
                            t = t1;
                            best_side = s;                   
                        }
                        break;
                    case 4:
                        if (r.intersectTriangle(s.get_point(0), s.get_point(1), s.get_point(3), out float t2) && (t == 0 || t2 < t))
                        {
                            t = t2;
                            best_side = s;
                        }
                        else if (r.intersectTriangle(s.get_point(3), s.get_point(1), s.get_point(2), out t2) && (t == 0 || t2 < t))
                        {
                            t = t2;
                            best_side = s;
                        }
                        break;
                    default:
                        break;
                }


            }


            if (best_side != null) {
                normal = Side.norm(best_side);
                mat.SetColor(best_side.drawing_pen.Color);
                return true;
            }

            return false;
        }


        /// <summary>
        ///  Calculate visibility of each side and lighting intensifyer of every visible vertex
        /// </summary>
        /// <param name="eye_pos"> Postion of Camera</param>
        /// <param name="light_pos">Position of ligthing</param>
       


        ///
        /// ----------------------------- TRANSFORMS  SUPPORT METHODS --------------------------------
        ///


        public float[,] get_matrix()
        {
            var res = new float[points.Count, 4];
            for (int i = 0; i < points.Count; i++)
            {
                res[i, 0] = points[i].x;
                res[i, 1] = points[i].y;
                res[i, 2] = points[i].z;
                res[i, 3] = 1;
            }
            return res;
        }
        public void apply_matrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = matrix[i, 0] / matrix[i, 3];
                points[i].y = matrix[i, 1] / matrix[i, 3];
                points[i].z = matrix[i, 2] / matrix[i, 3];

            }
        }
        private Point3D get_center()
        {
            Point3D res = new Point3D(0, 0, 0);
            foreach (Point3D p in points)
            {
                res.x += p.x;
                res.y += p.y;
                res.z += p.z;

            }
            res.x /= points.Count();
            res.y /= points.Count();
            res.z /= points.Count();
            return res;
        }


        ///
        /// ----------------------------- APHINE TRANSFORMS METHODS --------------------------------
        ///

        public void rotate_around_rad(float rangle, string type)
        {
            float[,] mt = get_matrix();
            Point3D center = get_center();
            switch (type)
            {
                case "CX":
                    mt = apply_offset(mt, -center.x, -center.y, -center.z);
                    mt = apply_rotation_X(mt, rangle);
                    mt = apply_offset(mt, center.x, center.y, center.z);
                    break;
                case "CY":
                    mt = apply_offset(mt, -center.x, -center.y, -center.z);
                    mt = apply_rotation_Y(mt, rangle);
                    mt = apply_offset(mt, center.x, center.y, center.z);
                    break;
                case "CZ":
                    mt = apply_offset(mt, -center.x, -center.y, -center.z);
                    mt = apply_rotation_Z(mt, rangle);
                    mt = apply_offset(mt, center.x, center.y, center.z);
                    break;
                case "X":
                    mt = apply_rotation_X(mt, rangle);
                    break;
                case "Y":
                    mt = apply_rotation_Y(mt, rangle);
                    break;
                case "Z":
                    mt = apply_rotation_Z(mt, rangle);
                    break;
                default:
                    break;
            }
            apply_matrix(mt);
        }
        public void rotate_around(float angle, string type)
        {
            rotate_around_rad(angle * (float)Math.PI / 180, type);
        }
        public void scale_axis(float xs, float ys, float zs)
        {
            float[,] pnts = get_matrix();
            pnts = apply_scale(pnts, xs, ys, zs);
            apply_matrix(pnts);
        }
        public void offset(float xs, float ys, float zs)
        {
            apply_matrix(apply_offset(get_matrix(), xs, ys, zs));
        }

        public void set_pen(Pen dw)
        {
            foreach (Side s in sides)
                s.drawing_pen = dw;

        }
        public void set_rand_color()
        {
            Random r = new Random();
            foreach (Side s in sides)
            {

                Color c = Color.FromArgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
                s.drawing_pen = new Pen(c);
            }

        }


        public void scale_around_center(float xs, float ys, float zs)
        {
            float[,] pnts = get_matrix();
            Point3D p = get_center();
            pnts = apply_offset(pnts, -p.x, -p.y, -p.z);
            pnts = apply_scale(pnts, xs, ys, zs);
            pnts = apply_offset(pnts, p.x, p.y, p.z);
            apply_matrix(pnts);
        }
        public void line_rotate_rad(float rang, Point3D p1, Point3D p2)
        {

            p2 = new Point3D(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
            p2 = Point3D.norm(p2);

            float[,] mt = get_matrix();
            apply_matrix(rotate_around_line(mt, p1, p2, rang));
        }
        /// <summary>
        /// rotate figure line
        /// </summary>
        /// <param name="ang">angle in degrees</param>
        /// <param name="p1">line start</param>
        /// <param name="p2">line end</param>
        public void line_rotate(float ang, Point3D p1, Point3D p2)
        {
            ang = ang * (float)Math.PI / 180;
            line_rotate_rad(ang, p1, p2);
        }

  

        ///
        /// ----------------------------- STATIC BACKEND FOR TRANSFROMS --------------------------------
        ///

        private static float[,] rotate_around_line(float[,] transform_matrix, Point3D start, Point3D dir, float angle)
        {
            float cos_angle = (float)Math.Cos(angle);
            float sin_angle = (float)Math.Sin(angle);
            float val00 = dir.x * dir.x + cos_angle * (1 - dir.x * dir.x);
            float val01 = dir.x * (1 - cos_angle) * dir.y + dir.z * sin_angle;
            float val02 = dir.x * (1 - cos_angle) * dir.z - dir.y * sin_angle;
            float val10 = dir.x * (1 - cos_angle) * dir.y - dir.z * sin_angle;
            float val11 = dir.y * dir.y + cos_angle * (1 - dir.y * dir.y);
            float val12 = dir.y * (1 - cos_angle) * dir.z + dir.x * sin_angle;
            float val20 = dir.x * (1 - cos_angle) * dir.z + dir.y * sin_angle;
            float val21 = dir.y * (1 - cos_angle) * dir.z - dir.x * sin_angle;
            float val22 = dir.z * dir.z + cos_angle * (1 - dir.z * dir.z);
            float[,] rotateMatrix = new float[,] { { val00, val01, val02, 0 }, { val10, val11, val12, 0 }, { val20, val21, val22, 0 }, { 0, 0, 0, 1 } };
            return apply_offset(multiply_matrix(apply_offset(transform_matrix, -start.x, -start.y, -start.z), rotateMatrix), start.x, start.y, start.z);
        }
        private static float[,] multiply_matrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;

        }
        private static float[,] apply_offset(float[,] transform_matrix, float offset_x, float offset_y, float offset_z)
        {
            float[,] translationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { offset_x, offset_y, offset_z, 1 } };
            return multiply_matrix(transform_matrix, translationMatrix);
        }
        private static float[,] apply_rotation_X(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, (float)Math.Cos(angle), (float)Math.Sin(angle), 0 },
                { 0, -(float)Math.Sin(angle), (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return multiply_matrix(transform_matrix, rotationMatrix);
        }
        private static float[,] apply_rotation_Y(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), 0, -(float)Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { (float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return multiply_matrix(transform_matrix, rotationMatrix);
        }
        private static float[,] apply_rotation_Z(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 }, { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, { 0, 0, 0, 1} };
            return multiply_matrix(transform_matrix, rotationMatrix);
        }
        private static float[,] apply_scale(float[,] transform_matrix, float scale_x, float scale_y, float scale_z)
        {
            float[,] scaleMatrix = new float[,] { { scale_x, 0, 0, 0 }, { 0, scale_y, 0, 0 }, { 0, 0, scale_z, 0 }, { 0, 0, 0, 1 } };
            return multiply_matrix(transform_matrix, scaleMatrix);
        }
     

        ///
        /// --------------------SAVE/LOAD METHODS------------------------------------------
        ///

        public static Figure parse_figure(string filename)
        {
            Figure res = new Figure();
            List<string> lines = System.IO.File.ReadLines(filename).ToList();
            var st = lines[0].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (st[0] == "rotation")
                return parse_rotation(lines);
            else
            {
                int count_points = Int32.Parse(st[0]);
                Dictionary<string, int> pnts = new Dictionary<string, int>();

                for (int i = 0; i < count_points; ++i)
                {
                    string[] str = lines[i + 1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    res.points.Add(new Point3D(float.Parse(str[1]), float.Parse(str[2]), float.Parse(str[3])));
                    pnts.Add(str[0], i);
                }

                int count_sides = Int32.Parse(lines[count_points + 1]);
                for (int i = count_points + 2; i < lines.Count(); ++i)
                {
                    Side s = new Side(res);
                    List<string> str = lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var id in str)
                        s.points.Add(pnts[id]);
                    res.sides.Add(s);
                }

                res.set_pen(new Pen(Color.Red));
                return res;
            }
        }

        public static Figure parse_rotation(List<string> lines)
        {

            string[] cnt = lines[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int count_points = Int32.Parse(cnt[0]);
            int count_divs = Int32.Parse(cnt[1]);

            if (count_points < 1 || count_divs < 1)
                return new Figure();

            List<Point3D> pnts = new List<Point3D>();
            for (int i = 2; i < count_points + 2; ++i)
            {
                string[] s = lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                pnts.Add(new Point3D(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
            }

            string[] str = lines[count_points + 2].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Point3D axis1 = new Point3D(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
            str = lines[count_points + 3].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Point3D axis2 = new Point3D(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));

            return get_Rotation(pnts, axis1, axis2, count_divs);
        }

        public static void save_figure(Figure fig, string filename)
        {
            List<string> lines = new List<string>();
            Dictionary<int, string> pnts = new Dictionary<int, string>();
            lines.Add(fig.points.Count().ToString());
            for (int i = 0; i < fig.points.Count(); ++i)
            {
                string ind = "p" + i.ToString();
                pnts.Add(i, ind);
                lines.Add(ind + ' ' + fig.points[i].x.ToString() + ' ' + fig.points[i].y.ToString() + ' ' + fig.points[i].z.ToString());
            }
            lines.Add(fig.sides.Count().ToString());
            for (int i = 0; i < fig.sides.Count(); ++i)
            {
                string side_points = "";
                foreach (int s in fig.sides[i].points)
                {
                    side_points += pnts[s] + ' ';
                }
                lines.Add(side_points);
            }
            System.IO.File.WriteAllLines(filename, lines);
        }

        ///
        /// ------------------------STATIC READY FIGURES-----------------------------
        ///

        static public Figure get_Hexahedron(float sz)
        {
            Figure res = new Figure();
            res.points.Add(new Point3D(sz / 2, sz / 2, sz / 2)); // 0 
            res.points.Add(new Point3D(-sz / 2, sz / 2, sz / 2)); // 1
            res.points.Add(new Point3D(-sz / 2, sz / 2, -sz / 2)); // 2
            res.points.Add(new Point3D(sz / 2, sz / 2, -sz / 2)); //3

            res.points.Add(new Point3D(sz / 2, -sz / 2, sz / 2)); // 4
            res.points.Add(new Point3D(-sz / 2, -sz / 2, sz / 2)); //5
            res.points.Add(new Point3D(-sz / 2, -sz / 2, -sz / 2)); // 6
            res.points.Add(new Point3D(sz / 2, -sz / 2, -sz / 2)); // 7



            Side s = new Side(res);
            s.points.AddRange(new int[] { 3, 2, 1, 0 });
            s.drawing_pen = new Pen(Color.Aquamarine);
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 4, 5, 6, 7 });
            s.drawing_pen = new Pen(Color.Green);
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 2, 6, 5, 1 });
            s.drawing_pen = new Pen(Color.Red);
            res.sides.Add(s);

            s = new Side(res);
            s.drawing_pen = new Pen(Color.Yellow);
            s.points.AddRange(new int[] { 0, 4, 7, 3 });
            res.sides.Add(s);

            s = new Side(res);
            s.drawing_pen = new Pen(Color.Blue);
            s.points.AddRange(new int[] { 1, 5, 4, 0 });

            res.sides.Add(s);

            s = new Side(res);
            s.drawing_pen = new Pen(Color.Brown);
            s.points.AddRange(new int[] { 2, 3, 7, 6 });
            res.sides.Add(s);

            return res;
        }

        static public Figure get_Coordinates()
        {
            Figure res = new Figure();
            res.points.Add(new Point3D(0, 0, 0));

            res.points.Add(new Point3D(0, 100, 0));
            res.points.Add(new Point3D(100, 0, 0));
            res.points.Add(new Point3D(0, 0, 100));

            res.sides.Add(new Side(res));
            res.sides.Last().points = new List<int> { 0, 1 };
            res.sides.Last().drawing_pen.Color = Color.Green;
            res.sides.Add(new Side(res));
            res.sides.Last().points = new List<int> { 0, 2 };
            res.sides.Last().drawing_pen.Color = Color.Red;
            res.sides.Add(new Side(res));
            res.sides.Last().points = new List<int> { 0, 3 };
            res.sides.Last().drawing_pen.Color = Color.Blue;

            return res;
        }


        static public Figure get_Octahedron(float sz)
        {
            Figure res = new Figure();
            res.points.Add(new Point3D(sz / 2, 0, 0)); //0
            res.points.Add(new Point3D(-sz / 2, 0, 0)); //1
            res.points.Add(new Point3D(0, sz / 2, 0)); //2
            res.points.Add(new Point3D(0, -sz / 2, 0));//3
            res.points.Add(new Point3D(0, 0, sz / 2));//4
            res.points.Add(new Point3D(0, 0, -sz / 2));//5

            Side s = new Side(res);
            s.points.AddRange(new int[] { 0, 4, 3 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 0, 2, 4 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 4, 2 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 3, 4 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 0, 5, 2 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 2, 5 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 0, 3, 5 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 5, 3 });
            res.sides.Add(s);

            res.set_rand_color();

            return res;
        }

        static public Figure get_Tetrahedron(float sz)
        {
            Figure res = new Figure();
            sz = sz / 2;
            res.points.Add(new Point3D(sz, sz, sz));
            res.points.Add(new Point3D(-sz, -sz, sz));
            res.points.Add(new Point3D(sz, -sz, -sz));
            res.points.Add(new Point3D(-sz, sz, -sz));
            res.sides.Add(new Side(res));
            res.sides.Last().points.AddRange(new List<int> { 0, 1, 2 });
            res.sides.Add(new Side(res));
            res.sides.Last().points.AddRange(new List<int> { 1, 3, 2 });
            res.sides.Add(new Side(res));
            res.sides.Last().points.AddRange(new List<int> { 0, 2, 3 });
            res.sides.Add(new Side(res));
            res.sides.Last().points.AddRange(new List<int> { 0, 3, 1 });
            res.set_rand_color();
            return res;
        }

        static public Figure get_Torus(float sz, int d = 100)
        {

            sz /= 2;
            List<Point3D> crcl = new List<Point3D>();
            float ang = 0;
            float a = (float)(2 * Math.PI / d);
            for (int i = 0; i <= d; ++i)
            {
                crcl.Add(new Point3D((float)Math.Cos(ang) * sz, 0, (float)Math.Sin(ang) * sz));
                ang += a;
            }

            Figure res = get_Rotation(crcl, new Point3D(-(float)(sz * 2.5), 0, 0), new Point3D(-(float)(sz * 2.5), 0, 1), d);
            res.offset((float)(sz * 2.5), 0, 0);
            res.set_pen(new Pen(Color.Tomato));
            return res;
        }


        static public Figure get_Icosahedron(float sz)
        {
            Figure res = new Figure();
            float ang = (float)(Math.PI / 5);

            bool is_upper = true;
            int ind = 0;
            float a = 0;
            for (int i = 0; i < 10; ++i)
            {
                res.points.Add(new Point3D((float)Math.Cos((float)a), (float)Math.Sin((float)a), is_upper ? (float)0.5 : (float)-0.5));
                is_upper = !is_upper;
                ind++;
                a += ang;
            }
            Side s;
            for (int i = 0; i < ind; i++)
            {
                s = new Side(res);
                if (i % 2 == 0)
                {
                    s.points.AddRange(new int[] { i, (i + 1) % ind, (i + 2) % ind });
                    //  s.drawing_pen = new Pen(Color.Green);
                }
                else
                {
                    s.points.AddRange(new int[] { (i + 2) % ind, (i + 1) % ind, i });
                    //   s.drawing_pen = new Pen(Color.Red);
                }

                res.sides.Add(s);
            }




            res.points.Add(new Point3D(0, 0, (float)Math.Sqrt(5) / 2)); // ind
            res.points.Add(new Point3D(0, 0, -(float)Math.Sqrt(5) / 2)); // ind+1
            for (int i = 0; i < ind; i += 2)
            {
                s = new Side(res);
                s.points.AddRange(new int[] { i, ind, (i + 2) % ind });
                s.points.Reverse();

                res.sides.Add(s);
            }

            for (int i = 1; i < ind; i += 2)
            {
                s = new Side(res);
                s.points.AddRange(new int[] { i, (i + 2) % ind, ind + 1 });
                s.points.Reverse();
                res.sides.Add(s);
            }

            res.scale_around_center(sz, sz, sz);

            res.set_rand_color();
            return res;
        }

        public static Figure get_curve(float x0, float x1, float y0, float y1, int n_x, int n_y, Func<float, float, float> f)
        {
            float step_x = (x1 - x0) / n_x;
            float step_y = (y1 - y0) / n_y;
            Figure res = new Figure();

            float x = x0;
            float y = y0;

            for (int i = 0; i <= n_x; ++i)
            {
                y = y0;
                for (int j = 0; j <= n_y; ++j)
                {
                    res.points.Add(new Point3D(x, y, f(x, y)));
                    y += step_y;
                }
                x += step_x;
            }

            for (int i = 0; i < res.points.Count; ++i)
            {
                if ((i + 1) % (n_y + 1) == 0)
                    continue;
                if (i / (n_y + 1) == n_x)
                    break;

                Side s = new Side(res);
                s.points.AddRange(new int[] { i, i + 1, i + n_y + 2, i + n_y + 1 });
                s.points.Reverse();
                res.sides.Add(s);
            }
            res.set_rand_color();
            return res;
        }


        public static Figure get_Rotation(List<Point3D> pnts, Point3D axis1, Point3D axis2, int divs)
        {
            Figure res = new Figure();
            Figure edge = new Figure();
            int cnt_pnt = pnts.Count;
            edge.points = pnts.Select(x => new Point3D(x)).ToList();
            res.points = pnts.Select(x => new Point3D(x)).ToList();
            int cur_ind = res.points.Count;
            float ang = (float)360 / divs;
            for (int i = 0; i < divs; i++)
            {
                edge.line_rotate(ang, axis1, axis2);
                cur_ind = res.points.Count;
                for (int j = 0; j < cnt_pnt; j++)
                {
                    res.points.Add(new Point3D(edge.points[j]));

                }

                for (int j = cur_ind; j < res.points.Count - 1; j++)
                {
                    Side s = new Side(res);
                    s.points.AddRange(new int[] { j, j + 1, j + 1 - cnt_pnt, j - cnt_pnt });
                    res.sides.Add(s);

                }


            }




            res.set_pen(new Pen(Color.Magenta));
            return res;
        }
        ///
        /// ---------------------------------------------------------------------------------------
        ///

    }

    public class Sphere: Figure
    {
        public Point3D position
        {
            get
            {
                return points[0];

            }

            set
            {
                points[0] = value;

            }
        }

        public float rad;
        public Sphere(Point3D pos, float r) : base()
        {
            points.Add(pos);
            rad = r;

        }

        public override bool RayIntersection(Ray r, out float t, out Point3D normal)
        {
            t = 0;
            normal = null;
            Point3D k = r.start - position;
            float b = Point3D.scalar(k, r.dir);
            float c = Point3D.scalar(k, k) - rad * rad;
            float d = b * b - c;
            if (d >= 0)
            { 
                float sqrtfd = (float)Math.Sqrt(d);
                float t1 = -b + sqrtfd;
                float t2 = -b - sqrtfd;
                float min_t = Math.Min(t1, t2);
                float max_t = Math.Max(t1, t2);
                t = (min_t > Ray.EPSILON) ? min_t : max_t;
                if (t > Ray.EPSILON) {
                    normal = Point3D.norm(r.tpos(t) - position);
                    return true;
                }

            }
            return false;
            


        }
    }

    public class Light: Figure
    {
        public Point3D position
        {
            get
            {
                return points[0];

            }

            set
            {
                points[0] = value;

            }
        }
        public Point3D clr;
        public Point3D amb;
        public Light(Point3D pos): base()
        {
            points.Add(pos);

        }

    }
}
