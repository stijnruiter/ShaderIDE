using OpenTK.Graphics.OpenGL;
using System;
using System.Windows;

namespace ShaderIDE.Render;

internal class ShaderSource : IDisposable
{
    public ShaderType Type { get; private set; }
    public int Handle { get; private set; }

    private ShaderSource()
    {
    }

    public static ShaderSource? Compile(ShaderType type, string source)
    {
        var handle = GL.CreateShader(type);
        GL.ShaderSource(handle, source);
        GL.CompileShader(handle);
        GL.GetShader(handle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string compileInfo = GL.GetShaderInfoLog(handle);
            MessageBox.Show(compileInfo);
            GL.DeleteShader(handle);
            return null;
        }
        return new ShaderSource { Handle = handle, Type = type };
    }

    public void Attach(int shaderProgram)
    {
        GL.AttachShader(shaderProgram, Handle);
    }

    public void Detach(int shaderProgram)
    {
        GL.DetachShader(shaderProgram, Handle);
    }

    public void Dispose()
    {
        GL.DeleteShader(Handle);
    }
}


internal class Shader : IDisposable
{
    public int Handle { get; private set; }

    private Shader()
    {

    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public static Shader? Create(string vertexShader, string fragmentShader)
    {
        using var vert = ShaderSource.Compile(ShaderType.VertexShader, vertexShader);
        using var frag = ShaderSource.Compile(ShaderType.FragmentShader, fragmentShader);

        if (vert is null || frag is null)
            return null;

        var shaderHandle = GL.CreateProgram();
        vert.Attach(shaderHandle);
        frag.Attach(shaderHandle);
        GL.LinkProgram(shaderHandle);

        GL.GetProgram(shaderHandle, GetProgramParameterName.LinkStatus, out var success);
        if (success == 0)
        {
            string linkInfo = GL.GetProgramInfoLog(shaderHandle);
            MessageBox.Show(linkInfo);
            GL.DeleteProgram(shaderHandle);
            return null;
        }
        vert.Detach(shaderHandle);
        frag.Detach(shaderHandle);
        return new Shader { Handle = shaderHandle };
    }

    public int GetUniformLocation(string uniformName)
    {
        return GL.GetUniformLocation(Handle, uniformName);
    }

    public bool SetUniform1<T>(string uniformName, T value) where T : struct
    {
        var location = GetUniformLocation(uniformName);
        if (location < 0)
            return false;

        if (value is double doubleValue)
        {
            GL.Uniform1(location, doubleValue);
            return true;
        }
        if (value is float floatValue)
        {
            GL.Uniform1(location, floatValue);
            return true;
        }
        if (value is int intValue)
        {
            GL.Uniform1(location, intValue);
            return true;
        }
        if (value is uint uintValue)
        {
            GL.Uniform1(location, uintValue);
            return true;
        }
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GL.DeleteShader(Handle);
        Handle = 0;
    }

    public static readonly string DefaultVertexShader = EmbeddedLoader.GetContent("ShaderIDE.Resources.vertex.glsl");

    public static readonly string DefaultFragmentShader = EmbeddedLoader.GetContent("ShaderIDE.Resources.mandelbrot.glsl");
}
