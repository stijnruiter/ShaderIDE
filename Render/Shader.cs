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

    public void Dispose()
    {
        GL.DeleteShader(Handle);
        Handle = 0;
    }

    public const string DefaultVertexShader = "#version 330 core\r\n" +
        "layout (location = 0) in vec3 aPosition;\r\n" +
        "\r\n" +
        "void main()\r\n" +
        "{\r\n" +
        "    gl_Position = vec4(aPosition, 1.0);\r\n" +
        "}";

    public const string DefaultFragmentShader = "#version 330 core\r\n" +
        "out vec4 FragColor;\r\n" +
        "\r\n" +
        "void main()\r\n" +
        "{\r\n" +
        "    FragColor = vec4(0.5f, 0.5f, 0.5f, 1.0f);\r\n" +
        "}";
}
