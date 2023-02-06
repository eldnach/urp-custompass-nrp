using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPassFeature : ScriptableRendererFeature
{


    class CustomRenderPass : ScriptableRenderPass
    {
        MyCustomPass pass;
        public CustomRenderPass()
        {
            pass = new MyCustomPass()
            {
                bufferName = "Custom Pass"
            };
            pass.Initialize();
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            //ConfigureInput()
        }

        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            pass.Execute(context, ref renderingData);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
       
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
    class MyCustomPass
    {
        public string bufferName;

        Mesh quadMesh;
        Shader shader0;
        Shader shader1;
        Material mat0;
        Material mat1;
        Camera camera;
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

        public void Execute(ScriptableRenderContext context, ref RenderingData renderData)
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

            col.loadAction = RenderBufferLoadAction.DontCare;
            depth.loadAction = RenderBufferLoadAction.DontCare;
            tmpCol.loadAction = RenderBufferLoadAction.DontCare;

            col.storeAction = RenderBufferStoreAction.StoreAndResolve;
            depth.storeAction = RenderBufferStoreAction.DontCare;
            tmpCol.storeAction = RenderBufferStoreAction.DontCare;

            // Bind the color surface to the current camera target, so the final pass will render the Scene to the screen backbuffer
            // The second argument specifies whether the existing contents of the surface need to be loaded as the initial values;
            // The third argument specifies whether the rendering results need to be written out to memory at the end of
            // the renderpass. We need this as we'll be generating the final image there.
            col.ConfigureTarget(colID, true, true);
            //col.resolveTarget = colID; Use ConfigureResolveTarget instead, which binds the render target for MSAA resolve, and sets the correct store actions
            col.ConfigureResolveTarget(colID);

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
}


