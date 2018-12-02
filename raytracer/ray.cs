using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace raytracer
{
    public class Ray
    {
        public static  float EPSILON = 0.0001f;

        public Point3D start, dir;

        public Ray(Point3D st, Point3D end) {
            start = new Point3D(st);
            dir = Point3D.norm(end-st);
            
        }

        private Ray() { }

        public Point3D tpos(float t)
        {
            return start + dir * t;
        }

        public static Ray buildRay(Point3D st, Point3D dir)
        {
            Ray r = new Ray();
            r.start = new Point3D(st);
            r.dir = new Point3D(dir);
            return r;
        }

               
        public bool intersectTriangle(Point3D vertex0 ,Point3D vertex1, Point3D vertex2,out float t)
               {
                   t = -1;
                   const float EPSILON = 0.0001f;
                   Point3D edge1, edge2, h, s, q;
                   float a, f, u, v;
                   edge1 = vertex1 - vertex0;
                   edge2 = vertex2 - vertex0;
                   h = dir*edge2;
                   a = Point3D.scalar(edge1,h);
                   if (a > -EPSILON && a < EPSILON)
                       return false;    // This ray is parallel to this triangle.
                   f = 1.0f / a;
                   s = start - vertex0;
                   u = Point3D.scalar(s,h)*f;
                   if (u < 0.0 || u > 1.0)
                       return false;
                   q = s*edge1;
                   v = Point3D.scalar(dir,q)*f ;
                   if (v < 0.0 || u + v > 1.0)
                       return false;
                   // At this stage we can compute t to find out where the intersection point is on the line.
                   t = Point3D.scalar(edge2,q)*f ;
                   if (t > EPSILON) // ray intersection
                   {

                       return true;
                   }
                   else // This means that there is a line intersection but not a ray intersection.
                       return false;
               }
       
      
}


    public class Hit
    {
        public float hit_point;
        public Ray casted_ray;
        public Point3D normal;
        public Material mat;
        public bool succes;

        public Hit(float hp, Ray r, Point3D n, Material m)
        {
            hit_point = hp;
            casted_ray = r;
            normal = new Point3D(n);
            mat = m;
            succes = true;
        }

        

        public Hit()
        {
            succes = false;
            hit_point = -1;
            casted_ray = null;
            normal = null;
           
        }

        public Point3D Shade(Light l, Point3D hp, Point3D eye) {
          //  Point3D d = Point3D.norm(hp - l.position);
          //  d = l.clr * Math.Max(Point3D.scalar(d, normal),0) ;

            Point3D l2 = Point3D.norm(l.position - hp);
            Point3D v2 = Point3D.norm(eye - hp);
            Point3D r = reflectVec(l2 * -1, normal);
            Point3D diff = mat.dif_coef * l.clr * Math.Max(Point3D.scalar(normal, l2), 0.0f);
            Point3D spec = mat.spec_coef * l.clr * (float)Math.Pow(Math.Max(Point3D.scalar(r,v2), 0.0f), mat.shine_coef);

            
            return Point3D.blend(diff, mat.clr);
        }

        public Ray Reflect(Point3D hp)
        {
            return Ray.buildRay(hp, Point3D.norm(casted_ray.dir - 2*normal* Point3D.scalar(normal,casted_ray.dir)));

        }

        public static Point3D reflectVec(Point3D v, Point3D n)
        {
            return Point3D.norm(v - 2 * n * Point3D.scalar(n, v));

        }

        public Ray Refract(Point3D hp, float eta)
        {
            float nidot = Point3D.scalar(normal, casted_ray.dir);
            float k = 1.0f - eta * eta * (1.0f - nidot*nidot);
            if (k >= 0)
            {
                k = (float)Math.Sqrt(k);
                return Ray.buildRay(hp, Point3D.norm(eta * casted_ray.dir - (k+eta*nidot) * normal));
            }
            else
                return null;
                

        }



    }

    public struct Material
    {
        public Point3D clr;
        public float reflection_coef;
        public float refraction_coef;
        public float env_coef;

        public float amb_coef;
        public float dif_coef;
        public float spec_coef;
        public float shine_coef;

        public Material(Point3D c,float refl,float refr, float ec, float ac, float dc, float sc, float shc) {
            clr =new Point3D(c);
            reflection_coef = refl;
            refraction_coef = refr;
            env_coef = ec;
            amb_coef = ac;
            dif_coef = dc;
            spec_coef = sc;
            shine_coef = shc;

        }

        public void SetColor(Color c) {
            clr = new Point3D(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
        }

    }



}
