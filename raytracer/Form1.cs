using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace raytracer
{

    

    public partial class Form1 : Form
    {
        List<Figure> scene = new List<Figure>();
        List<Light> lights = new List<Light>();
        Point3D[,] points;
        Color[,] colormap;
        Point3D f;
        Point3D cam_normal;

        const float EPS = 0.001f;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            build_scene();
            foreach (var f in scene)
                f.CalculateNormals();
           
            run();
        }

        public void build_scene() {

            var room = Figure.get_Hexahedron(10);
            var cam = new Figure(room);
           // cam.scale_axis(2, 2, 2);
            buildColorMap(cam.sides[0].get_point(0), cam.sides[0].get_point(1), cam.sides[0].get_point(3), cam.sides[0].get_point(2), pictureBox1.Width, pictureBox1.Height);
            f = CalculateFocus(5, cam.sides[0].get_point(0), cam.sides[0].get_point(1), cam.sides[0].get_point(3), cam.sides[0].get_point(2));
            room.mat = new Material(new Point3D(1, 0, 0), 0.4f, 0, 0,0.2f,0.4f,0.5f,1);
            Light l = new Light(new Point3D(0, 0, 4.5f));
            l.clr = new Point3D(new Point3D(1f, 1f, 1f));
            l.amb = new Point3D(new Point3D(0.5f, 0.5f, 0.5f));
            var obj = new Sphere(new Point3D(0, 0, -3), 3f);
            //var obj = Figure.get_Icosahedron(2);
            obj.offset(0, 0, -2);
            obj.set_pen(new Pen(Color.Silver));
            obj.mat = new Material(new Point3D(0, 0, 0),0,1f,1.5f,0.0f,0,1f,4);

            scene.Add(room);
            scene.Add(obj);
            lights.Add(l);

        }

        public void run() {
            for (int i = 0; i < pictureBox1.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Height; j++)
                {
                    Ray r = new Ray(f, points[i, j]);
                    r.start = points[i, j];
                   
                   var c = RayTrace(r, 10);

                    if (Math.Max(Math.Max(c.x, c.y), c.z) > 1)
                        c = Point3D.norm(c);
                        
                    colormap[i, j] = Color.FromArgb((int)(255 * c.x), (int)(255 * c.y), (int)(255 * c.z));
                    (pictureBox1.Image as Bitmap).SetPixel(i, j, colormap[i, j]);
                }
                pictureBox1.Invalidate();
            }
        }


        Point3D RayTrace(Ray r, int rec, float env = 1) {
            bool BackToAir = false;
            Point3D clr = new Point3D(0, 0, 0);
            if (rec <= 0)
                return clr;
            Hit h = GenerateHit(r);
            if (!h.succes)
                return clr;
            if (Point3D.scalar(r.dir, h.normal) > 0)
            {
                h.normal = h.normal * -1;
                BackToAir = true ;
            }

            Point3D hit_pos = r.tpos(h.hit_point);
            foreach (var l in lights)
            {
                clr += Point3D.blend(h.mat.clr, l.amb * h.mat.amb_coef);
                if (IsVisibleLight(l.points[0], hit_pos))
                    clr += h.Shade(l, hit_pos,f);
                
                    
            }

            if (h.mat.reflection_coef > 0)

            {

                Ray reflRay = h.Reflect(hit_pos);
                clr += h.mat.reflection_coef * RayTrace(reflRay,rec-1,env);

            }


            if (h.mat.refraction_coef > 0)

            {

                Ray refrRay = h.Refract(hit_pos,BackToAir ? env/1 : env/h.mat.env_coef );
                if(refrRay != null)
                    clr += h.mat.refraction_coef * RayTrace(refrRay,rec-1, h.mat.env_coef);

            }



            return clr;
        }

        Hit GenerateHit(Ray r) {
            float t = 0;
            Figure fig = null;
            Point3D n = null;
            foreach (Figure f in scene)
            {
                if (f.RayIntersection(r, out float t1, out Point3D norm)) 
                    if( (t == 0 || t1 < t) &&  t1 > 0) {
                        t = t1;
                        fig = f;
                        n = norm;
                    }
            }
            if (t != 0)
            
                return new Hit(t, r, n, fig.mat);
            
            else
                return new Hit();
        }


        public  bool IsVisibleLight(Point3D light, Point3D hit_pos)
        {
            float max_t = (light - hit_pos).length();
            float min_t = 0;
            Ray r = new Ray(hit_pos,light);

            foreach (Figure f in scene)
                if (f.RayIntersection(r, out float t, out Point3D p) && t < max_t &&  t > EPS)
                    return false;
            return true;


        }


        void buildColorMap(Point3D up1, Point3D up2, Point3D down1, Point3D down2, int w, int h)
        {
           points = new Point3D[w, h];
           colormap = new Color[w, h];

            Point3D stepup = (up2 - up1) / (w - 1);
            Point3D stepdown = (down2 - down1) / (w - 1);

            Point3D u = new Point3D(up1);
            Point3D d = new Point3D(down1);

            for (int i = 0; i < w; i++)
            {

                Point3D stepy = (u - d) / (h - 1);
                Point3D p = new Point3D(d);
                for (int j = 0; j < h; j++)
                {
                    points[i, j] = p;
                    p += stepy;
                }


                d += stepdown;
                u += stepup;

                

            }

            


        }

        Point3D CalculateFocus( float dist, Point3D up1, Point3D up2, Point3D down1, Point3D down2)
        {
            Point3D center = (up1 + up2 + down1 + down2) / 4;
            Point3D norm = Point3D.norm((up2 - up1)* (down1 - up1));
            cam_normal = norm;
            center += norm * dist;
            return center;

        }
    }
}
