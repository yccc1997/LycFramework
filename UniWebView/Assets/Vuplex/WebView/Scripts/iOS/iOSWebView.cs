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
#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

#if NET_4_6 || NET_STANDARD_2_0
    using System.Threading.Tasks;
#endif

namespace Vuplex.WebView {

    /// <summary>
    /// The IWebView implementation used by 3D WebView for iOS.
    /// </summary>
    public class iOSWebView : BaseWebView,
                              IWebView,
                              IWithMovablePointer,
                              IWithNative2DMode,
                              IWithNativeOnScreenKeyboard,
                              IWithPointerDownAndUp,
                              IWithSettableUserAgent {

        /// <see cref="IWithNative2DMode"/>
        public bool Native2DModeEnabled { get; private set; }

        public WebPluginType PluginType {
            get {
                return WebPluginType.iOS;
            }
        }

        /// <see cref="IWithNative2DMode"/>
        public Rect Rect { get; private set; }

        /// <see cref="IWithNative2DMode"/>
        public bool Visible { get; private set; }

        public static void ClearAllData() {

            WebView_clearAllData();
        }

        public override void Click(Vector2 point, bool preventStealingFocus) {

            _assertValidState();
            if (preventStealingFocus) {
                int nativeX = (int) (point.x * _nativeWidth);
                int nativeY = (int) (point.y * _nativeHeight);
                WebView_clickWithoutStealingFocus(_nativeWebViewPtr, nativeX, nativeY);
            } else {
                Click(point);
            }
        }

        /// <summary>
        /// Overrides `BaseWebView.CaptureScreenshot()` because it uses too much
        /// memory on iOS.
        /// </summary>
        public override void CaptureScreenshot(Action<byte[]> callback) {

            _assertValidState();
            IntPtr unmanagedBytes = IntPtr.Zero;
            int unmanagedBytesLength = 0;
            WebView_captureScreenshot(_nativeWebViewPtr, ref unmanagedBytes, ref unmanagedBytesLength);

            // Load the results into a managed array.
            var managedBytes = new byte[unmanagedBytesLength];
            Marshal.Copy(unmanagedBytes, managedBytes, 0, unmanagedBytesLength);
            WebView_freeMemory(unmanagedBytes);
            callback(managedBytes);
        }

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Gets the cookie that matches the given URL and cookie name, or
        /// null if no cookie matches.
        /// </summary>
        public static Task<Cookie> GetCookie(string url, string cookieName) {

            var task = new TaskCompletionSource<Cookie>();
            GetCookie(url, cookieName, task.SetResult);
            return task.Task;
        }
    #endif

        /// <summary>
        /// Like the other version of GetCookie(), except it uses a callback
        /// instead of a Task in order to be compatible with legacy .NET.
        /// </summary>
        public static void GetCookie(string url, string cookieName, Action<Cookie> callback) {

            var resultCallbackId = Guid.NewGuid().ToString();
            _pendingGetCookieResultCallbacks[resultCallbackId] = callback;
            WebView_getCookie(url, cookieName, resultCallbackId);
        }

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Gets all of the cookies that match the given URL.
        /// </summary>
        public static Task<Cookie[]> GetCookies(string url) {

            var task = new TaskCompletionSource<Cookie[]>();
            var resultCallbackId = Guid.NewGuid().ToString();
            _pendingGetCookiesResultCallbacks[resultCallbackId] = task.SetResult;
            WebView_getCookies(url, resultCallbackId);
            return task.Task;
        }
    #endif

        [Obsolete("iOSWebView.GetFileUrlForBundleResource is now deprecated. You can now use LoadUrl(\"streaming-assets://{path}\") to load a file from StreamingAssets instead.")]
        public static string GetFileUrlForBundleResource(string fileName) {

            var fileNameSegments = fileName.Split(new char[] {'.'});
            if (fileNameSegments.Length < 2) {
                throw new ArgumentException(String.Format("The file name must include an extension, but the name provided ({0}) does not.", fileName));
            }

            var fileExtension = fileNameSegments[fileNameSegments.Length - 1];
            var fileNameWithoutExtension = String.Join(".", fileNameSegments.ToList().GetRange(0, fileNameSegments.Length - 1).ToArray());

            var stringPtr = WebView_getFileUrlForBundleResource(fileNameWithoutExtension, fileExtension);
            var fileUrl = Marshal.PtrToStringAnsi(stringPtr);
            return fileUrl;
        }

        /// <summary>
        /// Overrides `BaseWebView.GetRawTextureData()` because it uses too much
        /// memory on iOS.
        /// </summary>
        public override void GetRawTextureData(Action<byte[]> callback) {

            _assertValidState();
            IntPtr unmanagedBytes = IntPtr.Zero;
            int unmanagedBytesLength = 0;
            WebView_getRawTextureData(_nativeWebViewPtr, ref unmanagedBytes, ref unmanagedBytesLength);

            // Load the results into a managed array.
            var managedBytes = new byte[unmanagedBytesLength];
            Marshal.Copy(unmanagedBytes, managedBytes, 0, unmanagedBytesLength);
            WebView_freeMemory(unmanagedBytes);
            callback(managedBytes);
        }

        public static void GloballySetUserAgent(bool mobile) {

            WebView_globallySetUserAgentToMobile(mobile);
        }

        public static void GloballySetUserAgent(string userAgent) {

            WebView_globallySetUserAgent(userAgent);
        }

        public override void Init(Texture2D viewportTexture, float width, float height, Texture2D videoTexture) {

            base.Init(viewportTexture, width, height, videoTexture);
            _nativeWebViewPtr = WebView_new(
                gameObject.name,
                _nativeWidth,
                _nativeHeight,
                videoTexture != null,
                SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal
            );
        }

        /// <see cref="IWithNative2DMode"/>
        public void InitInNative2DMode(Rect rect) {

            _numberOfPixelsPerUnityUnit = 1;
            Native2DModeEnabled = true;
            this.Rect = rect;
            Visible = true;
            base.Init(null, rect.width, rect.height, null);
            _nativeWebViewPtr = WebView_newInNative2DMode(
                gameObject.name,
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );
        }

        public static iOSWebView Instantiate() {

            return (iOSWebView) new GameObject().AddComponent<iOSWebView>();
        }

        /// <see cref="IWithMovablePointer"/>
        public void MovePointer(Vector2 point) {

            _assertValidState();
            int nativeX = (int) (point.x * _nativeWidth);
            int nativeY = (int) (point.y * _nativeHeight);
            WebView_movePointer(_nativeWebViewPtr, nativeX, nativeY);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point) {

            _pointerDown(point, MouseButton.Left, 1);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerDown(point, options.Button, options.ClickCount);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point) {

            _pointerUp(point, MouseButton.Left, 1);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerUp(point, options.Button, options.ClickCount);
        }

        /// <summary>
        /// Sets whether horizontal swipe gestures trigger backward and forward page navigation.
        /// The default is `false`.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     var iOSWebViewInstance = webViewPrefab.Webview as iOSWebView;
        ///     iOSWebViewInstance.SetAllowsBackForwardNavigationGestures(true);
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.apple.com/documentation/webkit/wkwebview/1414995-allowsbackforwardnavigationgestu">WKWebView.allowsBackForwardNavigationGestures</seealso>
        public void SetAllowsBackForwardNavigationGestures(bool allow) {

            _assertValidState();
            WebView_setAllowsBackForwardNavigationGestures(_nativeWebViewPtr, allow);
        }

        /// <summary>
        /// Sets whether HTML5 videos play inline or use the native full-screen controller.
        /// The default is `true`. This method is static because the WKWebView's configuration
        /// cannot be modified at runtime after the webview is created.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     iOSWebView.SetAllowsInlineMediaPlayback(false);
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.apple.com/documentation/webkit/wkwebviewconfiguration/1614793-allowsinlinemediaplayback">WKWebViewConfiguration.allowsInlineMediaPlayback</seealso>
        public static void SetAllowsInlineMediaPlayback(bool allow) {

            WebView_setAllowsInlineMediaPlayback(allow);
        }

        public static void SetAutoplayEnabled(bool enabled) {

            WebView_setAutoplayEnabled(enabled);
        }

    #if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// Sets the given cookie and returns a Task&lt;bool&gt; indicating
        /// whether the cookie was set successfully.
        /// </summary>
        /// <example>
        /// <code>
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     var success = await iOSWebView.SetCookie(new Cookie {
        ///         Domain = "vuplex.com",
        ///         Path = "/",
        ///         Name = "example_name",
        ///         Value = "example_value",
        ///         Secure = true,
        ///         // Expire one day from now
        ///         ExpirationDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 60 * 60 * 24
        ///     });
        /// #endif
        /// </code>
        /// </example>
        public static Task<bool> SetCookie(Cookie cookie) {

            if (cookie == null) {
                throw new ArgumentException("Cookie cannot be null.");
            }
            if (!cookie.IsValid) {
                throw new ArgumentException("Cannot set invalid cookie: " + cookie);
            }
            WebView_setCookie(cookie.ToJson());
            return Task.FromResult(true);
        }
    #endif

        [Obsolete("iOSWebView.SetCustomUriSchemesEnabled() has been removed. Now when a page redirects to a URI with a custom scheme, 3D WebView will automatically emit the UrlChanged and LoadProgressChanged events for the navigation, but a deep link (i.e. to an external application) won't occur.", true)]
        public static void SetCustomUriSchemesEnabled(bool enabled) {}

        public static void SetIgnoreCertificateErrors(bool ignore) {

            WebView_setIgnoreCertificateErrors(ignore);
        }

        /// <summary>
        /// When Native 2D Mode is enabled, this method sets whether long press
        /// gestures are enabled. The default is `true`. When Native 2D Mode is
        /// not enabled, this method has no effect.
        /// </summary>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     var iOSWebViewInstance = webViewPrefab.WebView as iOSWebView;
        ///     iOSWebViewInstance.SetLongPressGesturesEnabled(false);
        /// #endif
        /// </summary>
        public void SetLongPressGesturesEnabled(bool enabled) {

            _assertValidState();
            WebView_setLongPressGesturesEnabled(_nativeWebViewPtr, enabled);
        }

        [Obsolete("iOSWebView.SetNativeKeyboardEnabled() is now deprecated. Instead, please use the NativeOnScreenKeyboardEnabled property of WebViewPrefab / CanvasWebViewPrefab or the IWithNativeOnScreenKeyboard interface.")]
        public static void SetNativeKeyboardEnabled(bool enabled) {

            SetTouchScreenKeyboardEnabled(enabled);
        }

        /// <see cref="IWithNativeOnScreenKeyboard"/>
        public void SetNativeOnScreenKeyboardEnabled(bool enabled) {

            _assertValidState();
            WebView_setNativeOnScreenKeyboardEnabled(_nativeWebViewPtr, enabled);
        }

        /// <see cref="IWithNative2DMode"/>
        public void SetNativeZoomEnabled(bool enabled) {

            _assertValidState();
            _assertNative2DModeEnabled();
            WebView_setNativeZoomEnabled(_nativeWebViewPtr, enabled);
        }

        /// <see cref="IWithNative2DMode"/>
        public void SetRect(Rect rect) {

            _assertValidState();
            _assertNative2DModeEnabled();
            this.Rect = rect;
            WebView_setRect(_nativeWebViewPtr, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }

        public override void SetRenderingEnabled(bool enabled) {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("SetRenderingEnabled");
                return;
            }
            base.SetRenderingEnabled(enabled);
            if (enabled && _currentVideoNativeTexture != IntPtr.Zero) {
                VideoTexture.UpdateExternalTexture(_currentVideoNativeTexture);
            }
        }

        public override void SetResolution(float pixelsPerUnityUnit) {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("SetResolution");
                return;
            }
            base.SetResolution(pixelsPerUnityUnit);
        }

        /// <summary>
        /// When Native 2D Mode is enabled, this method sets whether the scroll view bounces past
        /// the edge of content and back again. The default is `true`. When Native 2D Mode is
        /// disabled, this method has no effect.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     var iOSWebViewInstance = webViewPrefab.Webview as iOSWebView;
        ///     iOSWebViewInstance.SetScrollViewBounces(false);
        /// #endif
        /// </code>
        /// </example>
        /// <seealso href="https://developer.apple.com/documentation/uikit/uiscrollview/1619420-bounces">UIScrollView.bounces</seealso>
        public void SetScrollViewBounces(bool bounces) {

            _assertValidState();
            WebView_setScrollViewBounces(_nativeWebViewPtr, bounces);
        }

        public static void SetStorageEnabled(bool enabled) {

            WebView_setStorageEnabled(enabled);
        }

        /// <summary>
        /// Sets the target web frame rate. The default is `30`, which is also the maximum value.
        /// This method can be used to lower the target web frame rate in order to decrease energy and CPU usage.
        /// 3D WebView's rendering speed is limited by the speed of the underlying iOS APIs, so
        /// the actual web frame rate achieved is always lower than the default target of 30 FPS.
        /// This method is only used for the default render mode and is ignored when Native 2D Mode is enabled.
        /// </summary>
        /// <example>
        /// <code>
        /// await webViewPrefab.WaitUntilInitialized();
        /// #if UNITY_IOS &amp;&amp; !UNITY_EDITOR
        ///     var iOSWebViewInstance = webViewPrefab.WebView as iOSWebView;
        ///     iOSWebViewInstance.SetTargetFrameRate(15);
        /// #endif
        /// </code>
        /// </example>
        public void SetTargetFrameRate(uint targetFrameRate) {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("SetTargetFrameRate");
                return;
            }
            if (targetFrameRate == 0 || targetFrameRate > 30) {
                throw new ArgumentException(String.Format("SetTargetFrameRate() called with invalid frame rate: {0}. The target frame rate must be between 1 and 30.", targetFrameRate));
            }
            WebView_setTargetFrameRate(_nativeWebViewPtr, targetFrameRate);
        }

        // Deprecated
        public static void SetTouchScreenKeyboardEnabled(bool enabled) {

            WebView_setTouchScreenKeyboardEnabled(enabled);
        }


        /// <see cref="IWithSettableUserAgent"/>
        public void SetUserAgent(bool mobile) {

            _assertValidState();
            WebView_setUserAgentToMobile(_nativeWebViewPtr, mobile);
        }

        /// <see cref="IWithSettableUserAgent"/>
        public void SetUserAgent(string userAgent) {

            _assertValidState();
            WebView_setUserAgent(_nativeWebViewPtr, userAgent);
        }

        /// <see cref="IWithNative2DMode"/>
        public void SetVisible(bool visible) {

            _assertValidState();
            _assertNative2DModeEnabled();
            Visible = visible;
            WebView_setVisible(_nativeWebViewPtr, visible);
        }

        public override void ZoomIn() {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("ZoomIn");
                return;
            }
            base.ZoomIn();
        }

        public override void ZoomOut() {

            if (Native2DModeEnabled) {
                VXUtils.LogNative2DModeWarning("ZoomOut");
                return;
            }
            base.ZoomOut();
        }

        delegate void GetCookieCallback(string requestId, string serializedCookie);
        delegate void SetCookieCallback(string requestId, bool success);

        IntPtr _currentVideoNativeTexture;
        static Dictionary<string, Action<Cookie>> _pendingGetCookieResultCallbacks = new Dictionary<string, Action<Cookie>>();
        static Dictionary<string, Action<Cookie[]>> _pendingGetCookiesResultCallbacks = new Dictionary<string, Action<Cookie[]>>();
        readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        void _applyVideoTexture() {

            if (_currentVideoNativeTexture == IntPtr.Zero) {
                return;
            }
            var previousNativeTexturePtr = VideoTexture.GetNativeTexturePtr();
            VideoTexture.UpdateExternalTexture(_currentVideoNativeTexture);
            VideoTexture.Apply();
            var newNativeTexturePtr = VideoTexture.GetNativeTexturePtr();
            if (!(previousNativeTexturePtr == IntPtr.Zero || previousNativeTexturePtr == newNativeTexturePtr)) {
                WebView_destroyTexture(previousNativeTexturePtr, SystemInfo.graphicsDeviceType.ToString());
            }
        }

        void _assertNative2DModeEnabled() {

            if (!Native2DModeEnabled) {
                throw new InvalidOperationException("IWithNative2DMode methods can only be called on a webview with Native 2D Mode enabled.");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(GetCookieCallback))]
        static void _handleGetCookieResult(string resultCallbackId, string serializedCookie) {

            var callback = _pendingGetCookieResultCallbacks[resultCallbackId];
            _pendingGetCookieResultCallbacks.Remove(resultCallbackId);
            if (callback != null) {
                var cookie = Cookie.FromJson(serializedCookie);
                callback(cookie);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(GetCookieCallback))]
        static void _handleGetCookiesResult(string resultCallbackId, string serializedCookies) {

            var callback = _pendingGetCookiesResultCallbacks[resultCallbackId];
            _pendingGetCookiesResultCallbacks.Remove(resultCallbackId);
            var cookies = Cookie.ArrayFromJson(serializedCookies);
            callback(cookies);
        }

        /// <summary>
        /// The native plugin invokes this method.
        /// </summary>
        void HandleVideoTextureChanged(string textureString) {

            var nativeTexture = new IntPtr(Int64.Parse(textureString));
            if (nativeTexture == _currentVideoNativeTexture) {
                return;
            }
            var previousNativeTexture = _currentVideoNativeTexture;
            _currentVideoNativeTexture = nativeTexture;
            if (_renderingEnabled) {
                VideoTexture.UpdateExternalTexture(_currentVideoNativeTexture);
            }

            if (previousNativeTexture != IntPtr.Zero && previousNativeTexture != _currentVideoNativeTexture) {
                WebView_destroyTexture(previousNativeTexture, SystemInfo.graphicsDeviceType.ToString());
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _initializePlugin() {

            // The generic GetFunctionPointerForDelegate<T> is unavailable in legacy .NET 3.5.
            WebView_setCookieCallbacks(
                Marshal.GetFunctionPointerForDelegate((GetCookieCallback)_handleGetCookieResult),
                Marshal.GetFunctionPointerForDelegate((GetCookieCallback)_handleGetCookiesResult)
            );
        }

        void OnEnable() {

            // Start the coroutine from OnEnable so that the coroutine
            // is restarted if the object is deactivated and then reactivated.
            StartCoroutine(_renderPluginOncePerFrame());
        }

        void _pointerDown(Vector2 point, MouseButton mouseButton, int clickCount) {

            _assertValidState();
            int nativeX = (int) (point.x * _nativeWidth);
            int nativeY = (int) (point.y * _nativeHeight);
            WebView_pointerDown(_nativeWebViewPtr, nativeX, nativeY, (int)mouseButton, clickCount);
        }

        void _pointerUp(Vector2 point, MouseButton mouseButton, int clickCount) {

            _assertValidState();
            int nativeX = (int) (point.x * _nativeWidth);
            int nativeY = (int) (point.y * _nativeHeight);
            WebView_pointerUp(_nativeWebViewPtr, nativeX, nativeY, (int)mouseButton, clickCount);
        }

        IEnumerator _renderPluginOncePerFrame() {

            while (true) {
                yield return _waitForEndOfFrame;
                if (Native2DModeEnabled) {
                    // The native render function isn't needed for 2D optimized mode.
                    break;
                }
                if (!_renderingEnabled || IsDisposed) {
                    continue;
                }
                int pointerId = WebView_depositPointer(_nativeWebViewPtr);
                GL.IssuePluginEvent(WebView_getRenderFunction(), pointerId);
            }
        }

        [DllImport(_dllName)]
        private static extern void WebView_captureScreenshot(IntPtr webViewPtr, ref IntPtr bytes, ref int length);

        [DllImport(_dllName)]
        static extern void WebView_clearAllData();

        [DllImport(_dllName)]
        static extern void WebView_clickWithoutStealingFocus(IntPtr webViewPtr, int x, int y);

        [DllImport(_dllName)]
        static extern int WebView_depositPointer(IntPtr pointer);

        [DllImport(_dllName)]
        static extern void WebView_freeMemory(IntPtr bytes);

        [DllImport(_dllName)]
        static extern void WebView_getCookie(string url, string name, string resultCallbackId);

        [DllImport(_dllName)]
        static extern void WebView_getCookies(string url, string resultCallbackId);

        [DllImport(_dllName)]
        static extern IntPtr WebView_getFileUrlForBundleResource(string fileNameWithoutExtension, string fileExtension);

        [DllImport(_dllName)]
        static extern void WebView_getRawTextureData(IntPtr webViewPtr, ref IntPtr bytes, ref int length);

        [DllImport(_dllName)]
        static extern IntPtr WebView_getRenderFunction();

        [DllImport(_dllName)]
        static extern void WebView_globallySetUserAgentToMobile(bool mobile);

        [DllImport(_dllName)]
        static extern void WebView_globallySetUserAgent(string userAgent);

        [DllImport (_dllName)]
        static extern void WebView_movePointer(IntPtr webViewPtr, int x, int y);

        [DllImport(_dllName)]
        static extern IntPtr WebView_new(string gameObjectName, int width, int height, bool fallbackVideoSupportEnabled, bool useOpenGL);

        [DllImport(_dllName)]
        static extern IntPtr WebView_newInNative2DMode(string gameObjectName, int x, int y, int width, int height);

        [DllImport (_dllName)]
        static extern void WebView_pointerDown(IntPtr webViewPtr, int x, int y, int mouseButton, int clickCount);

        [DllImport (_dllName)]
        static extern void WebView_pointerUp(IntPtr webViewPtr, int x, int y, int mouseButton, int clickCount);

        [DllImport(_dllName)]
        static extern void WebView_setAllowsBackForwardNavigationGestures(IntPtr webViewPtr, bool allow);

        [DllImport(_dllName)]
        static extern void WebView_setAllowsInlineMediaPlayback(bool allow);

        [DllImport(_dllName)]
        static extern void WebView_setAutoplayEnabled(bool ignore);

        [DllImport(_dllName)]
        static extern void WebView_setCookie(string serializedCookie);

        [DllImport(_dllName)]
        static extern int WebView_setCookieCallbacks(IntPtr getCookieCallback, IntPtr getCookiesCallback);

        [DllImport(_dllName)]
        static extern void WebView_setIgnoreCertificateErrors(bool ignore);

        [DllImport(_dllName)]
        static extern void WebView_setLongPressGesturesEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setNativeOnScreenKeyboardEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setNativeZoomEnabled(IntPtr webViewPtr, bool enabled);

        [DllImport (_dllName)]
        static extern void WebView_setRect(IntPtr webViewPtr, int x, int y, int width, int height);

        [DllImport (_dllName)]
        static extern void WebView_setScrollViewBounces(IntPtr webViewPtr, bool bounces);

        [DllImport(_dllName)]
        static extern void WebView_setStorageEnabled(bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setTargetFrameRate(IntPtr webViewPtr, uint targetFrameRate);

        [DllImport(_dllName)]
        static extern void WebView_setTouchScreenKeyboardEnabled(bool enabled);

        [DllImport(_dllName)]
        static extern void WebView_setUserAgentToMobile(IntPtr webViewPtr, bool mobile);

        [DllImport(_dllName)]
        static extern void WebView_setUserAgent(IntPtr webViewPtr, string userAgent);

        [DllImport (_dllName)]
        static extern void WebView_setVisible(IntPtr webViewPtr, bool visible);
    }
}
#endif
