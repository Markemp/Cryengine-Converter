using CgfConverter.Utililities;
using System.Numerics;
using Extensions;

Console.WriteLine("Bone 5");
var q = new Quaternion(0.000000f, 0.999048f, 0.000000f, -0.043619f);  // bone 5 quat
var v = new Vector3(0.022410f, 0.157970f, 0.102130f);

var m3 = Matrix4x4.CreateFromQuaternion(q);
m3.Translation = new Vector3(-0.104450f, 0.078181f, -0.047510f);
m3.Translation = v;

Console.WriteLine($"m3 transform matrix: {m3}");

Matrix4x4.Invert(m3, out Matrix4x4 m4);  // m3 is right, except scale <-> translation

Console.WriteLine($"m4 transform matrix: {m4}");

Console.WriteLine();
Console.WriteLine("Bone 15");

var q2 = new Quaternion(0, 0, 0, 1);
var v2 = new Vector3(0.000000f, 0.056223f, 0.042497f);

var m5 = Matrix4x4.CreateFromQuaternion(q2);
m5.Translation = v2;

Console.WriteLine($"m5 transform matrix: {m5}");

Matrix4x4.Invert(m5, out Matrix4x4 m6);  // m6 is right, except scale <-> translation

Console.WriteLine($"m6 transform matrix: {m6}");


