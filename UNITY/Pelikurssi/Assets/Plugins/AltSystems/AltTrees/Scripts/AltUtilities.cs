using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace AltSystems.AltTrees
{
    public class AltUtilities
    {
        static public bool color32Equals(ref Color32 col1, ref Color32 col2)
        {
            if (col1.r == col2.r && col1.g == col2.g && col1.b == col2.b && col1.a == col2.a)
                return true;
            else
                return false;
        }

        static public void vector3Scale(ref Vector3 vect1, ref Vector3 vect2)
        {
            vect1.x *= vect2.x;
            vect1.y *= vect2.y;
            vect1.z *= vect2.z;
        }

        static public bool color32isZero(ref Color32 col1)
        {
            if (col1.r == 0 && col1.g == 0 && col1.b == 0 && col1.a == 0)
                return true;
            else
                return false;
        }

        static public float fastDistanceSqrt(ref Vector3 lhs, ref Vector3 rhs)
        {
            float _x, _y, _z;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            _z = lhs.z - rhs.z;
            return (_x * _x + _y * _y + _z * _z);
        }
        static public float fastDistanceSqrt(ref Vector3 lhs, Vector3 rhs)
        {
            float _x, _y, _z;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            _z = lhs.z - rhs.z;
            return (_x * _x + _y * _y + _z * _z);
        }
        static public float fastDistanceSqrt(Vector3 lhs, Vector3 rhs)
        {
            float _x, _y, _z;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            _z = lhs.z - rhs.z;
            return (_x * _x + _y * _y + _z * _z);
        }

        static public float fastDistance(ref Vector3 lhs, ref Vector3 rhs)
        {
            float _x, _y, _z;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            _z = lhs.z - rhs.z;
            return Mathf.Sqrt(_x * _x + _y * _y + _z * _z);
        }
        static public float fastDistanceSqrt2D(ref Vector2 lhs, ref Vector2 rhs)
        {
            float _x, _y;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            return (_x * _x + _y * _y);
        }
        static public float fastDistanceSqrt2D(ref Vector2 lhs, Vector3 rhs)
        {
            float _x, _y;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.z;
            return (_x * _x + _y * _y);
        }

        static public float fastDistance2D(ref Vector2 lhs, ref Vector2 rhs)
        {
            float _x, _y;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            return Mathf.Sqrt(_x * _x + _y * _y);
        }

        static public float fastDistance2D(ref Vector2 lhs, Vector2 rhs)
        {
            float _x, _y;
            _x = lhs.x - rhs.x;
            _y = lhs.y - rhs.y;
            return Mathf.Sqrt(_x * _x + _y * _y);
        }

        static public void WriteBytes(int value, byte[] buffer, int offset)
        {
            if (offset + 4 > buffer.Length)
                Debug.LogException(new System.IndexOutOfRangeException());

            if (checkEndian())
            {
                buffer[offset + 3] = (byte)(value >> 24);
                buffer[offset + 2] = (byte)(value >> 16);
                buffer[offset + 1] = (byte)(value >> 8);
                buffer[offset] = (byte)value;
            }
            else
            {
                buffer[offset] = (byte)(value >> 24);
                buffer[offset + 1] = (byte)(value >> 16);
                buffer[offset + 2] = (byte)(value >> 8);
                buffer[offset + 3] = (byte)value;
            }
        }

        static public int ReadBytesInt(byte[] buffer, int offset)
        {
            if (offset + 4 > buffer.Length)
                Debug.LogException(new System.IndexOutOfRangeException());

            int value = 0;

            if (checkEndian())
            {
                value = (value & ~(0xFF << 24)) | (buffer[offset + 3] << 24);
                value = (value & ~(0xFF << 16)) | (buffer[offset + 2] << 16);
                value = (value & ~(0xFF << 8)) | (buffer[offset + 1] << 8);
                value = (value & ~(0xFF << 0)) | (buffer[offset] << 0);
            }
            else
            {
                value = (value & ~(0xFF << 24)) | (buffer[offset] << 24);
                value = (value & ~(0xFF << 16)) | (buffer[offset + 1] << 16);
                value = (value & ~(0xFF << 8)) | (buffer[offset + 2] << 8);
                value = (value & ~(0xFF << 0)) | (buffer[offset + 3] << 0);
            }
            return value;
        }

        static public void WriteBytes(short value, byte[] buffer, int offset)
        {
            if (offset + 2 > buffer.Length)
                Debug.LogException(new System.IndexOutOfRangeException());

            if (checkEndian())
            {
                buffer[offset + 1] = (byte)(value >> 8);
                buffer[offset] = (byte)value;
            }
            else
            {
                buffer[offset] = (byte)(value >> 8);
                buffer[offset + 1] = (byte)value;
            }
        }

        static public short ReadBytesShort(byte[] buffer, int offset)
        {
            if (offset + 4 > buffer.Length)
                Debug.LogException(new System.IndexOutOfRangeException());

            int value = 0;

            if (checkEndian())
            {
                value = (value & ~(0xFF << 8)) | (buffer[offset + 1] << 8);
                value = (value & ~(0xFF << 0)) | (buffer[offset] << 0);
            }
            else
            {
                value = (value & ~(0xFF << 24)) | (buffer[offset] << 8);
                value = (value & ~(0xFF << 16)) | (buffer[offset + 1] << 0);
            }
            return (short)value;
        }



        static public void WriteBytes(float value, byte[] buffer, int offset)
        {
            WriteBytes(SingleToInt32Bits(value), buffer, offset);
        }

        static public float ReadBytesFloat(byte[] buffer, int offset)
        {
            return Int32ToSingleBits(ReadBytesInt(buffer, offset));
        }

        static public void WriteBytes(Vector3 value, byte[] buffer, int offset)
        {
            WriteBytes(SingleToInt32Bits(value.x), buffer, offset);
            WriteBytes(SingleToInt32Bits(value.y), buffer, offset + 4);
            WriteBytes(SingleToInt32Bits(value.z), buffer, offset + 8);
        }

        static public Vector3 ReadBytesVector3(byte[] buffer, int offset)
        {
            return new Vector3(ReadBytesFloat(buffer, offset), ReadBytesFloat(buffer, offset + 4), ReadBytesFloat(buffer, offset + 8));
        }

        static public void WriteBytes(Color value, byte[] buffer, int offset)
        {
            WriteBytes(SingleToInt32Bits(value.r), buffer, offset);
            WriteBytes(SingleToInt32Bits(value.g), buffer, offset + 4);
            WriteBytes(SingleToInt32Bits(value.b), buffer, offset + 8);
            WriteBytes(SingleToInt32Bits(value.a), buffer, offset + 12);
        }

        static public Color ReadBytesColor(byte[] buffer, int offset)
        {
            return new Color(ReadBytesFloat(buffer, offset), ReadBytesFloat(buffer, offset + 4), ReadBytesFloat(buffer, offset + 8), ReadBytesFloat(buffer, offset + 12));
        }

        static public void WriteBytes(bool value, byte[] buffer, int offset)
        {
            if (offset + 1 > buffer.Length)
                Debug.LogException(new System.IndexOutOfRangeException());

            buffer[offset] = BitConverter.GetBytes(value)[0];
        }

        static public bool ReadBytesBool(byte[] buffer, int offset)
        {
            return BitConverter.ToBoolean(buffer, offset);
        }


        static bool checkEndian()
        {
            #if !UNITY_EDITOR && UNITY_WIIU
				return true;
            #else
                return BitConverter.IsLittleEndian;
			#endif
        }

        [StructLayout(LayoutKind.Explicit)]
        struct Int32SingleUnion
        {
            [FieldOffset(0)]
            int i;
            [FieldOffset(0)]
            float f;

            internal Int32SingleUnion(int i)
            {
                this.f = 0;
                this.i = i;
            }

            internal Int32SingleUnion(float f)
            {
                this.i = 0;
                this.f = f;
            }

            internal int AsInt32
            {
                get { return i; }
            }

            internal float AsSingle
            {
                get { return f; }
            }
        }

        static int SingleToInt32Bits(float value)
        {
            return new Int32SingleUnion(value).AsInt32;
        }
        static float Int32ToSingleBits(int value)
        {
            return new Int32SingleUnion(value).AsSingle;
        }

        /*#if UNITY_IPHONE || UNITY_XBOX360
            [DllImport("__Internal", EntryPoint = "AltDotCpp")]
        #else
            [DllImport("AltUtilitiesCpp", EntryPoint = "AltDotCpp")]
        #endif
        public static extern float AltDotCpp(ref Vector3 lhs, ref Vector3 rhs);

        #if UNITY_IPHONE || UNITY_XBOX360
            [DllImport("__Internal", EntryPoint = "AltDotCpp")]
        #else
            [DllImport("AltUtilitiesCpp", EntryPoint = "boxInFrustumCpp")]
        #endif
        public static extern bool boxInFrustumCppDll(ref MyPlane myPlanes, ref Vector3 boxVertexes);*/

        /*static float AltDotCpp2(ref Vector3 lhs, ref Vector3 rhs)
        {
            //gch = GCHandle.Alloc(lhs, GCHandleType.Pinned);
            //gch2 = GCHandle.Alloc(rhs, GCHandleType.Pinned);
            float ret = AltDotCpp_dll(intPtr, intPtr2);

            //gch.Free();
            //gch2.Free();

            return ret;
        }*/

        /*static bool boxInFrustumCpp(MyPlane[] myPlanes, Vector3[] boxVertexes)
        {
            return boxInFrustumCppDll(ref myPlanes[0], ref boxVertexes[0]);
        }
        */

        static float MyDot(ref Vector3 lhs, ref Vector3 rhs)
        {

            return (float)(lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z);
        }

        //  false - outside, true - inside or intersects
        public static bool boxInFrustum(MyPlane[] myPlanes, Bounds3D box)
        {
            int out_boxInFrustum = 0;
            int i_boxInFrustum = 0;
            //if (!enableFrustum)
            //    return GeometryUtility.TestPlanesAABB(planes, box.bound);

            for (i_boxInFrustum = 0; i_boxInFrustum < 6; i_boxInFrustum++)
            {
                out_boxInFrustum = 0;

                /*out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.min.x, box.min.y, box.min.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.max.x, box.min.y, box.min.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.min.x, box.max.y, box.min.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.max.x, box.max.y, box.min.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.min.x, box.min.y, box.max.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.max.x, box.min.y, box.max.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.min.x, box.max.y, box.max.z)) < 0.0) ? 1 : 0);
                out_boxInFrustum += ((Vector3.Dot(myPlanes[i_boxInFrustum].normal, new Vector3(box.max.x, box.max.y, box.max.z)) < 0.0) ? 1 : 0);*/

                /*
                if (!enableFrustum)
                {
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[0]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[1]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[2]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[3]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[4]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[5]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[6]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[7]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                }
                else
                {
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[0]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[1]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[2]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[3]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[4]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[5]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[6]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                    out_boxInFrustum += ((AltDotCpp(ref myPlanes[i_boxInFrustum].normal, ref boxVertexes[7]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                }*/
                
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[0]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[1]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[2]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[3]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[4]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[5]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[6]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);
                out_boxInFrustum += ((MyDot(ref myPlanes[i_boxInFrustum].normal, ref box._Vertexes[7]) < -myPlanes[i_boxInFrustum].distance) ? 1 : 0);

                if (out_boxInFrustum == 8)
                    return false;
            }

            return true;
        }


        static System.Random rand = null;
        static System.Object randLock = null;

        static public void initRandom()
        {
            if (rand == null)
            {
                rand = new System.Random();
                randLock = new System.Object();
            }
        }

        static public double getRandomDouble()
        {
            double randDouble;
            lock (randLock)
            {
                randDouble = rand.NextDouble();
            }
            return randDouble;
        }
    }

    public class Bounds2D
    {
        public float left;
        public float right;
        public float up;
        public float down;

        public Bounds2D(Vector2 pos, float size)
        {
            left = pos.x - size / 2f;
            right = pos.x + size / 2f;

            up = pos.y + size / 2f;
            down = pos.y - size / 2f;
        }

        public Bounds2D(float _left, float _right, float _up, float _down)
        {
            left = _left;
            right = _right;
            up = _up;
            down = _down;
        }

        public bool inBounds(float _posX, float _posY, int quadId = 0)
        {
            if ((_posX > left && _posX <= right) && (_posY > down && _posY <= up))
                return true;
            else
                return false;
        }

        public bool inBounds(Vector2 _pos)
        {
            if ((_pos.x > left && _pos.x <= right) && (_pos.y > down && _pos.y <= up))
                return true;
            else
                return false;
        }

        public bool isIntersection(Bounds2D bound)
        {
            if (left > bound.right || right < bound.left || up < bound.down || down > bound.up)
                return false;
            else
                return true;
        }
    }

    //[StructLayout(LayoutKind.Explicit, Size = 16)]
    public class MyPlane
    {
        //[FieldOffset(0)]
        public Vector3 normal;
        //[FieldOffset(12)]
        public float distance;

        public MyPlane(Plane pl)
        {
            normal = pl.normal;
            distance = pl.distance;
        }
        public MyPlane()
        {

        }
        public void setPlane(ref Plane pl)
        {
            normal = pl.normal;
            distance = pl.distance;
        }
    }

    public class Bounds3D
    {
        private Vector3 _Center;
        private Vector3 _Size;
        private Vector3 _Extents;
        public Vector3[] _Vertexes;
        private bool init = false;
        public Bounds bound;

        /*public Vector3[] _vertexes
        {
            get
            {
                if (!init)
                {
                    CreateVertexes();
                    RefreshVertexes();
                }
                return this._Vertexes;
            }
        }*/

        public Vector3 center
        {
            get
            {
                return this._Center;
            }
            set
            {
                if (!init)
                    CreateVertexes();
                this._Center = value;
                bound.center = value;
                RefreshVertexes();
            }
        }

        public Vector3 size
        {
            get
            {
                return this._Size;
            }
            set
            {
                if (!init)
                    CreateVertexes();
                this._Size = value;
                this._Extents = value * 0.5f;
                bound.extents = value * 0.5f;
                RefreshVertexes();
            }
        }

        public Vector3 extents
        {
            get
            {
                return this._Extents;
            }
            set
            {
                if (!init)
                    CreateVertexes();
                this._Size = value * 2f;
                this._Extents = value;
                bound.extents = value;
                RefreshVertexes();
            }
        }

        public Bounds3D()
        {
            this._Center = Vector3.zero;
            this._Size = Vector3.zero;
            bound = new Bounds();
            CreateVertexes();
            RefreshVertexes();
        }

        public Bounds3D(Vector3 center, Vector3 size)
        {
            this._Center = center;
            this._Size = size;
            bound = new Bounds();
            CreateVertexes();
            RefreshVertexes();
        }

        private void CreateVertexes()
        {
            _Vertexes = new Vector3[8];
            _Vertexes[0] = new Vector3();
            _Vertexes[1] = new Vector3();
            _Vertexes[2] = new Vector3();
            _Vertexes[3] = new Vector3();
            _Vertexes[4] = new Vector3();
            _Vertexes[5] = new Vector3();
            _Vertexes[6] = new Vector3();
            _Vertexes[7] = new Vector3();

            init = true;
        }

        private void RefreshVertexes()
        {
            _Vertexes[0].Set(this.center.x - this.extents.x, this.center.y - this.extents.y, this.center.z - this.extents.z);
            _Vertexes[1].Set(this.center.x + this.extents.x, this.center.y - this.extents.y, this.center.z - this.extents.z);
            _Vertexes[2].Set(this.center.x - this.extents.x, this.center.y + this.extents.y, this.center.z - this.extents.z);
            _Vertexes[3].Set(this.center.x + this.extents.x, this.center.y + this.extents.y, this.center.z - this.extents.z);
            _Vertexes[4].Set(this.center.x - this.extents.x, this.center.y - this.extents.y, this.center.z + this.extents.z);
            _Vertexes[5].Set(this.center.x + this.extents.x, this.center.y - this.extents.y, this.center.z + this.extents.z);
            _Vertexes[6].Set(this.center.x - this.extents.x, this.center.y + this.extents.y, this.center.z + this.extents.z);
            _Vertexes[7].Set(this.center.x + this.extents.x, this.center.y + this.extents.y, this.center.z + this.extents.z);
        }
    }
}
