using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;

namespace VoxelCraft.Rendering
{
    internal class BaseShader:IDisposable
    {
        private bool disposed = false;

        private int _vao;
        private int _vbo;

        private int _shaderProgram;

        private byte[]? _atlasData;
        private int _shaderTexture;

        private TextureLoader? _textureLoader;

        public string vertexShaderSource = @"
                #version 450 core

                layout(location = 0) in vec3 aPos;
                layout(location = 1) in vec3 aTexCoord;

                uniform mat4 projection;
                uniform mat4 view;
                uniform int atlasSize;
                uniform vec3 chunkPos;

                out vec2 TexCoord;
                out float ao;
                void main(){
                    vec3 pos = aPos/8 + atlasSize * chunkPos;
                    gl_Position = projection * view * vec4(pos , 1.0);
                    TexCoord = aTexCoord.xy/atlasSize/atlasSize;
                    ao = aTexCoord.z/31.0;
                }

            ";
        public string fragmentShaderSource = @"
                #version 450 core

                in vec2 TexCoord;
                in float ao;
                out vec4 FragColor;
                uniform sampler2D ourTexture;
                void main()
                {
                    vec4 t = texture(ourTexture,TexCoord);
                    vec3 lit = t.rgb * ao;
                    FragColor = vec4(lit,t.a);
                }
            ";


        /// <summary>
        /// 区块内方块的顶点数据,一行6个byte, 每行0~2:顶点坐标,3~4:UV坐标,5:法线枚举(0~5)
        /// </summary>
        private byte[] _blockVertices = new byte[] {};
        
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;

        private Vector3 _chunkPosition;
        public void LoadShader()
        {
            //加载与绑定vao,vbo
            //_vao = GL.GenVertexArray();
            //GL.BindVertexArray(_vao);

            //_vbo = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            //编译与链接着色器
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            CheckShaderCompileError(vertexShader, "顶点着色器");//检查编译错误

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            CheckShaderCompileError(fragmentShader, "片段着色器");//检查编译错误

            _shaderProgram = GL.CreateProgram();

            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            CheckProgramLinkError(_shaderProgram);//检查链接错误

            //卸载编译完成后无用的数据
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //需要加载纹理图集
            LoadTexture();
        }
        public void Draw()
        {
            GL.UseProgram(_shaderProgram);

            int textureLocation = GL.GetUniformLocation(_shaderProgram, "ourTexture");
            GL.Uniform1(textureLocation, 0);

            int location4 = GL.GetUniformLocation(_shaderProgram, "chunkPos");
            GL.Uniform3(location4, _chunkPosition);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _blockVertices.Length / 6);
        }
        public void GetMatrix(Vector3 cameraPos,Vector3 cameraDir,Vector2i size)
        {
            GL.UseProgram(_shaderProgram);
            float aspectRatio = (float)size.X / size.Y;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f),
                                                                    aspectRatio,
                                                                    0.1f,
                                                                    1000f
                                                                    );
            _viewMatrix = Matrix4.LookAt(cameraPos, cameraPos+cameraDir, Vector3.UnitY);
            int location = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(location, false, ref _projectionMatrix);

            int location2 = GL.GetUniformLocation(_shaderProgram, "view");
            GL.UniformMatrix4(location2, false, ref _viewMatrix);

            int location3 = GL.GetUniformLocation(_shaderProgram, "atlasSize");
            GL.Uniform1(location3, 16);


        }
        protected void LoadTexture()
        {
            _textureLoader = new TextureLoader();
            _textureLoader.LoadTexture();

            _atlasData = _textureLoader.pixelData;

            _shaderTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _shaderTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _textureLoader.imageX, _textureLoader.imageY, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, _atlasData);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        private void CheckShaderCompileError(int shader, string shaderType)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"{shaderType}编译错误：{infoLog}");
            }
        }
        private void CheckProgramLinkError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"着色器程序链接错误：{infoLog}");
            }
        }
        public void GenVertices(int vao,int vbo, byte[] vertices,Vector3i pos)
        {
            _blockVertices = vertices;
            _chunkPosition = pos;
            this._vao = vao;
            this._vbo = vbo;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose( bool disposing )
        {
            if (disposed) return;

            if(disposing)
            {
                //释放托管资源
                
            }

            disposed = true;
        }
    }
}
