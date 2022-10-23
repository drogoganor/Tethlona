﻿using Tethlona.Data;
using Tethlona.SampleBase;
using Tethlona.UI;
using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Tethlona
{
    public class Game : SampleApplication
    {
        public Action OnEndGame;

        private GameResources gameResources;

        private readonly VertexPositionTexture[] _vertices;
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _worldBuffer;
        private DeviceBuffer _vertexBuffer;
        private CommandList _cl;
        private Pipeline _pipeline;
        private ResourceSet _projViewSet;
        private ResourceSet _worldTextureSet;
        private TextureView _surfaceTextureView;
        private float _ticks;

        private InGameMenu inGameMenu;
        private GameMapReader gameMapReader;
        private Camera camera;

        public Game(
            IApplicationWindow window,
            Camera camera) : base(window, camera)
        {
            this.camera = camera;

            gameMapReader = new GameMapReader("mapdemo2.json");

            _vertices = gameMapReader.GetVertexArray();

            inGameMenu = new InGameMenu(window);
            inGameMenu.OnReturnToGame += InGameMenu_OnReturnToGame;
            inGameMenu.OnEndGame += InGameMenu_OnEndGame;
        }

        private void InGameMenu_OnEndGame()
        {
            Hide();
            OnEndGame?.Invoke();
        }

        private void ShowInGameMenu()
        {
            Hide();
            inGameMenu.Show();
        }

        private void InGameMenu_OnReturnToGame()
        {
            Show();
        }

        protected unsafe override void CreateResources(ResourceFactory factory)
        {
            gameResources = new GameResources(GraphicsDevice, ResourceFactory);

            _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            if (_vertices != null)
            {
                _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionTexture.SizeInBytes * _vertices.Length), BufferUsage.VertexBuffer));
                GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);
            }

            _surfaceTextureView = gameResources.TextureView;

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
                },
                factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

            ResourceLayout projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projViewLayout, worldTextureLayout },
                MainSwapchain.Framebuffer.OutputDescription));

            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
                projViewLayout,
                _projectionBuffer,
                _viewBuffer));

            _worldTextureSet = factory.CreateResourceSet(new ResourceSetDescription(
                worldTextureLayout,
                _worldBuffer,
                _surfaceTextureView,
                GraphicsDevice.Aniso4xSampler));

            _cl = factory.CreateCommandList();

        }

        protected override void OnDeviceDestroyed()
        {
            base.OnDeviceDestroyed();

            inGameMenu.OnReturnToGame -= InGameMenu_OnReturnToGame;
            inGameMenu.OnEndGame -= InGameMenu_OnEndGame;
        }

        protected override void Draw(float deltaSeconds)
        {
            // TODO: Place in Update/PreDraw
            if (InputTracker.GetKey(Key.Escape))
            {
                ShowInGameMenu();
            }

            camera.Update(deltaSeconds);

            if (_vertices == null)
            {
                return;
            }

            // Render
            _ticks += deltaSeconds * 1000f;
            _cl.Begin();

            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                1.0f,
                (float)Window.Width / Window.Height,
                0.5f,
                1000f);

            _cl.UpdateBuffer(_projectionBuffer, 0, projection);

            var direction = Vector3.Transform(
                Vector3.Transform(
                    Vector3.UnitZ,
                    Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, camera.Pitch)),
                Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, -camera.Yaw));

            var view = Matrix4x4.CreateLookAt(camera.Position, camera.Position + direction, Vector3.UnitY);

            _cl.UpdateBuffer(_viewBuffer, 0, view);
            //_cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateRotationX(0.785398f) * Matrix4x4.CreateRotationY(0.523599f));

            _cl.UpdateBuffer(_worldBuffer, 0, Matrix4x4.Identity);

            _cl.SetFramebuffer(MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            //_cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _worldTextureSet);
            _cl.Draw((uint)_vertices.Length);
            //_cl.DrawIndexed(36, 1, 0, 0, 0);

            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);
            GraphicsDevice.SwapBuffers(MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }

        private const string VertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 TexCoords;
layout(location = 0) out vec3 fsin_texCoords;

void main()
{
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_texCoords = TexCoords;
}";

        private const string FragmentCode = @"
#version 450

layout(location = 0) in vec3 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 1, binding = 1) uniform texture2DArray SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

void main()
{
    fsout_color =  texture(sampler2DArray(SurfaceTexture, SurfaceSampler), fsin_texCoords);
}";
    }
}
