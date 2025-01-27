using OpenTK.Graphics.OpenGL;
using System;

namespace ShaderIDE.Render;

internal class VertexArray : IDisposable
{
    public int Handle { get; private set; }

    public VertexArray()
    {
        Handle = GL.GenVertexArray();
        Bind();
    }

    public void Bind()
    {
        GL.BindVertexArray(Handle);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        Unbind();
        GL.DeleteVertexArray(Handle);
        Handle = 0;
    }
}
