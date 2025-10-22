using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SevenZipExtractor.Interop;

namespace SevenZipExtractor
{
    internal class SevenZipHandle : IDisposable
    {
        private IntPtr sevenZipSafeHandle;


        internal CreateObjectDelegate LibCreateObject { get; init; }

/*
        //Non-Windows additional exports.
        EXTERN_C BSTR SysAllocStringByteLen(LPCSTR psz, UINT len);
        EXTERN_C BSTR SysAllocStringLen(const OLECHAR *sz, UINT len);
        EXTERN_C BSTR SysAllocString(const OLECHAR *sz);
        EXTERN_C void SysFreeString(BSTR bstr);
        EXTERN_C UINT SysStringByteLen(BSTR bstr);
        EXTERN_C UINT SysStringLen(BSTR bstr);
        */
        internal struct BString
        {

            internal SysAllocBStringByteLenDelegate AllocByteLen { get; set; }

            internal SysAllocBStringLenDelegate AllocLen { get; set; }

            internal SysAllocBStringDelegate Alloc { get; set; }
            internal SysFreeBStringDelegate Free { get; set; }
            internal SysBStringByteLenDelegate ByteLength { get; set; }
            internal SysBStringLenDelegate Length { get; set; }
        };
        internal BString BSTR;
        internal static SevenZipHandle? thisHandle;

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
            }
            ;
            IntPtr exportAddress;
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "CreateObject", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            LibCreateObject = Marshal.GetDelegateForFunctionPointer<CreateObjectDelegate>(exportAddress);
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysFreeString", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.Free= Marshal.GetDelegateForFunctionPointer<SysFreeBStringDelegate>(exportAddress);
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysStringByteLen", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.ByteLength= Marshal.GetDelegateForFunctionPointer<SysBStringByteLenDelegate>(exportAddress);
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysStringLen", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.Length= Marshal.GetDelegateForFunctionPointer<SysBStringLenDelegate>(exportAddress);
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysAllocStringByteLen", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.AllocByteLen = Marshal.GetDelegateForFunctionPointer<SysAllocBStringByteLenDelegate>(exportAddress);
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysAllocStringLen", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.AllocLen = Marshal.GetDelegateForFunctionPointer<SysAllocBStringLenDelegate>(exportAddress);
            if (!NativeLibrary.TryGetExport(sevenZipSafeHandle, "SysAllocString", out exportAddress))
            {
                NativeLibrary.Free(this.sevenZipSafeHandle);
                throw new ArgumentException();
            }
            BSTR.Alloc = Marshal.GetDelegateForFunctionPointer<SysAllocBStringDelegate>(exportAddress);
            } else
            {
                BSTR.Alloc = (_) =>
                {
                    throw new NotImplementedException();
                };
                BSTR.AllocByteLen = (_, __) =>
                {
                    throw new NotImplementedException();
                };
                BSTR.AllocLen = (_, __) =>
                {
                    throw new NotImplementedException();
                };
                BSTR.ByteLength = (_) =>
                {
                    throw new NotImplementedException();
                };
                BSTR.Length = (_) =>
                {
                    throw new NotImplementedException();
                };
                BSTR.Free = (_) =>
                {
                    throw new NotImplementedException();
                };
            }
            
            thisHandle = this;
        }

        ~SevenZipHandle()
        {
            thisHandle = null;
            this.Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (this.sevenZipSafeHandle != 0)
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
            if (this.sevenZipSafeHandle == 0)
            {
                throw new ObjectDisposedException("SevenZipHandle");
            }
            /*
                        IntPtr procAddress = Kernel32Dll.GetProcAddress(this.sevenZipSafeHandle, "CreateObject");
                       
            */
            //CreateObjectDelegate createObject = (CreateObjectDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(CreateObjectDelegate));
            IntPtr result;
            Guid interfaceId = typeof(IInArchive).GUID;
            //createObject(ref classId, ref interfaceId, out result);
            unsafe
            {
                LibCreateObject(&classId, &interfaceId, &result);
                return ComInterfaceMarshaller<IInArchive>.ConvertToManaged((void*)result)!;
            }
        }
        /*
        //Non-Windows additional exports.
        EXTERN_C BSTR SysAllocStringByteLen(LPCSTR psz, UINT len);
        EXTERN_C BSTR SysAllocStringLen(const OLECHAR *sz, UINT len);
        EXTERN_C BSTR SysAllocString(const OLECHAR *sz);
        EXTERN_C void SysFreeString(BSTR bstr);
        EXTERN_C UINT SysStringByteLen(BSTR bstr);
        EXTERN_C UINT SysStringLen(BSTR bstr);

        //Object Creation
        STDAPI CreateObject(const GUID *clsid, const GUID *iid, void **outObject);
        */
        
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr SysAllocBStringByteLenDelegate(IntPtr psz,uint len);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr SysAllocBStringLenDelegate(IntPtr sz,uint len);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr SysAllocBStringDelegate(IntPtr sz);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SysFreeBStringDelegate(IntPtr bstr);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate uint SysBStringByteLenDelegate(IntPtr bstr);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate uint SysBStringLenDelegate(IntPtr bstr);
    }
    

}