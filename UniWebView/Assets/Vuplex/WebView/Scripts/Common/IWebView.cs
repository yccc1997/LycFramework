/**
* Copyright (c) 2021 Vuplex Inc. All rights reserved.
*
* Licensed under the Vuplex Commercial Software Library License, you may
* not use this file except in compliance with the License. You may obtain
* a copy of the License at
*
*     https://vuplex.com/commercial-library-license
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using UnityEngine;

#if NET_4_6 || NET_STANDARD_2_0
    using System.Threading.Tasks;
#endif

namespace Vuplex.WebView {

    /// <summary>
    /// IWebView is the primary interface for loading and interacting with web content.
    /// It contains methods and properties for common browser-related functionality,
    /// like LoadUrl(), GoBack(), Reload(), and ExecuteJavaScript().
    /// </summary>
    /// <remarks>
    /// <para>
    /// To create an IWebView, instantiate a WebViewPrefab or CanvasWebViewPrefab. After
    /// the prefab is initialized, you can access the IWebView via the WebViewPrefab.WebView property.
    /// If your use case requires a high degree of customization, you can instead create
    /// an IWebView outside of a prefab (to connect to your own custom GameObject) by
    /// using Web.CreateWebView().
    /// </para>
    /// <para>
    /// For additional functionality, you can cast an IWebView to an interface for a specific
    /// feature, like IWithDownloads or IWithPopups. For a list of additional feature interfaces and
    /// information about how to use them, see this page: https://developer.vuplex.com/webview/additional-interfaces
    /// </para>
    /// See also:
    /// <list type="bullet">
    ///   <item>WebViewPrefab: https://developer.vuplex.com/webview/WebViewPrefab</item>
    ///   <item>CanvasWebViewPrefab: https://developer.vuplex.com/webview/CanvasWebViewPrefab</item>
    ///   <item>Web (static methods): https://developer.vuplex.com/webview/Web</item>
    /// </list>
    /// </remarks>
    public interface IWebView {

        /// <summary>
        /// Indicates that the page has requested to close (i.e. via `window.close()`).
        /// </summary>
        event EventHandler CloseRequested;

        /// <summary>
        /// Indicates that a message was logged to the JavaScript console.
        /// </summary>
        /// <remarks>
        /// The 3D WebView packages for Android with Gecko, iOS, and UWP have the following limitations:
        /// - Messages from iframes aren't captured
        /// - Messages logged early when the page starts loading may be missed
        /// </remarks>
        event EventHandler<ConsoleMessageEventArgs> ConsoleMessageLogged;

        /// <summary>
        /// Indicates when an input field has been focused or unfocused. This can be used,
        /// for example, to determine when to show or hide an on-screen keyboard.
        /// Note that this event is currently only fired for input fields focused in the main frame
        /// and is not fired for input fields in iframes.
        /// </summary>
        event EventHandler<FocusedInputFieldChangedEventArgs> FocusedInputFieldChanged;

        /// <summary>
        /// Indicates that the page load progress changed.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this event isn't raised for intermediate load progress (`ProgressChangeType.Updated`)
        /// because Chromium doesn't provide an API for estimated load progress.
        /// </remarks>
        event EventHandler<ProgressChangedEventArgs> LoadProgressChanged;

        /// <summary>
        /// Indicates that JavaScript running in the page used the `window.vuplex.postMessage`
        /// JavaScript API to emit a message to the Unity application. For more details, please see
        /// [this support article](https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp).
        /// </summary>
        /// <example>
        /// // JavaScript example
        /// function sendMessageToCSharp() {
        ///   // This object passed to `postMessage()` is automatically serialized as JSON
        ///   // and is emitted via the C# MessageEmitted event. This API mimics the window.postMessage API.
        ///   window.vuplex.postMessage({ type: 'greeting', message: 'Hello from JavaScript!' });
        /// }
        ///
        /// if (window.vuplex) {
        ///   // The window.vuplex object has already been initialized after page load,
        ///   // so we can go ahead and send the message.
        ///   sendMessageToCSharp();
        /// } else {
        ///   // The window.vuplex object hasn't been initialized yet because the page is still
        ///   // loading, so add an event listener to send the message once it's initialized.
        ///   window.addEventListener('vuplexready', sendMessageToCSharp);
        /// }
        /// </example>
        /// <seealso cref="IWebView.ExecuteJavaScript"/>
        /// <seealso cref="IWebView.PageLoadScripts"/>
        event EventHandler<EventArgs<string>> MessageEmitted;

        /// <summary>
        /// Indicates that the page failed to load. This can happen, for instance,
        /// if DNS is unable to resolve the hostname.
        /// </summary>
        event EventHandler PageLoadFailed;

        /// <summary>
        /// Indicates that the page's title changed.
        /// </summary>
        event EventHandler<EventArgs<string>> TitleChanged;

        /// <summary>
        /// Indicates that the URL of the webview changed, either
        /// due to user interaction or JavaScript.
        /// </summary>
        /// <seealso cref="IWebView.Url"/>
        event EventHandler<UrlChangedEventArgs> UrlChanged;

        /// <summary>
        /// Indicates that the rect of the playing video changed.
        /// </summary>
        /// <remarks>
        /// Note that `WebViewPrefab` automatically handles this event for you.
        /// </remarks>
        event EventHandler<EventArgs<Rect>> VideoRectChanged;

        /// <summary>
        /// Indicates whether the instance has been disposed via `Dispose()`.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Indicates whether the instance has been initialized via `Init()`.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// A list of JavaScript scripts that are automatically executed in every new page that is loaded.
        /// </summary>
        /// <remarks>
        /// This list is empty by default, but the application can add scripts. When used in conjunction
        /// with 3D WebView's [message passing API](https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp),
        /// it's possible to modify the browser's behavior in significant ways, similar to creating browser extensions.
        /// </remarks>
        /// <example>
        /// // Add a script that automatically hides all scrollbars.
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.PageLoadScripts.Add(@"
        ///     var styleElement = document.createElement('style');
        ///     styleElement.innerText = 'body::-webkit-scrollbar { display: none; }';
        ///     document.head.appendChild(styleElement);
        /// ");
        /// </example>
        /// <seealso cref="IWebView.ExecuteJavaScript"/>
        /// <seealso href="https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp">JS-to-C# message passing</seealso>
        List<string> PageLoadScripts { get; }

        /// <summary>
        /// Indicates the instance's plugin type.
        /// </summary>
        WebPluginType PluginType { get; }

        /// <summary>
        /// The webview's resolution in pixels per Unity unit.
        /// </summary>
        /// <seealso cref="IWebView.SetResolution"/>
        /// <seealso cref="IWebView.Size"/>
        float Resolution { get; }

        /// <summary>
        /// The webview's current size in Unity units.
        /// </summary>
        /// <seealso cref="IWebView.SizeInPixels"/>
        /// <seealso cref="IWebView.Resize"/>
        /// <seealso cref="IWebView.Resolution"/>
        Vector2 Size { get; }

        /// <summary>
        /// The webview's current size in pixels.
        /// </summary>
        /// <seealso cref="IWebView.Size"/>
        /// <seealso cref="IWebView.Resize"/>
        /// <seealso cref="IWebView.Resolution"/>
        Vector2 SizeInPixels { get; }

        /// <summary>
        /// The texture for the webview's web content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This texture is an "external texture" created with
        /// `Texture2D.CreateExternalTexture()`. An undocumented characteristic
        /// of external textures in Unity is that not all `Texture2D` methods work for them.
        /// For example, `Texture2D.GetRawTextureData()` and `ImageConversion.EncodeToPNG()`
        /// fail for external textures. To compensate, the `IWebView` interface includes
        /// its own`GetRawTextureData()` and `CaptureScreenshot()` methods to replace them.
        /// </para>
        /// <para>
        /// Another quirk of this texture is that Unity always reports its size as
        /// 1300px × 1300px in the editor. In reality, 3D WebView resizes the
        /// texture in native code to match the dimensions of the webview, but
        /// Unity doesn't provide an API to notify the engine that an external texture's size
        /// has changed. So, Unity always reports its size as the initial size that was
        /// passed to `Texture2D.CreateExternalTexture()`, which in 3D WebView's case is
        /// 1300px × 1300px.
        /// </para>
        /// <para>
        /// This method is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </para>
        /// </remarks>
        Texture2D Texture { get; }

        /// <summary>
        /// The current web page title.
        /// </summary>
        /// <seealso cref="IWebView.TitleChanged"/>
        string Title { get; }

        /// <summary>
        /// The current URL.
        /// </summary>
        /// <seealso cref="IWebView.UrlChanged"/>
        string Url { get; }

        /// <summary>
        /// The video texture used by the [fallback video implementation](https://support.vuplex.com/articles/fallback-video)
        /// for iOS and Android.
        /// </summary>
        Texture2D VideoTexture { get; }

        /// <summary>
        /// Initializes a newly created webview with the given textures created with
        /// `Web.CreateMaterial()` and the dimensions in Unity units.
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>If you're using `WebViewPrefab`, you don't need to call this method, because it calls it for you.</item>
        ///   <item>A separate video texture is only used on Android and iOS.</item>
        ///   <item>A webview's default resolution is 1300px per Unity unit but can be changed with `IWebView.SetResolution()`.</item>
        /// </list>
        /// </remarks>
        void Init(Texture2D viewportTexture, float width, float height, Texture2D videoTexture);

        /// <summary>
        /// Like the other `Init()` method, but with video support disabled on Android and iOS.
        /// </summary>
        void Init(Texture2D viewportTexture, float width, float height);

        [Obsolete("Blur() is now deprecated. Please use SetFocused(false) instead.")]
        void Blur();

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Checks whether the webview can go back with a call to `GoBack()`.
        /// </summary>
        /// <seealso cref="IWebView.GoBack"/>
        Task<bool> CanGoBack();

        /// <summary>
        /// Checks whether the webview can go forward with a call to `GoForward()`.
        /// </summary>
        /// <seealso cref="IWebView.GoForward"/>
        Task<bool> CanGoForward();
    #endif

        /// <summary>
        /// Like the other version of `CanGoBack()`, except it uses a callback
        /// instead of a `Task` in order to be compatible with legacy .NET.
        /// </summary>
        void CanGoBack(Action<bool> callback);

        /// <summary>
        /// Like the other version of `CanGoForward()`, except it uses a callback
        /// instead of a `Task` in order to be compatible with legacy .NET.
        /// </summary>
        void CanGoForward(Action<bool> callback);

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Returns a PNG image of the content visible in the webview.
        /// </summary>
        /// <remarks>
        /// Note that on iOS, screenshots do not include video content, which appears black.
        /// </remarks>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/ImageConversion.LoadImage.html">ImageConversion.LoadImage()</seealso>
        /// <seealso cref="IWebView.GetRawTextureData"/>
        Task<byte[]> CaptureScreenshot();
    #endif

        /// <summary>
        /// Like the other version of `CaptureScreenshot()`, except it uses a callback
        /// instead of a `Task` in order to be compatible with legacy .NET.
        /// </summary>
        void CaptureScreenshot(Action<byte[]> callback);

        /// <summary>
        /// Clicks at the given point in the webpage, dispatching both a mouse
        /// down and a mouse up event.
        /// </summary>
        /// <param name="point">
        /// The x and y components of the point are values
        /// between 0 and 1 that are normalized to the width and height, respectively. For example,
        /// `point.x = x in Unity units / width in Unity units`.
        /// Like in the browser, the origin is in the upper-left corner,
        /// the positive direction of the y-axis is down, and the positive
        /// direction of the x-axis is right.
        /// </param>
        void Click(Vector2 point);

        /// <summary>
        /// Like `Click()` but with an additional option to prevent stealing focus.
        /// </summary>
        void Click(Vector2 point, bool preventStealingFocus);

        /// <summary>
        /// Copies the selected text to the clipboard.
        /// </summary>
        /// <seealso cref="IWebView.Cut"/>
        /// <seealso cref="IWebView.Paste"/>
        /// <seealso cref="IWebView.SelectAll"/>
        void Copy();

        /// <summary>
        /// Copies the selected text to the clipboard and removes it.
        /// </summary>
        /// <seealso cref="IWebView.Copy"/>
        /// <seealso cref="IWebView.Paste"/>
        /// <seealso cref="IWebView.SelectAll"/>
        void Cut();

        [Obsolete("DisableViewUpdates() is now deprecated. Please use SetRenderingEnabled(false) instead.")]
        void DisableViewUpdates();

        /// <summary>
        /// Destroys the webview, releasing all of its resources.
        /// </summary>
        /// <remarks>
        /// Note that if you're using `WebViewPrefab`, you should call
        /// `WebViewPrefab.Destroy()` instead.
        /// </remarks>
        void Dispose();

        [Obsolete("EnableViewUpdates() is now deprecated. Please use SetRenderingEnabled(true) instead.")]
        void EnableViewUpdates();

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Executes the given JavaScript in the context of the page and returns the result.
        /// </summary>
        /// <remarks>
        /// In order to run JavaScript, a web page must first be loaded. You can use the
        /// <see cref="IWebView.LoadProgressChanged">LoadProgressChanged event</see> to run JavaScript
        /// after a page loads, like this:
        /// </remarks>
        /// <example>
        /// await webViewPrefab.WaitUntilInitialized();
        /// webViewPrefab.WebView.LoadProgressChanged += async (sender, eventArgs) => {
        ///     if (eventArgs.Type == ProgressChangeType.Finished) {
        ///         var headerText = await webViewPrefab.WebView.ExecuteJavaScript("document.getElementsByTagName('h1')[0].innerText");
        ///         Debug.Log("H1 text: " + headerText);
        ///     }
        /// };
        /// </example>
        /// <seealso cref="IWebView.PageLoadScripts"/>
        /// <seealso href="https://support.vuplex.com/articles/how-to-send-messages-from-javascript-to-c-sharp">JS-to-C# message passing</seealso>
        Task<string> ExecuteJavaScript(string javaScript);
    #else
        /// <summary>
        /// Executes the given script in the context of the webpage's main frame.
        /// </summary>
        void ExecuteJavaScript(string javaScript);
    #endif

        /// <summary>
        /// Like the other version of `ExecuteJavaScript()`, except it uses a callback instead
        /// of a `Task` in order to be compatible with legacy .NET.
        /// </summary>
        void ExecuteJavaScript(string javaScript, Action<string> callback);

        [Obsolete("Focus() is now deprecated. Please use SetFocused(true) instead.")]
        void Focus();

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// A replacement for [`Texture2D.GetRawTextureData()`](https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html)
        /// for IWebView.Texture.
        /// </summary>
        /// <remarks>
        /// Unity's `Texture2D.GetRawTextureData()` method currently does not work for textures created with
        /// `Texture2D.CreateExternalTexture()`. So, this method serves as a replacement by providing
        /// the equivalent functionality. You can load the bytes returned by this method into another
        /// texture using [`Texture2D.LoadRawTextureData()`](https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html).
        /// Note that on iOS, the texture data excludes video content, which appears black.
        /// </remarks>
        /// <example>
        /// var textureData = await webView.GetRawTextureData();
        /// var texture = new Texture2D(
        ///     (int)webView.SizeInPixels.x,
        ///     (int)webView.SizeInPixels.y,
        ///     TextureFormat.RGBA32,
        ///     false,
        ///     false
        /// );
        /// texture.LoadRawTextureData(textureData);
        /// texture.Apply();
        /// </example>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html">Texture2D.GetRawTextureData()</seealso>
        /// <seealso cref="IWebView.CaptureScreenshot"/>
        Task<byte[]> GetRawTextureData();
    #endif

        /// <summary>
        /// Like the other version of `GetRawTextureData()`, except it uses a callback
        /// instead of a `Task` in order to be compatible with legacy .NET.
        /// </summary>
        void GetRawTextureData(Action<byte[]> callback);

        /// <summary>
        /// Navigates back to the previous page in the webview's history.
        /// </summary>
        /// <seealso cref="IWebView.CanGoBack"/>
        void GoBack();

        /// <summary>
        /// Navigates forward to the next page in the webview's history.
        /// </summary>
        /// <seealso cref="IWebView.CanGoForward"/>
        void GoForward();

        /// <summary>
        /// Dispatches keyboard input to the webview.
        /// </summary>
        /// <param name="key">
        /// A key can either be a single character representing
        /// a unicode character (e.g. "A", "b", "?") or a [JavaScript key value](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values)
        /// (e.g. "ArrowUp", "Enter", "Backspace", "Delete").
        /// </param>
        /// <seealso cref="IWithKeyDownAndUp"/>
        void HandleKeyboardInput(string key);

        /// <summary>
        /// Loads the webpage contained in the given HTML string.
        /// </summary>
        /// <![CDATA[
        /// Example:
        /// ```
        /// webView.LoadHtml(@"
        ///     <!DOCTYPE html>
        ///     <html>
        ///         <head>
        ///             <title>Test Page</title>
        ///             <style>
        ///                 h1 {
        ///                     font-family: Helvetica, Arial, Sans-Serif;
        ///                 }
        ///             </style>
        ///         </head>
        ///         <body>
        ///             <h1>LoadHtml Example</h1>
        ///             <script>
        ///                 console.log('This page was loaded!');
        ///             </script>
        ///         </body>
        ///     </html>"
        /// );
        /// ```
        /// ]]>
        /// <seealso cref="IWebView.LoadUrl"/>
        void LoadHtml(string html);

        /// <summary>
        /// Loads the given URL. Supported URL schemes:
        /// - `http://`, `https://` - loads a remote page over HTTP
        /// - `streaming-assets://` - loads a local page from StreamingAssets
        ///     (equivalent to `"file://" + Application.streamingAssetsPath + path`)
        /// - `file://` - some platforms support loading arbitrary file URLs
        /// </summary>
        /// <seealso href="https://support.vuplex.com/articles/how-to-load-local-files">How to load local files</seealso>
        /// <seealso cref="IWebView.LoadHtml"/>
        /// <seealso cref="WebViewPrefab.InitialUrl"/>
        void LoadUrl(string url);

        /// <summary>
        /// Like `LoadUrl(string url)`, but also sends the given additional HTTP request headers
        /// when loading the URL. The headers are sent for the initial page load request but are not sent
        /// for requests for subsequent resources, like linked JavaScript or CSS files.
        /// </summary>
        /// <remarks>
        /// On Windows and macOS, this method cannot be used to set the Accept-Language header.
        /// For more info, please see [this article](https://support.vuplex.com/articles/how-to-change-accept-language-header).
        /// </remarks>
        void LoadUrl(string url, Dictionary<string, string> additionalHttpHeaders);

        /// <summary>
        /// Pastes text from the clipboard.
        /// </summary>
        /// <seealso cref="IWebView.Copy"/>
        /// <seealso cref="IWebView.Cut"/>
        void Paste();

        /// <summary>
        /// Posts a message that JavaScript within the webview can listen for
        /// using `window.vuplex.addEventListener('message', function(message) {})`.
        /// </summary>
        /// <param name="data">
        /// String that is passed as the data property of the message object.
        /// </param>
        void PostMessage(string data);

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        void Reload();

        /// <summary>
        /// Resizes the webview to the dimensions given in Unity units.
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>If you're using `WebViewPrefab`, you should call `WebViewPrefab.Resize()` instead.</item>
        ///   <item>A webview's default resolution is 1300px per Unity unit but can be changed with `IWebView.SetResolution()`.</item>
        /// </list>
        /// </remarks>
        /// <seealso cref="IWebView.Size"/>
        /// <seealso cref="IWebView.SetResolution"/>
        void Resize(float width, float height);

        /// <summary>
        /// Scrolls the top-level document by the given scroll delta.
        /// This method works by calling window.scrollBy(), which doesn't
        /// work for all web pages. An alternative is to instead use `Scroll(scrollDelta, position)`
        /// to scroll at a specific location in the page. For example, you can scroll
        /// at the middle of the page by calling `Scroll(scrollDelta, new Vector2(0.5f, 0.5f))`.
        /// </summary>
        /// <param name="scrollDelta">
        /// The scroll delta in Unity units. Because the browser's origin
        /// is in the upper-left corner, the y-axis' positive direction
        /// is down and the x-axis' positive direction is right.
        /// </param>
        void Scroll(Vector2 scrollDelta);

        /// <summary>
        /// Scrolls by the given delta at the given pointer position.
        /// </summary>
        /// <param name="scrollDelta">
        /// The scroll delta in Unity units. Because the browser's origin
        /// is in the upper-left corner, the y-axis' positive direction
        /// is down and the x-axis' positive direction is right.
        /// </param>
        /// <param name="point">
        /// The pointer position at the time of the scroll. The x and y components of are values
        /// between 0 and 1 that are normalized to the width and height, respectively. For example,
        /// `point.x = x in Unity units / width in Unity units`.
        /// </param>
        void Scroll(Vector2 scrollDelta, Vector2 point);

        /// <summary>
        /// Selects all text, depending on the page's focused element.
        /// </summary>
        /// <seealso cref="Copy"/>
        void SelectAll();

        /// <summary>
        /// Makes the webview take or relinquish focus.
        /// </summary>
        void SetFocused(bool focused);

        /// <summary>
        /// Enables or disables the webview's ability to render to its texture.
        /// By default, a webview renders web content to its texture, but you can
        /// use this method to disable or re-enable rendering.
        /// </summary>
        /// <remarks>
        /// This method is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </remarks>
        void SetRenderingEnabled(bool enabled);

        /// <summary>
        /// Sets the webview's resolution in pixels per Unity unit.
        /// You can change the resolution to make web content appear larger or smaller.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unlike the other IWebView methods, this method can be called before the webview
        /// is initialized with `Init()`. The default resolution is 1300 pixels per Unity unit.
        /// Setting a lower resolution decreases the pixel density, but has the effect
        /// of making web content appear larger. Setting a higher resolution increases
        /// the pixel density, but has the effect of making content appear smaller.
        /// For more information on scaling web content, see
        /// [this support article](https://support.vuplex.com/articles/how-to-scale-web-content).
        /// </para>
        /// <para>
        /// This method is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </para>
        /// </remarks>
        /// <seealso cref="IWebView.Resolution"/>
        /// <seealso cref="IWebView.Resize"/>
        void SetResolution(float pixelsPerUnityUnit);

        /// <summary>
        /// Zooms into the currently loaded web content.
        /// </summary>
        /// <remarks>
        /// Note that the zoom level gets reset when a new page is loaded.
        /// </remarks>
        void ZoomIn();

        /// <summary>
        /// Zooms back out after a previous call to `ZoomIn()`.
        /// </summary>
        /// <remarks>
        /// Note that the zoom level gets reset when a new page is loaded.
        /// </remarks>
        void ZoomOut();
    }
}
