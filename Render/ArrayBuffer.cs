using OpenTK.Graphics.OpenGL;
using System;

namespace ShaderIDE.Render;

internal class ArrayBuffer : IDisposable
{
    public ArrayBuffer(float[] data, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
    {
        Handle = GL.GenBuffer();
        SetData(data, bufferUsage);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
    }

    public void SetData(float[] data, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
    {
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, bufferUsage);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Dispose()
    {
        Unbind();
        GL.DeleteBuffer(Handle);
        Handle = 0;
    }

    public int Handle { get; private set; }
}
