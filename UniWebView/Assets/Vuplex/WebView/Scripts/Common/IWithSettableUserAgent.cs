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

namespace Vuplex.WebView {

    /// <summary>
    /// An interface implemented by a webview if it supports changing the
    /// [User-Agent](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent) of
    /// an individual webview instance.
    /// </summary>
    public interface IWithSettableUserAgent {

        /// <summary>
        /// Configures the webview instance to use a mobile or desktop
        /// [User-Agent](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent).
        /// By default, webviews use the browser engine's default User-Agent, but you
        /// can force them to use a mobile User-Agent by calling `SetUserAgent(true)` or a
        /// desktop User-Agent with `SetUserAgent(false)`.
        /// </summary>
        /// <seealso cref="Web.SetUserAgent"/>
        void SetUserAgent(bool mobile);

        /// <summary>
        /// Configures the webview instance to use a custom.
        /// [User-Agent](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent).
        /// </summary>
        /// <seealso cref="Web.SetUserAgent"/>
        void SetUserAgent(string userAgent);
    }
}
