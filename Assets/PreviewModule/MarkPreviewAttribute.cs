using System;

namespace Tools.PreviewModule
{
    public class MarkPreviewAttribute : Attribute
    {
        public Type type;

        public MarkPreviewAttribute(Type previewType)
        {
            type = previewType;
        }
    }
}
