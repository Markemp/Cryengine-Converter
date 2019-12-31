using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CgfConverter.CryEngineCore.Components;

namespace CgfConverter.Services
{
    public class TSpace
    {
        public CryEngineCore.Components.Vector4 Tangent { get; set; }
        public Vector3 Bitangent { get; set; }
        public Vector3 Normal { get; set; }

        //CIG Vertex Shader - translated assembly code
        public static TSpace VSassembly(Vector3 positions, uint tangentHex, uint bitangentHex, bool debug = false)
        {
            TSpace tspac = new TSpace();

            //REGISTERS
            CryEngineCore.Components.Vector4
                v0 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                v1 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                v2 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                v3 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                v4 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                v5 = new CryEngineCore.Components.Vector4(0, 0, 0, 0);
            CryEngineCore.Components.Vector4
                r0 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                r1 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                r2 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                r3 = new CryEngineCore.Components.Vector4(0, 0, 0, 0);
            CryEngineCore.Components.Vector4
                o0 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                o1 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                o3 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                o4 = new CryEngineCore.Components.Vector4(0, 0, 0, 0),
                o5 = new CryEngineCore.Components.Vector4(0, 0, 0, 0);
            Vector3 o2 = new Vector3();

            CryEngineCore.Components.Vector4 InstanceBufferRaw0 = new CryEngineCore.Components.Vector4(1, 0, 0, 31.85579f);
            CryEngineCore.Components.Vector4 InstanceBufferRaw1 = new CryEngineCore.Components.Vector4(0, 1, 0, -17.761f);
            CryEngineCore.Components.Vector4 InstanceBufferRaw2 = new CryEngineCore.Components.Vector4(0, 0, 1, 12.2023f);
            //Vector4 InstanceBufferRaw0 = new Vector4(1, 0, 0, 0);
            //Vector4 InstanceBufferRaw1 = new Vector4(0, 1, 0, 0);
            //Vector4 InstanceBufferRaw2 = new Vector4(0, 0, 1, 0);
            //--/
            //byte_array tanBA = new byte_array(), btanBA = new byte_array();
            //tanBA.uint1 = tangentHex;
            //btanBA.uint1 = bitangentHex;

            if (debug) Console.WriteLine("---VS DEBUG-------------------------------");

            //v0.x = positions.x; v0.y = positions.y; v0.z = positions.z;
            v3.xuint = tangentHex; if (debug) Console.WriteLine("v3.x: {0}", v3.x);
            v4.xuint = bitangentHex; if (debug) Console.WriteLine("v4.x: {0}", v4.x);

            r0.w = 1; if (debug) Console.WriteLine("0: r0.w: {0}", r0.w);
            // r1.xyz = v0.xyz * MeshPosDecompressionScale.xyz + MeshPosDecompressionBias.xyz;
            r1.x = (float)positions.x; r1.y = (float)positions.y; r1.z = (float)positions.z; if (debug) Console.WriteLine("1: r1.x: {0} r1.y: {1} r1.z: {2}", r1.x, r1.y, r1.z);
            r1.w = 1; if (debug) Console.WriteLine("2: r1.w: {0}", r1.w);
            r2.xint = v5.xint + 47; if (debug) Console.WriteLine("3: r2.x: {0}", r2.x);
            r2.xint = r2.xint * 14; if (debug) Console.WriteLine("4: r2.x: {0}", r2.x);
            r0.x = InstanceBufferRaw0.Dot(r1); if (debug) Console.WriteLine("5: r0.x: {0}", r0.x);
            r0.y = InstanceBufferRaw1.Dot(r1); if (debug) Console.WriteLine("6: r0.y: {0}", r0.y);
            r0.z = InstanceBufferRaw2.Dot(r1); if (debug) Console.WriteLine("7: r0.z: {0}", r0.z);
            //r0.x = dot(InstanceBuffer[r2.x]._m00_m01_m02_m03, r1.xyzw);r0.x = InstanceBufferRaw1.Dot(r1);
            //r0.y = dot(InstanceBuffer[r2.x]._m10_m11_m12_m13, r1.xyzw);
            //r0.z = dot(InstanceBuffer[r2.x]._m20_m21_m22_m23, r1.xyzw);
            //o0.x = dot(g_VS_ViewProjZeroMatr._m00_m01_m02_m03, r0.xyzw);
            //o0.y = dot(g_VS_ViewProjZeroMatr._m10_m11_m12_m13, r0.xyzw);
            //o0.z = dot(g_VS_ViewProjZeroMatr._m20_m21_m22_m23, r0.xyzw);
            //o0.w = dot(g_VS_ViewProjZeroMatr._m30_m31_m32_m33, r0.xyzw);
            o5.x = r0.x; o5.y = r0.y; o5.z = r0.z; if (debug) Console.WriteLine("12: o5.x: {0} o5.y: {1} o5.z: {2}", o5.x, o5.y, o5.z);//o5.xyz = r0.xyzx;
            r0.xuint = v3.xuint; if (debug) Console.WriteLine("13: r0.x: {0}", r0.x);
            r0.zuint = v4.xuint; if (debug) Console.WriteLine("14: r0.z: {0}", r0.z);
            r0.yuint = r0.xuint >> 15; r0.wuint = r0.zuint >> 15; if (debug) Console.WriteLine("15: r0.y: {0} r0.w: {1}", r0.y, r0.w);//r0.yw = (uint2)r0.xz >> int2(15, 15);
                                                                                                                                      //r1.xyzw = (int4)r0.xyzw & int4(32767, 32767, 32767, 32767);
            r1.xuint = r0.xuint & 0x00007fff;
            r1.yuint = r0.yuint & 0x00007fff;
            r1.zuint = r0.zuint & 0x00007fff;
            r1.wuint = r0.wuint & 0x00007fff; if (debug) Console.WriteLine("16: r1.x: {0} r1.y: {1} r1.z: {2} r1.w: {3}", r1.xint, r1.yint, r1.zint, r1.wint);
            //r0.xy = (int2)r0.xz & int2(2, 2);
            r0.xuint = r0.xuint & 0x40000000;
            r0.yuint = r0.zuint & 0x40000000; if (debug) Console.WriteLine("17: r0.x: {0} r0.y: {1}", r0.xint, r0.yint);
            //r1.xyzw = (int4)r1.xyzw + int4(-16383, -16383, -16383, -16383);
            r1.xuint = r1.xuint + 0xffffc001;
            r1.yuint = r1.yuint + 0xffffc001;
            r1.zuint = r1.zuint + 0xffffc001;
            r1.wuint = r1.wuint + 0xffffc001; if (debug) Console.WriteLine("18: r1.x: {0} r1.y: {1} r1.z: {2} r1.w: {3}", r1.xint, r1.yint, r1.zint, r1.wint);
            //r1.xyzw = (int4)r1.xyzw;
            r1.x = r1.xint;
            r1.y = r1.yint;
            r1.z = r1.zint;
            r1.w = r1.wint; if (debug) Console.WriteLine("19: r1.x: {0} r1.y: {1} r1.z: {2} r1.w: {3}", r1.xint, r1.yint, r1.zint, r1.wint);
            //r3.xyzw = float4(0.000061, 0.000061, 0.000061, 0.000061) * r1.xyzw;
            r3.x = r1.x * 6.10388815e-005f;
            r3.y = r1.y * 6.10388815e-005f;
            r3.z = r1.z * 6.10388815e-005f;
            r3.w = r1.w * 6.10388815e-005f; if (debug) Console.WriteLine("20: r3.x: {0} r3.y: {1} r3.z: {2} r3.w: {3}", r3.xint, r3.yint, r3.zint, r3.wint);
            r3.x = new Vector2(r3.x, r3.y).Dot(new Vector2(r3.x, r3.y)); if (debug) Console.WriteLine("21: r3.x: {0}", r3.xint);//r3.x = dot(r3.xy, r3.xy);
            r3.y = new Vector2(r3.z, r3.w).Dot(new Vector2(r3.z, r3.w)); if (debug) Console.WriteLine("22: r3.y: {0}", r3.yint);//r3.y = dot(r3.zw, r3.zw);
                                                                                                                                //r0.zw = float2(1, 1) + -r3.xy
            r0.z = 1.0f + (-r3.x);
            r0.w = 1.0f + (-r3.y); if (debug) Console.WriteLine("23: r0.z: {0} r0.w : {1}", r0.zint, r0.wint);
            //r0.zw = max(float2(0, 0), r0.zw);
            r0.z = Math.Max(0.0f, r0.z);
            r0.w = Math.Max(0.0f, r0.w); if (debug) Console.WriteLine("24: r0.z: {0} r0.w : {1}", r0.zint, r0.wint);
            //r0.zw = sqrt(r0.zw);
            r0.z = (float)Math.Sqrt(r0.z);
            r0.w = (float)Math.Sqrt(r0.w); if (debug) Console.WriteLine("25: r0.z: {0} r0.w : {1}", r0.zint, r0.wint);
            //r0.xy = r0.xy ? -r0.zw : r0.zw;
            if (r0.xuint != 0) r0.x = -r0.z; else r0.x = r0.z;
            if (r0.yuint != 0) r0.y = -r0.w; else r0.y = r0.w; if (debug) Console.WriteLine("26: r0.x: {0} r0.y : {1}", r0.xint, r0.yint);
            //r0.zw = r1.xy * float2(0.000061, 0.000061) + float2(0.000001, 0);
            r0.z = r1.y * 6.10388815e-005f;//<------- do spraawdzenia y zmiana z x
            r0.w = r1.x * 6.10388815e-005f + 9.99999997e-007f; if (debug) Console.WriteLine("27: r0.z: {0} r0.w : {1}", r0.zint, r0.wint);
            //r1.xy = r1.zw * float2(0.000061, 0.000061) + float2(-0.000001, 0);
            r1.x = r1.z * 6.10388815e-005f + (-9.99999997e-007f);
            r1.y = r1.w * 6.10388815e-005f; if (debug) Console.WriteLine("28: r1.x: {0} r1.y : {1}", r1.xint, r1.yint);
            //r3.x = dot(InstanceBuffer[r2.x]._m02_m00_m01, r0.xzw);
            if (debug) Console.WriteLine("  r0.x: {0} r0.y: {1} r0.z: {2} r0.w: {3}", r0.xint, r0.yint, r0.zint, r0.wint);
            if (debug) Console.WriteLine("  r1.x: {0} r1.y: {1} r1.z: {2} r1.w: {3}", r1.xint, r1.yint, r1.zint, r1.wint);
            r3.x = new CryEngineCore.Components.Vector3(InstanceBufferRaw0.z, InstanceBufferRaw0.y, InstanceBufferRaw0.x).Dot(new CryEngineCore.Components.Vector3(r0.x, r0.z, r0.w)); if (debug) Console.WriteLine("29: r3.x: {0} | r0.x {1} r0.y {2} r0.z {3} r0.w {4}", r3.x, r0.x, r0.y, r0.z, r0.w);
            //r3.y = dot(InstanceBuffer[r2.x]._m12_m10_m11, r0.xzw);
            r3.y = new CryEngineCore.Components.Vector3(InstanceBufferRaw1.z, InstanceBufferRaw1.y, InstanceBufferRaw1.x).Dot(new CryEngineCore.Components.Vector3(r0.x, r0.z, r0.w)); if (debug) Console.WriteLine("30: r3.y: {0}", r3.y);
            //r3.z = dot(InstanceBuffer[r2.x]._m22_m20_m21, r0.xzw);
            r3.z = new CryEngineCore.Components.Vector3(InstanceBufferRaw2.z, InstanceBufferRaw2.y, InstanceBufferRaw2.x).Dot(new CryEngineCore.Components.Vector3(r0.x, r0.z, r0.w)); if (debug) Console.WriteLine("31: r3.z: {0}", r3.z);
            r1.w = new CryEngineCore.Components.Vector3(r3.x, r3.y, r3.z).Dot(new CryEngineCore.Components.Vector3(r3.x, r3.y, r3.z)); if (debug) Console.WriteLine("32: r1.w: {0}", r1.w);//r1.w = dot(r3.xyz, r3.xyz);
            r1.w = (1.0f / (float)Math.Sqrt(r1.w)); if (debug) Console.WriteLine("33: r1.w: {0}", r1.w);//r1.w = rsqrt(r1.w);
                                                                                                        //o1.xyz = r1.www * r3.xyz;
            o1.x = r1.w * r3.x;
            o1.y = r1.w * r3.y;
            o1.z = r1.w * r3.z; if (debug) Console.WriteLine("34: o1.x: {0} o1.y: {1} o1.z: {2}", o1.x, o1.y, o1.z);
            r1.wuint = v3.xuint & 0x80000000; if (debug) Console.WriteLine("35: r1.w: {0}", r1.w);
            //r1.w = r1.w ? -1 : 1;
            if (r1.wuint != 0) r1.w = -1; else r1.w = 1; if (debug) Console.WriteLine("36: r1.w: {0}", r1.w);
            o1.w = r1.w; if (debug) Console.WriteLine("37: o1.w: {0}", o1.w);
            r1.z = r0.y; if (debug) Console.WriteLine("38: r1.z: {0}", r1.z);
            r3.x = new CryEngineCore.Components.Vector3(InstanceBufferRaw0.x, InstanceBufferRaw0.y, InstanceBufferRaw0.z).Dot(new CryEngineCore.Components.Vector3(r1.x, r1.y, r1.z)); if (debug) Console.WriteLine("39: r3.x: {0}", r3.x);//r3.x = dot(InstanceBuffer[r2.x]._m00_m01_m02, r1.xyz);
            r3.y = new CryEngineCore.Components.Vector3(InstanceBufferRaw1.x, InstanceBufferRaw1.y, InstanceBufferRaw1.z).Dot(new CryEngineCore.Components.Vector3(r1.x, r1.y, r1.z)); if (debug) Console.WriteLine("40: r3.y: {0}", r3.y);//r3.y = dot(InstanceBuffer[r2.x]._m10_m11_m12, r1.xyz);
            r3.z = new CryEngineCore.Components.Vector3(InstanceBufferRaw2.x, InstanceBufferRaw2.y, InstanceBufferRaw2.z).Dot(new CryEngineCore.Components.Vector3(r1.x, r1.y, r1.z)); if (debug) Console.WriteLine("41: r3.z: {0}", r3.z);//r3.z = dot(InstanceBuffer[r2.x]._m20_m21_m22, r1.xyz);
            r0.y = new CryEngineCore.Components.Vector3(new CryEngineCore.Components.Vector3(r3.x, r3.y, r3.z)).Dot(new CryEngineCore.Components.Vector3(r3.x, r3.y, r3.z)); if (debug) Console.WriteLine("42: r0.y: {0}", r0.y);//r0.y = dot(r3.xyz, r3.xyz);
            r0.y = (float)(1.0 / Math.Sqrt(r0.y)); if (debug) Console.WriteLine("43: r0.y: {0}", r0.y);//r0.y = rsqrt(r0.y);
                                                                                                       //o2.xyz = r0.yyy * r3.xyz;
            o2.x = r0.y * r3.x;
            o2.y = r0.y * r3.y;
            o2.z = r0.y * r3.z; if (debug) Console.WriteLine("44: o2.x: {0} o2.y: {1} o2.z: {2}", o2.x, o2.y, o2.z);

            if (debug) Console.WriteLine("-----------------------------------------");
            if (debug) Console.WriteLine("");

            tspac.Tangent = o1;
            tspac.Bitangent = o2;

            return tspac;
        }
    }
}
