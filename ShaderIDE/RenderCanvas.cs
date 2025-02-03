using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ShaderIDE.Render;
using System;
using System.Diagnostics;

namespace ShaderIDE;

internal class RenderCanvas : IDisposable
{
    public RenderCanvas()
    {
        _shader = Shader.Create(VertexShader, FragmentShader);
        _shader?.Use();

        _vertexArray = new VertexArray();
        _arrayBuffer = new ArrayBuffer(_vertices);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        _timer = new Stopwatch();
        _timer.Start();
    }

    public void UpdateShader(string vertexShader, string fragmentShader)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        _shader?.Dispose();
        _shader = Shader.Create(VertexShader, FragmentShader);
        _shader?.Use();
    }

    public void Clear()
    {
        GL.ClearColor(Color4.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void Draw(TimeSpan delta)
    {
        _shader?.Use();
        _vertexArray.Bind();
        _shader?.SetUniform1("time", (float)_timer.Elapsed.TotalSeconds);
        GL.DrawArrays(PrimitiveType.Quads, 0, _vertices.Length / 3);
    }

    public void Dispose()
    {
        _vertexArray.Dispose();
        _arrayBuffer.Dispose();
        _shader?.Dispose();
        _shader = null;
    }

    public string VertexShader { get; private set; } = Shader.DefaultVertexShader;
    public string FragmentShader { get; private set; } = Shader.DefaultFragmentShader;

    private Stopwatch _timer;
    private Shader? _shader;
    private readonly VertexArray _vertexArray;
    private readonly ArrayBuffer _arrayBuffer;

    private readonly float[] _vertices = {
        -1f, -1f, 0.0f, // Bottom Left
         1f, -1f, 0.0f, // Bottom Right
         1f,  1f, 0.0f, // Top Right
        -1f,  1f, 0.0f, // Top Left
    };
}
