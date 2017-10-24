Setup
=====
- I have set up tests for different approaches to sine computation.
- The same algorithms are being tested with C# and C++ implementations.
- Tests run 144000000 iterations and are timed with StopWatch (C#) or chrono::high_resolution_clock (C++).
- C++ code is compiled with VS2017 in Release mode with optimization enabled
- .NET C# code is compiled with VS2017 in Release mode with optimization enabled
- Unity (Mono) C# code is compiled with Unity 2017.2.0f3 (64-bit)
- UnityStandalone3.5 means that Scripting Runtime Version is set to '.NET 3.5 equivalent' 
- UnityStandalone4.6 means that Scripting Runtime Version is set to '.NET 4.6 equivalent' 


Overview of Results
===================

- The fastest approach to sine computation was a look-up table (LUT)
  - tables with 2K to 16M floats performed equally well.
- C++ (x64) performs rougly on par with C# (.NET)
- C# (.NET) outperforms C# (Unity) by a factor of 0.2-17, depending on the test type
- If 'Prefer 32-bit' is enabled in VS2017, C# performs *very poorly*, up to 16x slower



Test Results
============
- All tests were run on an Intel Core i7-6700HQ 2.60 GHz.


Library Sine Test
-----------------

Code::

    float sum = 0.0f;
    float angle_per_iteration = PI2 / iterations;
    for (long i = 0; i < iterations; ++i) {
        float angle = i * angle_per_iteration;
        // C++ uses sinf(), C# uses (float)Math.Sin()
        sum += SIN(angle); // make sure that code isn't optimized away
    }

Results (144000000 iterations):
- C++ (x86)               : 1011 iterations/smp (1 smp = 1/48000 s)
- C++ (x64)               : 5747 iterations/smp !
- C# (UnityStandalone3.5) :  442 iterations/smp 
- C# (UnityStandalone4.6) :  443 iterations/smp 
- C# (UnityEditor)        :  425 iterations/smp 
- C# (.NET)               :  501 iterations/smp 
- C# (.NET Prefer 32-bit) :  388 iterations/smp 


Polynomial Approximation Test
-----------------------------

Code::

    float sum = 0.0f;
    const float periods_per_iteration = 1.0f / iterations;
    const float F = -8.0f * PI;
    for (long i = 0; i < iterations; ++i) {
        float p = i * periods_per_iteration;
        // C++ uses abs, C# uses Math.Abs
        float sine = 8 * p + F * p * ABS(p * PI2);
        sum += sine;
    }

Results (144000000 iterations):
- C++ (x86)               :  8264 iterations/smp 
- C++ (x64)               :  8955 iterations/smp 
- C# (UnityStandalone3.5) :   717 iterations/smp 
- C# (UnityStandalone4.6) :   540 iterations/smp 
- C# (UnityEditor)        :   689 iterations/smp
- C# (.NET)               : 11905 iterations/smp !
- C# (.NET Prefer 32-bit) :  1066 iterations/smp


Array Test
----------

Code::

    // C++ uses 'float *', C# uses 'float[]'
    FLOAT_ARRAY sine = new float[TABLE_SIZE];
    float sum = 0.0f;
    const float periods_per_iteration = 1.0f / iterations;
    for (long i = 0; i < iterations; ++i)
    {
        float periods = i * periods_per_iteration;
        int idx = periods * TABLE_SIZE;
        sum += sine[idx];
    }

Results (144000000 iterations, TABLE_SIZE=2048)
- C++ (x86)               : 16042 iterations/smp
- C++ (x64)               : 16216 iterations/smp !
- C# (UnityStandalone3.5) :  1588 iterations/smp 
- C# (UnityStandalone4.6) :  1614 iterations/smp 
- C# (UnityEditor)        :  1435 iterations/smp
- C# (.NET)               : 15544 iterations/smp
- C# (Prefer 32-bit)      :   976 iterations/smp
Results (144000000 iterations, TABLE_SIZE=16M)
- C++ (x86)               : 15873 iterations/smp !
- C++ (x64)               : 15789 iterations/smp
- C# (UnityStandalone3.5) :  1682 iterations/smp 
- C# (UnityStandalone4.6) :  1614 iterations/smp 
- C# (UnityEditor)        :  1458 iterations/smp
- C# (.NET)               : 15544 iterations/smp
- C# (.NET Prefer 32-bit) :   967 iterations/smp


C++ Notes
=========
- For the Array Test at least, the C++ binary is SSE optimized:

            // Array Test inner loop:

            float periods = i * periods_per_iteration;
    00007FF62B02C8BE  cvtsi2ss    xmm0,dword ptr [rbp+44h]  
    00007FF62B02C8C3  mulss       xmm0,dword ptr [periods_per_iteration]  
    00007FF62B02C8C8  movss       dword ptr [rbp+64h],xmm0  
            int idx = periods * table_size;
    00007FF62B02C8CD  mov         rax,qword ptr [this]  
    00007FF62B02C8D4  cvtsi2ss    xmm0,dword ptr [rax+38h]  
    00007FF62B02C8D9  movss       xmm1,dword ptr [rbp+64h]  
    00007FF62B02C8DE  mulss       xmm1,xmm0  
    00007FF62B02C8E2  movaps      xmm0,xmm1  
    00007FF62B02C8E5  cvttss2si   eax,xmm0  
    00007FF62B02C8E9  mov         dword ptr [rbp+84h],eax  
            sum += sine[idx];
    00007FF62B02C8EF  movsxd      rax,dword ptr [rbp+84h]  
    00007FF62B02C8F6  mov         rcx,qword ptr [this]  
    00007FF62B02C8FD  mov         rcx,qword ptr [rcx+40h]  
    00007FF62B02C901  movss       xmm0,dword ptr [sum]  
    00007FF62B02C906  addss       xmm0,dword ptr [rcx+rax*4]  
    00007FF62B02C90B  movss       dword ptr [sum],xmm0   

