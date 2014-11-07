// Guids.cs
// MUST match guids.h
using System;

namespace Nu.MultiTargetFramework
{
    static class GuidList
    {
        public const string guidMultiTargetFrameworkPkgString = "2ad04cce-7ba3-43dc-b9b5-8196a0ba6bc1";
        public const string guidMultiTargetFrameworkCmdSetString = "073e0a0a-79f5-499e-a4d5-2f8faf02d8a6";
        public const string guidToolWindowPersistanceString = "26030b6c-1ac3-4a9f-b303-0804ad25ba12";

        public static readonly Guid guidMultiTargetFrameworkCmdSet = new Guid(guidMultiTargetFrameworkCmdSetString);
    };
}