using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TensorFlowSharp.Windows
{
    public class NativeBinding : TensorFlow.NativeBinding
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        NativeBinding(bool isGpu = false)
        {
            try
            {
                InitDLL(isGpu);
            }
            catch(Exception ex)
            {
                Log($"TF first init errored. isGPU:{isGpu}");
                Log(ex.ToString());

                if (isGpu)
                {
                    try
                    { 
                        InitDLL(false);
                    }
                    catch(Exception non_ex)
                    {
                        Log("gpu exception");
                        Log(ex.ToString());
                        Log("cpu exception");
                        Log(non_ex.ToString());
                        throw new Exception("gpu, none gpu failed");
                    }
                }
                else
                {
                    throw ex;
                }
            }
        }

        void InitDLL(bool isGpu)
        {
            IsGpu = isGpu;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (isGpu)
            {
                SetDllDirectory(Path.Combine(baseDir, "gpu"));
            }
            else
            {
                SetDllDirectory(Path.Combine(baseDir, "cpu"));
            }

            var version = TensorFlow.TFCore.Version;
        }

        public static void Init(bool isGpu = false)
        {
            try
            {
                Current = new NativeBinding(isGpu);
            }
            catch(Exception ex)
            {
                Log("TF Init failed");
                Log(ex.ToString());
                Current = null;
            }
        }

        protected override unsafe void InternalMemoryCopy(void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
        {
            Buffer.MemoryCopy(source, destination, destinationSizeInBytes, sourceBytesToCopy);
        }
    }
}
