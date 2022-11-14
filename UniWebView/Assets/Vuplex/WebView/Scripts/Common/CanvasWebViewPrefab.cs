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
using UnityEngine;
using UnityEngine.UI;
#if NET_4_6 || NET_STANDARD_2_0
    using System.Threading.Tasks;
#endif
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// CanvasWebViewPrefab is a prefab that makes it easy to view and interact with an IWebView in a 2D Canvas.
    /// It takes care of creating an IWebView, displaying its texture, and handling pointer interactions
    /// from the user, like clicking, dragging, and scrolling. So, all you need to do is specify a URL or HTML to load,
    /// and then the user can view and interact with it. For use outside of a Canvas, see WebViewPrefab instead.
    /// </summary>
    /// <remarks>
    /// There are two ways to create a CanvasWebViewPrefab:
    /// <list type="number">
    ///   <item>
    ///     By dragging the CanvasWebViewPrefab.prefab file into your scene via the editor and setting its "Initial URL" property.</item>
    ///   <item>
    ///     Or by creating an instance programmatically with CanvasWebViewPrefab.Instantiate(), waiting for
    ///     it to initialize, and then calling methods on its WebView property, like LoadUrl().
    ///   </item>
    /// </list>
    /// <para>
    /// If your use case requires a high degree of customization, you can instead create an IWebView
    /// outside of the prefab with Web.CreateWebView().
    /// </para>
    /// See also:
    /// <list type="bullet">
    ///   <item>WebViewPrefab: https://developer.vuplex.com/webview/WebViewPrefab</item>
    ///   <item>How clicking and scrolling works: https://support.vuplex.com/articles/clicking</item>
    ///   <item>IWebView: https://developer.vuplex.com/webview/IWebView</item>
    ///   <item>Web (static methods): https://developer.vuplex.com/webview/Web</item>
    /// </list>
    /// </remarks>
    [HelpURL("https://developer.vuplex.com/webview/CanvasWebViewPrefab")]
    public class CanvasWebViewPrefab : BaseWebViewPrefab {

        public override event EventHandler<ClickedEventArgs> Clicked {
            add {
                if (_native2DModeActive) {
                    _logNative2DModeWarning("The CanvasWebViewPrefab.Clicked event is not supported in Native 2D Mode.");
                }
                base.Clicked += value;
            }
            remove {
                base.Clicked -= value;
            }
        }

        public override event EventHandler<ScrolledEventArgs> Scrolled {
            add {
                if (_native2DModeActive) {
                    _logNative2DModeWarning("The CanvasWebViewPrefab.Scrolled event is not supported in Native 2D Mode.");
                }
                base.Scrolled += value;
            }
            remove {
                base.Scrolled -= value;
            }
        }

        /// <summary>
        /// Enables or disables Native 2D Mode, which makes it so that 3D WebView positions a native 2D webview in front of the Unity game view
        /// instead of displaying web content as a texture in the Unity scene. If the 3D WebView package
        /// in use doesn't support Native 2D Mode, then the default rendering mode is used instead.
        /// For more info on Native 2D Mode, please see [this article](https://support.vuplex.com/articles/native-2d-mode).
        /// </summary>
        /// <remarks>
        /// Important notes:
        /// <list type="bullet">
        ///   <item>
        ///     Native 2D Mode is only supported for 3D WebView for Android (non-Gecko) and 3D WebView for iOS.
        ///     For other packages, the default render mode is used instead.
        ///   </item>
        ///   <item>Native 2D Mode requires that the canvas's render mode be set to "Screen Space - Overlay".</item>
        /// </list>
        /// </remarks>
        [Label("Native 2D Mode (Android and iOS only)")]
        [Tooltip("Native 2D Mode positions a native 2D webview in front of the Unity game view instead of rendering web content as a texture in the Unity scene. Native 2D Mode provides better performance on iOS, because the default mode of rendering web content to a texture is slower. \n\nImportant notes:\n• Native 2D Mode is only supported for 3D WebView for Android (non-Gecko) and 3D WebView for iOS. For other packages, the default render mode is used instead.\n• Native 2D Mode requires that the canvas's render mode be set to \"Screen Space - Overlay\".")]
        [HideInInspector]
        [Header("Platform-specific")]
        public bool Native2DModeEnabled;

        /// <summary>
        /// Determines whether the operating system's native on-screen keyboard is
        /// automatically shown when a text input in the webview is focused. The default for
        /// CanvasWebViewPrefab is `true`.
        /// </summary>
        /// <remarks>
        /// The native on-screen keyboard is only supported for the following packages:
        /// <list type="bullet">
        ///   <item>3D WebView for Android (non-Gecko)</item>
        ///   <item>3D WebView for iOS</item>
        /// </list>
        /// </remarks>
        [Label("Native On-Screen Keyboard (Android and iOS only)")]
        [Tooltip("Determines whether the operating system's native on-screen keyboard is automatically shown when a text input in the webview is focused. The native on-screen keyboard is only supported for the following packages:\n• 3D WebView for Android (non-Gecko)\n• 3D WebView for iOS")]
        public bool NativeOnScreenKeyboardEnabled = true;

        /// <summary>
        /// Sets the webview's initial resolution in pixels per Unity unit.
        /// You can change the resolution to make web content appear larger or smaller.
        /// For more information on scaling web content, see
        /// [this support article](https://support.vuplex.com/articles/how-to-scale-web-content).
        /// </summary>
        /// <remarks>
        /// This property is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </remarks>
        [Label("Initial Resolution (px / Unity unit)")]
        [Tooltip("You can change this to make web content appear larger or smaller. Note that This property is ignored when running in Native 2D Mode.")]
        [HideInInspector]
        public float InitialResolution = 1;

        /// <summary>
        /// Determines the scroll sensitivity. The default sensitivity for CanvasWebViewPrefab is `15`.
        /// </summary>
        /// <remarks>
        /// This property is ignored when running in [Native 2D Mode](https://support.vuplex.com/articles/native-2d-mode).
        /// </remarks>
        [HideInInspector]
        [Tooltip("Determines the scroll sensitivity. Note that This property is ignored when running in Native 2D Mode.")]
        public float ScrollingSensitivity = 15;

        public override bool Visible {
            get {
                var native2DWebView = _getNative2DWebViewIfActive();
                if (native2DWebView != null) {
                    return native2DWebView.Visible;
                }
                return base.Visible;
            }
            set {
                var native2DWebView = _getNative2DWebViewIfActive();
                if (native2DWebView != null) {
                    native2DWebView.SetVisible(value);
                    return;
                }
                base.Visible = value;
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <remarks>
        /// The WebView property is available after initialization completes,
        /// which is indicated by the Initialized event or WaitUntilInitialized().
        /// </remarks>
        /// <example>
        /// var canvas = GameObject.Find("Canvas");
        /// canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate();
        /// canvasWebViewPrefab.transform.parent = canvas.transform;
        /// var rectTransform = canvasWebViewPrefab.transform as RectTransform;
        /// rectTransform.anchoredPosition3D = Vector3.zero;
        /// rectTransform.offsetMin = Vector2.zero;
        /// rectTransform.offsetMax = Vector2.zero;
        /// canvasWebViewPrefab.transform.localScale = Vector3.one;
        /// canvasWebViewPrefab.Initialized += (sender, e) => {
        ///     canvasWebViewPrefab.WebView.LoadUrl("https://vuplex.com");
        /// };
        /// </example>
        public static CanvasWebViewPrefab Instantiate() {

            return Instantiate(new WebViewOptions());
        }

        /// <summary>
        /// Like Instantiate(), except it also accepts an object
        /// of options flags that can be used to alter the generated webview's behavior.
        /// </summary>
        public static CanvasWebViewPrefab Instantiate(WebViewOptions options) {

            var prefabPrototype = (GameObject) Resources.Load("CanvasWebViewPrefab");
            var gameObject = (GameObject) Instantiate(prefabPrototype);
            var canvasWebViewPrefab = gameObject.GetComponent<CanvasWebViewPrefab>();
            canvasWebViewPrefab._options = options;
            return canvasWebViewPrefab;
        }

        /// <summary>
        /// Like `Instantiate()`, except it initializes the instance with an existing, initialized
        /// `IWebView` instance. This causes the `CanvasWebViewPrefab` to use the existing
        /// `IWebView` instance instead of creating a new one.
        /// </summary>
        public static CanvasWebViewPrefab Instantiate(IWebView webView) {

            var prefabPrototype = (GameObject) Resources.Load("CanvasWebViewPrefab");
            var gameObject = (GameObject) Instantiate(prefabPrototype);
            var canvasWebViewPrefab = gameObject.GetComponent<CanvasWebViewPrefab>();
            canvasWebViewPrefab.SetWebViewForInitialization(webView);
            return canvasWebViewPrefab;
        }

        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called.", true)]
        public void Init() {}

        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called.", true)]
        public void Init(WebViewOptions options) {}

        [Obsolete("CanvasWebViewPrefab.Init() has been removed. The CanvasWebViewPrefab script now initializes itself automatically, so Init() no longer needs to be called. Please use CanvasWebViewPrefab.SetWebViewForInitialization(IWebView) instead.", true)]
        public void Init(IWebView webView) {}

        RectTransform _cachedRectTransform;
        RenderMode? _canvasRenderMode {
            get {
                var canvas = GetComponentInParent<Canvas>();
                return canvas == null ? (RenderMode?)null : canvas.renderMode;
            }
        }
        bool _native2DModeActive {
            get {
                var webViewWith2DMode = WebView as IWithNative2DMode;
                return webViewWith2DMode != null && webViewWith2DMode.Native2DModeEnabled;
            }
        }
        RectTransform _rectTransform {
            get {
                if (_cachedRectTransform == null) {
                    _cachedRectTransform = GetComponent<RectTransform>();
                }
                return _cachedRectTransform;
            }
        }
        bool _setCustomPointerInputDetector;

        bool _canNative2DModeBeEnabled(bool logWarnings = false) {

            // Verify that the Canvas's renderMode is set to ScreenSpaceOverlay because that is the only
            // mode for which Native 2D Mode is supported. For more info, see the comments in _getScreenSpaceRect().
            if (_canvasRenderMode != RenderMode.ScreenSpaceOverlay) {
                if (logWarnings) {
                    _logNative2DModeWarning(String.Format("CanvasWebViewPrefab.Native2DModeEnabled is enabled but the canvas's render mode is set to {0}, so Native 2D Mode will not be enabled. In order to use Native 2D Mode, please switch the canvas's render mode to \"Screen Space - Overlay\".", _canvasRenderMode));
                }
                return false;
            }
            if (XrUtils.XRSettings.enabled) {
                if (logWarnings) {
                    _logNative2DModeWarning("CanvasWebViewPrefab.Native2DModeEnabled is enabled but XR is enabled, so Native 2D Mode will not be enabled.");
                }
                return false;
            }
            return true;
        }

        protected override Vector2 _convertRatioPointToUnityUnits(Vector2 point) {

            // Use Vector2.Scale() because Vector2 * Vector2 isn't supported in Unity 2017.
            return Vector2.Scale(point, _rectTransform.rect.size);
        }

        protected override float _getInitialResolution() {

            return InitialResolution;
        }

        IWithNative2DMode _getNative2DWebViewIfActive() {

            var webViewWith2DMode = WebView as IWithNative2DMode;
            if (webViewWith2DMode != null && webViewWith2DMode.Native2DModeEnabled) {
                return webViewWith2DMode;
            }
            return null;
        }

        protected override bool _getNativeOnScreenKeyboardEnabled() {

            return NativeOnScreenKeyboardEnabled;
        }

        protected override float _getScrollingSensitivity() {

            return ScrollingSensitivity;
        }

        Rect _getScreenSpaceRect() {

            // Reference: http://answers.unity.com/answers/1111759/view.html
            // This approach only works when the render mode is set to ScreenSpaceOverlay, and I haven't found a solution
            // that works correctly with ScreenSpaceCamera. So, Native 2D Mode is only supported with ScreenSpaceOverlay for now.
            var size = Vector2.Scale(_rectTransform.rect.size, _rectTransform.lossyScale);
            // Use Vector2.Scale() because Vector2 * Vector2 isn't supported in Unity 2017.
            var flippedPosition = (Vector2)_rectTransform.position - Vector2.Scale(size, _rectTransform.pivot);
            // Unity uses the bottom-left corner as the origin, but 3D WebView always uses the top-left
            // corner as the origin to be consistent with the browser coordinate system. So, translate the Y coordinate.
            var correctedPosition = new Vector2(flippedPosition.x, Screen.height - (flippedPosition.y + size.y));
            return new Rect(correctedPosition, size);
        }

        Rect? _getScreenSpaceRectIfNative2DModeIsEnabled() {

            if (!Native2DModeEnabled) {
                return null;
            }
            if (_canNative2DModeBeEnabled(true)) {
                return _getScreenSpaceRect();
            }
            return null;
        }

        protected override ViewportMaterialView _getVideoLayer() {

            return transform.Find("VideoLayer").GetComponent<ViewportMaterialView>();
        }

        protected override ViewportMaterialView _getView() {

            return transform.Find("CanvasWebViewPrefabView").GetComponent<ViewportMaterialView>();
        }

        void _initCanvasPrefab() {

            _logMacWarningIfNeeded();
            Initialized += _logNative2DRecommendationIfNeeded;
            _init(_rectTransform.rect.size, _getScreenSpaceRectIfNative2DModeIsEnabled());
        }

        void _logMacWarningIfNeeded() {

            #if UNITY_STANDALONE_OSX
                if (_canvasRenderMode == RenderMode.ScreenSpaceOverlay) {
                    WebViewLogger.LogWarning("Unity's macOS player currently has a bug that sometimes prevents 3D WebView's external textures from appearing properly in a \"Screen Space - Overlay\" Canvas (https://issuetracker.unity3d.com/issues/external-texture-is-not-visible-in-player-slash-build-when-canvas-render-mode-is-set-to-screen-space-overlay). If you encounter this issue, please either switch the Canvas's render mode to \"Screen Space - Camera\" or add a script to temporarily resize the player's window with Screen.SetResolution().");
                }
            #endif
        }

        void _logNative2DModeWarning(string message) {

            WebViewLogger.LogWarning(message + " For more info, please see this article: <em>https://support.vuplex.com/articles/native-2d-mode</em>");
        }

        void _logNative2DRecommendationIfNeeded(object sender, EventArgs eventArgs) {

            var webViewWith2DMode = WebView as IWithNative2DMode;
            if (_canNative2DModeBeEnabled() && webViewWith2DMode != null && !webViewWith2DMode.Native2DModeEnabled) {
                WebViewLogger.LogTip("This platform supports Native 2D Mode, so consider enabling CanvasWebViewPrefab.Native2DModeEnabled for best results. For more info, see https://support.vuplex.com/articles/native-2d-mode .");
            }
        }

        void OnDisable() {

            // When in Native 2D Mode, hide the webview when the CanvasWebViewPrefab is deactivated.
            var native2DWebView = _getNative2DWebViewIfActive();
            if (native2DWebView != null) {
                native2DWebView.SetVisible(false);
            }
        }

        void OnEnable() {

            // When in Native 2D Mode, show the webview when the CanvasWebViewPrefab is activated.
            var native2DWebView = _getNative2DWebViewIfActive();
            if (native2DWebView != null) {
                native2DWebView.SetVisible(true);
            }
        }

        protected override void _setVideoLayerPosition(Rect videoRect) {

            var videoRectTransform = _videoLayer.transform as RectTransform;
            // Use Vector2.Scale() because Vector2 * Vector2 isn't supported in Unity 2017.
            videoRectTransform.anchoredPosition = Vector2.Scale(Vector2.Scale(videoRect.position, _rectTransform.rect.size), new Vector2(1, -1));
            videoRectTransform.sizeDelta = Vector2.Scale(videoRect.size, _rectTransform.rect.size);
        }

        void Start() {

            _initCanvasPrefab();
        }

        void Update() {

            if (WebView == null) {
                return;
            }
            // Handle updating the rect for a native 2D webview.
            var native2DWebView = _getNative2DWebViewIfActive();
            if (native2DWebView != null) {
                var screenSpaceRect = _getScreenSpaceRect();
                if (native2DWebView.Rect != screenSpaceRect) {
                    native2DWebView.SetRect(screenSpaceRect);
                }
                return;
            }
            // Handle resizing a regular webview.
            var rect = _rectTransform.rect;
            if (WebView.Size != rect.size) {
                WebView.Resize(rect.width, rect.height);
            }
        }
    }
}
