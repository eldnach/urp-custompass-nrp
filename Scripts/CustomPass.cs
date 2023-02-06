using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;


// This script will break if NativeRenderPass is enabled in the UniversalRenderer settings!
public class CustomPass: MonoBehaviour
{

    MyPass pass;
    const string passname = "Custom Pass";

    void OnEnable()
    {
  
        pass = new MyPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox,
            bufferName = passname,
        };
        pass.Initialize();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        pass.Destroy();

    }

    private void Update()
    {
       
    }



    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        ScriptableRenderer rend = camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer;

        if (rend != null) {
            rend.EnqueuePass(pass);
        }
    }

}

class MyPass : ScriptableRenderPass
{
    public string bufferName;

    Camera camera;
    Mesh quadMesh;
    Shader shader0;
    Shader shader1;
    Material mat0;
    Material mat1;
    CommandBuffer cmdbuffer;


    RenderTextureFormat colFormat;
    RenderTargetIdentifier colID;
    int colWidth;
    int colHeight;
    int colSamples;

    public void Initialize()
    {
        shader0 = (Shader)Resources.Load("BlitShader0");
        mat0 = new Material(shader0);

        shader1 = (Shader)Resources.Load("BlitShader1");
        mat1 = new Material(shader1);

        quadMesh = new Mesh();
        quadMesh = MeshResources.generateScreenQuad();
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
    {
        camera = renderData.cameraData.camera;
        colID = renderData.cameraData.renderer.cameraColorTargetHandle;
        colFormat = renderData.cameraData.cameraTargetDescriptor.colorFormat;
        colWidth = renderData.cameraData.cameraTargetDescriptor.width;
        colHeight = renderData.cameraData.cameraTargetDescriptor.height;
        colSamples = renderData.cameraData.cameraTargetDescriptor.msaaSamples;

        cmdbuffer = new CommandBuffer { name = bufferName };
        cmdbuffer.BeginSample(bufferName);
        ExecuteBuffer(context, cmdbuffer);

        // Create the attachment descriptors. If these attachments are not specifically bound to any RenderTexture using the ConfigureTarget calls,
        // these are treated as temporary surfaces that are discarded at the end of the renderpass
        var col = new AttachmentDescriptor(colFormat, colID, true, true);
        var depth = new AttachmentDescriptor(RenderTextureFormat.Depth);
        var tmpCol = new AttachmentDescriptor(RenderTextureFormat.ARGB32);

        // Bind the color surface to the current camera target, so the final pass will render the Scene to the screen backbuffer
        // The second argument specifies whether the existing contents of the surface need to be loaded as the initial values;
        // The third argument specifies whether the rendering results need to be written out to memory at the end of
        // the renderpass. We need this as we'll be generating the final image there.
        col.ConfigureTarget(colID, true, true);

        depth.loadAction = RenderBufferLoadAction.DontCare;
        tmpCol.loadAction = RenderBufferLoadAction.DontCare;
        col.loadAction = RenderBufferLoadAction.DontCare;

        depth.storeAction = RenderBufferStoreAction.DontCare;
        tmpCol.storeAction = RenderBufferStoreAction.DontCare;
        col.storeAction = RenderBufferStoreAction.Store;

        depth.ConfigureClear(new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f, 0);

        // Start the renderpass using the given scriptable rendercontext, resolution, samplecount, array of attachments that will be used within the renderpass and the depth surface
        var attachments = new NativeArray<AttachmentDescriptor>(3, Allocator.Temp);
        const int depthIndex = 0, colIndex = 1, colTmpIndex = 2;
        attachments[depthIndex] = depth;
        attachments[colIndex] = col;
        attachments[colTmpIndex] = tmpCol;

        context.BeginRenderPass(colWidth, colHeight, colSamples, attachments, depthIndex);
        attachments.Dispose();

        //context.SetupCameraProperties(camera); // setup camera's view-projection matrix

        cmdbuffer.BeginSample("Subpass 0");
        ExecuteBuffer(context, cmdbuffer);
        var outputs = new NativeArray<int>(1, Allocator.Temp);
        outputs[0] = colTmpIndex;
        context.BeginSubPass(outputs);
        outputs.Dispose();
        FullScreenQuadDrawBG(cmdbuffer, context, mat0);
        context.EndSubPass();
        cmdbuffer.EndSample("Subpass 0");
        context.ExecuteCommandBuffer(cmdbuffer);

        cmdbuffer.BeginSample("Subpass 1");
        ExecuteBuffer(context, cmdbuffer);
        var inputs = new NativeArray<int>(1, Allocator.Temp);
        inputs[0] = colTmpIndex;
        outputs = new NativeArray<int>(1, Allocator.Temp);
        outputs[0] = colIndex;
        context.BeginSubPass(outputs, inputs, true);
        outputs.Dispose();
        inputs.Dispose();
        FullScreenQuadDrawBG(cmdbuffer, context, mat1);
        context.EndSubPass();
        cmdbuffer.EndSample("Subpass 1");
        context.ExecuteCommandBuffer(cmdbuffer);

        context.EndRenderPass();
        cmdbuffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(cmdbuffer);

        context.Submit();
    }

    void FullScreenQuadDrawBG(CommandBuffer buffer, ScriptableRenderContext context, Material material)
    {

        // Add fullscreen quad draw call to command buffer
        buffer.DrawMesh(quadMesh, Matrix4x4.identity, material);
        ExecuteBuffer(context, buffer);
    }

    void ExecuteBuffer(ScriptableRenderContext context, CommandBuffer buffer)
    {
        context.ExecuteCommandBuffer(buffer); // Copy command buffer to context's buffer
        buffer.Clear();
    }

    public void Destroy()
    {

    }
}