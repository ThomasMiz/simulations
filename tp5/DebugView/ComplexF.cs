// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using Vector2 = System.Numerics.Vector2;

namespace tp5.DebugView
{
    public struct ComplexF
    {
        private static readonly ComplexF _one = new ComplexF(1, 0);
        private static readonly ComplexF _imaginaryOne = new ComplexF(0, 1);

        public float R;
        public float i;

        public static ComplexF One { get { return _one; } }
        public static ComplexF ImaginaryOne { get { return _imaginaryOne; } }

        public float Phase
        {
            get { return (float)Math.Atan2(i, R); }
            set 
            {
                if (value == 0)
                {
                    this = ComplexF.One;
                    return;
                }
                this.R = (float)Math.Cos(value);
                this.i = (float)Math.Sin(value);
            }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(MagnitudeSquared()); }
        }


        public ComplexF(float real, float imaginary)
        {
            R = real;
            i = imaginary;
        }
                
        public static ComplexF FromAngle(float angle)
        {
            if (angle == 0)
                return ComplexF.One;

            return new ComplexF(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle));
        }        

        public void Conjugate()
        {
            i = -i;
        }
                
        public void Negate()
        {
            R = -R;
            i = -i;
        }

        public float MagnitudeSquared()
        {
            return (R * R) + (i * i);
        }

        public void Normalize()
        {
            var mag = Magnitude;
            R = R / mag;
            i = i / mag;            
        }

        public Vector2 ToVector2()
        {
            return new Vector2(R, i);
        }
        
        public static ComplexF Multiply(ref ComplexF left, ref ComplexF right)
        {
            return new ComplexF( left.R * right.R  - left.i * right.i,
                                left.i * right.R  + left.R * right.i);
        }

        public static ComplexF Divide(ref ComplexF left, ref ComplexF right)
        {
            return new ComplexF( right.R * left.R + right.i * left.i,
                                right.R * left.i - right.i * left.R);
        }
        public static void Divide(ref ComplexF left, ref ComplexF right, out ComplexF result)
        {
            result = new ComplexF(right.R * left.R + right.i * left.i,
                                 right.R * left.i - right.i * left.R);
        }

        public static Vector2 Multiply(ref Vector2 left, ref ComplexF right)
        {
            return new Vector2(left.X * right.R - left.Y * right.i,
                               left.Y * right.R + left.X * right.i);
        }
        public static void Multiply(ref Vector2 left, ref ComplexF right, out Vector2 result)
        {
            result = new Vector2(left.X * right.R - left.Y * right.i,
                                 left.Y * right.R + left.X * right.i);
        }
        public static Vector2 Multiply(Vector2 left, ref ComplexF right)
        {
            return new Vector2(left.X * right.R - left.Y * right.i,
                               left.Y * right.R + left.X * right.i);
        }

        public static Vector2 Divide(ref Vector2 left, ref ComplexF right)
        {
            return new Vector2(left.X * right.R + left.Y * right.i,
                               left.Y * right.R - left.X * right.i);
        }

        public static Vector2 Divide(Vector2 left, ref ComplexF right)
        {
            return new Vector2(left.X * right.R + left.Y * right.i,
                               left.Y * right.R - left.X * right.i);
        }
        public static void Divide(Vector2 left, ref ComplexF right, out Vector2 result)
        {
            result = new Vector2(left.X * right.R + left.Y * right.i,
                                 left.Y * right.R - left.X * right.i);
        }
        
        public static ComplexF Conjugate(ref ComplexF value)
        {
            return new ComplexF(value.R, -value.i);
        }

        public static ComplexF Negate(ref ComplexF value)
        {
            return new ComplexF(-value.R, -value.i);
        }

        public static ComplexF Normalize(ref ComplexF value)
        {
            var mag = value.Magnitude;
            return new ComplexF(value.R / mag, -value.i / mag);
        }
        
        public override string ToString()
        {
            return String.Format("{{R: {0} i: {1} Phase: {2} Magnitude: {3}}}", R, i, Phase, Magnitude);
        }
    }
}