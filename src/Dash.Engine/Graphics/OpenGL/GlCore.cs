﻿using System;
using System.Runtime.InteropServices;

namespace Dash.Engine.Graphics.OpenGL
{
    partial class GL
    {
        internal static partial class Core
        {
            [DllImport(GL.Library, EntryPoint = "glActiveShaderProgram", ExactSpelling = true)]
            internal extern static void ActiveShaderProgram(UInt32 pipeline, UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glActiveTexture", ExactSpelling = true)]
            internal extern static void ActiveTexture(OpenGL.TextureUnit texture);
            [DllImport(GL.Library, EntryPoint = "glAttachShader", ExactSpelling = true)]
            internal extern static void AttachShader(UInt32 program, UInt32 shader);
            [DllImport(GL.Library, EntryPoint = "glBeginConditionalRender", ExactSpelling = true)]
            internal extern static void BeginConditionalRender(UInt32 id, OpenGL.ConditionalRenderType mode);
            [DllImport(GL.Library, EntryPoint = "glEndConditionalRender", ExactSpelling = true)]
            internal extern static void EndConditionalRender();
            [DllImport(GL.Library, EntryPoint = "glBeginQuery", ExactSpelling = true)]
            internal extern static void BeginQuery(OpenGL.QueryTarget target, UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glEndQuery", ExactSpelling = true)]
            internal extern static void EndQuery(OpenGL.QueryTarget target);
            [DllImport(GL.Library, EntryPoint = "glBeginQueryIndexed", ExactSpelling = true)]
            internal extern static void BeginQueryIndexed(UInt32 target, UInt32 index, UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glEndQueryIndexed", ExactSpelling = true)]
            internal extern static void EndQueryIndexed(OpenGL.QueryTarget target, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glBeginTransformFeedback", ExactSpelling = true)]
            internal extern static void BeginTransformFeedback(OpenGL.BeginFeedbackMode primitiveMode);
            [DllImport(GL.Library, EntryPoint = "glEndTransformFeedback", ExactSpelling = true)]
            internal extern static void EndTransformFeedback();
            [DllImport(GL.Library, EntryPoint = "glBindAttribLocation", ExactSpelling = true)]
            internal extern static void BindAttribLocation(UInt32 program, UInt32 index, String name);
            [DllImport(GL.Library, EntryPoint = "glBindBuffer", ExactSpelling = true)]
            internal extern static void BindBuffer(OpenGL.BufferTarget target, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glBindBufferBase", ExactSpelling = true)]
            internal extern static void BindBufferBase(OpenGL.BufferTarget target, UInt32 index, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glBindBufferRange", ExactSpelling = true)]
            internal extern static void BindBufferRange(BufferTarget target, UInt32 index, UInt32 buffer, IntPtr offset, IntPtr size);
            [DllImport(GL.Library, EntryPoint = "glBindBuffersBase", ExactSpelling = true)]
            internal extern static void BindBuffersBase(OpenGL.BufferTarget target, UInt32 first, Int32 count, UInt32[] buffers);
            [DllImport(GL.Library, EntryPoint = "glBindBuffersRange", ExactSpelling = true)]
            internal extern static void BindBuffersRange(OpenGL.BufferTarget target, UInt32 first, Int32 count, UInt32[] buffers, IntPtr[] offsets, IntPtr[] sizes);
            [DllImport(GL.Library, EntryPoint = "glBindFragDataLocation", ExactSpelling = true)]
            internal extern static void BindFragDataLocation(UInt32 program, UInt32 colorNumber, String name);
            [DllImport(GL.Library, EntryPoint = "glBindFragDataLocationIndexed", ExactSpelling = true)]
            internal extern static void BindFragDataLocationIndexed(UInt32 program, UInt32 colorNumber, UInt32 index, String name);
            [DllImport(GL.Library, EntryPoint = "glBindFramebuffer", ExactSpelling = true)]
            internal extern static void BindFramebuffer(OpenGL.FramebufferTarget target, UInt32 framebuffer);
            [DllImport(GL.Library, EntryPoint = "glBindImageTexture", ExactSpelling = true)]
            internal extern static void BindImageTexture(UInt32 unit, UInt32 texture, Int32 level, Boolean layered, Int32 layer, OpenGL.BufferAccess access, OpenGL.PixelInternalFormat format);
            [DllImport(GL.Library, EntryPoint = "glBindImageTextures", ExactSpelling = true)]
            internal extern static void BindImageTextures(UInt32 first, Int32 count, UInt32[] textures);
            [DllImport(GL.Library, EntryPoint = "glBindProgramPipeline", ExactSpelling = true)]
            internal extern static void BindProgramPipeline(UInt32 pipeline);
            [DllImport(GL.Library, EntryPoint = "glBindRenderbuffer", ExactSpelling = true)]
            internal extern static void BindRenderbuffer(OpenGL.RenderbufferTarget target, UInt32 renderbuffer);
            [DllImport(GL.Library, EntryPoint = "glBindSampler", ExactSpelling = true)]
            internal extern static void BindSampler(UInt32 unit, UInt32 sampler);
            [DllImport(GL.Library, EntryPoint = "glBindSamplers", ExactSpelling = true)]
            internal extern static void BindSamplers(UInt32 first, Int32 count, UInt32[] samplers);
            [DllImport(GL.Library, EntryPoint = "glBindTexture", ExactSpelling = true)]
            internal extern static void BindTexture(OpenGL.TextureTarget target, UInt32 texture);
            [DllImport(GL.Library, EntryPoint = "glBindTextures", ExactSpelling = true)]
            internal extern static void BindTextures(UInt32 first, Int32 count, UInt32[] textures);
            [DllImport(GL.Library, EntryPoint = "glBindTextureUnit", ExactSpelling = true)]
            internal extern static void BindTextureUnit(UInt32 unit, UInt32 texture);
            [DllImport(GL.Library, EntryPoint = "glBindTransformFeedback", ExactSpelling = true)]
            internal extern static void BindTransformFeedback(OpenGL.NvTransformFeedback2 target, UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glBindVertexArray", ExactSpelling = true)]
            internal extern static void BindVertexArray(UInt32 array);
            [DllImport(GL.Library, EntryPoint = "glBindVertexBuffer", ExactSpelling = true)]
            internal extern static void BindVertexBuffer(UInt32 bindingindex, UInt32 buffer, IntPtr offset, IntPtr stride);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayVertexBuffer", ExactSpelling = true)]
            internal extern static void VertexArrayVertexBuffer(UInt32 vaobj, UInt32 bindingindex, UInt32 buffer, IntPtr offset, Int32 stride);
            [DllImport(GL.Library, EntryPoint = "glBindVertexBuffers", ExactSpelling = true)]
            internal extern static void BindVertexBuffers(UInt32 first, Int32 count, UInt32[] buffers, IntPtr[] offsets, Int32[] strides);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayVertexBuffers", ExactSpelling = true)]
            internal extern static void VertexArrayVertexBuffers(UInt32 vaobj, UInt32 first, Int32 count, UInt32[] buffers, IntPtr[] offsets, Int32[] strides);
            [DllImport(GL.Library, EntryPoint = "glBlendColor", ExactSpelling = true)]
            internal extern static void BlendColor(Single red, Single green, Single blue, Single alpha);
            [DllImport(GL.Library, EntryPoint = "glBlendEquation", ExactSpelling = true)]
            internal extern static void BlendEquation(OpenGL.BlendEquationMode mode);
            [DllImport(GL.Library, EntryPoint = "glBlendEquationi", ExactSpelling = true)]
            internal extern static void BlendEquationi(UInt32 buf, OpenGL.BlendEquationMode mode);
            [DllImport(GL.Library, EntryPoint = "glBlendEquationSeparate", ExactSpelling = true)]
            internal extern static void BlendEquationSeparate(OpenGL.BlendEquationMode modeRGB, OpenGL.BlendEquationMode modeAlpha);
            [DllImport(GL.Library, EntryPoint = "glBlendEquationSeparatei", ExactSpelling = true)]
            internal extern static void BlendEquationSeparatei(UInt32 buf, OpenGL.BlendEquationMode modeRGB, OpenGL.BlendEquationMode modeAlpha);
            [DllImport(GL.Library, EntryPoint = "glBlendFunc", ExactSpelling = true)]
            internal extern static void BlendFunc(OpenGL.BlendingFactorSrc sfactor, OpenGL.BlendingFactorDest dfactor);
            [DllImport(GL.Library, EntryPoint = "glBlendFunci", ExactSpelling = true)]
            internal extern static void BlendFunci(UInt32 buf, OpenGL.BlendingFactorSrc sfactor, OpenGL.BlendingFactorDest dfactor);
            [DllImport(GL.Library, EntryPoint = "glBlendFuncSeparate", ExactSpelling = true)]
            internal extern static void BlendFuncSeparate(OpenGL.BlendingFactorSrc srcRGB, OpenGL.BlendingFactorDest dstRGB, OpenGL.BlendingFactorSrc srcAlpha, OpenGL.BlendingFactorDest dstAlpha);
            [DllImport(GL.Library, EntryPoint = "glBlendFuncSeparatei", ExactSpelling = true)]
            internal extern static void BlendFuncSeparatei(UInt32 buf, OpenGL.BlendingFactorSrc srcRGB, OpenGL.BlendingFactorDest dstRGB, OpenGL.BlendingFactorSrc srcAlpha, OpenGL.BlendingFactorDest dstAlpha);
            [DllImport(GL.Library, EntryPoint = "glBlitFramebuffer", ExactSpelling = true)]
            internal extern static void BlitFramebuffer(Int32 srcX0, Int32 srcY0, Int32 srcX1, Int32 srcY1, Int32 dstX0, Int32 dstY0, Int32 dstX1, Int32 dstY1, OpenGL.ClearBufferMask mask, OpenGL.BlitFramebufferFilter filter);
            [DllImport(GL.Library, EntryPoint = "glBlitNamedFramebuffer", ExactSpelling = true)]
            internal extern static void BlitNamedFramebuffer(UInt32 readFramebuffer, UInt32 drawFramebuffer, Int32 srcX0, Int32 srcY0, Int32 srcX1, Int32 srcY1, Int32 dstX0, Int32 dstY0, Int32 dstX1, Int32 dstY1, OpenGL.ClearBufferMask mask, OpenGL.BlitFramebufferFilter filter);
            [DllImport(GL.Library, EntryPoint = "glBufferData", ExactSpelling = true)]
            internal extern static void BufferData(OpenGL.BufferTarget target, IntPtr size, IntPtr data, OpenGL.BufferUsageHint usage);
            [DllImport(GL.Library, EntryPoint = "glNamedBufferData", ExactSpelling = true)]
            internal extern static void NamedBufferData(UInt32 buffer, Int32 size, IntPtr data, OpenGL.BufferUsageHint usage);
            [DllImport(GL.Library, EntryPoint = "glBufferStorage", ExactSpelling = true)]
            internal extern static void BufferStorage(OpenGL.BufferTarget target, IntPtr size, IntPtr data, UInt32 flags);
            [DllImport(GL.Library, EntryPoint = "glNamedBufferStorage", ExactSpelling = true)]
            internal extern static void NamedBufferStorage(UInt32 buffer, Int32 size, IntPtr data, UInt32 flags);
            [DllImport(GL.Library, EntryPoint = "glBufferSubData", ExactSpelling = true)]
            internal extern static void BufferSubData(OpenGL.BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glNamedBufferSubData", ExactSpelling = true)]
            internal extern static void NamedBufferSubData(UInt32 buffer, IntPtr offset, Int32 size, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCheckFramebufferStatus", ExactSpelling = true)]
            internal extern static OpenGL.FramebufferErrorCode CheckFramebufferStatus(OpenGL.FramebufferTarget target);
            [DllImport(GL.Library, EntryPoint = "glCheckNamedFramebufferStatus", ExactSpelling = true)]
            internal extern static OpenGL.FramebufferErrorCode CheckNamedFramebufferStatus(UInt32 framebuffer, OpenGL.FramebufferTarget target);
            [DllImport(GL.Library, EntryPoint = "glClampColor", ExactSpelling = true)]
            internal extern static void ClampColor(OpenGL.ClampColorTarget target, OpenGL.ClampColorMode clamp);
            [DllImport(GL.Library, EntryPoint = "glClear", ExactSpelling = true)]
            internal extern static void Clear(OpenGL.ClearBufferMask mask);
            [DllImport(GL.Library, EntryPoint = "glClearBufferiv", ExactSpelling = true)]
            internal extern static void ClearBufferiv(OpenGL.ClearBuffer buffer, Int32 drawbuffer, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glClearBufferuiv", ExactSpelling = true)]
            internal extern static void ClearBufferuiv(OpenGL.ClearBuffer buffer, Int32 drawbuffer, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glClearBufferfv", ExactSpelling = true)]
            internal extern static void ClearBufferfv(OpenGL.ClearBuffer buffer, Int32 drawbuffer, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glClearBufferfi", ExactSpelling = true)]
            internal extern static void ClearBufferfi(OpenGL.ClearBuffer buffer, Int32 drawbuffer, Single depth, Int32 stencil);
            [DllImport(GL.Library, EntryPoint = "glClearNamedFramebufferiv", ExactSpelling = true)]
            internal extern static void ClearNamedFramebufferiv(UInt32 framebuffer, OpenGL.ClearBuffer buffer, Int32 drawbuffer, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glClearNamedFramebufferuiv", ExactSpelling = true)]
            internal extern static void ClearNamedFramebufferuiv(UInt32 framebuffer, OpenGL.ClearBuffer buffer, Int32 drawbuffer, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glClearNamedFramebufferfv", ExactSpelling = true)]
            internal extern static void ClearNamedFramebufferfv(UInt32 framebuffer, OpenGL.ClearBuffer buffer, Int32 drawbuffer, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glClearNamedFramebufferfi", ExactSpelling = true)]
            internal extern static void ClearNamedFramebufferfi(UInt32 framebuffer, OpenGL.ClearBuffer buffer, Single depth, Int32 stencil);
            [DllImport(GL.Library, EntryPoint = "glClearBufferData", ExactSpelling = true)]
            internal extern static void ClearBufferData(OpenGL.BufferTarget target, OpenGL.SizedInternalFormat internalFormat, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClearNamedBufferData", ExactSpelling = true)]
            internal extern static void ClearNamedBufferData(UInt32 buffer, OpenGL.SizedInternalFormat internalFormat, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClearBufferSubData", ExactSpelling = true)]
            internal extern static void ClearBufferSubData(OpenGL.BufferTarget target, OpenGL.SizedInternalFormat internalFormat, IntPtr offset, IntPtr size, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClearNamedBufferSubData", ExactSpelling = true)]
            internal extern static void ClearNamedBufferSubData(UInt32 buffer, OpenGL.SizedInternalFormat internalFormat, IntPtr offset, Int32 size, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClearColor", ExactSpelling = true)]
            internal extern static void ClearColor(Single red, Single green, Single blue, Single alpha);
            [DllImport(GL.Library, EntryPoint = "glClearDepth", ExactSpelling = true)]
            internal extern static void ClearDepth(Double depth);
            [DllImport(GL.Library, EntryPoint = "glClearDepthf", ExactSpelling = true)]
            internal extern static void ClearDepthf(Single depth);
            [DllImport(GL.Library, EntryPoint = "glClearStencil", ExactSpelling = true)]
            internal extern static void ClearStencil(Int32 s);
            [DllImport(GL.Library, EntryPoint = "glClearTexImage", ExactSpelling = true)]
            internal extern static void ClearTexImage(UInt32 texture, Int32 level, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClearTexSubImage", ExactSpelling = true)]
            internal extern static void ClearTexSubImage(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelInternalFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glClientWaitSync", ExactSpelling = true)]
            internal extern static OpenGL.ArbSync ClientWaitSync(IntPtr sync, UInt32 flags, UInt64 timeout);
            [DllImport(GL.Library, EntryPoint = "glClipControl", ExactSpelling = true)]
            internal extern static void ClipControl(OpenGL.ClipControlOrigin origin, OpenGL.ClipControlDepth depth);
            [DllImport(GL.Library, EntryPoint = "glColorMask", ExactSpelling = true)]
            internal extern static void ColorMask(Boolean red, Boolean green, Boolean blue, Boolean alpha);
            [DllImport(GL.Library, EntryPoint = "glColorMaski", ExactSpelling = true)]
            internal extern static void ColorMaski(UInt32 buf, Boolean red, Boolean green, Boolean blue, Boolean alpha);
            [DllImport(GL.Library, EntryPoint = "glCompileShader", ExactSpelling = true)]
            internal extern static void CompileShader(UInt32 shader);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexImage1D", ExactSpelling = true)]
            internal extern static void CompressedTexImage1D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 border, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexImage2D", ExactSpelling = true)]
            internal extern static void CompressedTexImage2D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexImage3D", ExactSpelling = true)]
            internal extern static void CompressedTexImage3D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth, Int32 border, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexSubImage1D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage1D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTextureSubImage1D", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage1D(UInt32 texture, Int32 level, Int32 xoffset, Int32 width, OpenGL.PixelInternalFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexSubImage2D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage2D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTextureSubImage2D", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage2D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenGL.PixelInternalFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTexSubImage3D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage3D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCompressedTextureSubImage3D", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage3D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelInternalFormat format, Int32 imageSize, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glCopyBufferSubData", ExactSpelling = true)]
            internal extern static void CopyBufferSubData(OpenGL.BufferTarget readTarget, OpenGL.BufferTarget writeTarget, IntPtr readOffset, IntPtr writeOffset, IntPtr size);
            [DllImport(GL.Library, EntryPoint = "glCopyNamedBufferSubData", ExactSpelling = true)]
            internal extern static void CopyNamedBufferSubData(UInt32 readBuffer, UInt32 writeBuffer, IntPtr readOffset, IntPtr writeOffset, Int32 size);
            [DllImport(GL.Library, EntryPoint = "glCopyImageSubData", ExactSpelling = true)]
            internal extern static void CopyImageSubData(UInt32 srcName, OpenGL.BufferTarget srcTarget, Int32 srcLevel, Int32 srcX, Int32 srcY, Int32 srcZ, UInt32 dstName, OpenGL.BufferTarget dstTarget, Int32 dstLevel, Int32 dstX, Int32 dstY, Int32 dstZ, Int32 srcWidth, Int32 srcHeight, Int32 srcDepth);
            [DllImport(GL.Library, EntryPoint = "glCopyTexImage1D", ExactSpelling = true)]
            internal extern static void CopyTexImage1D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 x, Int32 y, Int32 width, Int32 border);
            [DllImport(GL.Library, EntryPoint = "glCopyTexImage2D", ExactSpelling = true)]
            internal extern static void CopyTexImage2D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [DllImport(GL.Library, EntryPoint = "glCopyTexSubImage1D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage1D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [DllImport(GL.Library, EntryPoint = "glCopyTextureSubImage1D", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage1D(UInt32 texture, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [DllImport(GL.Library, EntryPoint = "glCopyTexSubImage2D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage2D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glCopyTextureSubImage2D", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage2D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glCopyTexSubImage3D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage3D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glCopyTextureSubImage3D", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage3D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glCreateBuffers", ExactSpelling = true)]
            internal extern static void CreateBuffers(Int32 n, UInt32[] buffers);
            [DllImport(GL.Library, EntryPoint = "glCreateFramebuffers", ExactSpelling = true)]
            internal extern static void CreateFramebuffers(Int32 n, UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glCreateProgram", ExactSpelling = true)]
            internal extern static UInt32 CreateProgram();
            [DllImport(GL.Library, EntryPoint = "glCreateProgramPipelines", ExactSpelling = true)]
            internal extern static void CreateProgramPipelines(Int32 n, UInt32[] pipelines);
            [DllImport(GL.Library, EntryPoint = "glCreateQueries", ExactSpelling = true)]
            internal extern static void CreateQueries(OpenGL.QueryTarget target, Int32 n, UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glCreateRenderbuffers", ExactSpelling = true)]
            internal extern static void CreateRenderbuffers(Int32 n, UInt32[] renderbuffers);
            [DllImport(GL.Library, EntryPoint = "glCreateSamplers", ExactSpelling = true)]
            internal extern static void CreateSamplers(Int32 n, UInt32[] samplers);
            [DllImport(GL.Library, EntryPoint = "glCreateShader", ExactSpelling = true)]
            internal extern static UInt32 CreateShader(OpenGL.ShaderType shaderType);
            [DllImport(GL.Library, EntryPoint = "glCreateShaderProgramv", ExactSpelling = true)]
            internal extern static UInt32 CreateShaderProgramv(OpenGL.ShaderType type, Int32 count, String strings);
            [DllImport(GL.Library, EntryPoint = "glCreateTextures", ExactSpelling = true)]
            internal extern static void CreateTextures(OpenGL.TextureTarget target, Int32 n, UInt32[] textures);
            [DllImport(GL.Library, EntryPoint = "glCreateTransformFeedbacks", ExactSpelling = true)]
            internal extern static void CreateTransformFeedbacks(Int32 n, UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glCreateVertexArrays", ExactSpelling = true)]
            internal extern static void CreateVertexArrays(Int32 n, UInt32[] arrays);
            [DllImport(GL.Library, EntryPoint = "glCullFace", ExactSpelling = true)]
            internal extern static void CullFace(OpenGL.CullFaceMode mode);
            [DllImport(GL.Library, EntryPoint = "glDeleteBuffers", ExactSpelling = true)]
            internal extern static void DeleteBuffers(Int32 n, UInt32[] buffers);
            [DllImport(GL.Library, EntryPoint = "glDeleteFramebuffers", ExactSpelling = true)]
            internal extern static void DeleteFramebuffers(Int32 n, UInt32[] framebuffers);
            [DllImport(GL.Library, EntryPoint = "glDeleteProgram", ExactSpelling = true)]
            internal extern static void DeleteProgram(UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glDeleteProgramPipelines", ExactSpelling = true)]
            internal extern static void DeleteProgramPipelines(Int32 n, UInt32[] pipelines);
            [DllImport(GL.Library, EntryPoint = "glDeleteQueries", ExactSpelling = true)]
            internal extern static void DeleteQueries(Int32 n, UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glDeleteRenderbuffers", ExactSpelling = true)]
            internal extern static void DeleteRenderbuffers(Int32 n, UInt32[] renderbuffers);
            [DllImport(GL.Library, EntryPoint = "glDeleteSamplers", ExactSpelling = true)]
            internal extern static void DeleteSamplers(Int32 n, UInt32[] samplers);
            [DllImport(GL.Library, EntryPoint = "glDeleteShader", ExactSpelling = true)]
            internal extern static void DeleteShader(UInt32 shader);
            [DllImport(GL.Library, EntryPoint = "glDeleteSync", ExactSpelling = true)]
            internal extern static void DeleteSync(IntPtr sync);
            [DllImport(GL.Library, EntryPoint = "glDeleteTextures", ExactSpelling = true)]
            internal extern static void DeleteTextures(Int32 n, UInt32[] textures);
            [DllImport(GL.Library, EntryPoint = "glDeleteTransformFeedbacks", ExactSpelling = true)]
            internal extern static void DeleteTransformFeedbacks(Int32 n, UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glDeleteVertexArrays", ExactSpelling = true)]
            internal extern static void DeleteVertexArrays(Int32 n, UInt32[] arrays);
            [DllImport(GL.Library, EntryPoint = "glDepthFunc", ExactSpelling = true)]
            internal extern static void DepthFunc(OpenGL.DepthFunction func);
            [DllImport(GL.Library, EntryPoint = "glDepthMask", ExactSpelling = true)]
            internal extern static void DepthMask(Boolean flag);
            [DllImport(GL.Library, EntryPoint = "glDepthRange", ExactSpelling = true)]
            internal extern static void DepthRange(Double nearVal, Double farVal);
            [DllImport(GL.Library, EntryPoint = "glDepthRangef", ExactSpelling = true)]
            internal extern static void DepthRangef(Single nearVal, Single farVal);
            [DllImport(GL.Library, EntryPoint = "glDepthRangeArrayv", ExactSpelling = true)]
            internal extern static void DepthRangeArrayv(UInt32 first, Int32 count, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glDepthRangeIndexed", ExactSpelling = true)]
            internal extern static void DepthRangeIndexed(UInt32 index, Double nearVal, Double farVal);
            [DllImport(GL.Library, EntryPoint = "glDetachShader", ExactSpelling = true)]
            internal extern static void DetachShader(UInt32 program, UInt32 shader);
            [DllImport(GL.Library, EntryPoint = "glDispatchCompute", ExactSpelling = true)]
            internal extern static void DispatchCompute(UInt32 num_groups_x, UInt32 num_groups_y, UInt32 num_groups_z);
            [DllImport(GL.Library, EntryPoint = "glDispatchComputeIndirect", ExactSpelling = true)]
            internal extern static void DispatchComputeIndirect(IntPtr indirect);
            [DllImport(GL.Library, EntryPoint = "glDrawArrays", ExactSpelling = true)]
            internal extern static void DrawArrays(OpenGL.BeginMode mode, Int32 first, Int32 count);
            [DllImport(GL.Library, EntryPoint = "glDrawArraysIndirect", ExactSpelling = true)]
            internal extern static void DrawArraysIndirect(OpenGL.BeginMode mode, IntPtr indirect);
            [DllImport(GL.Library, EntryPoint = "glDrawArraysInstanced", ExactSpelling = true)]
            internal extern static void DrawArraysInstanced(OpenGL.BeginMode mode, Int32 first, Int32 count, Int32 primcount);
            [DllImport(GL.Library, EntryPoint = "glDrawArraysInstancedBaseInstance", ExactSpelling = true)]
            internal extern static void DrawArraysInstancedBaseInstance(OpenGL.BeginMode mode, Int32 first, Int32 count, Int32 primcount, UInt32 baseinstance);
            [DllImport(GL.Library, EntryPoint = "glDrawBuffer", ExactSpelling = true)]
            internal extern static void DrawBuffer(OpenGL.DrawBufferMode buf);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferDrawBuffer", ExactSpelling = true)]
            internal extern static void NamedFramebufferDrawBuffer(UInt32 framebuffer, OpenGL.DrawBufferMode buf);
            [DllImport(GL.Library, EntryPoint = "glDrawBuffers", ExactSpelling = true)]
            internal extern static void DrawBuffers(Int32 n, OpenGL.DrawBuffersEnum[] bufs);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferDrawBuffers", ExactSpelling = true)]
            internal extern static void NamedFramebufferDrawBuffers(UInt32 framebuffer, Int32 n, OpenGL.DrawBufferMode[] bufs);
            [DllImport(GL.Library, EntryPoint = "glDrawElements", ExactSpelling = true)]
            internal extern static void DrawElements(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsBaseVertex", ExactSpelling = true)]
            internal extern static void DrawElementsBaseVertex(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 basevertex);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsIndirect", ExactSpelling = true)]
            internal extern static void DrawElementsIndirect(OpenGL.BeginMode mode, OpenGL.DrawElementsType type, IntPtr indirect);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsInstanced", ExactSpelling = true)]
            internal extern static void DrawElementsInstanced(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsInstancedBaseInstance", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedBaseInstance(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount, UInt32 baseinstance);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsInstancedBaseVertex", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedBaseVertex(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount, Int32 basevertex);
            [DllImport(GL.Library, EntryPoint = "glDrawElementsInstancedBaseVertexBaseInstance", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedBaseVertexBaseInstance(OpenGL.BeginMode mode, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount, Int32 basevertex, UInt32 baseinstance);
            [DllImport(GL.Library, EntryPoint = "glDrawRangeElements", ExactSpelling = true)]
            internal extern static void DrawRangeElements(OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count, OpenGL.DrawElementsType type, IntPtr indices);
            [DllImport(GL.Library, EntryPoint = "glDrawRangeElementsBaseVertex", ExactSpelling = true)]
            internal extern static void DrawRangeElementsBaseVertex(OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count, OpenGL.DrawElementsType type, IntPtr indices, Int32 basevertex);
            [DllImport(GL.Library, EntryPoint = "glDrawTransformFeedback", ExactSpelling = true)]
            internal extern static void DrawTransformFeedback(OpenGL.NvTransformFeedback2 mode, UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glDrawTransformFeedbackInstanced", ExactSpelling = true)]
            internal extern static void DrawTransformFeedbackInstanced(OpenGL.BeginMode mode, UInt32 id, Int32 primcount);
            [DllImport(GL.Library, EntryPoint = "glDrawTransformFeedbackStream", ExactSpelling = true)]
            internal extern static void DrawTransformFeedbackStream(OpenGL.NvTransformFeedback2 mode, UInt32 id, UInt32 stream);
            [DllImport(GL.Library, EntryPoint = "glDrawTransformFeedbackStreamInstanced", ExactSpelling = true)]
            internal extern static void DrawTransformFeedbackStreamInstanced(OpenGL.BeginMode mode, UInt32 id, UInt32 stream, Int32 primcount);
            [DllImport(GL.Library, EntryPoint = "glEnable", ExactSpelling = true)]
            internal extern static void Enable(OpenGL.EnableCap cap);
            [DllImport(GL.Library, EntryPoint = "glDisable", ExactSpelling = true)]
            internal extern static void Disable(OpenGL.EnableCap cap);
            [DllImport(GL.Library, EntryPoint = "glEnablei", ExactSpelling = true)]
            internal extern static void Enablei(OpenGL.EnableCap cap, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glDisablei", ExactSpelling = true)]
            internal extern static void Disablei(OpenGL.EnableCap cap, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glEnableVertexAttribArray", ExactSpelling = true)]
            internal extern static void EnableVertexAttribArray(UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glDisableVertexAttribArray", ExactSpelling = true)]
            internal extern static void DisableVertexAttribArray(UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glEnableVertexArrayAttrib", ExactSpelling = true)]
            internal extern static void EnableVertexArrayAttrib(UInt32 vaobj, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glDisableVertexArrayAttrib", ExactSpelling = true)]
            internal extern static void DisableVertexArrayAttrib(UInt32 vaobj, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glFenceSync", ExactSpelling = true)]
            internal extern static IntPtr FenceSync(OpenGL.ArbSync condition, UInt32 flags);
            [DllImport(GL.Library, EntryPoint = "glFinish", ExactSpelling = true)]
            internal extern static void Finish();
            [DllImport(GL.Library, EntryPoint = "glFlush", ExactSpelling = true)]
            internal extern static void Flush();
            [DllImport(GL.Library, EntryPoint = "glFlushMappedBufferRange", ExactSpelling = true)]
            internal extern static void FlushMappedBufferRange(OpenGL.BufferTarget target, IntPtr offset, IntPtr length);
            [DllImport(GL.Library, EntryPoint = "glFlushMappedNamedBufferRange", ExactSpelling = true)]
            internal extern static void FlushMappedNamedBufferRange(UInt32 buffer, IntPtr offset, Int32 length);
            [DllImport(GL.Library, EntryPoint = "glFramebufferParameteri", ExactSpelling = true)]
            internal extern static void FramebufferParameteri(OpenGL.FramebufferTarget target, OpenGL.FramebufferPName pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferParameteri", ExactSpelling = true)]
            internal extern static void NamedFramebufferParameteri(UInt32 framebuffer, OpenGL.FramebufferPName pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glFramebufferRenderbuffer", ExactSpelling = true)]
            internal extern static void FramebufferRenderbuffer(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, OpenGL.RenderbufferTarget renderbuffertarget, UInt32 renderbuffer);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferRenderbuffer", ExactSpelling = true)]
            internal extern static void NamedFramebufferRenderbuffer(UInt32 framebuffer, OpenGL.FramebufferAttachment attachment, OpenGL.RenderbufferTarget renderbuffertarget, UInt32 renderbuffer);
            [DllImport(GL.Library, EntryPoint = "glFramebufferTexture", ExactSpelling = true)]
            internal extern static void FramebufferTexture(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [DllImport(GL.Library, EntryPoint = "glFramebufferTexture1D", ExactSpelling = true)]
            internal extern static void FramebufferTexture1D(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [DllImport(GL.Library, EntryPoint = "glFramebufferTexture2D", ExactSpelling = true)]
            internal extern static void FramebufferTexture2D(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [DllImport(GL.Library, EntryPoint = "glFramebufferTexture3D", ExactSpelling = true)]
            internal extern static void FramebufferTexture3D(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, OpenGL.TextureTarget textarget, UInt32 texture, Int32 level, Int32 layer);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferTexture", ExactSpelling = true)]
            internal extern static void NamedFramebufferTexture(UInt32 framebuffer, OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [DllImport(GL.Library, EntryPoint = "glFramebufferTextureLayer", ExactSpelling = true)]
            internal extern static void FramebufferTextureLayer(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferTextureLayer", ExactSpelling = true)]
            internal extern static void NamedFramebufferTextureLayer(UInt32 framebuffer, OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [DllImport(GL.Library, EntryPoint = "glFrontFace", ExactSpelling = true)]
            internal extern static void FrontFace(OpenGL.FrontFaceDirection mode);
            [DllImport(GL.Library, EntryPoint = "glGenBuffers", ExactSpelling = true)]
            internal extern static void GenBuffers(Int32 n, [OutAttribute] UInt32[] buffers);
            [DllImport(GL.Library, EntryPoint = "glGenerateMipmap", ExactSpelling = true)]
            internal extern static void GenerateMipmap(OpenGL.GenerateMipmapTarget target);
            [DllImport(GL.Library, EntryPoint = "glGenerateTextureMipmap", ExactSpelling = true)]
            internal extern static void GenerateTextureMipmap(UInt32 texture);
            [DllImport(GL.Library, EntryPoint = "glGenFramebuffers", ExactSpelling = true)]
            internal extern static void GenFramebuffers(Int32 n, [OutAttribute] UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glGenProgramPipelines", ExactSpelling = true)]
            internal extern static void GenProgramPipelines(Int32 n, [OutAttribute] UInt32[] pipelines);
            [DllImport(GL.Library, EntryPoint = "glGenQueries", ExactSpelling = true)]
            internal extern static void GenQueries(Int32 n, [OutAttribute] UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glGenRenderbuffers", ExactSpelling = true)]
            internal extern static void GenRenderbuffers(Int32 n, [OutAttribute] UInt32[] renderbuffers);
            [DllImport(GL.Library, EntryPoint = "glGenSamplers", ExactSpelling = true)]
            internal extern static void GenSamplers(Int32 n, [OutAttribute] UInt32[] samplers);
            [DllImport(GL.Library, EntryPoint = "glGenTextures", ExactSpelling = true)]
            internal extern static void GenTextures(Int32 n, [OutAttribute] UInt32[] textures);
            [DllImport(GL.Library, EntryPoint = "glGenTransformFeedbacks", ExactSpelling = true)]
            internal extern static void GenTransformFeedbacks(Int32 n, [OutAttribute] UInt32[] ids);
            [DllImport(GL.Library, EntryPoint = "glGenVertexArrays", ExactSpelling = true)]
            internal extern static void GenVertexArrays(Int32 n, [OutAttribute] UInt32[] arrays);
            [DllImport(GL.Library, EntryPoint = "glGetBooleanv", ExactSpelling = true)]
            internal extern static void GetBooleanv(OpenGL.GetPName pname, [OutAttribute] Boolean[] data);
            [DllImport(GL.Library, EntryPoint = "glGetDoublev", ExactSpelling = true)]
            internal extern static void GetDoublev(OpenGL.GetPName pname, [OutAttribute] Double[] data);
            [DllImport(GL.Library, EntryPoint = "glGetFloatv", ExactSpelling = true)]
            internal extern static void GetFloatv(OpenGL.GetPName pname, [OutAttribute] Single[] data);
            [DllImport(GL.Library, EntryPoint = "glGetIntegerv", ExactSpelling = true)]
            internal extern static void GetIntegerv(OpenGL.GetPName pname, [OutAttribute] Int32[] data);
            [DllImport(GL.Library, EntryPoint = "glGetInteger64v", ExactSpelling = true)]
            internal extern static void GetInteger64v(OpenGL.ArbSync pname, [OutAttribute] Int64[] data);
            [DllImport(GL.Library, EntryPoint = "glGetBooleani_v", ExactSpelling = true)]
            internal extern static void GetBooleani_v(OpenGL.GetPName target, UInt32 index, [OutAttribute] Boolean[] data);
            [DllImport(GL.Library, EntryPoint = "glGetIntegeri_v", ExactSpelling = true)]
            internal extern static void GetIntegeri_v(OpenGL.GetPName target, UInt32 index, [OutAttribute] Int32[] data);
            [DllImport(GL.Library, EntryPoint = "glGetFloati_v", ExactSpelling = true)]
            internal extern static void GetFloati_v(OpenGL.GetPName target, UInt32 index, [OutAttribute] Single[] data);
            [DllImport(GL.Library, EntryPoint = "glGetDoublei_v", ExactSpelling = true)]
            internal extern static void GetDoublei_v(OpenGL.GetPName target, UInt32 index, [OutAttribute] Double[] data);
            [DllImport(GL.Library, EntryPoint = "glGetInteger64i_v", ExactSpelling = true)]
            internal extern static void GetInteger64i_v(OpenGL.GetPName target, UInt32 index, [OutAttribute] Int64[] data);
            [DllImport(GL.Library, EntryPoint = "glGetActiveAtomicCounterBufferiv", ExactSpelling = true)]
            internal extern static void GetActiveAtomicCounterBufferiv(UInt32 program, UInt32 bufferIndex, OpenGL.AtomicCounterParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetActiveAttrib", ExactSpelling = true)]
            internal extern static void GetActiveAttrib(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] size, [OutAttribute] OpenGL.ActiveAttribType[] type, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetActiveSubroutineName", ExactSpelling = true)]
            internal extern static void GetActiveSubroutineName(UInt32 program, OpenGL.ShaderType shadertype, UInt32 index, Int32 bufsize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetActiveSubroutineUniformiv", ExactSpelling = true)]
            internal extern static void GetActiveSubroutineUniformiv(UInt32 program, OpenGL.ShaderType shadertype, UInt32 index, OpenGL.SubroutineParameterName pname, [OutAttribute] Int32[] values);
            [DllImport(GL.Library, EntryPoint = "glGetActiveSubroutineUniformName", ExactSpelling = true)]
            internal extern static void GetActiveSubroutineUniformName(UInt32 program, OpenGL.ShaderType shadertype, UInt32 index, Int32 bufsize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetActiveUniform", ExactSpelling = true)]
            internal extern static void GetActiveUniform(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] size, [OutAttribute] OpenGL.ActiveUniformType[] type, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetActiveUniformBlockiv", ExactSpelling = true)]
            internal extern static void GetActiveUniformBlockiv(UInt32 program, UInt32 uniformBlockIndex, OpenGL.ActiveUniformBlockParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetActiveUniformBlockName", ExactSpelling = true)]
            internal extern static void GetActiveUniformBlockName(UInt32 program, UInt32 uniformBlockIndex, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder uniformBlockName);
            [DllImport(GL.Library, EntryPoint = "glGetActiveUniformName", ExactSpelling = true)]
            internal extern static void GetActiveUniformName(UInt32 program, UInt32 uniformIndex, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder uniformName);
            [DllImport(GL.Library, EntryPoint = "glGetActiveUniformsiv", ExactSpelling = true)]
            internal extern static void GetActiveUniformsiv(UInt32 program, Int32 uniformCount, [OutAttribute] UInt32[] uniformIndices, OpenGL.ActiveUniformType pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetAttachedShaders", ExactSpelling = true)]
            internal extern static void GetAttachedShaders(UInt32 program, Int32 maxCount, [OutAttribute] Int32[] count, [OutAttribute] UInt32[] shaders);
            [DllImport(GL.Library, EntryPoint = "glGetAttribLocation", ExactSpelling = true)]
            internal extern static Int32 GetAttribLocation(UInt32 program, String name);
            [DllImport(GL.Library, EntryPoint = "glGetBufferParameteriv", ExactSpelling = true)]
            internal extern static void GetBufferParameteriv(OpenGL.BufferTarget target, OpenGL.BufferParameterName value, [OutAttribute] Int32[] data);
            [DllImport(GL.Library, EntryPoint = "glGetBufferParameteri64v", ExactSpelling = true)]
            internal extern static void GetBufferParameteri64v(OpenGL.BufferTarget target, OpenGL.BufferParameterName value, [OutAttribute] Int64[] data);
            [DllImport(GL.Library, EntryPoint = "glGetNamedBufferParameteriv", ExactSpelling = true)]
            internal extern static void GetNamedBufferParameteriv(UInt32 buffer, OpenGL.BufferParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetNamedBufferParameteri64v", ExactSpelling = true)]
            internal extern static void GetNamedBufferParameteri64v(UInt32 buffer, OpenGL.BufferParameterName pname, [OutAttribute] Int64[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetBufferPointerv", ExactSpelling = true)]
            internal extern static void GetBufferPointerv(OpenGL.BufferTarget target, OpenGL.BufferPointer pname, [OutAttribute] IntPtr @params);
            [DllImport(GL.Library, EntryPoint = "glGetNamedBufferPointerv", ExactSpelling = true)]
            internal extern static void GetNamedBufferPointerv(UInt32 buffer, OpenGL.BufferPointer pname, [OutAttribute] IntPtr @params);
            [DllImport(GL.Library, EntryPoint = "glGetBufferSubData", ExactSpelling = true)]
            internal extern static void GetBufferSubData(OpenGL.BufferTarget target, IntPtr offset, IntPtr size, [OutAttribute] IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glGetNamedBufferSubData", ExactSpelling = true)]
            internal extern static void GetNamedBufferSubData(UInt32 buffer, IntPtr offset, Int32 size, [OutAttribute] IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glGetCompressedTexImage", ExactSpelling = true)]
            internal extern static void GetCompressedTexImage(OpenGL.TextureTarget target, Int32 level, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetnCompressedTexImage", ExactSpelling = true)]
            internal extern static void GetnCompressedTexImage(OpenGL.TextureTarget target, Int32 level, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetCompressedTextureImage", ExactSpelling = true)]
            internal extern static void GetCompressedTextureImage(UInt32 texture, Int32 level, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetCompressedTextureSubImage", ExactSpelling = true)]
            internal extern static void GetCompressedTextureSubImage(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetError", ExactSpelling = true)]
            internal extern static OpenGL.ErrorCode GetError();
            [DllImport(GL.Library, EntryPoint = "glGetFragDataIndex", ExactSpelling = true)]
            internal extern static Int32 GetFragDataIndex(UInt32 program, String name);
            [DllImport(GL.Library, EntryPoint = "glGetFragDataLocation", ExactSpelling = true)]
            internal extern static Int32 GetFragDataLocation(UInt32 program, String name);
            [DllImport(GL.Library, EntryPoint = "glGetFramebufferAttachmentParameteriv", ExactSpelling = true)]
            internal extern static void GetFramebufferAttachmentParameteriv(OpenGL.FramebufferTarget target, OpenGL.FramebufferAttachment attachment, OpenGL.FramebufferParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetNamedFramebufferAttachmentParameteriv", ExactSpelling = true)]
            internal extern static void GetNamedFramebufferAttachmentParameteriv(UInt32 framebuffer, OpenGL.FramebufferAttachment attachment, OpenGL.FramebufferParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetFramebufferParameteriv", ExactSpelling = true)]
            internal extern static void GetFramebufferParameteriv(OpenGL.FramebufferTarget target, OpenGL.FramebufferPName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetNamedFramebufferParameteriv", ExactSpelling = true)]
            internal extern static void GetNamedFramebufferParameteriv(UInt32 framebuffer, OpenGL.FramebufferPName pname, [OutAttribute] Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glGetGraphicsResetStatus", ExactSpelling = true)]
            internal extern static OpenGL.GraphicResetStatus GetGraphicsResetStatus();
            [DllImport(GL.Library, EntryPoint = "glGetInternalformativ", ExactSpelling = true)]
            internal extern static void GetInternalformativ(OpenGL.TextureTarget target, OpenGL.PixelInternalFormat internalFormat, OpenGL.GetPName pname, Int32 bufSize, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetInternalformati64v", ExactSpelling = true)]
            internal extern static void GetInternalformati64v(OpenGL.TextureTarget target, OpenGL.PixelInternalFormat internalFormat, OpenGL.GetPName pname, Int32 bufSize, [OutAttribute] Int64[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetMultisamplefv", ExactSpelling = true)]
            internal extern static void GetMultisamplefv(OpenGL.GetMultisamplePName pname, UInt32 index, [OutAttribute] Single[] val);
            [DllImport(GL.Library, EntryPoint = "glGetObjectLabel", ExactSpelling = true)]
            internal extern static void GetObjectLabel(OpenGL.ObjectLabel identifier, UInt32 name, Int32 bifSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder label);
            [DllImport(GL.Library, EntryPoint = "glGetObjectPtrLabel", ExactSpelling = true)]
            internal extern static void GetObjectPtrLabel([OutAttribute] IntPtr ptr, Int32 bifSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder label);
            [DllImport(GL.Library, EntryPoint = "glGetPointerv", ExactSpelling = true)]
            internal extern static void GetPointerv(OpenGL.GetPointerParameter pname, [OutAttribute] IntPtr @params);
            [DllImport(GL.Library, EntryPoint = "glGetProgramiv", ExactSpelling = true)]
            internal extern static void GetProgramiv(UInt32 program, OpenGL.ProgramParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetProgramBinary", ExactSpelling = true)]
            internal extern static void GetProgramBinary(UInt32 program, Int32 bufsize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] binaryFormat, [OutAttribute] IntPtr binary);
            [DllImport(GL.Library, EntryPoint = "glGetProgramInfoLog", ExactSpelling = true)]
            internal extern static void GetProgramInfoLog(UInt32 program, Int32 maxLength, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder infoLog);
            [DllImport(GL.Library, EntryPoint = "glGetProgramInterfaceiv", ExactSpelling = true)]
            internal extern static void GetProgramInterfaceiv(UInt32 program, OpenGL.ProgramInterface programInterface, OpenGL.ProgramInterfaceParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetProgramPipelineiv", ExactSpelling = true)]
            internal extern static void GetProgramPipelineiv(UInt32 pipeline, Int32 pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetProgramPipelineInfoLog", ExactSpelling = true)]
            internal extern static void GetProgramPipelineInfoLog(UInt32 pipeline, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder infoLog);
            [DllImport(GL.Library, EntryPoint = "glGetProgramResourceiv", ExactSpelling = true)]
            internal extern static void GetProgramResourceiv(UInt32 program, OpenGL.ProgramInterface programInterface, UInt32 index, Int32 propCount, [OutAttribute] OpenGL.ProgramResourceParameterName[] props, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetProgramResourceIndex", ExactSpelling = true)]
            internal extern static UInt32 GetProgramResourceIndex(UInt32 program, OpenGL.ProgramInterface programInterface, String name);
            [DllImport(GL.Library, EntryPoint = "glGetProgramResourceLocation", ExactSpelling = true)]
            internal extern static Int32 GetProgramResourceLocation(UInt32 program, OpenGL.ProgramInterface programInterface, String name);
            [DllImport(GL.Library, EntryPoint = "glGetProgramResourceLocationIndex", ExactSpelling = true)]
            internal extern static Int32 GetProgramResourceLocationIndex(UInt32 program, OpenGL.ProgramInterface programInterface, String name);
            [DllImport(GL.Library, EntryPoint = "glGetProgramResourceName", ExactSpelling = true)]
            internal extern static void GetProgramResourceName(UInt32 program, OpenGL.ProgramInterface programInterface, UInt32 index, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetProgramStageiv", ExactSpelling = true)]
            internal extern static void GetProgramStageiv(UInt32 program, OpenGL.ShaderType shadertype, OpenGL.ProgramStageParameterName pname, [OutAttribute] Int32[] values);
            [DllImport(GL.Library, EntryPoint = "glGetQueryIndexediv", ExactSpelling = true)]
            internal extern static void GetQueryIndexediv(OpenGL.QueryTarget target, UInt32 index, OpenGL.GetQueryParam pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetQueryiv", ExactSpelling = true)]
            internal extern static void GetQueryiv(OpenGL.QueryTarget target, OpenGL.GetQueryParam pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
            internal extern static void GetQueryObjectiv(UInt32 id, OpenGL.GetQueryObjectParam pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetQueryObjectuiv", ExactSpelling = true)]
            internal extern static void GetQueryObjectuiv(UInt32 id, OpenGL.GetQueryObjectParam pname, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetQueryObjecti64v", ExactSpelling = true)]
            internal extern static void GetQueryObjecti64v(UInt32 id, OpenGL.GetQueryObjectParam pname, [OutAttribute] Int64[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetQueryObjectui64v", ExactSpelling = true)]
            internal extern static void GetQueryObjectui64v(UInt32 id, OpenGL.GetQueryObjectParam pname, [OutAttribute] UInt64[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetRenderbufferParameteriv", ExactSpelling = true)]
            internal extern static void GetRenderbufferParameteriv(OpenGL.RenderbufferTarget target, OpenGL.RenderbufferParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetNamedRenderbufferParameteriv", ExactSpelling = true)]
            internal extern static void GetNamedRenderbufferParameteriv(UInt32 renderbuffer, OpenGL.RenderbufferParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetSamplerParameterfv", ExactSpelling = true)]
            internal extern static void GetSamplerParameterfv(UInt32 sampler, Int32 pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetSamplerParameteriv", ExactSpelling = true)]
            internal extern static void GetSamplerParameteriv(UInt32 sampler, Int32 pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetSamplerParameterIiv", ExactSpelling = true)]
            internal extern static void GetSamplerParameterIiv(UInt32 sampler, OpenGL.TextureParameterName pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetSamplerParameterIuiv", ExactSpelling = true)]
            internal extern static void GetSamplerParameterIuiv(UInt32 sampler, OpenGL.TextureParameterName pname, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetShaderiv", ExactSpelling = true)]
            internal extern static void GetShaderiv(UInt32 shader, OpenGL.ShaderParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetShaderInfoLog", ExactSpelling = true)]
            internal extern static void GetShaderInfoLog(UInt32 shader, Int32 maxLength, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder infoLog);
            [DllImport(GL.Library, EntryPoint = "glGetShaderPrecisionFormat", ExactSpelling = true)]
            internal extern static void GetShaderPrecisionFormat(OpenGL.ShaderType shaderType, Int32 precisionType, [OutAttribute] Int32[] range, [OutAttribute] Int32[] precision);
            [DllImport(GL.Library, EntryPoint = "glGetShaderSource", ExactSpelling = true)]
            internal extern static void GetShaderSource(UInt32 shader, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] System.Text.StringBuilder source);
            [DllImport(GL.Library, EntryPoint = "glGetString", ExactSpelling = true)]
            internal extern static IntPtr GetString(OpenGL.StringName name);
            [DllImport(GL.Library, EntryPoint = "glGetStringi", ExactSpelling = true)]
            internal extern static IntPtr GetStringi(OpenGL.StringName name, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glGetSubroutineIndex", ExactSpelling = true)]
            internal extern static UInt32 GetSubroutineIndex(UInt32 program, OpenGL.ShaderType shadertype, String name);
            [DllImport(GL.Library, EntryPoint = "glGetSubroutineUniformLocation", ExactSpelling = true)]
            internal extern static Int32 GetSubroutineUniformLocation(UInt32 program, OpenGL.ShaderType shadertype, String name);
            [DllImport(GL.Library, EntryPoint = "glGetSynciv", ExactSpelling = true)]
            internal extern static void GetSynciv(IntPtr sync, OpenGL.ArbSync pname, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] values);
            [DllImport(GL.Library, EntryPoint = "glGetTexImage", ExactSpelling = true)]
            internal extern static void GetTexImage(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelFormat format, OpenGL.PixelType type, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetnTexImage", ExactSpelling = true)]
            internal extern static void GetnTexImage(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelFormat format, OpenGL.PixelType type, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetTextureImage", ExactSpelling = true)]
            internal extern static void GetTextureImage(UInt32 texture, Int32 level, OpenGL.PixelFormat format, OpenGL.PixelType type, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetTexLevelParameterfv", ExactSpelling = true)]
            internal extern static void GetTexLevelParameterfv(OpenGL.GetPName target, Int32 level, OpenGL.GetTextureLevelParameter pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTexLevelParameteriv", ExactSpelling = true)]
            internal extern static void GetTexLevelParameteriv(OpenGL.GetPName target, Int32 level, OpenGL.GetTextureLevelParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureLevelParameterfv", ExactSpelling = true)]
            internal extern static void GetTextureLevelParameterfv(UInt32 texture, Int32 level, OpenGL.GetTextureLevelParameter pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureLevelParameteriv", ExactSpelling = true)]
            internal extern static void GetTextureLevelParameteriv(UInt32 texture, Int32 level, OpenGL.GetTextureLevelParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTexParameterfv", ExactSpelling = true)]
            internal extern static void GetTexParameterfv(OpenGL.TextureTarget target, OpenGL.GetTextureParameter pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTexParameteriv", ExactSpelling = true)]
            internal extern static void GetTexParameteriv(OpenGL.TextureTarget target, OpenGL.GetTextureParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTexParameterIiv", ExactSpelling = true)]
            internal extern static void GetTexParameterIiv(OpenGL.TextureTarget target, OpenGL.GetTextureParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTexParameterIuiv", ExactSpelling = true)]
            internal extern static void GetTexParameterIuiv(OpenGL.TextureTarget target, OpenGL.GetTextureParameter pname, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureParameterfv", ExactSpelling = true)]
            internal extern static void GetTextureParameterfv(UInt32 texture, OpenGL.GetTextureParameter pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureParameteriv", ExactSpelling = true)]
            internal extern static void GetTextureParameteriv(UInt32 texture, OpenGL.GetTextureParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureParameterIiv", ExactSpelling = true)]
            internal extern static void GetTextureParameterIiv(UInt32 texture, OpenGL.GetTextureParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureParameterIuiv", ExactSpelling = true)]
            internal extern static void GetTextureParameterIuiv(UInt32 texture, OpenGL.GetTextureParameter pname, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetTextureSubImage", ExactSpelling = true)]
            internal extern static void GetTextureSubImage(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelFormat format, OpenGL.PixelType type, Int32 bufSize, [OutAttribute] IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glGetTransformFeedbackiv", ExactSpelling = true)]
            internal extern static void GetTransformFeedbackiv(UInt32 xfb, OpenGL.TransformFeedbackParameterName pname, [OutAttribute] Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glGetTransformFeedbacki_v", ExactSpelling = true)]
            internal extern static void GetTransformFeedbacki_v(UInt32 xfb, OpenGL.TransformFeedbackParameterName pname, UInt32 index, [OutAttribute] Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glGetTransformFeedbacki64_v", ExactSpelling = true)]
            internal extern static void GetTransformFeedbacki64_v(UInt32 xfb, OpenGL.TransformFeedbackParameterName pname, UInt32 index, [OutAttribute] Int64[] param);
            [DllImport(GL.Library, EntryPoint = "glGetTransformFeedbackVarying", ExactSpelling = true)]
            internal extern static void GetTransformFeedbackVarying(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32[] length, [OutAttribute] Int32[] size, [OutAttribute] OpenGL.ActiveAttribType[] type, [OutAttribute] System.Text.StringBuilder name);
            [DllImport(GL.Library, EntryPoint = "glGetUniformfv", ExactSpelling = true)]
            internal extern static void GetUniformfv(UInt32 program, Int32 location, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetUniformiv", ExactSpelling = true)]
            internal extern static void GetUniformiv(UInt32 program, Int32 location, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetUniformuiv", ExactSpelling = true)]
            internal extern static void GetUniformuiv(UInt32 program, Int32 location, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetUniformdv", ExactSpelling = true)]
            internal extern static void GetUniformdv(UInt32 program, Int32 location, [OutAttribute] Double[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetnUniformfv", ExactSpelling = true)]
            internal extern static void GetnUniformfv(UInt32 program, Int32 location, Int32 bufSize, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetnUniformiv", ExactSpelling = true)]
            internal extern static void GetnUniformiv(UInt32 program, Int32 location, Int32 bufSize, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetnUniformuiv", ExactSpelling = true)]
            internal extern static void GetnUniformuiv(UInt32 program, Int32 location, Int32 bufSize, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetnUniformdv", ExactSpelling = true)]
            internal extern static void GetnUniformdv(UInt32 program, Int32 location, Int32 bufSize, [OutAttribute] Double[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetUniformBlockIndex", ExactSpelling = true)]
            internal extern static UInt32 GetUniformBlockIndex(UInt32 program, String uniformBlockName);
            [DllImport(GL.Library, EntryPoint = "glGetUniformIndices", ExactSpelling = true)]
            internal extern static void GetUniformIndices(UInt32 program, Int32 uniformCount, String uniformNames, [OutAttribute] UInt32[] uniformIndices);
            [DllImport(GL.Library, EntryPoint = "glGetUniformLocation", ExactSpelling = true)]
            internal extern static Int32 GetUniformLocation(UInt32 program, String name);
            [DllImport(GL.Library, EntryPoint = "glGetUniformSubroutineuiv", ExactSpelling = true)]
            internal extern static void GetUniformSubroutineuiv(OpenGL.ShaderType shadertype, Int32 location, [OutAttribute] UInt32[] values);
            [DllImport(GL.Library, EntryPoint = "glGetVertexArrayIndexed64iv", ExactSpelling = true)]
            internal extern static void GetVertexArrayIndexed64iv(UInt32 vaobj, UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Int64[] param);
            [DllImport(GL.Library, EntryPoint = "glGetVertexArrayIndexediv", ExactSpelling = true)]
            internal extern static void GetVertexArrayIndexediv(UInt32 vaobj, UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glGetVertexArrayiv", ExactSpelling = true)]
            internal extern static void GetVertexArrayiv(UInt32 vaobj, OpenGL.VertexAttribParameter pname, [OutAttribute] Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribdv", ExactSpelling = true)]
            internal extern static void GetVertexAttribdv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Double[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribfv", ExactSpelling = true)]
            internal extern static void GetVertexAttribfv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribiv", ExactSpelling = true)]
            internal extern static void GetVertexAttribiv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribIiv", ExactSpelling = true)]
            internal extern static void GetVertexAttribIiv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribIuiv", ExactSpelling = true)]
            internal extern static void GetVertexAttribIuiv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribLdv", ExactSpelling = true)]
            internal extern static void GetVertexAttribLdv(UInt32 index, OpenGL.VertexAttribParameter pname, [OutAttribute] Double[] @params);
            [DllImport(GL.Library, EntryPoint = "glGetVertexAttribPointerv", ExactSpelling = true)]
            internal extern static void GetVertexAttribPointerv(UInt32 index, OpenGL.VertexAttribPointerParameter pname, [OutAttribute] IntPtr pointer);
            [DllImport(GL.Library, EntryPoint = "glHint", ExactSpelling = true)]
            internal extern static void Hint(OpenGL.HintTarget target, OpenGL.HintMode mode);
            [DllImport(GL.Library, EntryPoint = "glInvalidateBufferData", ExactSpelling = true)]
            internal extern static void InvalidateBufferData(UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glInvalidateBufferSubData", ExactSpelling = true)]
            internal extern static void InvalidateBufferSubData(UInt32 buffer, IntPtr offset, IntPtr length);
            [DllImport(GL.Library, EntryPoint = "glInvalidateFramebuffer", ExactSpelling = true)]
            internal extern static void InvalidateFramebuffer(OpenGL.FramebufferTarget target, Int32 numAttachments, OpenGL.FramebufferAttachment[] attachments);
            [DllImport(GL.Library, EntryPoint = "glInvalidateNamedFramebufferData", ExactSpelling = true)]
            internal extern static void InvalidateNamedFramebufferData(UInt32 framebuffer, Int32 numAttachments, OpenGL.FramebufferAttachment[] attachments);
            [DllImport(GL.Library, EntryPoint = "glInvalidateSubFramebuffer", ExactSpelling = true)]
            internal extern static void InvalidateSubFramebuffer(OpenGL.FramebufferTarget target, Int32 numAttachments, OpenGL.FramebufferAttachment[] attachments, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glInvalidateNamedFramebufferSubData", ExactSpelling = true)]
            internal extern static void InvalidateNamedFramebufferSubData(UInt32 framebuffer, Int32 numAttachments, OpenGL.FramebufferAttachment[] attachments, Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glInvalidateTexImage", ExactSpelling = true)]
            internal extern static void InvalidateTexImage(UInt32 texture, Int32 level);
            [DllImport(GL.Library, EntryPoint = "glInvalidateTexSubImage", ExactSpelling = true)]
            internal extern static void InvalidateTexSubImage(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth);
            [DllImport(GL.Library, EntryPoint = "glIsBuffer", ExactSpelling = true)]
            internal extern static Boolean IsBuffer(UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glIsEnabled", ExactSpelling = true)]
            internal extern static Boolean IsEnabled(OpenGL.EnableCap cap);
            [DllImport(GL.Library, EntryPoint = "glIsEnabledi", ExactSpelling = true)]
            internal extern static Boolean IsEnabledi(OpenGL.EnableCap cap, UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glIsFramebuffer", ExactSpelling = true)]
            internal extern static Boolean IsFramebuffer(UInt32 framebuffer);
            [DllImport(GL.Library, EntryPoint = "glIsProgram", ExactSpelling = true)]
            internal extern static Boolean IsProgram(UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glIsProgramPipeline", ExactSpelling = true)]
            internal extern static Boolean IsProgramPipeline(UInt32 pipeline);
            [DllImport(GL.Library, EntryPoint = "glIsQuery", ExactSpelling = true)]
            internal extern static Boolean IsQuery(UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glIsRenderbuffer", ExactSpelling = true)]
            internal extern static Boolean IsRenderbuffer(UInt32 renderbuffer);
            [DllImport(GL.Library, EntryPoint = "glIsSampler", ExactSpelling = true)]
            internal extern static Boolean IsSampler(UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glIsShader", ExactSpelling = true)]
            internal extern static Boolean IsShader(UInt32 shader);
            [DllImport(GL.Library, EntryPoint = "glIsSync", ExactSpelling = true)]
            internal extern static Boolean IsSync(IntPtr sync);
            [DllImport(GL.Library, EntryPoint = "glIsTexture", ExactSpelling = true)]
            internal extern static Boolean IsTexture(UInt32 texture);
            [DllImport(GL.Library, EntryPoint = "glIsTransformFeedback", ExactSpelling = true)]
            internal extern static Boolean IsTransformFeedback(UInt32 id);
            [DllImport(GL.Library, EntryPoint = "glIsVertexArray", ExactSpelling = true)]
            internal extern static Boolean IsVertexArray(UInt32 array);
            [DllImport(GL.Library, EntryPoint = "glLineWidth", ExactSpelling = true)]
            internal extern static void LineWidth(Single width);
            [DllImport(GL.Library, EntryPoint = "glLinkProgram", ExactSpelling = true)]
            internal extern static void LinkProgram(UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glLogicOp", ExactSpelling = true)]
            internal extern static void LogicOp(OpenGL.LogicOp opcode);
            [DllImport(GL.Library, EntryPoint = "glMapBuffer", ExactSpelling = true)]
            internal extern static IntPtr MapBuffer(OpenGL.BufferTarget target, OpenGL.BufferAccess access);
            [DllImport(GL.Library, EntryPoint = "glMapNamedBuffer", ExactSpelling = true)]
            internal extern static IntPtr MapNamedBuffer(UInt32 buffer, OpenGL.BufferAccess access);
            [DllImport(GL.Library, EntryPoint = "glMapBufferRange", ExactSpelling = true)]
            internal extern static IntPtr MapBufferRange(OpenGL.BufferTarget target, IntPtr offset, IntPtr length, OpenGL.BufferAccessMask access);
            [DllImport(GL.Library, EntryPoint = "glMapNamedBufferRange", ExactSpelling = true)]
            internal extern static IntPtr MapNamedBufferRange(UInt32 buffer, IntPtr offset, Int32 length, UInt32 access);
            [DllImport(GL.Library, EntryPoint = "glMemoryBarrier", ExactSpelling = true)]
            internal extern static void MemoryBarrier(UInt32 barriers);
            [DllImport(GL.Library, EntryPoint = "glMemoryBarrierByRegion", ExactSpelling = true)]
            internal extern static void MemoryBarrierByRegion(UInt32 barriers);
            [DllImport(GL.Library, EntryPoint = "glMinSampleShading", ExactSpelling = true)]
            internal extern static void MinSampleShading(Single value);
            [DllImport(GL.Library, EntryPoint = "glMultiDrawArrays", ExactSpelling = true)]
            internal extern static void MultiDrawArrays(OpenGL.BeginMode mode, Int32[] first, Int32[] count, Int32 drawcount);
            [DllImport(GL.Library, EntryPoint = "glMultiDrawArraysIndirect", ExactSpelling = true)]
            internal extern static void MultiDrawArraysIndirect(OpenGL.BeginMode mode, IntPtr indirect, Int32 drawcount, Int32 stride);
            [DllImport(GL.Library, EntryPoint = "glMultiDrawElements", ExactSpelling = true)]
            internal extern static void MultiDrawElements(OpenGL.BeginMode mode, Int32[] count, OpenGL.DrawElementsType type, IntPtr indices, Int32 drawcount);
            [DllImport(GL.Library, EntryPoint = "glMultiDrawElementsBaseVertex", ExactSpelling = true)]
            internal extern static void MultiDrawElementsBaseVertex(OpenGL.BeginMode mode, Int32[] count, OpenGL.DrawElementsType type, IntPtr indices, Int32 drawcount, Int32[] basevertex);
            [DllImport(GL.Library, EntryPoint = "glMultiDrawElementsIndirect", ExactSpelling = true)]
            internal extern static void MultiDrawElementsIndirect(OpenGL.BeginMode mode, OpenGL.DrawElementsType type, IntPtr indirect, Int32 drawcount, Int32 stride);
            [DllImport(GL.Library, EntryPoint = "glObjectLabel", ExactSpelling = true)]
            internal extern static void ObjectLabel(OpenGL.ObjectLabel identifier, UInt32 name, Int32 length, String label);
            [DllImport(GL.Library, EntryPoint = "glObjectPtrLabel", ExactSpelling = true)]
            internal extern static void ObjectPtrLabel(IntPtr ptr, Int32 length, String label);
            [DllImport(GL.Library, EntryPoint = "glPatchParameteri", ExactSpelling = true)]
            internal extern static void PatchParameteri(Int32 pname, Int32 value);
            [DllImport(GL.Library, EntryPoint = "glPatchParameterfv", ExactSpelling = true)]
            internal extern static void PatchParameterfv(Int32 pname, Single[] values);
            [DllImport(GL.Library, EntryPoint = "glPixelStoref", ExactSpelling = true)]
            internal extern static void PixelStoref(OpenGL.PixelStoreParameter pname, Single param);
            [DllImport(GL.Library, EntryPoint = "glPixelStorei", ExactSpelling = true)]
            internal extern static void PixelStorei(OpenGL.PixelStoreParameter pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glPointParameterf", ExactSpelling = true)]
            internal extern static void PointParameterf(OpenGL.PointParameterName pname, Single param);
            [DllImport(GL.Library, EntryPoint = "glPointParameteri", ExactSpelling = true)]
            internal extern static void PointParameteri(OpenGL.PointParameterName pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glPointParameterfv", ExactSpelling = true)]
            internal extern static void PointParameterfv(OpenGL.PointParameterName pname, Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glPointParameteriv", ExactSpelling = true)]
            internal extern static void PointParameteriv(OpenGL.PointParameterName pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glPointSize", ExactSpelling = true)]
            internal extern static void PointSize(Single size);
            [DllImport(GL.Library, EntryPoint = "glPolygonMode", ExactSpelling = true)]
            internal extern static void PolygonMode(OpenGL.MaterialFace face, OpenGL.PolygonMode mode);
            [DllImport(GL.Library, EntryPoint = "glPolygonOffset", ExactSpelling = true)]
            internal extern static void PolygonOffset(Single factor, Single units);
            [DllImport(GL.Library, EntryPoint = "glPrimitiveRestartIndex", ExactSpelling = true)]
            internal extern static void PrimitiveRestartIndex(UInt32 index);
            [DllImport(GL.Library, EntryPoint = "glProgramBinary", ExactSpelling = true)]
            internal extern static void ProgramBinary(UInt32 program, Int32 binaryFormat, IntPtr binary, Int32 length);
            [DllImport(GL.Library, EntryPoint = "glProgramParameteri", ExactSpelling = true)]
            internal extern static void ProgramParameteri(UInt32 program, OpenGL.Version32 pname, Int32 value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1f", ExactSpelling = true)]
            internal extern static void ProgramUniform1f(UInt32 program, Int32 location, Single v0);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2f", ExactSpelling = true)]
            internal extern static void ProgramUniform2f(UInt32 program, Int32 location, Single v0, Single v1);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3f", ExactSpelling = true)]
            internal extern static void ProgramUniform3f(UInt32 program, Int32 location, Single v0, Single v1, Single v2);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4f", ExactSpelling = true)]
            internal extern static void ProgramUniform4f(UInt32 program, Int32 location, Single v0, Single v1, Single v2, Single v3);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1i", ExactSpelling = true)]
            internal extern static void ProgramUniform1i(UInt32 program, Int32 location, Int32 v0);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2i", ExactSpelling = true)]
            internal extern static void ProgramUniform2i(UInt32 program, Int32 location, Int32 v0, Int32 v1);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3i", ExactSpelling = true)]
            internal extern static void ProgramUniform3i(UInt32 program, Int32 location, Int32 v0, Int32 v1, Int32 v2);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4i", ExactSpelling = true)]
            internal extern static void ProgramUniform4i(UInt32 program, Int32 location, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1ui", ExactSpelling = true)]
            internal extern static void ProgramUniform1ui(UInt32 program, Int32 location, UInt32 v0);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2ui", ExactSpelling = true)]
            internal extern static void ProgramUniform2ui(UInt32 program, Int32 location, Int32 v0, UInt32 v1);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3ui", ExactSpelling = true)]
            internal extern static void ProgramUniform3ui(UInt32 program, Int32 location, Int32 v0, Int32 v1, UInt32 v2);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4ui", ExactSpelling = true)]
            internal extern static void ProgramUniform4ui(UInt32 program, Int32 location, Int32 v0, Int32 v1, Int32 v2, UInt32 v3);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1fv", ExactSpelling = true)]
            internal extern static void ProgramUniform1fv(UInt32 program, Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2fv", ExactSpelling = true)]
            internal extern static void ProgramUniform2fv(UInt32 program, Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3fv", ExactSpelling = true)]
            internal extern static void ProgramUniform3fv(UInt32 program, Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4fv", ExactSpelling = true)]
            internal extern static void ProgramUniform4fv(UInt32 program, Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1iv", ExactSpelling = true)]
            internal extern static void ProgramUniform1iv(UInt32 program, Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2iv", ExactSpelling = true)]
            internal extern static void ProgramUniform2iv(UInt32 program, Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3iv", ExactSpelling = true)]
            internal extern static void ProgramUniform3iv(UInt32 program, Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4iv", ExactSpelling = true)]
            internal extern static void ProgramUniform4iv(UInt32 program, Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform1uiv", ExactSpelling = true)]
            internal extern static void ProgramUniform1uiv(UInt32 program, Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform2uiv", ExactSpelling = true)]
            internal extern static void ProgramUniform2uiv(UInt32 program, Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform3uiv", ExactSpelling = true)]
            internal extern static void ProgramUniform3uiv(UInt32 program, Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniform4uiv", ExactSpelling = true)]
            internal extern static void ProgramUniform4uiv(UInt32 program, Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix2fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix2fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix3fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix3fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix4fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix4fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix2x3fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix2x3fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix3x2fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix3x2fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix2x4fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix2x4fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix4x2fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix4x2fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix3x4fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix3x4fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProgramUniformMatrix4x3fv", ExactSpelling = true)]
            internal extern static void ProgramUniformMatrix4x3fv(UInt32 program, Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glProvokingVertex", ExactSpelling = true)]
            internal extern static void ProvokingVertex(OpenGL.ProvokingVertexMode provokeMode);
            [DllImport(GL.Library, EntryPoint = "glQueryCounter", ExactSpelling = true)]
            internal extern static void QueryCounter(UInt32 id, Int32 target);
            [DllImport(GL.Library, EntryPoint = "glReadBuffer", ExactSpelling = true)]
            internal extern static void ReadBuffer(OpenGL.ReadBufferMode mode);
            [DllImport(GL.Library, EntryPoint = "glNamedFramebufferReadBuffer", ExactSpelling = true)]
            internal extern static void NamedFramebufferReadBuffer(UInt32 framebuffer, OpenGL.BeginMode mode);
            [DllImport(GL.Library, EntryPoint = "glReadPixels", ExactSpelling = true)]
            internal extern static void ReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenGL.PixelFormat format, OpenGL.PixelType type, Int32[] data);
            [DllImport(GL.Library, EntryPoint = "glReadnPixels", ExactSpelling = true)]
            internal extern static void ReadnPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenGL.PixelFormat format, OpenGL.PixelType type, Int32 bufSize, Int32[] data);
            [DllImport(GL.Library, EntryPoint = "glRenderbufferStorage", ExactSpelling = true)]
            internal extern static void RenderbufferStorage(OpenGL.RenderbufferTarget target, OpenGL.RenderbufferStorage internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glNamedRenderbufferStorage", ExactSpelling = true)]
            internal extern static void NamedRenderbufferStorage(UInt32 renderbuffer, OpenGL.RenderbufferStorage internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glRenderbufferStorageMultisample", ExactSpelling = true)]
            internal extern static void RenderbufferStorageMultisample(OpenGL.RenderbufferTarget target, Int32 samples, OpenGL.RenderbufferStorage internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glNamedRenderbufferStorageMultisample", ExactSpelling = true)]
            internal extern static void NamedRenderbufferStorageMultisample(UInt32 renderbuffer, Int32 samples, OpenGL.RenderbufferStorage internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glSampleCoverage", ExactSpelling = true)]
            internal extern static void SampleCoverage(Single value, Boolean invert);
            [DllImport(GL.Library, EntryPoint = "glSampleMaski", ExactSpelling = true)]
            internal extern static void SampleMaski(UInt32 maskNumber, UInt32 mask);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameterf", ExactSpelling = true)]
            internal extern static void SamplerParameterf(UInt32 sampler, Int32 pname, Single param);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameteri", ExactSpelling = true)]
            internal extern static void SamplerParameteri(UInt32 sampler, Int32 pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameterfv", ExactSpelling = true)]
            internal extern static void SamplerParameterfv(UInt32 sampler, Int32 pname, Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameteriv", ExactSpelling = true)]
            internal extern static void SamplerParameteriv(UInt32 sampler, Int32 pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameterIiv", ExactSpelling = true)]
            internal extern static void SamplerParameterIiv(UInt32 sampler, OpenGL.TextureParameterName pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glSamplerParameterIuiv", ExactSpelling = true)]
            internal extern static void SamplerParameterIuiv(UInt32 sampler, OpenGL.TextureParameterName pname, UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glScissor", ExactSpelling = true)]
            internal extern static void Scissor(Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glScissorArrayv", ExactSpelling = true)]
            internal extern static void ScissorArrayv(UInt32 first, Int32 count, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glScissorIndexed", ExactSpelling = true)]
            internal extern static void ScissorIndexed(UInt32 index, Int32 left, Int32 bottom, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glScissorIndexedv", ExactSpelling = true)]
            internal extern static void ScissorIndexedv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glShaderBinary", ExactSpelling = true)]
            internal extern static void ShaderBinary(Int32 count, UInt32[] shaders, Int32 binaryFormat, IntPtr binary, Int32 length);
            [DllImport(GL.Library, EntryPoint = "glShaderSource", ExactSpelling = true)]
            internal extern static void ShaderSource(UInt32 shader, Int32 count, String[] @string, Int32[] length);
            [DllImport(GL.Library, EntryPoint = "glShaderStorageBlockBinding", ExactSpelling = true)]
            internal extern static void ShaderStorageBlockBinding(UInt32 program, UInt32 storageBlockIndex, UInt32 storageBlockBinding);
            [DllImport(GL.Library, EntryPoint = "glStencilFunc", ExactSpelling = true)]
            internal extern static void StencilFunc(OpenGL.StencilFunction func, Int32 @ref, UInt32 mask);
            [DllImport(GL.Library, EntryPoint = "glStencilFuncSeparate", ExactSpelling = true)]
            internal extern static void StencilFuncSeparate(OpenGL.StencilFace face, OpenGL.StencilFunction func, Int32 @ref, UInt32 mask);
            [DllImport(GL.Library, EntryPoint = "glStencilMask", ExactSpelling = true)]
            internal extern static void StencilMask(UInt32 mask);
            [DllImport(GL.Library, EntryPoint = "glStencilMaskSeparate", ExactSpelling = true)]
            internal extern static void StencilMaskSeparate(OpenGL.StencilFace face, UInt32 mask);
            [DllImport(GL.Library, EntryPoint = "glStencilOp", ExactSpelling = true)]
            internal extern static void StencilOp(OpenGL.StencilOp sfail, OpenGL.StencilOp dpfail, OpenGL.StencilOp dppass);
            [DllImport(GL.Library, EntryPoint = "glStencilOpSeparate", ExactSpelling = true)]
            internal extern static void StencilOpSeparate(OpenGL.StencilFace face, OpenGL.StencilOp sfail, OpenGL.StencilOp dpfail, OpenGL.StencilOp dppass);
            [DllImport(GL.Library, EntryPoint = "glTexBuffer", ExactSpelling = true)]
            internal extern static void TexBuffer(OpenGL.TextureBufferTarget target, OpenGL.SizedInternalFormat internalFormat, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glTextureBuffer", ExactSpelling = true)]
            internal extern static void TextureBuffer(UInt32 texture, OpenGL.SizedInternalFormat internalFormat, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glTexBufferRange", ExactSpelling = true)]
            internal extern static void TexBufferRange(OpenGL.BufferTarget target, OpenGL.SizedInternalFormat internalFormat, UInt32 buffer, IntPtr offset, IntPtr size);
            [DllImport(GL.Library, EntryPoint = "glTextureBufferRange", ExactSpelling = true)]
            internal extern static void TextureBufferRange(UInt32 texture, OpenGL.SizedInternalFormat internalFormat, UInt32 buffer, IntPtr offset, Int32 size);
            [DllImport(GL.Library, EntryPoint = "glTexImage1D", ExactSpelling = true)]
            internal extern static void TexImage1D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 border, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glTexImage2D", ExactSpelling = true)]
            internal extern static void TexImage2D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Int32 border, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glTexImage2DMultisample", ExactSpelling = true)]
            internal extern static void TexImage2DMultisample(OpenGL.TextureTargetMultisample target, Int32 samples, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTexImage3D", ExactSpelling = true)]
            internal extern static void TexImage3D(OpenGL.TextureTarget target, Int32 level, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth, Int32 border, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr data);
            [DllImport(GL.Library, EntryPoint = "glTexImage3DMultisample", ExactSpelling = true)]
            internal extern static void TexImage3DMultisample(OpenGL.TextureTargetMultisample target, Int32 samples, OpenGL.PixelInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTexParameterf", ExactSpelling = true)]
            internal extern static void TexParameterf(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, Single param);
            [DllImport(GL.Library, EntryPoint = "glTexParameteri", ExactSpelling = true)]
            internal extern static void TexParameteri(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glTextureParameterf", ExactSpelling = true)]
            internal extern static void TextureParameterf(UInt32 texture, OpenGL.TextureParameter pname, Single param);
            [DllImport(GL.Library, EntryPoint = "glTextureParameteri", ExactSpelling = true)]
            internal extern static void TextureParameteri(UInt32 texture, OpenGL.TextureParameter pname, Int32 param);
            [DllImport(GL.Library, EntryPoint = "glTexParameterfv", ExactSpelling = true)]
            internal extern static void TexParameterfv(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, Single[] @params);
            [DllImport(GL.Library, EntryPoint = "glTexParameteriv", ExactSpelling = true)]
            internal extern static void TexParameteriv(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glTexParameterIiv", ExactSpelling = true)]
            internal extern static void TexParameterIiv(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glTexParameterIuiv", ExactSpelling = true)]
            internal extern static void TexParameterIuiv(OpenGL.TextureTarget target, OpenGL.TextureParameterName pname, UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glTextureParameterfv", ExactSpelling = true)]
            internal extern static void TextureParameterfv(UInt32 texture, OpenGL.TextureParameter pname, Single[] paramtexture);
            [DllImport(GL.Library, EntryPoint = "glTextureParameteriv", ExactSpelling = true)]
            internal extern static void TextureParameteriv(UInt32 texture, OpenGL.TextureParameter pname, Int32[] param);
            [DllImport(GL.Library, EntryPoint = "glTextureParameterIiv", ExactSpelling = true)]
            internal extern static void TextureParameterIiv(UInt32 texture, OpenGL.TextureParameter pname, Int32[] @params);
            [DllImport(GL.Library, EntryPoint = "glTextureParameterIuiv", ExactSpelling = true)]
            internal extern static void TextureParameterIuiv(UInt32 texture, OpenGL.TextureParameter pname, UInt32[] @params);
            [DllImport(GL.Library, EntryPoint = "glTexStorage1D", ExactSpelling = true)]
            internal extern static void TexStorage1D(OpenGL.TextureTarget target, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width);
            [DllImport(GL.Library, EntryPoint = "glTextureStorage1D", ExactSpelling = true)]
            internal extern static void TextureStorage1D(UInt32 texture, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width);
            [DllImport(GL.Library, EntryPoint = "glTexStorage2D", ExactSpelling = true)]
            internal extern static void TexStorage2D(OpenGL.TextureTarget target, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glTextureStorage2D", ExactSpelling = true)]
            internal extern static void TextureStorage2D(UInt32 texture, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glTexStorage2DMultisample", ExactSpelling = true)]
            internal extern static void TexStorage2DMultisample(OpenGL.TextureTarget target, Int32 samples, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTextureStorage2DMultisample", ExactSpelling = true)]
            internal extern static void TextureStorage2DMultisample(UInt32 texture, Int32 samples, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTexStorage3D", ExactSpelling = true)]
            internal extern static void TexStorage3D(OpenGL.TextureTarget target, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth);
            [DllImport(GL.Library, EntryPoint = "glTextureStorage3D", ExactSpelling = true)]
            internal extern static void TextureStorage3D(UInt32 texture, Int32 levels, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth);
            [DllImport(GL.Library, EntryPoint = "glTexStorage3DMultisample", ExactSpelling = true)]
            internal extern static void TexStorage3DMultisample(OpenGL.TextureTarget target, Int32 samples, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTextureStorage3DMultisample", ExactSpelling = true)]
            internal extern static void TextureStorage3DMultisample(UInt32 texture, Int32 samples, OpenGL.SizedInternalFormat internalFormat, Int32 width, Int32 height, Int32 depth, Boolean fixedsamplelocations);
            [DllImport(GL.Library, EntryPoint = "glTexSubImage1D", ExactSpelling = true)]
            internal extern static void TexSubImage1D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTextureSubImage1D", ExactSpelling = true)]
            internal extern static void TextureSubImage1D(UInt32 texture, Int32 level, Int32 xoffset, Int32 width, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTexSubImage2D", ExactSpelling = true)]
            internal extern static void TexSubImage2D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTextureSubImage2D", ExactSpelling = true)]
            internal extern static void TextureSubImage2D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTexSubImage3D", ExactSpelling = true)]
            internal extern static void TexSubImage3D(OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTextureSubImage3D", ExactSpelling = true)]
            internal extern static void TextureSubImage3D(UInt32 texture, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenGL.PixelFormat format, OpenGL.PixelType type, IntPtr pixels);
            [DllImport(GL.Library, EntryPoint = "glTextureBarrier", ExactSpelling = true)]
            internal extern static void TextureBarrier();
            [DllImport(GL.Library, EntryPoint = "glTextureView", ExactSpelling = true)]
            internal extern static void TextureView(UInt32 texture, OpenGL.TextureTarget target, UInt32 origtexture, OpenGL.PixelInternalFormat internalFormat, UInt32 minlevel, UInt32 numlevels, UInt32 minlayer, UInt32 numlayers);
            [DllImport(GL.Library, EntryPoint = "glTransformFeedbackBufferBase", ExactSpelling = true)]
            internal extern static void TransformFeedbackBufferBase(UInt32 xfb, UInt32 index, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glTransformFeedbackBufferRange", ExactSpelling = true)]
            internal extern static void TransformFeedbackBufferRange(UInt32 xfb, UInt32 index, UInt32 buffer, IntPtr offset, Int32 size);
            [DllImport(GL.Library, EntryPoint = "glTransformFeedbackVaryings", ExactSpelling = true)]
            internal extern static void TransformFeedbackVaryings(UInt32 program, Int32 count, String[] varyings, OpenGL.TransformFeedbackMode bufferMode);
            [DllImport(GL.Library, EntryPoint = "glUniform1f", ExactSpelling = true)]
            internal extern static void Uniform1f(Int32 location, Single v0);
            [DllImport(GL.Library, EntryPoint = "glUniform2f", ExactSpelling = true)]
            internal extern static void Uniform2f(Int32 location, Single v0, Single v1);
            [DllImport(GL.Library, EntryPoint = "glUniform3f", ExactSpelling = true)]
            internal extern static void Uniform3f(Int32 location, Single v0, Single v1, Single v2);
            [DllImport(GL.Library, EntryPoint = "glUniform4f", ExactSpelling = true)]
            internal extern static void Uniform4f(Int32 location, Single v0, Single v1, Single v2, Single v3);
            [DllImport(GL.Library, EntryPoint = "glUniform1i", ExactSpelling = true)]
            internal extern static void Uniform1i(Int32 location, Int32 v0);
            [DllImport(GL.Library, EntryPoint = "glUniform2i", ExactSpelling = true)]
            internal extern static void Uniform2i(Int32 location, Int32 v0, Int32 v1);
            [DllImport(GL.Library, EntryPoint = "glUniform3i", ExactSpelling = true)]
            internal extern static void Uniform3i(Int32 location, Int32 v0, Int32 v1, Int32 v2);
            [DllImport(GL.Library, EntryPoint = "glUniform4i", ExactSpelling = true)]
            internal extern static void Uniform4i(Int32 location, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [DllImport(GL.Library, EntryPoint = "glUniform1ui", ExactSpelling = true)]
            internal extern static void Uniform1ui(Int32 location, UInt32 v0);
            [DllImport(GL.Library, EntryPoint = "glUniform2ui", ExactSpelling = true)]
            internal extern static void Uniform2ui(Int32 location, UInt32 v0, UInt32 v1);
            [DllImport(GL.Library, EntryPoint = "glUniform3ui", ExactSpelling = true)]
            internal extern static void Uniform3ui(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2);
            [DllImport(GL.Library, EntryPoint = "glUniform4ui", ExactSpelling = true)]
            internal extern static void Uniform4ui(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3);
            [DllImport(GL.Library, EntryPoint = "glUniform1fv", ExactSpelling = true)]
            internal extern static void Uniform1fv(Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform2fv", ExactSpelling = true)]
            internal extern static void Uniform2fv(Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform3fv", ExactSpelling = true)]
            internal extern static void Uniform3fv(Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform4fv", ExactSpelling = true)]
            internal extern static void Uniform4fv(Int32 location, Int32 count, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform1iv", ExactSpelling = true)]
            internal extern static void Uniform1iv(Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform2iv", ExactSpelling = true)]
            internal extern static void Uniform2iv(Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform3iv", ExactSpelling = true)]
            internal extern static void Uniform3iv(Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform4iv", ExactSpelling = true)]
            internal extern static void Uniform4iv(Int32 location, Int32 count, Int32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform1uiv", ExactSpelling = true)]
            internal extern static void Uniform1uiv(Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform2uiv", ExactSpelling = true)]
            internal extern static void Uniform2uiv(Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform3uiv", ExactSpelling = true)]
            internal extern static void Uniform3uiv(Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniform4uiv", ExactSpelling = true)]
            internal extern static void Uniform4uiv(Int32 location, Int32 count, UInt32[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix2fv", ExactSpelling = true)]
            internal extern static void UniformMatrix2fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix3fv", ExactSpelling = true)]
            internal extern static void UniformMatrix3fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix4fv", ExactSpelling = true)]
            internal extern static void UniformMatrix4fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix2x3fv", ExactSpelling = true)]
            internal extern static void UniformMatrix2x3fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix3x2fv", ExactSpelling = true)]
            internal extern static void UniformMatrix3x2fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix2x4fv", ExactSpelling = true)]
            internal extern static void UniformMatrix2x4fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix4x2fv", ExactSpelling = true)]
            internal extern static void UniformMatrix4x2fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix3x4fv", ExactSpelling = true)]
            internal extern static void UniformMatrix3x4fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformMatrix4x3fv", ExactSpelling = true)]
            internal extern static void UniformMatrix4x3fv(Int32 location, Int32 count, Boolean transpose, Single[] value);
            [DllImport(GL.Library, EntryPoint = "glUniformBlockBinding", ExactSpelling = true)]
            internal extern static void UniformBlockBinding(UInt32 program, UInt32 uniformBlockIndex, UInt32 uniformBlockBinding);
            [DllImport(GL.Library, EntryPoint = "glUniformSubroutinesuiv", ExactSpelling = true)]
            internal extern static void UniformSubroutinesuiv(OpenGL.ShaderType shadertype, Int32 count, UInt32[] indices);
            [DllImport(GL.Library, EntryPoint = "glUnmapBuffer", ExactSpelling = true)]
            internal extern static Boolean UnmapBuffer(OpenGL.BufferTarget target);
            [DllImport(GL.Library, EntryPoint = "glUnmapNamedBuffer", ExactSpelling = true)]
            internal extern static Boolean UnmapNamedBuffer(UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glUseProgram", ExactSpelling = true)]
            internal extern static void UseProgram(UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glUseProgramStages", ExactSpelling = true)]
            internal extern static void UseProgramStages(UInt32 pipeline, UInt32 stages, UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glValidateProgram", ExactSpelling = true)]
            internal extern static void ValidateProgram(UInt32 program);
            [DllImport(GL.Library, EntryPoint = "glValidateProgramPipeline", ExactSpelling = true)]
            internal extern static void ValidateProgramPipeline(UInt32 pipeline);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayElementBuffer", ExactSpelling = true)]
            internal extern static void VertexArrayElementBuffer(UInt32 vaobj, UInt32 buffer);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1f", ExactSpelling = true)]
            internal extern static void VertexAttrib1f(UInt32 index, Single v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1s", ExactSpelling = true)]
            internal extern static void VertexAttrib1s(UInt32 index, Int16 v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1d", ExactSpelling = true)]
            internal extern static void VertexAttrib1d(UInt32 index, Double v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI1i", ExactSpelling = true)]
            internal extern static void VertexAttribI1i(UInt32 index, Int32 v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI1ui", ExactSpelling = true)]
            internal extern static void VertexAttribI1ui(UInt32 index, UInt32 v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2f", ExactSpelling = true)]
            internal extern static void VertexAttrib2f(UInt32 index, Single v0, Single v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2s", ExactSpelling = true)]
            internal extern static void VertexAttrib2s(UInt32 index, Int16 v0, Int16 v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2d", ExactSpelling = true)]
            internal extern static void VertexAttrib2d(UInt32 index, Double v0, Double v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI2i", ExactSpelling = true)]
            internal extern static void VertexAttribI2i(UInt32 index, Int32 v0, Int32 v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI2ui", ExactSpelling = true)]
            internal extern static void VertexAttribI2ui(UInt32 index, UInt32 v0, UInt32 v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3f", ExactSpelling = true)]
            internal extern static void VertexAttrib3f(UInt32 index, Single v0, Single v1, Single v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3s", ExactSpelling = true)]
            internal extern static void VertexAttrib3s(UInt32 index, Int16 v0, Int16 v1, Int16 v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3d", ExactSpelling = true)]
            internal extern static void VertexAttrib3d(UInt32 index, Double v0, Double v1, Double v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI3i", ExactSpelling = true)]
            internal extern static void VertexAttribI3i(UInt32 index, Int32 v0, Int32 v1, Int32 v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI3ui", ExactSpelling = true)]
            internal extern static void VertexAttribI3ui(UInt32 index, UInt32 v0, UInt32 v1, UInt32 v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4f", ExactSpelling = true)]
            internal extern static void VertexAttrib4f(UInt32 index, Single v0, Single v1, Single v2, Single v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4s", ExactSpelling = true)]
            internal extern static void VertexAttrib4s(UInt32 index, Int16 v0, Int16 v1, Int16 v2, Int16 v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4d", ExactSpelling = true)]
            internal extern static void VertexAttrib4d(UInt32 index, Double v0, Double v1, Double v2, Double v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nub", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nub(UInt32 index, Byte v0, Byte v1, Byte v2, Byte v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4i", ExactSpelling = true)]
            internal extern static void VertexAttribI4i(UInt32 index, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4ui", ExactSpelling = true)]
            internal extern static void VertexAttribI4ui(UInt32 index, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL1d", ExactSpelling = true)]
            internal extern static void VertexAttribL1d(UInt32 index, Double v0);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL2d", ExactSpelling = true)]
            internal extern static void VertexAttribL2d(UInt32 index, Double v0, Double v1);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL3d", ExactSpelling = true)]
            internal extern static void VertexAttribL3d(UInt32 index, Double v0, Double v1, Double v2);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL4d", ExactSpelling = true)]
            internal extern static void VertexAttribL4d(UInt32 index, Double v0, Double v1, Double v2, Double v3);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1fv", ExactSpelling = true)]
            internal extern static void VertexAttrib1fv(UInt32 index, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1sv", ExactSpelling = true)]
            internal extern static void VertexAttrib1sv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib1dv", ExactSpelling = true)]
            internal extern static void VertexAttrib1dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI1iv", ExactSpelling = true)]
            internal extern static void VertexAttribI1iv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI1uiv", ExactSpelling = true)]
            internal extern static void VertexAttribI1uiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2fv", ExactSpelling = true)]
            internal extern static void VertexAttrib2fv(UInt32 index, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2sv", ExactSpelling = true)]
            internal extern static void VertexAttrib2sv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib2dv", ExactSpelling = true)]
            internal extern static void VertexAttrib2dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI2iv", ExactSpelling = true)]
            internal extern static void VertexAttribI2iv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI2uiv", ExactSpelling = true)]
            internal extern static void VertexAttribI2uiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3fv", ExactSpelling = true)]
            internal extern static void VertexAttrib3fv(UInt32 index, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3sv", ExactSpelling = true)]
            internal extern static void VertexAttrib3sv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib3dv", ExactSpelling = true)]
            internal extern static void VertexAttrib3dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI3iv", ExactSpelling = true)]
            internal extern static void VertexAttribI3iv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI3uiv", ExactSpelling = true)]
            internal extern static void VertexAttribI3uiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4fv", ExactSpelling = true)]
            internal extern static void VertexAttrib4fv(UInt32 index, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4sv", ExactSpelling = true)]
            internal extern static void VertexAttrib4sv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4dv", ExactSpelling = true)]
            internal extern static void VertexAttrib4dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4iv", ExactSpelling = true)]
            internal extern static void VertexAttrib4iv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4bv", ExactSpelling = true)]
            internal extern static void VertexAttrib4bv(UInt32 index, SByte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4ubv", ExactSpelling = true)]
            internal extern static void VertexAttrib4ubv(UInt32 index, Byte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4usv", ExactSpelling = true)]
            internal extern static void VertexAttrib4usv(UInt32 index, UInt16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4uiv", ExactSpelling = true)]
            internal extern static void VertexAttrib4uiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nbv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nbv(UInt32 index, SByte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nsv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nsv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Niv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Niv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nubv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nubv(UInt32 index, Byte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nusv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nusv(UInt32 index, UInt16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttrib4Nuiv", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nuiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4bv", ExactSpelling = true)]
            internal extern static void VertexAttribI4bv(UInt32 index, SByte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4ubv", ExactSpelling = true)]
            internal extern static void VertexAttribI4ubv(UInt32 index, Byte[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4sv", ExactSpelling = true)]
            internal extern static void VertexAttribI4sv(UInt32 index, Int16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4usv", ExactSpelling = true)]
            internal extern static void VertexAttribI4usv(UInt32 index, UInt16[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4iv", ExactSpelling = true)]
            internal extern static void VertexAttribI4iv(UInt32 index, Int32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribI4uiv", ExactSpelling = true)]
            internal extern static void VertexAttribI4uiv(UInt32 index, UInt32[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL1dv", ExactSpelling = true)]
            internal extern static void VertexAttribL1dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL2dv", ExactSpelling = true)]
            internal extern static void VertexAttribL2dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL3dv", ExactSpelling = true)]
            internal extern static void VertexAttribL3dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribL4dv", ExactSpelling = true)]
            internal extern static void VertexAttribL4dv(UInt32 index, Double[] v);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribP1ui", ExactSpelling = true)]
            internal extern static void VertexAttribP1ui(UInt32 index, OpenGL.VertexAttribPType type, Boolean normalized, UInt32 value);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribP2ui", ExactSpelling = true)]
            internal extern static void VertexAttribP2ui(UInt32 index, OpenGL.VertexAttribPType type, Boolean normalized, UInt32 value);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribP3ui", ExactSpelling = true)]
            internal extern static void VertexAttribP3ui(UInt32 index, OpenGL.VertexAttribPType type, Boolean normalized, UInt32 value);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribP4ui", ExactSpelling = true)]
            internal extern static void VertexAttribP4ui(UInt32 index, OpenGL.VertexAttribPType type, Boolean normalized, UInt32 value);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribBinding", ExactSpelling = true)]
            internal extern static void VertexAttribBinding(UInt32 attribindex, UInt32 bindingindex);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayAttribBinding", ExactSpelling = true)]
            internal extern static void VertexArrayAttribBinding(UInt32 vaobj, UInt32 attribindex, UInt32 bindingindex);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribDivisor", ExactSpelling = true)]
            internal extern static void VertexAttribDivisor(UInt32 index, UInt32 divisor);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribFormat", ExactSpelling = true)]
            internal extern static void VertexAttribFormat(UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, Boolean normalized, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribIFormat", ExactSpelling = true)]
            internal extern static void VertexAttribIFormat(UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribLFormat", ExactSpelling = true)]
            internal extern static void VertexAttribLFormat(UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayAttribFormat", ExactSpelling = true)]
            internal extern static void VertexArrayAttribFormat(UInt32 vaobj, UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, Boolean normalized, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayAttribIFormat", ExactSpelling = true)]
            internal extern static void VertexArrayAttribIFormat(UInt32 vaobj, UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayAttribLFormat", ExactSpelling = true)]
            internal extern static void VertexArrayAttribLFormat(UInt32 vaobj, UInt32 attribindex, Int32 size, OpenGL.VertexAttribFormat type, UInt32 relativeoffset);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribPointer", ExactSpelling = true)]
            internal extern static void VertexAttribPointer(UInt32 index, Int32 size, OpenGL.VertexAttribPointerType type, Boolean normalized, Int32 stride, IntPtr pointer);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribIPointer", ExactSpelling = true)]
            internal extern static void VertexAttribIPointer(UInt32 index, Int32 size, OpenGL.VertexAttribPointerType type, Int32 stride, IntPtr pointer);
            [DllImport(GL.Library, EntryPoint = "glVertexAttribLPointer", ExactSpelling = true)]
            internal extern static void VertexAttribLPointer(UInt32 index, Int32 size, OpenGL.VertexAttribPointerType type, Int32 stride, IntPtr pointer);
            [DllImport(GL.Library, EntryPoint = "glVertexBindingDivisor", ExactSpelling = true)]
            internal extern static void VertexBindingDivisor(UInt32 bindingindex, UInt32 divisor);
            [DllImport(GL.Library, EntryPoint = "glVertexArrayBindingDivisor", ExactSpelling = true)]
            internal extern static void VertexArrayBindingDivisor(UInt32 vaobj, UInt32 bindingindex, UInt32 divisor);
            [DllImport(GL.Library, EntryPoint = "glViewport", ExactSpelling = true)]
            internal extern static void Viewport(Int32 x, Int32 y, Int32 width, Int32 height);
            [DllImport(GL.Library, EntryPoint = "glViewportArrayv", ExactSpelling = true)]
            internal extern static void ViewportArrayv(UInt32 first, Int32 count, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glViewportIndexedf", ExactSpelling = true)]
            internal extern static void ViewportIndexedf(UInt32 index, Single x, Single y, Single w, Single h);
            [DllImport(GL.Library, EntryPoint = "glViewportIndexedfv", ExactSpelling = true)]
            internal extern static void ViewportIndexedfv(UInt32 index, Single[] v);
            [DllImport(GL.Library, EntryPoint = "glWaitSync", ExactSpelling = true)]
            internal extern static void WaitSync(IntPtr sync, UInt32 flags, UInt64 timeout);
        }
    }
}
