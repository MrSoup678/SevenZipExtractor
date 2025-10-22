

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZipExtractor.Interop
{
    
    internal unsafe static class BStrMarshaller7Zip
    {
        
        //This is for non-Windows binaries. For windows wchar_t is 2-byte wide.
        private static byte sizeOfOLECHAR;

        unsafe static BStrMarshaller7Zip()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sizeOfOLECHAR = 2; //we shouldn't run this anyway.
            } else
            {
                //We need to calculate size of OLECHAR as defined in 7z.so.
                //This is beacuse -fshort-wchar exists, as unlikely as it will be enabled.
                IntPtr dummyBstrHandle = SevenZipHandle.thisHandle!.BSTR.AllocLen(0, 1);
                uint stringSize = SevenZipHandle.thisHandle!.BSTR.Length(dummyBstrHandle);
                uint byteSize = SevenZipHandle.thisHandle!.BSTR.ByteLength(dummyBstrHandle);
                SevenZipHandle.thisHandle!.BSTR.Free(dummyBstrHandle);
                sizeOfOLECHAR = (byte)(byteSize / stringSize);
            }
            
        }
        internal static string NativeToManaged(IntPtr natBstr)
        {
            if(sizeOfOLECHAR == sizeof(char))
            {
                //passthrough.
                Marshal.PtrToStringBSTR(natBstr);
            } else if (sizeOfOLECHAR == sizeof(uint))
            {
                uint stringLength = SevenZipHandle.thisHandle!.BSTR.Length(natBstr);
                uint[] srcString = new uint[stringLength];
                fixed (uint* psrcString = srcString)
                {
                    uint* pnatBstr = (uint*)natBstr;
                    Unsafe.CopyBlock(psrcString, pnatBstr, stringLength * sizeof(uint));
                }
                char[] destString = Array.ConvertAll(srcString,new Converter<uint, char>((p)=>{ return (char)p; }));;
                return new string(destString);
            } else
            {
                throw new NotSupportedException(string.Format("OLECHAR of size {0} is not supported.",sizeOfOLECHAR));
            }
            return "";
        }

        internal static IntPtr ManagedToNative(string strIn)
        {
            if(sizeOfOLECHAR == sizeof(char))
            {
                //passthrough.
                return Marshal.StringToBSTR(strIn);
            } else if (sizeOfOLECHAR == sizeof(uint))
            {
                uint stringLength = (uint)strIn.Length;
                uint[] strBuffer = Array.ConvertAll(strIn.ToCharArray(),new Converter<char, uint>((p)=>{ return p; })); ;
                fixed (uint* pstrBuf = strBuffer)
                {
                    //string length, then string buffer, then null terminator. Note that this is using calloc, so inserting terminator is redundant.
                    uint* initialPtr = (uint*)NativeMemory.AllocZeroed(sizeof(uint) + stringLength * sizeof(uint) + sizeof(uint));

                    NativeMemory.Copy(&stringLength, initialPtr, sizeof(uint));
                    uint* finalPtr = initialPtr + 1;
                    NativeMemory.Copy(pstrBuf, finalPtr, stringLength * sizeof(uint));
                    return (IntPtr)finalPtr;
                    
                }
                
            } else
            {
                throw new NotSupportedException(string.Format("OLECHAR of size {0} is not supported.",sizeOfOLECHAR));
            }
        }
        
    }
}