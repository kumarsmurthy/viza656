using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Custom.Math;

namespace RayTracer2010
{
    public enum ShaderType
    {
        lambert = 0, 
        phong, 
        gooch
    };
    public class Shader
    {
        public ShaderType myShaderType = ShaderType.phong;

        public virtual void colorize(Ray r)
        {
            r.color = new vect3d(1, 1, 1);
        }

        protected void lightDirectPointSpec(Ray r, double specular_coeff, ref vect3d diff_accum, ref vect3d spec_accum)
        {
            Ray shadow_ray = new Ray();
            shadow_ray.pos = r.hit_pos;

            foreach (var l in r.scene.lights)
            {
                vect3d dir = l.pos - r.hit_pos;
                double dist2 = dir.length2();//dist2 is distance Squared
                double dist2_inv = 1.0 / dist2;
                dir *= Math.Sqrt(dist2_inv);


                // calculate incidences to do early reject of shadow ray
                double incidence_dif = (dir * r.hit_norm);
                vect3d dir_r = r.hit_norm * ((dir * r.hit_norm) * 2) - dir;
                double incidence_spec = (dir_r * r.hit_norm);

               if (incidence_dif < 0 && incidence_spec < 0)
                    continue;
                
                // shadow ray test
                shadow_ray.dir = dir;
                shadow_ray.hit_dist = Double.MaxValue;
                r.scene.trace(shadow_ray);
                if (shadow_ray.hit_dist * shadow_ray.hit_dist < dist2)
                    continue;
                
                // diffuse
                if (incidence_dif > 0)
                {
                    if(myShaderType == ShaderType.phong)
                        diff_accum += l.color * incidence_dif * dist2_inv;
                    else if (myShaderType == ShaderType.gooch)
                    {
                        vect3d warmColor = new vect3d(1,0.2,0.2);
                        vect3d coolColor = new vect3d(0.1, 0.3, 1);
                        double dotProd = dir * r.hit_norm;
                        diff_accum += ((1 + dotProd) * 0.5 * coolColor + (1 - dotProd) * 0.5 * warmColor);
                        //diff_accum += l.color * incidence_dif * dist2_inv;
                    }
                }
                // specular highlight
                if (incidence_spec > 0)
                    spec_accum += l.color * Math.Pow(incidence_spec, specular_coeff) * (dist2_inv*2);
            }
        }

        protected void lightDirectPointSpecNoShadow(Ray r, double specular_coeff, ref vect3d diff_accum, ref vect3d spec_accum)
        {
            Ray shadow_ray = new Ray();
            shadow_ray.pos = r.hit_pos;

            foreach (var l in r.scene.lights)
            {
                vect3d dir = l.pos - r.hit_pos;
                double dist2 = 1.0 / dir.length2();
                dir *= Math.Sqrt(dist2);

                // diffuse
                double incidence_dif = (dir * r.hit_norm);
                /*if (incidence_dif > 0)
                    diff_accum += l.color * incidence_dif * dist2;
                */
                if (myShaderType == ShaderType.phong)
                    diff_accum += l.color * incidence_dif * dist2;
                else if (myShaderType == ShaderType.gooch)
                {
                    vect3d warmColor = new vect3d(1, 0, 0);
                    vect3d coolColor = new vect3d(0, 0, 1);
                    double dotProd = dir * r.hit_norm;
                    diff_accum += ((1 + dotProd) * 0.5 * coolColor + (1 - dotProd) * 0.5 * warmColor);
                    //diff_accum += l.color * incidence_dif * dist2_inv;
                }
                // specular highlight
                vect3d dir_r = r.hit_norm * ((dir * r.hit_norm) * 2) - dir;
                double incidence_spec = (dir_r * r.hit_norm);
                if (incidence_spec > 0)
                    spec_accum += l.color * Math.Pow(incidence_spec, specular_coeff) * dist2;
            }
        }
    }
}
