// Copyright (c) 2015-present, LeanCloud, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;

namespace LeanCloud.Internal {
    internal class FileState {
        public string ObjectId { get; internal set; }
        public string Name { get; internal set; }
        public string MimeType { get; internal set; }
        public Uri Url { get; internal set; }
        public IDictionary<string,object> MetaData { get; internal set; }
        public long Size { get; internal set; }
    }
}
