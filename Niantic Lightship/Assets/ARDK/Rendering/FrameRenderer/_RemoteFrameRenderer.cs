// Copyright 2022 Niantic, Inc. All Rights Reserved.
using Niantic.ARDK.AR;

using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.ARDK.Rendering
{
  internal sealed class _RemoteFrameRenderer: 
    ARFrameRenderer
  {
    private CommandBuffer _commandBuffer;
    private Texture2D _textureY;
    private Texture2D _textureCbCr;

    protected override Shader Shader { get; }

    public _RemoteFrameRenderer(RenderTarget target)
      : base(target)
    {
      Shader = Resources.Load<Shader>("ARKitFrame");
    }

    public _RemoteFrameRenderer
    (
      RenderTarget target,
      float near,
      float far,
      Shader customShader = null
    ) : base(target, near, far)
    {
      Shader = customShader ? customShader : Resources.Load<Shader>("ARKitFrame");
    }

    protected override GraphicsFence? OnConfigurePipeline
    (
      RenderTarget target,
      Resolution targetResolution,
      Resolution sourceResolution,
      Material renderMaterial
    )
    {
      _commandBuffer = new CommandBuffer
      {
        name = "RemoteFrameEditor"
      };

      _commandBuffer.Blit(null, target.Identifier, renderMaterial);

#if UNITY_2019_1_OR_NEWER
      return _commandBuffer.CreateAsyncGraphicsFence();
#else
      return _commandBuffer.CreateGPUFence();
#endif
    }

    protected override bool OnUpdateState
    (
      IARFrame frame,
      Matrix4x4 projectionTransform,
      Matrix4x4 displayTransform,
      Material material
    )
    {
      if (frame.CapturedImageBuffer == null)
        return false;

      if (_textureY == null)
      {
        var resolution = frame.Camera.CPUImageResolution;
        _textureY = new Texture2D(resolution.width, resolution.height, TextureFormat.R8, false);
        _textureCbCr =
          new Texture2D(resolution.width / 2, resolution.height / 2, TextureFormat.RG16, false);
      }

      // Update source textures
      _textureY.LoadRawTextureData(frame.CapturedImageBuffer.Planes[0].Data);
      _textureCbCr.LoadRawTextureData(frame.CapturedImageBuffer.Planes[1].Data);
      _textureY.Apply();
      _textureCbCr.Apply();

      // Bind the texture and the display transform
      material.SetTexture(PropertyBindings.YChannel, _textureY);
      material.SetTexture(PropertyBindings.CbCrChannel, _textureCbCr);
      material.SetMatrix(PropertyBindings.DisplayTransform, displayTransform);

      return true;
    }

    protected override void OnAddToCamera(Camera camera)
    {
      ARSessionBuffersHelper.AddBackgroundBuffer(camera, _commandBuffer);
    }

    protected override void OnRemoveFromCamera(Camera camera)
    {
      ARSessionBuffersHelper.RemoveBackgroundBuffer(camera, _commandBuffer);
    }

    protected override void OnIssueCommands()
    {
      Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    protected override void OnRelease()
    {
      if (_textureY != null)
        Object.Destroy(_textureY);

      if (_textureCbCr != null)
        Object.Destroy(_textureCbCr);

      _commandBuffer?.Dispose();
    }
  }
}