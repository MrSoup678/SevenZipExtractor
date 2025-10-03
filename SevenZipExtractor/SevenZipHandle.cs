using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SevenZipExtractor
{
    internal class SevenZipHandle : IDisposable
    {
        private IntPtr sevenZipSafeHandle;

        public SevenZipHandle(string sevenZipLibPath)
        {
            if (!NativeLibrary.TryLoad(sevenZipLibPath, typeof(SevenZipHandle).Assembly, DllImportSearchPath.AssemblyDirectory, out sevenZipSafeHandle))
            {
                throw new DllNotFoundException("Could not load native library.");
            }
            //IntPtr handler;
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "GetHandlerProperty", out _))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            };
        }

        ~SevenZipHandle()
        {
            this.Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (this.sevenZipSafeHandle != null)
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IInArchive CreateInArchive(Guid classId)
        {
            if (this.sevenZipSafeHandle == null)
            {
                throw new ObjectDisposedException("SevenZipHandle");
            }
            /*
                        IntPtr procAddress = Kernel32Dll.GetProcAddress(this.sevenZipSafeHandle, "CreateObject");
                       
            */
            IntPtr procAddress;
            NativeLibrary.TryGetExport(sevenZipSafeHandle, "CreateObject", out procAddress);
            CreateObjectDelegate createObject = (CreateObjectDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(CreateObjectDelegate));
            object result;
            Guid interfaceId = typeof (IInArchive).GUID;
            createObject(ref classId, ref interfaceId, out result);

            return result as IInArchive;
        }
    }
}