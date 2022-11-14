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
using UnityEngine;

namespace Vuplex.WebView.Demos {

    /// <summary>
    /// Sets up the CanvasWebViewDemo scene, which displays a CanvasWebViewPrefab
    /// inside a screen space canvas. "Native 2D Mode" is enabled on the
    /// CanvasWebViewPrefab, so a native 2D webview is displayed on Android and iOS.
    /// </summary>
    /// <remarks>
    /// Links: <br/>
    /// - CanvasWebViewPrefab docs: https://developer.vuplex.com/webview/CanvasWebViewPrefab <br/>
    /// - Native 2D Mode: https://support.vuplex.com/articles/native-2d-mode <br/>
    /// - How clicking works: https://support.vuplex.com/articles/clicking <br/>
    /// - Other examples: https://developer.vuplex.com/webview/overview#examples <br/>
    /// </remarks>
    class CanvasWebViewDemo : MonoBehaviour {

        CanvasWebViewPrefab _canvasWebViewPrefab;
        HardwareKeyboardListener _hardwareKeyboardListener;

        void Start() {

            // The CanvasWebViewPrefab's `InitialUrl` property is set via the editor, so it
            // will automatically initialize itself with that URL.
            _canvasWebViewPrefab = GameObject.Find("CanvasWebViewPrefab").GetComponent<CanvasWebViewPrefab>();
            _setUpHardwareKeyboard();
        }

        void _setUpHardwareKeyboard() {

            // Send keys from the hardware (USB or Bluetooth) keyboard to the webview.
            // Use separate `KeyDown()` and `KeyUp()` methods if the webview supports
            // it, otherwise just use `IWebView.HandleKeyboardInput()`.
            // https://developer.vuplex.com/webview/IWithKeyDownAndUp
            _hardwareKeyboardListener = HardwareKeyboardListener.Instantiate();
            _hardwareKeyboardListener.KeyDownReceived += (sender, eventArgs) => {
                var webViewWithKeyDown = _canvasWebViewPrefab.WebView as IWithKeyDownAndUp;
                if (webViewWithKeyDown == null) {
                    _canvasWebViewPrefab.WebView.HandleKeyboardInput(eventArgs.Value);
                } else {
                    webViewWithKeyDown.KeyDown(eventArgs.Value, eventArgs.Modifiers);
                }
            };
            _hardwareKeyboardListener.KeyUpReceived += (sender, eventArgs) => {
                var webViewWithKeyUp = _canvasWebViewPrefab.WebView as IWithKeyDownAndUp;
                if (webViewWithKeyUp != null) {
                    webViewWithKeyUp.KeyUp(eventArgs.Value, eventArgs.Modifiers);
                }
            };
        }
    }
}
