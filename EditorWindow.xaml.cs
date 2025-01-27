using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Windows;

namespace ShaderIDE;

public partial class EditorWindow : Window
{
    public EditorWindow()
    {
        InitializeComponent();
        OpenTkControl.Start(new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3
        });
        TextBoxFragmentShader.Text = _fragmentShader;
        InitRendering(TextBoxFragmentShader.Text);
    }

    private void InitRendering(string fragmentShader)
    {
        _shaderHandle = LoadShader(_vertexShader, fragmentShader);
        GL.UseProgram(_shaderHandle);

        _vertexArrayObject = GL.GenVertexArray();
        _vertexBufferObject = GL.GenBuffer();
        
        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    private void OpenTkControl_OnRender(TimeSpan delta)
    {
        GL.ClearColor(Color4.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(_shaderHandle);
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        GL.UseProgram(0);
        GL.DeleteProgram(_shaderHandle);
        _shaderHandle = LoadShader(_vertexShader, TextBoxFragmentShader.Text);
        GL.UseProgram(_shaderHandle);
    }

    private static int LoadShader(string vertexShader, string fragmentShader)
    {
        var vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShaderHandle, vertexShader);
        GL.CompileShader(vertexShaderHandle);
        GL.GetShader(vertexShaderHandle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string compileInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            MessageBox.Show(compileInfo);
            GL.DeleteShader(vertexShaderHandle);
            return 0;
        }

        var fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShaderHandle, fragmentShader);
        GL.CompileShader(fragmentShaderHandle);
        GL.GetShader(fragmentShaderHandle, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string compileInfo = GL.GetShaderInfoLog(fragmentShaderHandle);
            MessageBox.Show(compileInfo);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            return 0;
        }

        var shaderHandle = GL.CreateProgram();
        GL.AttachShader(shaderHandle, vertexShaderHandle);
        GL.AttachShader(shaderHandle, fragmentShaderHandle);
        GL.LinkProgram(shaderHandle);

        GL.GetProgram(shaderHandle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string linkInfo = GL.GetProgramInfoLog(shaderHandle);
            MessageBox.Show(linkInfo);
            GL.DeleteProgram(shaderHandle);
            shaderHandle = 0;
        }

        GL.DetachShader(shaderHandle, vertexShaderHandle);
        GL.DetachShader(shaderHandle, fragmentShaderHandle);
        GL.DeleteShader(fragmentShaderHandle);
        GL.DeleteShader(vertexShaderHandle);

        return shaderHandle;
    }

    private void DisposeRendering()
    {
        GL.BindVertexArray(0);
        GL.DeleteVertexArray(_vertexArrayObject);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vertexBufferObject);

        GL.UseProgram(0);
        GL.DeleteProgram(_shaderHandle);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        DisposeRendering();
    }

    private int _shaderHandle;
    private int _vertexArrayObject;
    private int _vertexBufferObject;

    private readonly float[] _vertices = {
        -1f, -1f, 0.0f,
         1f, -1f, 0.0f,
        -1f,  1f, 0.0f,
        -1f,  1f, 0.0f,
         1f, -1f, 0.0f,
         1f,  1f, 0.0f
    };

    private readonly string _vertexShader = "#version 330 core\r\n" +
        "layout (location = 0) in vec3 aPosition;\r\n" +
        "\r\n" +
        "void main()\r\n" +
        "{\r\n" +
        "    gl_Position = vec4(aPosition, 1.0);\r\n" +
        "}";

    private readonly string _fragmentShader = "#version 330 core\r\n" +
        "out vec4 FragColor;\r\n" +
        "\r\n" +
        "void main()\r\n" +
        "{\r\n" +
        "    FragColor = vec4(0.5f, 0.5f, 0.5f, 1.0f);\r\n" +
        "}";
}