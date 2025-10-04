using SevenZipExtractor.Interface;
using System;
#if !NET9_0_OR_GREATER
using System.Diagnostics;
#endif
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable StringLiteralTypo

namespace SevenZipExtractor.Unmanaged
{
    internal static partial class NativeMethods
    {
        private const string SevenZipPlaceHolderName = "Native7Zip";
        private const string SevenZipMainDllName  = "7z.dll";
        private const string SevenZipMainSoName  = "7z.so";
        private const string SevenZipMainDllPathDir  = "Lib/";
/*
#if NET9_0_OR_GREATER
        private static readonly string CurrentProcessPath = Environment.ProcessPath ?? "";
#else
*/
        private static readonly string CurrentProcessPath = AppDomain.CurrentDomain.BaseDirectory;
//#endif

        static NativeMethods()
        {
            // Use custom Dll import resolver
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        }


        private static string GetLibraryName(string libraryName) {
            if(libraryName == SevenZipPlaceHolderName) {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                return SevenZipMainDllName;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return SevenZipMainSoName;
            } else {
                return "null";
            }
            }
            else return libraryName;
        }
        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            
            string platformLibraryName = GetLibraryName(libraryName);
           
            // Load other library
            IntPtr dllLoadPtr= TryLoadRedirectedDll(platformLibraryName, SevenZipMainDllPathDir, assembly, false);
            
            

            // Try load 7z.dll first

           
        
            // If it returns non zero, return pointer.
            if (dllLoadPtr != IntPtr.Zero)
            {
                return dllLoadPtr;
            }
            
            // Otherwise, throw as it's not found.
            throw new DllNotFoundException("Cannot find or load either 7zxa.dll or 7za.dll or 7z.dll from stock \"Lib\" folder or \"Program Files\" on your machine!");
        }
        private static IntPtr TryLoadRedirectedDll(string dllFileName, string dllFilePath, Assembly assembly, bool throwIfFail = true)
        {
            
            // Get the root directory of the module, then try to get the stock .dll path
            string? assemblyParentPath = Path.GetDirectoryName(CurrentProcessPath);
            string sevenZipStockPath  = Path.Combine(assemblyParentPath ?? "", dllFilePath);
            string fullDllFilePath = sevenZipStockPath+dllFileName;
            // Load stock 7-zip dll if exist
            if (File.Exists(fullDllFilePath))
            {
                // Load the stock <dllFileName> library
                return LoadDllInternal(fullDllFilePath, assembly, null, throwIfFail);
            }

            // If the stock .dll is not found in <collapse_install_path>\Lib\<dllFileName>,
            // then try fallback to the one installed in the system (if exist)
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Try fallback to the official 7-Zip's .dll
                if (File.Exists(sevenZipStockPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", dllFileName))
                // If not found, try fallback to the ZStandard 7-Zip's .dll
                || File.Exists(sevenZipStockPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip-Zstandard", dllFileName))
                // If those two do not exist, then try fallback to the .dll in the <collapse_install_path>\<dllFileName>
                || File.Exists(sevenZipStockPath = Path.Combine(assemblyParentPath ?? "", dllFileName)))
                return LoadDllInternal(sevenZipStockPath, assembly, null, throwIfFail);
            }
           

            // If all fails, then return zero as fail
            return IntPtr.Zero;
        }

        private static IntPtr LoadDllInternal(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, bool throwIfFail)
        {
            Console.WriteLine($"[7-zip][NativeMethods::DllImportResolver] Loading library from path: {libraryName} | Search path: {(searchPath == null ? "Default" : searchPath)}");
            // Try load the library and if fails, then throw.
            bool isLoadSuccessful = NativeLibrary.TryLoad(libraryName, assembly, searchPath, out IntPtr pResult);
            if ((!isLoadSuccessful || pResult == IntPtr.Zero) && throwIfFail)
                throw new FileLoadException($"Failed while loading library from this path: {libraryName}\r\nMake sure that the library/.dll is a valid Win32 library and not corrupted!");

            // If success, then return the pointer to the library
            return pResult;
        }

        internal static unsafe IInArchive? CreateInArchiveClassId(Guid formatClassId)
        {
            Guid interfaceId = Constants.IID_IInArchive_Guid;
            Console.WriteLine($"[7-zip][NativeMethods::CreateInArchiveClassId] Creating IInArcive {interfaceId} with Format Class ID: {formatClassId}");
            nint outObjectNative = 0;
            int result;
            if ((result = CreateObjectDelegate(&formatClassId, &interfaceId, &outObjectNative)) != 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            return ComInterfaceMarshaller<IInArchive>.ConvertToManaged((void*)outObjectNative);
        }

        [LibraryImport(SevenZipPlaceHolderName, EntryPoint = "CreateObject")]
        internal static unsafe partial int CreateObjectDelegate(Guid* classIDNative, Guid* interfaceIDNative, nint* outObjectNative);
    }
}